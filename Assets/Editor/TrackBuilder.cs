using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;

// Monta as cenas das pistas a partir dos projetos em TrackDesigns:
// chão, malha da pista (asfalto e terra), muros, largada, itens,
// decoração, luzes e, por fim, as partes comuns (RaceSceneBuilder).
// Menu: Última Volta > 4. Construir pistas novas
public static class TrackBuilder
{
    const string MeshFolder = "Assets/Pistas/Malhas";
    const string SceneFolder = "Assets/Scenes/Pistas";

    // Medidas das texturas da pista, em pixels (100 pixels = 1 unidade).
    const float TileLength = 5.12f;          // comprimento de uma repetição
    const float DrivableHalf = 2.56f;        // metade da parte pavimentada
    const float AsphaltHalf = 3.44f;         // metade da textura com as guias
    const float DirtHalf = 3.58f;

    // Ordem de desenho: chão, água, terra, asfalto, coisas no chão, itens,
    // carros (5 a 12, ver CarPrefabBuilder) e, por cima, árvores e prédios.
    const int OrderGround = 0, OrderWater = 1, OrderDirt = 2, OrderAsphalt = 3, OrderOnRoad = 4, OrderItem = 6, OrderTall = 14;

    static Material spriteLit;
    static Material spriteUnlit;

    [MenuItem("Última Volta/4. Construir pistas novas")]
    public static void BuildAll()
    {
        foreach (TrackDesign design in TrackDesign.All())
            Build(design);
        SceneList.Update();
    }

    public static void BuildOne(string trackId)
    {
        foreach (TrackDesign design in TrackDesign.All())
            if (design.trackId == trackId)
                Build(design);
        SceneList.Update();
    }

