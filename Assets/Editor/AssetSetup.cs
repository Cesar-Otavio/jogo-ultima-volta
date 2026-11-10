using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

// Prepara os assets do projeto: importação dos sprites, texturas geradas
// pelo próprio jogo, materiais, animações com estados e os dados das
// pistas, carros e oficina. Pode ser rodado de novo sem problema.
// Menu: Última Volta > 1. Preparar assets
public static class AssetSetup
{
    public const string SpritesFolder = "Assets/Sprites";
    public const string GeneratedFolder = "Assets/Sprites/Gerados";
    public const string MaterialsFolder = "Assets/Materiais";
    public const string AnimationsFolder = "Assets/Animacoes";
    public const string DataFolder = "Assets/Dados";
    public const string CatalogPath = "Assets/Dados/Catalogo.asset";

    public const int MinimapLayer = 8;
    public const int GarageLayer = 9;

    [MenuItem("Última Volta/1. Preparar assets")]
    public static void PrepareAll()
    {
        CreateFolders();
        ConfigureImports();
        GenerateTextures();
        ConfigureLayers();
        CreateMaterials();
        CreateAnimators();
        CreateData();
        CreateAtlases();
        ConfigureMusic();
        AssetDatabase.SaveAssets();
        Debug.Log("Assets preparados.");
    }

    // Músicas tocam direto do disco (Streaming) e com compressão maior,
    // para o executável ficar menor.
    static void ConfigureMusic()
    {
        foreach (string guid in AssetDatabase.FindAssets("t:AudioClip", new[] { "Assets/Audio/Musica" }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            AudioImporter importer = (AudioImporter)AssetImporter.GetAtPath(path);
            AudioImporterSampleSettings settings = importer.defaultSampleSettings;
            if (settings.loadType == AudioClipLoadType.Streaming && Mathf.Approximately(settings.quality, 0.5f))
                continue;
            settings.loadType = AudioClipLoadType.Streaming;
            settings.compressionFormat = AudioCompressionFormat.Vorbis;
            settings.quality = 0.5f;
            importer.defaultSampleSettings = settings;
            importer.SaveAndReimport();
        }
    }

    // ---------- Atlas ----------

    // Muitos PNGs da CraftPix têm medidas que não são múltiplas de 4, e a
    // compressão da placa de vídeo não aceita isso: eles iriam sem compressão
    // para o executável. Juntar os sprites em atlas resolve e diminui o build.
    static void CreateAtlases()
    {
        string folder = SpritesFolder + "/Atlas";
        Directory.CreateDirectory(folder);
        Atlas(folder + "/Mundo.spriteatlasv2", true, "Carro", "Decor", "Efeitos", "Obstaculos");
        Atlas(folder + "/Interface.spriteatlasv2", false, "UI");
    }

    static void Atlas(string path, bool mipmaps, params string[] folders)
    {
        // Recria sempre, para a lista de pastas ficar igual à deste código.
        AssetDatabase.DeleteAsset(path);
        {
            UnityEditor.U2D.SpriteAtlasAsset atlas = new UnityEditor.U2D.SpriteAtlasAsset();
            Object[] packables = new Object[folders.Length];
            for (int i = 0; i < folders.Length; i++)
                packables[i] = AssetDatabase.LoadAssetAtPath<DefaultAsset>(SpritesFolder + "/" + folders[i]);
            atlas.Add(packables);
            UnityEditor.U2D.SpriteAtlasAsset.Save(atlas, path);
            AssetDatabase.ImportAsset(path);
        }

        UnityEditor.U2D.SpriteAtlasImporter importer = (UnityEditor.U2D.SpriteAtlasImporter)AssetImporter.GetAtPath(path);
        importer.packingSettings = new UnityEditor.U2D.SpriteAtlasPackingSettings
        {
            enableRotation = false,
            enableTightPacking = false,
            enableAlphaDilation = true,
            padding = 8,
            blockOffset = 1,
        };
        importer.textureSettings = new UnityEditor.U2D.SpriteAtlasTextureSettings
        {
            readable = false,
            generateMipMaps = mipmaps,
            sRGB = true,
            filterMode = FilterMode.Bilinear,
            anisoLevel = 1,
        };
        TextureImporterPlatformSettings platform = importer.GetPlatformSettings("DefaultTexturePlatform");
        platform.maxTextureSize = 4096;
        platform.textureCompression = TextureImporterCompression.Compressed;
        platform.format = TextureImporterFormat.Automatic;
        importer.SetPlatformSettings(platform);
        importer.SaveAndReimport();
    }

    static void CreateFolders()
    {
        foreach (string folder in new[] { GeneratedFolder, MaterialsFolder, AnimationsFolder,
                     DataFolder, DataFolder + "/Pistas", DataFolder + "/Carros", "Assets/Prefabs", "Assets/Scenes/Pistas" })
            Directory.CreateDirectory(folder);
        AssetDatabase.Refresh();
    }

    // ---------- Importação ----------

    static void ConfigureImports()
    {
        foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", new[] { SpritesFolder }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            bool isUI = path.Contains("/UI/");
            bool isTile = path.Contains("/Fundo/");
            ConfigureSprite(path, !isUI, isTile);
        }

        SetBorder("Assets/Sprites/UI/Faixa.png", new Vector4(75, 45, 75, 45));
        SetBorder("Assets/Sprites/UI/Janela.png", new Vector4(80, 210, 80, 170));
        SetBorder("Assets/Sprites/UI/HP_Bar.png", new Vector4(40, 20, 40, 20));

        foreach (string path in new[] { "Assets/Texturas/Asfalto.png", "Assets/Texturas/Terra.png" })
        {
            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
            if (importer == null || (importer.textureType == TextureImporterType.Default
                                     && importer.wrapModeU == TextureWrapMode.Repeat))
                continue;
            importer.textureType = TextureImporterType.Default;
            importer.wrapModeU = TextureWrapMode.Repeat;
            importer.wrapModeV = TextureWrapMode.Clamp;
            importer.mipmapEnabled = true;
            importer.alphaIsTransparency = true;
            importer.anisoLevel = 4;
            importer.SaveAndReimport();
        }
    }

