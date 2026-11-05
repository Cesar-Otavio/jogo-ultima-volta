using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Transforma a cena antiga "Corrida" na pista "Treino": mantém o traçado,
// as paredes, os barris e o óleo aprovados, e troca carro, câmera, telas e
// áudio pelos sistemas novos (os mesmos das outras pistas).
// Menu: Última Volta > 3. Converter a pista de Treino
public static class TrainingConverter
{
    const string OldScene = "Assets/Scenes/Corrida.unity";
    const string NewScene = "Assets/Scenes/Treino.unity";
    static readonly Vector2 FinishLine = new Vector2(-28.59f, 0.5f);

    [MenuItem("Última Volta/3. Converter a pista de Treino")]
    public static void Convert()
    {
        if (System.IO.File.Exists(OldScene) && !System.IO.File.Exists(NewScene))
            AssetDatabase.MoveAsset(OldScene, NewScene);

        EditorSceneManager.OpenScene(NewScene);

        // Remove os sistemas antigos (ou os de uma conversão anterior).
        foreach (string name in new[] { "Main Camera", "Carro", "CarroJogador", "GerenciadorCorrida", "Corrida", "Telas",
                     "EventSystem", "Audio", "LuzGlobal", "CameraMinimapa", "LinhaMinimapa", "FogosDeArtificio", "CaminhoPista" })
        {
            GameObject old = GameObject.Find(name);
            while (old != null)
            {
                Object.DestroyImmediate(old);
                old = GameObject.Find(name);
            }
        }

        // A linha de chegada fica só como desenho: as voltas agora vêm do caminho da pista.
        GameObject finish = GameObject.Find("LinhaChegada");
        if (finish != null)
        {
            GameObjectUtility.RemoveMonoBehavioursWithMissingScript(finish);
            Collider2D trigger = finish.GetComponent<Collider2D>();
            if (trigger != null)
                Object.DestroyImmediate(trigger);
        }
        GameObject checkpoint = GameObject.Find("PontoControle");
        if (checkpoint != null)
            Object.DestroyImmediate(checkpoint);

        TrackPath path = BuildPath();

        TrackInfo track = AssetSetup.Load<TrackInfo>(AssetSetup.DataFolder + "/Pistas/treino.asset");
        SpriteRenderer ground = GameObject.Find("Chao").GetComponent<SpriteRenderer>();

        RaceSceneBuilder.Build(new RaceSceneBuilder.Settings
        {
            track = track,
            path = path,
            ground = ground,
            background = new Color(0.1f, 0.2f, 0.12f),
            cameraSize = 8f,
            gridOffset = 6.5f,
        });

        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        SceneList.Update();
        Debug.Log("Treino convertido: volta de " + path.Length.ToString("0.0") + " unidades, " + path.points.Length + " pontos.");
    }

    // A linha central é o meio entre as duas paredes: para cada ponto da
    // parede de fora, pega o ponto mais próximo da parede de dentro.
    static TrackPath BuildPath()
    {
        EdgeCollider2D[] walls = GameObject.Find("Paredes").GetComponents<EdgeCollider2D>();
        EdgeCollider2D outer = walls[0].pointCount >= walls[1].pointCount ? walls[0] : walls[1];
        EdgeCollider2D inner = outer == walls[0] ? walls[1] : walls[0];

        List<Vector2> outerPoints = Resample(new List<Vector2>(outer.points), 0.5f);
        List<Vector2> middle = new List<Vector2>();
        foreach (Vector2 p in outerPoints)
            middle.Add((p + ClosestOnPolyline(inner.points, p)) / 2f);

        List<Vector2> centerline = Resample(middle, 1f);

        // O primeiro ponto fica na linha de chegada, e a pista segue para cima (sentido da largada).
        int start = 0;
        for (int i = 1; i < centerline.Count; i++)
            if ((centerline[i] - FinishLine).sqrMagnitude < (centerline[start] - FinishLine).sqrMagnitude)
                start = i;
        List<Vector2> ordered = new List<Vector2>();
        for (int i = 0; i < centerline.Count; i++)
            ordered.Add(centerline[(start + i) % centerline.Count]);
        if (ordered[1].y < ordered[0].y)
        {
            ordered.Reverse(1, ordered.Count - 1);
        }

        GameObject go = new GameObject("CaminhoPista");
        TrackPath path = go.AddComponent<TrackPath>();
        path.points = ordered.ToArray();
        path.surfaces = new SurfaceType[ordered.Count];
        path.roadHalfWidth = 2.51f;
        path.offRoadSurface = SurfaceType.Grama;
        path.sectorCount = 8;
        path.Build();
        return path;
    }

    // Distribui pontos a cada "step" unidades ao longo de um laço fechado.
    public static List<Vector2> Resample(List<Vector2> loop, float step)
    {
        List<Vector2> result = new List<Vector2>();
        float total = 0f;
        for (int i = 0; i < loop.Count; i++)
            total += Vector2.Distance(loop[i], loop[(i + 1) % loop.Count]);

        int count = Mathf.Max(8, Mathf.RoundToInt(total / step));
        float spacing = total / count;
        int segment = 0;
        float segmentStart = 0f;
        for (int k = 0; k < count; k++)
        {
            float target = k * spacing;
            float segmentLength = Vector2.Distance(loop[segment], loop[(segment + 1) % loop.Count]);
            while (segmentStart + segmentLength < target && segment < loop.Count - 1)
            {
                segmentStart += segmentLength;
                segment++;
                segmentLength = Vector2.Distance(loop[segment], loop[(segment + 1) % loop.Count]);
            }
            float t = segmentLength > 0f ? (target - segmentStart) / segmentLength : 0f;
            result.Add(Vector2.Lerp(loop[segment], loop[(segment + 1) % loop.Count], t));
        }
        return result;
    }

    static Vector2 ClosestOnPolyline(Vector2[] line, Vector2 p)
    {
        Vector2 best = line[0];
        float bestDistance = float.MaxValue;
        for (int i = 0; i < line.Length; i++)
        {
            Vector2 a = line[i];
            Vector2 b = line[(i + 1) % line.Length];
            Vector2 ab = b - a;
            float t = ab.sqrMagnitude > 0f ? Mathf.Clamp01(Vector2.Dot(p - a, ab) / ab.sqrMagnitude) : 0f;
            Vector2 q = a + ab * t;
            float d = (q - p).sqrMagnitude;
            if (d < bestDistance)
            {
                bestDistance = d;
                best = q;
            }
        }
        return best;
    }
}