    public static void Build(TrackDesign d)
    {
        spriteLit = AssetSetup.Load<Material>("Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Lit-Default.mat");
        spriteUnlit = AssetSetup.Load<Material>("Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Unlit-Default.mat");
        Directory.CreateDirectory(MeshFolder);
        Directory.CreateDirectory(SceneFolder);

        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        TrackInfo track = AssetSetup.Load<TrackInfo>(AssetSetup.DataFolder + "/Pistas/" + d.trackId + ".asset");

        float drivableHalf = DrivableHalf * d.roadScale;
        float wallDistance = drivableHalf + (d.runoff > 0f ? d.runoff : 0.5f);

        List<Vector2> points;
        List<SurfaceType> surfaces;
        Centerline(d, out points, out surfaces);

        GameObject trackRoot = new GameObject("Pista");
        TrackPath path = trackRoot.AddComponent<TrackPath>();
        path.points = points.ToArray();
        path.surfaces = surfaces.ToArray();
        path.roadHalfWidth = drivableHalf;
        path.offRoadSurface = d.offRoad;
        path.sectorCount = 8;
        path.Build();

        SpriteRenderer ground = Ground(d);
        RoadMeshes(d, path, trackRoot.transform);
        Walls(d, path, wallDistance, trackRoot.transform);
        StartLine(d, path, wallDistance, trackRoot.transform);
        if (d.sea)
            Sea(d);
        if (!float.IsNaN(d.riverX))
            River(d, path, wallDistance, trackRoot.transform);
        RaceSceneBuilder.Result race = RaceSceneBuilder.Build(new RaceSceneBuilder.Settings
        {
            track = track,
            path = path,
            ground = ground,
            background = d.background,
            cameraSize = d.cameraSize,
            botNames = d.botNames,
            botCars = d.botCars,
            botColors = d.botColors,
            botMapColors = d.botMapColors,
            gridOffset = 7f,
            gridSpacing = 6f,
            laneWidth = drivableHalf * 0.45f,
        });
        Items(d, path, drivableHalf, race, trackRoot.transform);
        Decor(d, path, wallDistance);
        if (track.isNight)
            NightLights(path, wallDistance);
        if (d.police)
            Police(path, race);

        string scenePath = SceneFolder + "/" + d.sceneName + ".unity";
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), scenePath);
        Debug.Log("Pista " + d.sceneName + ": volta de " + path.Length.ToString("0") + " unidades, menor raio de curva "
                  + MinRadius(path).ToString("0.0") + ".");
    }

    // ---------- Traçado ----------

    // Curva Catmull-Rom fechada pelos pontos de controle, com pontos a cada 1 unidade.
    static void Centerline(TrackDesign d, out List<Vector2> points, out List<SurfaceType> surfaces)
    {
        Vector2[] c = d.control;
        int n = c.Length;
        List<Vector2> dense = new List<Vector2>();
        List<int> segmentOf = new List<int>();
        const int steps = 40;
        for (int i = 0; i < n; i++)
        {
            Vector2 p0 = c[(i - 1 + n) % n], p1 = c[i], p2 = c[(i + 1) % n], p3 = c[(i + 2) % n];
            for (int s = 0; s < steps; s++)
            {
                float t = s / (float)steps;
                float t2 = t * t, t3 = t2 * t;
                Vector2 p = 0.5f * (2f * p1 + (-p0 + p2) * t + (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 + (-p0 + 3f * p1 - 3f * p2 + p3) * t3);
                dense.Add(p);
                segmentOf.Add(i);
            }
        }

        // Redistribui os pontos com espaçamento igual, guardando o trecho de cada um.
        float total = 0f;
        float[] cumulative = new float[dense.Count + 1];
        for (int i = 0; i < dense.Count; i++)
        {
            total += Vector2.Distance(dense[i], dense[(i + 1) % dense.Count]);
            cumulative[i + 1] = total;
        }
        int count = Mathf.RoundToInt(total);
        float spacing = total / count;
        points = new List<Vector2>();
        surfaces = new List<SurfaceType>();
        int k = 0;
        for (int j = 0; j < count; j++)
        {
            float target = j * spacing;
            while (k < dense.Count - 1 && cumulative[k + 1] < target)
                k++;
            float segment = cumulative[k + 1] - cumulative[k];
            float t = segment > 0f ? (target - cumulative[k]) / segment : 0f;
            points.Add(Vector2.Lerp(dense[k], dense[(k + 1) % dense.Count], t));
            int controlSegment = segmentOf[k];
            surfaces.Add(d.segmentSurface != null ? d.segmentSurface[controlSegment] : SurfaceType.Asfalto);
        }
    }

    static Vector2 Normal(TrackPath path, int i)
    {
        int n = path.points.Length;
        Vector2 direction = (path.points[(i + 1) % n] - path.points[(i - 1 + n) % n]).normalized;
        return new Vector2(-direction.y, direction.x);
    }

    static float MinRadius(TrackPath path)
    {
        float min = float.MaxValue;
        int n = path.points.Length;
        for (int i = 0; i < n; i++)
        {
            Vector2 a = path.points[(i - 3 + n) % n], b = path.points[i], c = path.points[(i + 3) % n];
            float angle = Mathf.Abs(Vector2.SignedAngle(b - a, c - b)) * Mathf.Deg2Rad;
            if (angle > 0.0001f)
                min = Mathf.Min(min, Vector2.Distance(a, c) / angle);
        }
        return min;
    }

    // ---------- Chão, pista e muros ----------

    static SpriteRenderer Ground(TrackDesign d)
    {
        SpriteRenderer ground = SpriteObject(null, "Chao", d.groundSprite, Vector2.zero, 0f, 1f, OrderGround, spriteLit);
        ground.drawMode = SpriteDrawMode.Tiled;
        ground.size = d.mapSize;
        ground.color = d.groundTint;
        return ground;
    }

    // Uma malha para cada trecho seguido de mesmo piso. A textura se repete
    // ao longo da pista, como as peças retas da CraftPix.
    static void RoadMeshes(TrackDesign d, TrackPath path, Transform parent)
    {
        int n = path.points.Length;
        int start = 0;
        while (start < n && path.surfaces[start] == path.surfaces[(start - 1 + n) % n])
            start++;
        if (start == n)
            start = 0;

        int runIndex = 0;
        int i = 0;
        while (i < n)
        {
            int first = (start + i) % n;
            SurfaceType surface = path.surfaces[first];
            int length = 0;
            while (i + length < n && path.surfaces[(start + i + length) % n] == surface)
                length++;

            List<int> indices = new List<int>();
            for (int k = -1; k <= length; k++)
                indices.Add(((first + k) % n + n) % n);

            RoadMesh(d, path, indices, surface, parent, runIndex++);
            i += length;
        }
    }

    static void RoadMesh(TrackDesign d, TrackPath path, List<int> indices, SurfaceType surface, Transform parent, int runIndex)
    {
        bool dirt = surface == SurfaceType.Terra;
        float half = (dirt ? DirtHalf : AsphaltHalf) * d.roadScale;
        float tile = TileLength * d.roadScale;

        Vector3[] vertices = new Vector3[indices.Count * 2];
        Vector2[] uv = new Vector2[indices.Count * 2];
        Color[] colors = new Color[indices.Count * 2];
        int[] triangles = new int[(indices.Count - 1) * 6];
        float distance = path.DistanceAtPoint(indices[0]);
        for (int k = 0; k < indices.Count; k++)
        {
            int index = indices[k];
            if (k > 0)
                distance += Vector2.Distance(path.points[indices[k - 1]], path.points[index]);
            Vector2 normal = Normal(path, index);
            vertices[k * 2] = path.points[index] + normal * half;
            vertices[k * 2 + 1] = path.points[index] - normal * half;
            uv[k * 2] = new Vector2(distance / tile, 1f);
            uv[k * 2 + 1] = new Vector2(distance / tile, 0f);
            colors[k * 2] = colors[k * 2 + 1] = Color.white;
            if (k < indices.Count - 1)
            {
                int t = k * 6, v = k * 2;
                triangles[t] = v; triangles[t + 1] = v + 2; triangles[t + 2] = v + 1;
                triangles[t + 3] = v + 1; triangles[t + 4] = v + 2; triangles[t + 5] = v + 3;
            }
        }

        Mesh mesh = new Mesh { name = d.sceneName + "_" + runIndex };
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.colors = colors;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
        string meshPath = MeshFolder + "/" + d.sceneName + "_" + runIndex + ".asset";
        AssetDatabase.DeleteAsset(meshPath);
        AssetDatabase.CreateAsset(mesh, meshPath);

        GameObject go = new GameObject((dirt ? "Terra_" : "Asfalto_") + runIndex);
        go.transform.SetParent(parent, false);
        go.AddComponent<MeshFilter>().sharedMesh = mesh;
        MeshRenderer renderer = go.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = AssetSetup.Load<Material>(AssetSetup.MaterialsFolder + (dirt ? "/Terra.mat" : "/Asfalto.mat"));
        renderer.sortingOrder = dirt ? OrderDirt : OrderAsphalt;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    }

    // Dois laços contínuos, um de cada lado, sem aberturas.
    static void Walls(TrackDesign d, TrackPath path, float distance, Transform parent)
    {
        GameObject walls = new GameObject("Muros");
        walls.transform.SetParent(parent, false);
        foreach (float side in new[] { 1f, -1f })
        {
            int n = path.points.Length;
            Vector2[] line = new Vector2[n + 1];
            for (int i = 0; i < n; i++)
                line[i] = path.points[i] + Normal(path, i) * distance * side;
            line[n] = line[0];
            EdgeCollider2D edge = walls.AddComponent<EdgeCollider2D>();
            edge.points = line;
            edge.edgeRadius = 0.1f;

            // Com área de escape, o muro é marcado por arbustos e pedras.
            if (d.runoff > 0f)
                for (int i = 0; i < n; i += 3)
                {
                    Vector2 position = path.points[i] + Normal(path, i) * (distance + 0.7f) * side;
                    if (InRiver(d, position, 0f))
                        continue;
                    bool rock = (i / 3) % 4 == 3;
                    SpriteRenderer decor = SpriteObject(walls.transform, rock ? "Pedra" : "Arbusto",
                        rock ? "Decor/Rock" : "Decor/Bush", position, Random(i * 7 + (side > 0 ? 1 : 2)) * 360f,
                        rock ? 0.28f : 0.33f, OrderItem, spriteLit);
                    if (d.style == DecorStyle.Deserto && !rock)
                        decor.color = new Color(0.85f, 0.8f, 0.5f);
                }
        }
    }

    static void StartLine(TrackDesign d, TrackPath path, float wallDistance, Transform parent)
    {
        Vector2 normal = Normal(path, 0);
        float angle = Vector2.SignedAngle(Vector2.right, normal);
        SpriteObject(parent, "LinhaChegada", "Decor/Finish", path.points[0], angle, 0.5f * d.roadScale / 1.4f, OrderOnRoad, spriteLit);

        // Arquibancadas do lado de dentro, junto da reta de largada.
        for (int k = 0; k < 2; k++)
        {
            float distanceAlong = path.Length - 14f + k * 18f;
            Vector2 direction = path.DirectionAt(distanceAlong);
            Vector2 left = new Vector2(-direction.y, direction.x);
            Vector2 position = path.PositionAt(distanceAlong) + left * (wallDistance + 3.2f);
            SpriteObject(parent, "Arquibancada", "Decor/Pavilion", position, Vector2.SignedAngle(Vector2.right, direction) + 180f,
                0.6f, OrderTall, spriteLit);
        }
    }

    // ---------- Água ----------

    static void Sea(TrackDesign d)
    {
        float bottom = -d.mapSize.y / 2f;
        SpriteRenderer water = SpriteObject(null, "Mar", "Fundo/Water_Tile", new Vector2(0f, bottom + 7f), 0f, 1f, OrderWater, spriteLit);
        water.drawMode = SpriteDrawMode.Tiled;
        water.size = new Vector2(d.mapSize.x, 14f);
        SpriteRenderer sand = SpriteObject(null, "Areia", "Fundo/Soil_Tile", new Vector2(0f, bottom + 15.5f), 0f, 1f, OrderWater, spriteLit);
        sand.drawMode = SpriteDrawMode.Tiled;
        sand.size = new Vector2(d.mapSize.x, 3f);
        sand.color = new Color(1f, 0.92f, 0.7f);
    }

    static bool InRiver(TrackDesign d, Vector2 position, float margin)
    {
        return !float.IsNaN(d.riverX) && Mathf.Abs(position.x - d.riverX) < d.riverWidth / 2f + margin;
    }

    // Rio vertical: embaixo do asfalto vira ponte; onde cruza a pista de
    // baixo, a água fica por cima (vau) e deixa o carro lento.
    static void River(TrackDesign d, TrackPath path, float wallDistance, Transform parent)
    {
        SpriteRenderer river = SpriteObject(null, "Rio", "Fundo/Water_Tile", new Vector2(d.riverX, 0f), 0f, 1f, OrderWater, spriteLit);
        river.drawMode = SpriteDrawMode.Tiled;
        river.size = new Vector2(d.riverWidth, d.mapSize.y);

        // Acha onde a linha central cruza o rio na metade de baixo do mapa.
        int n = path.points.Length;
        for (int i = 0; i < n; i++)
        {
            Vector2 a = path.points[i], b = path.points[(i + 1) % n];
            if ((a.x - d.riverX) * (b.x - d.riverX) > 0f || a.y > 0f)
                continue;

            Vector2 crossing = Vector2.Lerp(a, b, (d.riverX - a.x) / (b.x - a.x));
            float height = wallDistance * 2f + 2f;
            SpriteRenderer ford = SpriteObject(parent, "Vau", "Fundo/Water_Tile", crossing, 0f, 1f, OrderOnRoad, spriteLit);
            ford.drawMode = SpriteDrawMode.Tiled;
            ford.size = new Vector2(d.riverWidth, height);
            BoxCollider2D zone = ford.gameObject.AddComponent<BoxCollider2D>();
            zone.isTrigger = true;
            zone.size = new Vector2(d.riverWidth, height);
            ford.gameObject.AddComponent<SurfaceZone>().surface = SurfaceType.Agua;

            // Rampa antes do rio: quem chega rápido passa por cima da água.
            float crossingDistance = path.DistanceAtPoint(i) + Vector2.Distance(a, crossing);
            Ramp(path, crossingDistance - d.riverWidth / 2f - 1f - 2.75f, parent);
            return;
        }
    }

    static void Ramp(TrackPath path, float distance, Transform parent)
    {
        Vector2 direction = path.DirectionAt(distance);
        SpriteRenderer ramp = SpriteObject(parent, "Rampa", "Obstaculos/Jumping_Pad_02", path.PositionAt(distance),
            Vector2.SignedAngle(Vector2.up, direction), 0.55f, OrderOnRoad, spriteLit);
        // O pulo começa no fim da rampa, não no começo.
        BoxCollider2D trigger = ramp.gameObject.AddComponent<BoxCollider2D>();
        trigger.isTrigger = true;
        trigger.size = new Vector2(6.9f, 3f);
        trigger.offset = new Vector2(0f, 3.3f);
        ramp.gameObject.AddComponent<JumpPad>();
    }

    // ---------- Itens ----------

    static void Items(TrackDesign d, TrackPath path, float drivableHalf, RaceSceneBuilder.Result race, Transform parent)
    {
        GameObject root = new GameObject("Itens");
        root.transform.SetParent(parent, false);
        foreach (TrackItem item in d.items)
        {
            for (int c = 0; c < item.count; c++)
            {
                float distance = item.fraction * path.Length + c * 3f;
                Vector2 direction = path.DirectionAt(distance);
                Vector2 left = new Vector2(-direction.y, direction.x);
                Vector2 position = path.PositionAt(distance) + left * item.lateral;
                float along = Vector2.SignedAngle(Vector2.up, direction);
                Place(item.type, position, along, root.transform, race);
            }
        }
    }

    static void Place(ItemType type, Vector2 position, float along, Transform parent, RaceSceneBuilder.Result race)
    {
        switch (type)
        {
            case ItemType.Moedas:
            {
                SpriteRenderer coin = SpriteObject(parent, "Moeda", "Gerados/Moeda", position, 0f, 0.9f, OrderItem, spriteUnlit);
                coin.gameObject.AddComponent<CircleCollider2D>().isTrigger = true;
                CoinPickup pickup = coin.gameObject.AddComponent<CoinPickup>();
                pickup.raceManager = race.raceManager;
                pickup.audioManager = race.audioManager;
                break;
            }
            case ItemType.Barril:
            {
                SpriteRenderer barrel = SpriteObject(parent, "Barril", "Obstaculos/Barrel_01", position, 0f, 0.25f, OrderItem, spriteLit);
                barrel.gameObject.AddComponent<CircleCollider2D>();
                barrel.gameObject.AddComponent<Obstacle>();
                Rigidbody2D body = barrel.gameObject.AddComponent<Rigidbody2D>();
                body.gravityScale = 0f;
                body.mass = 2f;
                body.linearDamping = 3f;
                body.angularDamping = 3f;
                break;
            }
            case ItemType.Oleo:
            {
                SpriteRenderer oil = SpriteObject(parent, "Oleo", "Obstaculos/Oil", position, along + 90f, 0.42f, OrderOnRoad, spriteLit);
                oil.gameObject.AddComponent<BoxCollider2D>().isTrigger = true;
                oil.gameObject.AddComponent<OilSlick>();
                break;
            }
            case ItemType.Conserto:
            {
                SpriteRenderer repair = SpriteObject(parent, "Conserto", "Obstaculos/HP_Bonus", position, 0f, 0.3f, OrderItem, spriteUnlit);
                repair.gameObject.AddComponent<CircleCollider2D>().isTrigger = true;
                RepairPickup pickup = repair.gameObject.AddComponent<RepairPickup>();
                pickup.raceManager = race.raceManager;
                pickup.audioManager = race.audioManager;
                break;
            }
            case ItemType.Rocha:
            {
                SpriteRenderer rock = SpriteObject(parent, "Rocha", "Decor/Rock", position, along, 0.35f, OrderItem, spriteLit);
                rock.gameObject.AddComponent<CircleCollider2D>().radius = 2.2f;
                break;
            }
            case ItemType.Rampa:
            {
                TrackPath path = Object.FindAnyObjectByType<TrackPath>();
                path.Build();
                int index = path.FindNearestGlobal(position);
                Ramp(path, path.DistanceAtPoint(index), parent);
                break;
            }
        }
    }

    // ---------- Decoração ----------

    struct Placed { public Vector2 position; public float radius; }

    // Espalha prédios, árvores, arbustos e pedras longe da pista.
    static void Decor(TrackDesign d, TrackPath path, float wallDistance)
    {
        GameObject root = new GameObject("Decoracao");
        System.Random random = new System.Random(d.seed);
        List<Placed> placed = new List<Placed>();

        switch (d.style)
        {
            case DecorStyle.Cidade:
                Scatter(d, path, wallDistance, root.transform, random, placed, "Decor/Decor_Building", 90, 0.65f, 0.8f, 4.5f, OrderTall, true, Color.white);
                Scatter(d, path, wallDistance, root.transform, random, placed, "Decor/Tree", 80, 0.35f, 0.45f, 2f, OrderTall, false, Color.white);
                Scatter(d, path, wallDistance, root.transform, random, placed, "Decor/Bush", 50, 0.28f, 0.34f, 1f, OrderItem, false, Color.white);
                break;
            case DecorStyle.Floresta:
                Scatter(d, path, wallDistance, root.transform, random, placed, "Decor/Tree", 420, 0.35f, 0.55f, 2.1f, OrderTall, false, Color.white);
                Scatter(d, path, wallDistance, root.transform, random, placed, "Decor/Bush", 150, 0.28f, 0.36f, 1f, OrderItem, false, Color.white);
                Scatter(d, path, wallDistance, root.transform, random, placed, "Decor/Rock", 25, 0.25f, 0.35f, 1.2f, OrderItem, false, Color.white);
                break;
            case DecorStyle.Deserto:
                Scatter(d, path, wallDistance, root.transform, random, placed, "Decor/Rock", 120, 0.25f, 0.5f, 1.6f, OrderItem, false, Color.white);
                Scatter(d, path, wallDistance, root.transform, random, placed, "Decor/Bush", 40, 0.25f, 0.32f, 1f, OrderItem, false, new Color(0.85f, 0.8f, 0.5f));
                Scatter(d, path, wallDistance, root.transform, random, placed, "Decor/Tree", 10, 0.3f, 0.4f, 1.8f, OrderTall, false, new Color(0.8f, 0.85f, 0.6f));
                break;
            case DecorStyle.CidadeNoite:
                Scatter(d, path, wallDistance, root.transform, random, placed, "Decor/Decor_Building", 140, 0.6f, 0.8f, 4.2f, OrderTall, true, Color.white);
                Scatter(d, path, wallDistance, root.transform, random, placed, "Decor/Tree", 30, 0.32f, 0.4f, 1.8f, OrderTall, false, Color.white);
                break;
        }
    }

    static void Scatter(TrackDesign d, TrackPath path, float wallDistance, Transform parent, System.Random random,
        List<Placed> placed, string sprite, int count, float minScale, float maxScale, float radius, int order, bool rightAngles, Color tint)
    {
        Vector2 half = d.mapSize / 2f;
        int created = 0;
        for (int attempt = 0; attempt < count * 12 && created < count; attempt++)
        {
            Vector2 position = new Vector2(
                Mathf.Lerp(-half.x + radius, half.x - radius, (float)random.NextDouble()),
                Mathf.Lerp(-half.y + radius, half.y - radius, (float)random.NextDouble()));

            if (DistanceToPath(path, position) < wallDistance + radius + 1.2f)
                continue;
            if (d.sea && position.y < -half.y + 19f + radius)
                continue;
            if (InRiver(d, position, radius + 1f))
                continue;
            bool overlaps = false;
            foreach (Placed other in placed)
                if ((other.position - position).sqrMagnitude < (other.radius + radius) * (other.radius + radius) * 0.7f)
                {
                    overlaps = true;
                    break;
                }
            if (overlaps)
                continue;

            float scale = Mathf.Lerp(minScale, maxScale, (float)random.NextDouble());
            float angle = rightAngles ? random.Next(4) * 90f : (float)random.NextDouble() * 360f;
            SpriteRenderer renderer = SpriteObject(parent, Path.GetFileName(sprite), sprite, position, angle, scale, order, spriteLit);
            renderer.color = tint;
            placed.Add(new Placed { position = position, radius = radius });
            created++;
        }
    }

    static float DistanceToPath(TrackPath path, Vector2 position)
    {
        float best = float.MaxValue;
        for (int i = 0; i < path.points.Length; i += 2)
            best = Mathf.Min(best, (path.points[i] - position).sqrMagnitude);
        return Mathf.Sqrt(best);
    }

    // ---------- Noite e polícia ----------

    // Postes de neon dos dois lados da pista, alternando cores.
    static void NightLights(TrackPath path, float wallDistance)
    {
        GameObject root = new GameObject("LuzesNeon");
        Color[] colors = { new Color(1f, 0.3f, 0.8f), new Color(0.3f, 0.9f, 1f), new Color(1f, 0.85f, 0.4f) };
        int k = 0;
        for (float distance = 0f; distance < path.Length; distance += 22f, k++)
        {
            Vector2 direction = path.DirectionAt(distance);
            Vector2 left = new Vector2(-direction.y, direction.x);
            float side = k % 2 == 0 ? 1f : -1f;
            Vector2 position = path.PositionAt(distance) + left * (wallDistance + 1.2f) * side;

            GameObject lamp = new GameObject("Poste");
            lamp.transform.SetParent(root.transform, false);
            lamp.transform.position = position;
            Light2D light = lamp.AddComponent<Light2D>();
            light.lightType = Light2D.LightType.Point;
            light.pointLightOuterRadius = 12f;
            light.pointLightInnerRadius = 1f;
            light.intensity = 1.1f;
            light.color = colors[k % colors.Length];

            SpriteRenderer bulb = SpriteObject(lamp.transform, "Lampada", "Gerados/Disco", position, 0f, 1.6f, OrderTall + 1, spriteUnlit);
            bulb.color = colors[k % colors.Length];
        }
    }

    static void Police(TrackPath path, RaceSceneBuilder.Result race)
    {
        float distance = path.Length * 0.5f;
        Vector2 direction = path.DirectionAt(distance);
        GameObject car = (GameObject)PrefabUtility.InstantiatePrefab(AssetSetup.Load<GameObject>(CarPrefabBuilder.BotPrefabPath));
        car.name = "Viatura";
        car.transform.SetPositionAndRotation(path.PositionAt(distance), Quaternion.Euler(0f, 0f, Vector2.SignedAngle(Vector2.up, direction)));

        RaceCar raceCar = car.GetComponent<RaceCar>();
        raceCar.countsInRace = false;
        raceCar.driverName = "Polícia";
        raceCar.raceManager = race.raceManager;
        raceCar.track = path;
        car.GetComponent<CarController>().raceManager = race.raceManager;
        car.transform.Find("IconeMinimapa").GetComponent<SpriteRenderer>().color = new Color(0.2f, 0.4f, 1f);

        CarSetup setup = car.GetComponent<CarSetup>();
        setup.carId = "blindado";
        setup.colorId = "original";
        setup.botSkill = 0.35f;

        BotDriver bot = car.GetComponent<BotDriver>();
        bot.track = path;
        bot.skill = 0.35f;
        bot.player = null;

        SpriteRenderer beacon = SpriteObject(car.transform, "Sirene", "Efeitos/Police_Beacons_Red_000", (Vector2)car.transform.position, 0f, 0.12f, 9, spriteUnlit);
        beacon.transform.localPosition = new Vector3(0f, 0.4f, 0f);
        beacon.transform.localRotation = Quaternion.identity;
        Animator animator = beacon.gameObject.AddComponent<Animator>();
        animator.runtimeAnimatorController = AssetSetup.Load<RuntimeAnimatorController>(AssetSetup.AnimationsFolder + "/Sirene.controller");

        PoliceCar police = car.AddComponent<PoliceCar>();
        police.player = race.raceManager.player;
        police.siren = animator;
    }

    // ---------- Ajudantes ----------

    static SpriteRenderer SpriteObject(Transform parent, string name, string sprite, Vector2 position, float angle,
        float scale, int order, Material material)
    {
        GameObject go = new GameObject(name);
        if (parent != null)
            go.transform.SetParent(parent, false);
        go.transform.SetPositionAndRotation(position, Quaternion.Euler(0f, 0f, angle));
        go.transform.localScale = Vector3.one * scale;
        SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = AssetSetup.Load<Sprite>("Assets/Sprites/" + sprite + ".png");
        renderer.sortingOrder = order;
        renderer.sharedMaterial = material;
        return renderer;
    }

    // Número entre 0 e 1 sempre igual para a mesma semente (decoração estável).
    static float Random(int seed)
    {
        return Mathf.Repeat(Mathf.Sin(seed * 12.9898f) * 43758.5453f, 1f);
    }
}