    // Os PNGs entram como Multiple por padrão neste projeto: precisa ser Single.
    public static void ConfigureSprite(string path, bool mipmaps, bool fullRect)
    {
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
        if (importer == null)
            return;

        TextureImporterSettings settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        SpriteMeshType meshType = fullRect ? SpriteMeshType.FullRect : SpriteMeshType.Tight;

        // Sprites que vão para um atlas ficam sem compressão: quem é comprimido é o atlas.
        // Chão e peças de pista ficam fora do atlas: repetidos lado a lado,
        // dentro de um atlas eles mostram emendas.
        bool outsideAtlas = path.Contains("/Gerados/") || path.Contains("/Fundo/") || path.Contains("/Pista/");
        TextureImporterCompression compression = outsideAtlas
            ? TextureImporterCompression.Compressed
            : TextureImporterCompression.Uncompressed;

        bool ok = importer.textureType == TextureImporterType.Sprite
                  && importer.spriteImportMode == SpriteImportMode.Single
                  && Mathf.Approximately(importer.spritePixelsPerUnit, 100f)
                  && importer.mipmapEnabled == mipmaps
                  && importer.textureCompression == compression
                  && settings.spriteMeshType == meshType;
        if (ok)
            return;

        importer.textureCompression = compression;
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 100f;
        importer.mipmapEnabled = mipmaps;
        importer.alphaIsTransparency = true;
        importer.ReadTextureSettings(settings);
        settings.spriteMeshType = meshType;
        importer.SetTextureSettings(settings);
        importer.SaveAndReimport();
    }

    static void SetBorder(string path, Vector4 border)
    {
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
        if (importer == null || importer.spriteBorder == border)
            return;
        importer.spriteBorder = border;
        importer.SaveAndReimport();
    }

    // ---------- Texturas geradas ----------

    // Desenhos simples feitos por código: moeda, círculos e a marca de pneu.
    static void GenerateTextures()
    {
        WriteTexture("Circulo", 128, 128, (x, y) =>
        {
            float d = Vector2.Distance(new Vector2(x, y), new Vector2(63.5f, 63.5f)) / 63.5f;
            float a = Mathf.Clamp01(1f - d);
            return new Color(1f, 1f, 1f, a * a);
        });

        WriteTexture("Disco", 64, 64, (x, y) =>
        {
            float d = Vector2.Distance(new Vector2(x, y), new Vector2(31.5f, 31.5f));
            return new Color(1f, 1f, 1f, Mathf.Clamp01(30.5f - d));
        });

        WriteTexture("Pixel", 8, 8, (x, y) => Color.white);

        WriteTexture("MarcaPneu", 8, 32, (x, y) =>
        {
            float v = Mathf.Abs(y / 31f - 0.5f) * 2f;
            return new Color(1f, 1f, 1f, Mathf.Clamp01(1f - v * v));
        });

        WriteTexture("Moeda", 128, 128, (x, y) =>
        {
            Vector2 p = new Vector2(x, y) - new Vector2(63.5f, 63.5f);
            float d = p.magnitude;
            Color rim = new Color(0.78f, 0.53f, 0.08f);
            Color face = new Color(1f, 0.82f, 0.25f);
            Color line = new Color(0.88f, 0.62f, 0.12f);
            Color c = d < 50f ? face : rim;
            if (d > 36f && d < 41f)
                c = line;
            // Brilho no canto superior esquerdo.
            float shine = Mathf.Clamp01(1f - Vector2.Distance(p, new Vector2(-18f, 20f)) / 22f);
            c = Color.Lerp(c, Color.white, shine * 0.6f);
            c.a = Mathf.Clamp01(62f - d);
            return c;
        });
    }

    static void WriteTexture(string name, int width, int height, System.Func<int, int, Color> pixel)
    {
        string path = GeneratedFolder + "/" + name + ".png";
        if (File.Exists(path))
            return;

        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                texture.SetPixel(x, y, pixel(x, y));
        File.WriteAllBytes(path, texture.EncodeToPNG());
        Object.DestroyImmediate(texture);

        AssetDatabase.ImportAsset(path);
        ConfigureSprite(path, false, true);
    }

    // ---------- Camadas ----------

    // Minimapa: só a câmera do minimapa vê. Oficina: só a câmera da oficina vê.
    static void ConfigureLayers()
    {
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty layers = tagManager.FindProperty("layers");
        layers.GetArrayElementAtIndex(MinimapLayer).stringValue = "Minimapa";
        layers.GetArrayElementAtIndex(GarageLayer).stringValue = "Oficina";
        tagManager.ApplyModifiedProperties();
    }

    // ---------- Materiais ----------

    static void CreateMaterials()
    {
        Shader car = Shader.Find("UltimaVolta/Carro");
        Shader track = Shader.Find("UltimaVolta/Pista");
        Shader effect = Shader.Find("UltimaVolta/Efeito");

        MakeMaterial("Carro", car, null, -1);
        MakeMaterial("Asfalto", track, Load<Texture2D>("Assets/Texturas/Asfalto.png"), -1);
        MakeMaterial("Terra", track, Load<Texture2D>("Assets/Texturas/Terra.png"), -1);
        MakeMaterial("Fumaca", effect, Load<Texture2D>("Assets/Sprites/Efeitos/Smoke_000.png"), 10);
        MakeMaterial("Brilho", effect, Load<Texture2D>(GeneratedFolder + "/Circulo.png"), 1);
        MakeMaterial("MarcaPneu", effect, Load<Texture2D>(GeneratedFolder + "/MarcaPneu.png"), 10);
        MakeMaterial("Linha", effect, Load<Texture2D>(GeneratedFolder + "/Pixel.png"), 10);
    }

    static void MakeMaterial(string name, Shader shader, Texture texture, float dstBlend)
    {
        string path = MaterialsFolder + "/" + name + ".mat";
        Material material = Load<Material>(path);
        if (material == null)
        {
            material = new Material(shader);
            AssetDatabase.CreateAsset(material, path);
        }
        material.shader = shader;
        if (texture != null)
            material.SetTexture("_MainTex", texture);
        if (dstBlend >= 0f)
            material.SetFloat("_DstBlend", dstBlend);
        EditorUtility.SetDirty(material);
    }

    // ---------- Animações com estados ----------

    static void CreateAnimators()
    {
        Sprite[] stop = Frames("Stop_Signals_", 5);
        Sprite[] nitroOn = Frames("Nitro_ON_", 7);
        Sprite[] nitro = Frames("Nitro_", 10);
        Sprite[] nitroOff = Frames("Nitro_OFF_", 9);
        Sprite[] blue = Frames("Police_Beacons_Blue_", 6);
        Sprite[] red = Frames("Police_Beacons_Red_", 6);

        // Luz de freio: Apagada -> Acendendo -> Acesa -> Apagada.
        AnimatorController brake = NewController("LuzFreio", "Freando");
        AnimatorStateMachine brakeMachine = brake.layers[0].stateMachine;
        AnimatorState off = AddState(brakeMachine, "Apagada", Clip("LuzApagada", new[] { stop[0] }, 1, false, 0f));
        AnimatorState turningOn = AddState(brakeMachine, "Acendendo", Clip("LuzAcendendo", stop, 30, false, 1f));
        AnimatorState on = AddState(brakeMachine, "Acesa", Clip("LuzAcesa", new[] { stop[4] }, 1, false, 1f));
        brakeMachine.defaultState = off;
        Connect(off, turningOn, "Freando", true);
        ConnectAtEnd(turningOn, on);
        Connect(turningOn, off, "Freando", false);
        Connect(on, off, "Freando", false);

        // Nitro: Desligado -> Ligando -> Ativo -> Desligando -> Desligado.
        AnimatorController nitroController = NewController("Nitro", "Ativo");
        AnimatorStateMachine nitroMachine = nitroController.layers[0].stateMachine;
        AnimatorState nOff = AddState(nitroMachine, "Desligado", Clip("NitroDesligado", new[] { nitroOn[0] }, 1, false, 0f));
        AnimatorState nStart = AddState(nitroMachine, "Ligando", Clip("NitroLigando", nitroOn, 30, false, 1f));
        AnimatorState nOn = AddState(nitroMachine, "Ativo", Clip("NitroAtivo", nitro, 24, true, 1f));
        AnimatorState nStop = AddState(nitroMachine, "Desligando", Clip("NitroDesligando", nitroOff, 30, false, 1f));
        nitroMachine.defaultState = nOff;
        Connect(nOff, nStart, "Ativo", true);
        ConnectAtEnd(nStart, nOn);
        Connect(nStart, nStop, "Ativo", false);
        Connect(nOn, nStop, "Ativo", false);
        ConnectAtEnd(nStop, nOff);
        Connect(nStop, nStart, "Ativo", true);

        // Sirene da viatura: Desligada <-> Ligada (vermelho e azul alternando).
        AnimatorController siren = NewController("Sirene", "Ligada");
        AnimatorStateMachine sirenMachine = siren.layers[0].stateMachine;
        Sprite[] alternating = new Sprite[12];
        for (int i = 0; i < 6; i++)
        {
            alternating[i] = red[i];
            alternating[i + 6] = blue[i];
        }
        AnimatorState sOff = AddState(sirenMachine, "Desligada", Clip("SireneDesligada", new[] { red[0] }, 1, false, 0f));
        AnimatorState sOn = AddState(sirenMachine, "Ligada", Clip("SireneLigada", alternating, 16, true, 1f));
        sirenMachine.defaultState = sOff;
        Connect(sOff, sOn, "Ligada", true);
        Connect(sOn, sOff, "Ligada", false);

        // Carro: Parado (motor ligado, tremendo) <-> Andando <-> Derrapando (traseira balançando).
        AnimatorController body = NewController("Carro", "Derrapando");
        body.AddParameter("Velocidade", AnimatorControllerParameterType.Float);
        AnimatorStateMachine bodyMachine = body.layers[0].stateMachine;
        AnimatorState idle = AddState(bodyMachine, "Parado", MotionClip("CarroParado",
            new[] { 0f, 0.015f, -0.015f, 0.01f, 0f }, new[] { 0f, 0.02f, -0.01f, -0.02f, 0f }, 20));
        AnimatorState moving = AddState(bodyMachine, "Andando", MotionClip("CarroAndando",
            new[] { 0f, 0f }, new[] { 0f, 0f }, 10));
        AnimatorState drifting = AddState(bodyMachine, "Derrapando", MotionClip("CarroDerrapando",
            new[] { 0f, 0.05f, 0f, -0.05f, 0f }, new[] { 0f, 0f, 0f, 0f, 0f }, 12));
        bodyMachine.defaultState = idle;
        ConnectSpeed(idle, moving, AnimatorConditionMode.Greater, 0.5f);
        ConnectSpeed(moving, idle, AnimatorConditionMode.Less, 0.3f);
        ConnectSpeed(drifting, idle, AnimatorConditionMode.Less, 0.3f);
        Connect(moving, drifting, "Derrapando", true);
        Connect(drifting, moving, "Derrapando", false);

        AssetDatabase.SaveAssets();
    }

    static Sprite[] Frames(string prefix, int count)
    {
        Sprite[] frames = new Sprite[count];
        for (int i = 0; i < count; i++)
            frames[i] = Load<Sprite>("Assets/Sprites/Efeitos/" + prefix + i.ToString("000") + ".png");
        return frames;
    }

    // Cria (ou recria) um clipe que troca o sprite e define a transparência.
    static AnimationClip Clip(string name, Sprite[] frames, float fps, bool loop, float alpha)
    {
        string path = AnimationsFolder + "/" + name + ".anim";
        AnimationClip clip = Load<AnimationClip>(path);
        if (clip == null)
        {
            clip = new AnimationClip();
            AssetDatabase.CreateAsset(clip, path);
        }
        clip.ClearCurves();
        clip.frameRate = fps;

        EditorCurveBinding spriteBinding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
        ObjectReferenceKeyframe[] keys = new ObjectReferenceKeyframe[frames.Length];
        for (int i = 0; i < frames.Length; i++)
            keys[i] = new ObjectReferenceKeyframe { time = i / fps, value = frames[i] };
        AnimationUtility.SetObjectReferenceCurve(clip, spriteBinding, keys);

        float length = Mathf.Max(frames.Length / fps, 1f / fps);
        EditorCurveBinding alphaBinding = EditorCurveBinding.FloatCurve("", typeof(SpriteRenderer), "m_Color.a");
        AnimationUtility.SetEditorCurve(clip, alphaBinding, AnimationCurve.Constant(0f, length, alpha));

        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = loop;
        AnimationUtility.SetAnimationClipSettings(clip, settings);
        EditorUtility.SetDirty(clip);
        return clip;
    }

    // Clipe que mexe a posição do objeto (tremor do motor, balanço do drift), em repetição.
    static AnimationClip MotionClip(string name, float[] xs, float[] ys, float fps)
    {
        string path = AnimationsFolder + "/" + name + ".anim";
        AnimationClip clip = Load<AnimationClip>(path);
        if (clip == null)
        {
            clip = new AnimationClip();
            AssetDatabase.CreateAsset(clip, path);
        }
        clip.ClearCurves();
        clip.frameRate = fps;

        AnimationCurve x = new AnimationCurve(), y = new AnimationCurve();
        for (int i = 0; i < xs.Length; i++)
        {
            x.AddKey(i / fps, xs[i]);
            y.AddKey(i / fps, ys[i]);
        }
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve("", typeof(Transform), "m_LocalPosition.x"), x);
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve("", typeof(Transform), "m_LocalPosition.y"), y);

        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = true;
        AnimationUtility.SetAnimationClipSettings(clip, settings);
        EditorUtility.SetDirty(clip);
        return clip;
    }

    static void ConnectSpeed(AnimatorState from, AnimatorState to, AnimatorConditionMode mode, float threshold)
    {
        AnimatorStateTransition transition = from.AddTransition(to);
        transition.hasExitTime = false;
        transition.duration = 0.1f;
        transition.AddCondition(mode, threshold, "Velocidade");
    }

    static AnimatorController NewController(string name, string boolParameter)
    {
        string path = AnimationsFolder + "/" + name + ".controller";
        AssetDatabase.DeleteAsset(path);
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(path);
        controller.AddParameter(boolParameter, AnimatorControllerParameterType.Bool);
        return controller;
    }

    static AnimatorState AddState(AnimatorStateMachine machine, string name, Motion motion)
    {
        AnimatorState state = machine.AddState(name);
        state.motion = motion;
        return state;
    }

    static void Connect(AnimatorState from, AnimatorState to, string parameter, bool value)
    {
        AnimatorStateTransition transition = from.AddTransition(to);
        transition.hasExitTime = false;
        transition.duration = 0f;
        transition.AddCondition(value ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0f, parameter);
    }

    static void ConnectAtEnd(AnimatorState from, AnimatorState to)
    {
        AnimatorStateTransition transition = from.AddTransition(to);
        transition.hasExitTime = true;
        transition.exitTime = 1f;
        transition.duration = 0f;
    }

    // ---------- Dados ----------

    static void CreateData()
    {
        CarModel faisca = Car("faisca", "Faísca", "Equilibrado: bom em tudo. O carro do começo do jogo.",
            0, "Car_1", 0.31f, 5f, false,
            new CarStats { maxSpeed = 10f, acceleration = 8.5f, brakeForce = 14f, turnSpeed = 175f, grip = 10f, nitroCapacity = 2.5f, hearts = 4, mass = 1f });
        CarModel relampago = Car("relampago", "Relâmpago", "Leve: acelera e vira mais rápido, mas aguenta só 3 batidas.",
            450, "Car_3", 0.31f, 50f, false,
            new CarStats { maxSpeed = 10.4f, acceleration = 10f, brakeForce = 15f, turnSpeed = 190f, grip = 11f, nitroCapacity = 2.5f, hearts = 3, mass = 0.85f });
        CarModel blindado = Car("blindado", "Blindado", "Pesado: empurra os rivais e aguenta 6 batidas, mas acelera devagar.",
            700, "Car_2", 0.26f, 0f, true,
            new CarStats { maxSpeed = 9.8f, acceleration = 7.5f, brakeForce = 13f, turnSpeed = 160f, grip = 10f, nitroCapacity = 3f, hearts = 6, mass = 1.6f });

        GameCatalog catalog = Load<GameCatalog>(CatalogPath);
        if (catalog == null)
        {
            catalog = ScriptableObject.CreateInstance<GameCatalog>();
            AssetDatabase.CreateAsset(catalog, CatalogPath);
        }

        catalog.cars = new[] { faisca, relampago, blindado };
        catalog.colors = new[]
        {
            new ColorOption { id = "original", displayName = "Original", original = true },
            new ColorOption { id = "azul", displayName = "Azul", color = new Color(0.15f, 0.45f, 1f), price = 80 },
            new ColorOption { id = "verde", displayName = "Verde", color = new Color(0.2f, 0.85f, 0.3f), price = 80 },
            new ColorOption { id = "amarelo", displayName = "Amarelo", color = new Color(1f, 0.85f, 0.1f), price = 80 },
            new ColorOption { id = "roxo", displayName = "Roxo", color = new Color(0.6f, 0.25f, 0.95f), price = 120 },
            new ColorOption { id = "rosa", displayName = "Rosa", color = new Color(1f, 0.35f, 0.7f), price = 120 },
            new ColorOption { id = "preto", displayName = "Preto", color = new Color(0.08f, 0.08f, 0.08f), price = 150 },
            new ColorOption { id = "branco", displayName = "Branco", color = Color.white, price = 150 },
            new ColorOption { id = "dourado", displayName = "Dourado", color = new Color(1f, 0.72f, 0.15f), price = 300, challengesRequired = 6 },
        };
        catalog.engines = new[]
        {
            new EngineOption { id = "esportivo", displayName = "Esportivo", description = "O som original do motor.", minPitch = 0.8f, maxPitch = 1.8f, lowPassCutoff = 22000f },
            new EngineOption { id = "v8", displayName = "V8 grave", description = "Ronco mais grave e encorpado.", minPitch = 0.55f, maxPitch = 1.3f, lowPassCutoff = 3000f, price = 150 },
            new EngineOption { id = "turbo", displayName = "Turbo agudo", description = "Giro alto, como um carro de corrida.", minPitch = 1f, maxPitch = 2.3f, lowPassCutoff = 22000f, price = 250, challengesRequired = 3 },
        };
        catalog.exhausts = new[]
        {
            new ExhaustOption { id = "padrao", displayName = "Padrão", description = "Estoura de vez em quando.", popChance = 0.15f, flameColor = new Color(1f, 0.6f, 0.2f) },
            new ExhaustOption { id = "esportivo", displayName = "Esportivo", description = "Mais estouros e um pouco mais de aceleração.", popChance = 0.5f, flameColor = new Color(1f, 0.45f, 0.15f), accelerationBonus = 0.3f, price = 200 },
            new ExhaustOption { id = "turbo", displayName = "Turbo azul", description = "Chama azul e a maior aceleração extra.", popChance = 0.7f, flameColor = new Color(0.4f, 0.7f, 1f), accelerationBonus = 0.6f, price = 400, challengesRequired = 6 },
        };
        catalog.upgrades = new[]
        {
            new UpgradeInfo { type = UpgradeType.Motor, displayName = "Motor", description = "+0,6 de velocidade máxima e +0,5 de aceleração por nível.", prices = new[] { 150, 300, 500 } },
            new UpgradeInfo { type = UpgradeType.Pneus, displayName = "Pneus", description = "Mais aderência e curvas 6% mais rápidas por nível.", prices = new[] { 120, 250, 400 } },
            new UpgradeInfo { type = UpgradeType.Freios, displayName = "Freios", description = "Freia mais forte a cada nível.", prices = new[] { 100, 200, 350 } },
            new UpgradeInfo { type = UpgradeType.Blindagem, displayName = "Blindagem", description = "+1 coração por nível.", prices = new[] { 200, 400, 650 } },
            new UpgradeInfo { type = UpgradeType.Nitro, displayName = "Nitro", description = "Tanque de nitro maior a cada nível.", prices = new[] { 150, 300, 450 } },
        };

        catalog.tracks = new[]
        {
            Track("treino", "Treino", "Autódromo", "O circuito original: 5 voltas contra o relógio, sem adversários. Bom para aprender o drift e o nitro.",
                "Treino", 5, 180f, 0, 0f, 1f, false, new Color(0.55f, 0.8f, 0.4f), "pure_raceway_bpm160", "pure_raceway_climax_bpm175",
                new[] { Challenge(ChallengeType.TempoSobrando, 30, 100), Challenge(ChallengeType.SemDanos, 0, 100), Challenge(ChallengeType.TempoDeDrift, 5, 75) }),
            Track("rio", "Rio de Janeiro", "Brasil", "Corrida na orla, entre prédios e o mar. Asfalto com muros: cuidado nas curvas.",
                "Rio", 3, 240f, 3, 0.25f, 1f, false, new Color(1f, 0.75f, 0.3f), "sunny_city_bpm120", "sunny_city_climax_bpm135",
                new[] { Challenge(ChallengeType.TerminarEmPrimeiro, 0, 120), Challenge(ChallengeType.ColetarMoedas, 20, 100), Challenge(ChallengeType.MaximoDeDanos, 1, 100) }),
            Track("amazonia", "Amazônia", "Brasil", "Estrada de terra no meio da floresta, com um rio no caminho. Use a rampa para pular!",
                "Amazonia", 3, 225f, 3, 0.45f, 1.2f, false, new Color(0.35f, 0.85f, 0.45f), "joyful_jungle_bpm140", "joyful_jungle_climax_bpm155",
                new[] { Challenge(ChallengeType.TerminarNoPodio, 0, 100), Challenge(ChallengeType.TempoDeDrift, 8, 120), Challenge(ChallengeType.SemDanos, 0, 150) }),
            Track("nevada", "Nevada", "EUA", "Deserto com retas longas, rochas, óleo e uma viatura de polícia que não gosta de apressados.",
                "Nevada", 3, 255f, 3, 0.55f, 1.4f, false, new Color(1f, 0.55f, 0.3f), "hot_roadway_remake_bpm160", "hot_roadway_climax_remake_bpm175",
                new[] { Challenge(ChallengeType.TerminarEmPrimeiro, 0, 150), Challenge(ChallengeType.UsarNitro, 4, 120), Challenge(ChallengeType.ColetarMoedas, 25, 120) }),
            Track("toquio", "Tóquio", "Japão", "Corrida noturna entre luzes de neon, contra os rivais mais rápidos do jogo.",
                "Toquio", 3, 240f, 3, 0.75f, 1.6f, true, new Color(0.9f, 0.4f, 1f), "neon_sign_circuit_remake_bpm145", "neon_sign_circuit_climax_remake_bpm160",
                new[] { Challenge(ChallengeType.TerminarEmPrimeiro, 0, 200), Challenge(ChallengeType.MaximoDeDanos, 1, 150), Challenge(ChallengeType.TempoSobrando, 30, 150) }),
        };
        catalog.tracksUnlockedAtStart = 2;
        EditorUtility.SetDirty(catalog);
    }

    static CarModel Car(string id, string name, string description, int price, string spritePrefix,
        float scale, float baseHue, bool whiteBody, CarStats stats)
    {
        string path = DataFolder + "/Carros/" + id + ".asset";
        CarModel car = Load<CarModel>(path);
        if (car == null)
        {
            car = ScriptableObject.CreateInstance<CarModel>();
            AssetDatabase.CreateAsset(car, path);
        }
        car.id = id;
        car.displayName = name;
        car.description = description;
        car.price = price;
        car.scale = scale;
        car.baseHue = baseHue;
        car.whiteBody = whiteBody;
        car.stats = stats;
        car.damageSprites = new Sprite[5];
        for (int i = 0; i < 5; i++)
            car.damageSprites[i] = Load<Sprite>("Assets/Sprites/Carro/" + spritePrefix + "_0" + (i + 1) + ".png");
        EditorUtility.SetDirty(car);
        return car;
    }

    static TrackInfo Track(string id, string name, string country, string description, string scene,
        int laps, float timeLimit, int bots, float skill, float multiplier, bool night, Color theme,
        string music, string lastLapMusic, ChallengeInfo[] challenges)
    {
        string path = DataFolder + "/Pistas/" + id + ".asset";
        TrackInfo track = Load<TrackInfo>(path);
        if (track == null)
        {
            track = ScriptableObject.CreateInstance<TrackInfo>();
            AssetDatabase.CreateAsset(track, path);
        }
        track.id = id;
        track.displayName = name;
        track.country = country;
        track.description = description;
        track.sceneName = scene;
        track.laps = laps;
        track.timeLimit = timeLimit;
        track.botCount = bots;
        track.botSkill = skill;
        track.rewardMultiplier = multiplier;
        track.isNight = night;
        track.themeColor = theme;
        track.music = Load<AudioClip>("Assets/Audio/Musica/" + music + ".ogg");
        track.lastLapMusic = Load<AudioClip>("Assets/Audio/Musica/" + lastLapMusic + ".ogg");
        track.challenges = challenges;
        EditorUtility.SetDirty(track);
        return track;
    }

    static ChallengeInfo Challenge(ChallengeType type, float target, int reward)
    {
        return new ChallengeInfo { type = type, target = target, reward = reward };
    }

    public static T Load<T>(string path) where T : Object
    {
        return AssetDatabase.LoadAssetAtPath<T>(path);
    }
}
