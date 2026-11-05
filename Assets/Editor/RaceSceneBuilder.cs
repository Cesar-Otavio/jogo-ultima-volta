using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using UImage = UnityEngine.UI.Image;

// Coloca numa cena de pista tudo o que é igual em todas as corridas:
// luz, gerenciador da corrida, carros no grid, câmera, minimapa, áudio,
// fogos de artifício e as telas (HUD, contagem, pausa e resultado).
public static class RaceSceneBuilder
{
    public class Settings
    {
        public TrackInfo track;
        public TrackPath path;
        public SpriteRenderer ground;
        public Color background = new Color(0.1f, 0.2f, 0.12f);
        public float cameraSize = 8.5f;
        [Tooltip("Nomes e cores dos bots, na ordem do grid.")]
        public string[] botNames = new string[0];
        public string[] botCars = new string[0];
        public string[] botColors = new string[0];
        public Color[] botMapColors = new Color[0];
        public float gridSpacing = 5.5f;
        public float gridOffset = 6f;
        public float laneWidth = 1.7f;
    }

    public class Result
    {
        public RaceManager raceManager;
        public AudioManager audioManager;
    }

    public static Result Build(Settings s)
    {
        GameCatalog catalog = AssetSetup.Load<GameCatalog>(AssetSetup.CatalogPath);
        Result result = new Result();

        // Luz global: dia claro ou noite azulada.
        GameObject lightObject = new GameObject("LuzGlobal");
        Light2D globalLight = lightObject.AddComponent<Light2D>();
        globalLight.lightType = Light2D.LightType.Global;
        globalLight.intensity = s.track.isNight ? 0.28f : 1f;
        globalLight.color = s.track.isNight ? new Color(0.55f, 0.6f, 1f) : Color.white;

        // Gerenciador da corrida e cronômetro
        GameObject raceObject = new GameObject("Corrida");
        RaceManager race = raceObject.AddComponent<RaceManager>();
        RaceTimer timer = raceObject.AddComponent<RaceTimer>();
        race.catalog = catalog;
        race.track = s.track;
        race.path = s.path;
        race.timer = timer;
        timer.raceManager = race;
        result.raceManager = race;

        // Áudio geral
        GameObject audioObject = new GameObject("Audio");
        AudioManager audio = audioObject.AddComponent<AudioManager>();
        audio.raceManager = race;
        audio.musicSource = AudioChild(audioObject.transform, "Musica", true, 0.55f);
        audio.effectsSource = AudioChild(audioObject.transform, "Efeitos", false, 1f);
        audio.collisionClips = new[]
        {
            Clip("Efeitos/impactMetal_heavy_000.ogg"), Clip("Efeitos/impactMetal_heavy_001.ogg"), Clip("Efeitos/impactMetal_heavy_002.ogg")
        };
        audio.winClip = Clip("Efeitos/vitoria_race_finish_best.ogg");
        audio.loseClip = Clip("Efeitos/derrota_race_finish_bad.ogg");
        audio.countdownClip = Clip("Efeitos/largada_countdown.ogg");
        audio.coinClip = Clip("Efeitos/moeda_Coin01.aif");
        audio.repairClip = Clip("Efeitos/desbloqueio_Upper01.aif");
        result.audioManager = audio;

        // Câmera principal
        GameObject cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        Camera cam = cameraObject.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = s.cameraSize;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = s.background;
        cam.cullingMask = ~((1 << AssetSetup.MinimapLayer) | (1 << AssetSetup.GarageLayer));
        cameraObject.transform.position = new Vector3(0f, 0f, -10f);
        cameraObject.AddComponent<AudioListener>();
        cameraObject.AddComponent<UniversalAdditionalCameraData>();
        CameraFollow follow = cameraObject.AddComponent<CameraFollow>();
        follow.mapGround = s.ground;
        follow.baseSize = s.cameraSize;

        // Carros no grid, atrás da linha de chegada. O jogador larga por último.
        int carCount = 1 + s.track.botCount;
        RaceCar playerCar = null;
        for (int slot = 0; slot < carCount; slot++)
        {
            bool isPlayer = slot == carCount - 1;
            int row = slot / 2;
            float side = carCount == 1 ? 0f : (slot % 2 == 0 ? 1f : -1f) * s.laneWidth;
            float distance = s.path.Length - s.gridOffset - row * s.gridSpacing;
            Vector2 direction = s.path.DirectionAt(distance);
            Vector2 left = new Vector2(-direction.y, direction.x);
            Vector2 position = s.path.PositionAt(distance) + left * side;
            float angle = Vector2.SignedAngle(Vector2.up, direction);

            GameObject prefab = AssetSetup.Load<GameObject>(isPlayer ? CarPrefabBuilder.PlayerPrefabPath : CarPrefabBuilder.BotPrefabPath);
            GameObject car = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            car.transform.SetPositionAndRotation(position, Quaternion.Euler(0f, 0f, angle));

            RaceCar raceCar = car.GetComponent<RaceCar>();
            raceCar.raceManager = race;
            raceCar.track = s.path;
            car.GetComponent<CarController>().raceManager = race;

            if (isPlayer)
            {
                car.name = "CarroJogador";
                raceCar.driverName = "Você";
                raceCar.mapColor = new Color(1f, 0.9f, 0.2f);
                playerCar = raceCar;

                CarDamage damage = car.GetComponent<CarDamage>();
                damage.raceManager = race;
                damage.audioManager = audio;
                damage.cameraFollow = follow;

                race.player = raceCar;
                race.playerCar = car.GetComponent<CarController>();
                race.playerDamage = damage;
                follow.target = car.transform;
                follow.targetBody = car.GetComponent<Rigidbody2D>();
            }
            else
            {
                car.name = "Bot_" + s.botNames[slot];
                raceCar.driverName = s.botNames[slot];
                raceCar.mapColor = s.botMapColors[slot];
                car.transform.Find("IconeMinimapa").GetComponent<SpriteRenderer>().color = s.botMapColors[slot];

                CarSetup setup = car.GetComponent<CarSetup>();
                setup.carId = s.botCars[slot];
                setup.colorId = s.botColors[slot];
                setup.botSkill = s.track.botSkill;

                BotDriver bot = car.GetComponent<BotDriver>();
                bot.track = s.path;
                bot.skill = s.track.botSkill;
                bot.laneOffset = side * 0.6f;
            }
        }

        foreach (BotDriver bot in Object.FindObjectsByType<BotDriver>())
            bot.player = playerCar;

        BuildMinimap(s);
        BuildFireworks(race, cam);
        BuildUI(race, timer);
        UIBuilder.EventSystem();
        return result;
    }

    static AudioSource AudioChild(Transform parent, string name, bool loop, float volume)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        AudioSource source = go.AddComponent<AudioSource>();
        source.loop = loop;
        source.volume = volume;
        source.playOnAwake = false;
        return source;
    }

    static AudioClip Clip(string path) => AssetSetup.Load<AudioClip>("Assets/Audio/" + path);

    // ---------- Minimapa ----------

    // Uma segunda câmera vê só a camada "Minimapa" (linha da pista e
    // ícones dos carros) e desenha numa textura mostrada no HUD.
    static void BuildMinimap(Settings s)
    {
        RenderTexture texture = MinimapTexture();
        Bounds map = s.ground.bounds;

        GameObject cameraObject = new GameObject("CameraMinimapa");
        Camera cam = cameraObject.AddComponent<Camera>();
        cam.orthographic = true;
        float aspect = (float)texture.width / texture.height;
        cam.orthographicSize = Mathf.Max(map.size.y / 2f, map.size.x / (2f * aspect)) * 1.02f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.04f, 0.08f, 0.05f, 0.85f);
        cam.cullingMask = 1 << AssetSetup.MinimapLayer;
        cam.targetTexture = texture;
        cam.depth = -2;
        cameraObject.transform.position = new Vector3(map.center.x, map.center.y, -10f);
        cameraObject.AddComponent<UniversalAdditionalCameraData>();

        GameObject lineObject = new GameObject("LinhaMinimapa");
        lineObject.layer = AssetSetup.MinimapLayer;
        LineRenderer line = lineObject.AddComponent<LineRenderer>();
        line.sharedMaterial = AssetSetup.Load<Material>(AssetSetup.MaterialsFolder + "/Linha.mat");
        line.useWorldSpace = true;
        line.loop = true;
        line.widthMultiplier = 3.5f;
        line.startColor = line.endColor = new Color(0.95f, 0.95f, 0.95f, 0.85f);
        line.sortingOrder = 40;
        Vector3[] points = new Vector3[s.path.points.Length];
        for (int i = 0; i < points.Length; i++)
            points[i] = s.path.points[i];
        line.positionCount = points.Length;
        line.SetPositions(points);
    }

    public static RenderTexture MinimapTexture()
    {
        const string path = "Assets/Texturas/Minimapa.renderTexture";
        RenderTexture texture = AssetSetup.Load<RenderTexture>(path);
        if (texture == null)
        {
            // A câmera do URP exige uma textura com profundidade.
            texture = new RenderTexture(512, 384, 24, RenderTextureFormat.ARGB32);
            AssetDatabase.CreateAsset(texture, path);
        }
        else if (texture.depth == 0)
        {
            texture.Release();
            texture.depth = 24;
            EditorUtility.SetDirty(texture);
        }
        return texture;
    }

    // ---------- Fogos de artifício ----------

    static void BuildFireworks(RaceManager race, Camera cam)
    {
        GameObject go = new GameObject("FogosDeArtificio");
        Fireworks fireworks = go.AddComponent<Fireworks>();
        fireworks.raceManager = race;
        fireworks.cam = cam;

        ParticleSystem burst = CarPrefabBuilder.Particles(go.transform, "Explosao",
            AssetSetup.Load<Material>(AssetSetup.MaterialsFolder + "/Brilho.mat"), Color.white,
            new Vector2(0.9f, 1.5f), new Vector2(0.25f, 0.5f), new Vector2(4f, 10f), 30, 0.2f);
        ParticleSystem.LimitVelocityOverLifetimeModule drag = burst.limitVelocityOverLifetime;
        drag.enabled = true;
        drag.limit = 1.5f;
        drag.dampen = 0.08f;
        ParticleSystem.SizeOverLifetimeModule size = burst.sizeOverLifetime;
        size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0.3f));
        fireworks.burst = burst;

        fireworks.audioSource = go.AddComponent<AudioSource>();
        fireworks.audioSource.playOnAwake = false;
        fireworks.clips = new[]
        {
            Clip("Efeitos/fogos_fw_01.ogg"), Clip("Efeitos/fogos_fw_02.ogg"), Clip("Efeitos/fogos_fw_03.ogg"), Clip("Efeitos/fogos_fw_04.ogg")
        };
    }

    // ---------- Telas ----------

    static readonly Vector2 TopLeft = new Vector2(0f, 1f), TopRight = new Vector2(1f, 1f), TopCenter = new Vector2(0.5f, 1f),
        BottomLeft = new Vector2(0f, 0f), BottomRight = new Vector2(1f, 0f), Center = new Vector2(0.5f, 0.5f);

    static void BuildUI(RaceManager race, RaceTimer timer)
    {
        Canvas canvas = UIBuilder.Canvas("Telas", 10);
        RaceScreens screens = canvas.gameObject.AddComponent<RaceScreens>();
        screens.raceManager = race;
        screens.raceTimer = timer;
        Transform root = canvas.transform;

        // HUD
        RectTransform hud = UIBuilder.Fill(root, "HUD");
        HudController hudController = hud.gameObject.AddComponent<HudController>();
        hudController.raceManager = race;
        hudController.raceTimer = timer;
        hudController.carDamage = race.playerDamage;
        hudController.car = race.playerCar;
        screens.hud = hud.gameObject;

        UImage timePanel = UIBuilder.Image(hud, "PainelTempo", UIBuilder.UISprite("Faixa"), Color.white, TopLeft, new Vector2(30f, -30f), new Vector2(540f, 180f), true);
        hudController.timeText = UIBuilder.Text(timePanel.transform, "Tempo", "03:00", 72f, TopLeft, new Vector2(40f, -14f), new Vector2(260f, 90f), TextAlignmentOptions.Left);
        hudController.timeText.fontStyle = FontStyles.Bold;
        hudController.lapText = UIBuilder.Text(timePanel.transform, "Volta", "Volta 1/3", 38f, TopLeft, new Vector2(42f, -104f), new Vector2(280f, 54f), TextAlignmentOptions.Left);
        hudController.positionText = UIBuilder.Text(timePanel.transform, "Posicao", "1º/4", 84f, new Vector2(1f, 0.5f), new Vector2(-36f, 0f), new Vector2(220f, 140f), TextAlignmentOptions.Right);
        hudController.positionText.fontStyle = FontStyles.Bold;
        hudController.positionText.color = UIBuilder.Gold;

        RectTransform heartsRow = UIBuilder.Rect(hud, "Coracoes", TopRight, new Vector2(-30f, -40f), new Vector2(8 * 76f, 64f));
        hudController.hearts = new UImage[8];
        for (int i = 0; i < 8; i++)
        {
            UImage slot = UIBuilder.Image(heartsRow, "Coracao" + (i + 1), UIBuilder.UISprite("HP_Dot_BG"), Color.white,
                new Vector2(1f, 0.5f), new Vector2(-i * 76f, 0f), new Vector2(70f, 56f));
            hudController.hearts[i] = UIBuilder.Image(slot.transform, "Cheio", UIBuilder.UISprite("HP_Dot"), Color.white,
                Center, Vector2.zero, new Vector2(70f, 56f));
        }
        TMP_Text pauseHint = UIBuilder.Text(hud, "DicaPausa", "Esc: pausar", 28f, TopRight, new Vector2(-34f, -112f), new Vector2(400f, 44f), TextAlignmentOptions.Right);
        pauseHint.color = new Color(1f, 1f, 1f, 0.7f);

        UIBuilder.Image(hud, "IconeMoeda", UIBuilder.Generated("Moeda"), Color.white, TopCenter, new Vector2(-50f, -36f), new Vector2(64f, 64f));
        hudController.coinsText = UIBuilder.Text(hud, "Moedas", "0", 56f, TopCenter, new Vector2(60f, -32f), new Vector2(140f, 72f), TextAlignmentOptions.Left);
        hudController.coinsText.fontStyle = FontStyles.Bold;

        UImage nitroPanel = UIBuilder.Image(hud, "PainelNitro", UIBuilder.UISprite("Faixa"), Color.white, BottomLeft, new Vector2(30f, 30f), new Vector2(540f, 170f), true);
        hudController.speedText = UIBuilder.Text(nitroPanel.transform, "Velocidade", "0 km/h", 52f, TopLeft, new Vector2(40f, -14f), new Vector2(300f, 70f), TextAlignmentOptions.Left);
        hudController.speedText.fontStyle = FontStyles.Bold;
        UIBuilder.Text(nitroPanel.transform, "RotuloNitro", "NITRO (Shift)", 26f, TopRight, new Vector2(-40f, -26f), new Vector2(200f, 40f), TextAlignmentOptions.Right);
        UIBuilder.Image(nitroPanel.transform, "FundoNitro", UIBuilder.Generated("Pixel"), new Color(0f, 0f, 0f, 0.6f), BottomLeft, new Vector2(40f, 34f), new Vector2(460f, 36f));
        UImage nitroFill = UIBuilder.Image(nitroPanel.transform, "BarraNitro", UIBuilder.Generated("Pixel"), new Color(0.2f, 0.65f, 1f), BottomLeft, new Vector2(44f, 38f), new Vector2(452f, 28f));
        nitroFill.type = UImage.Type.Filled;
        nitroFill.fillMethod = UImage.FillMethod.Horizontal;
        nitroFill.preserveAspect = false;
        hudController.nitroFill = nitroFill;

        UImage minimapFrame = UIBuilder.Image(hud, "Minimapa", UIBuilder.Generated("Pixel"), new Color(0f, 0f, 0f, 0.5f), BottomRight, new Vector2(-30f, 30f), new Vector2(400f, 300f));
        RectTransform minimapArea = UIBuilder.Fill(minimapFrame.transform, "Imagem", 6f);
        RawImage minimap = minimapArea.gameObject.AddComponent<RawImage>();
        minimap.texture = MinimapTexture();
        minimap.raycastTarget = false;

        hudController.messageText = UIBuilder.Text(hud, "Mensagem", "", 84f, Center, new Vector2(0f, 230f), new Vector2(1400f, 130f));
        hudController.messageText.fontStyle = FontStyles.Bold;
        hudController.messageText.color = UIBuilder.Gold;

        // Contagem com semáforo
        RectTransform countdown = UIBuilder.Fill(root, "Contagem");
        screens.countdownScreen = countdown.gameObject;
        UIBuilder.Image(countdown, "Semaforo", UIBuilder.UISprite("Faixa"), Color.white, Center, new Vector2(0f, 220f), new Vector2(560f, 180f), true);
        screens.countdownLights = new UImage[3];
        for (int i = 0; i < 3; i++)
            screens.countdownLights[i] = UIBuilder.Image(countdown, "Luz" + (i + 1), UIBuilder.Generated("Disco"), Color.gray,
                Center, new Vector2((i - 1) * 165f, 220f), new Vector2(120f, 120f));
        screens.countdownText = UIBuilder.Text(countdown, "Numero", "3", 200f, Center, new Vector2(0f, 20f), new Vector2(600f, 240f));
        screens.countdownText.fontStyle = FontStyles.Bold;

        // Pausa
        UImage pause = UIBuilder.Overlay(root, "TelaPausa", 0.6f);
        screens.pauseScreen = pause.gameObject;
        RectTransform pauseWindow = UIBuilder.Window(pause.transform, "Janela", "Pausado", new Vector2(900f, 820f), out _);
        UIBuilder.Button(pauseWindow, "BotaoContinuar", "Continuar", Center, new Vector2(0f, 130f), new Vector2(560f, 116f), race.TogglePause);
        UIBuilder.Button(pauseWindow, "BotaoReiniciar", "Reiniciar corrida", Center, new Vector2(0f, -10f), new Vector2(560f, 116f), race.RestartRace);
        UIBuilder.Button(pauseWindow, "BotaoMenu", "Voltar ao menu", Center, new Vector2(0f, -150f), new Vector2(560f, 116f), race.BackToMenu);

        // Resultado
        UImage resultOverlay = UIBuilder.Overlay(root, "TelaResultado", 0.65f);
        screens.resultScreen = resultOverlay.gameObject;
        RectTransform resultWindow = UIBuilder.Window(resultOverlay.transform, "Janela", "Resultado", new Vector2(1560f, 1000f), out TMP_Text resultTitle);
        screens.resultTitle = resultTitle;
        screens.resultMessage = UIBuilder.Text(resultWindow, "Mensagem", "", 36f, TopCenter, new Vector2(0f, -140f), new Vector2(1360f, 60f));
        screens.rankingText = UIBuilder.Text(resultWindow, "Classificacao", "", 32f, Center, new Vector2(-400f, 50f), new Vector2(600f, 480f), TextAlignmentOptions.TopLeft);
        screens.rewardsText = UIBuilder.Text(resultWindow, "Recompensas", "", 28f, Center, new Vector2(340f, 50f), new Vector2(720f, 480f), TextAlignmentOptions.TopLeft);
        UIBuilder.Button(resultWindow, "BotaoTentar", "Tentar de novo", Center, new Vector2(-470f, -330f), new Vector2(430f, 112f), race.RestartRace);
        screens.nextTrackButton = UIBuilder.Button(resultWindow, "BotaoProxima", "Próxima pista", Center, new Vector2(0f, -330f), new Vector2(430f, 112f), race.GoToNextTrack).gameObject;
        UIBuilder.Button(resultWindow, "BotaoMenu", "Voltar ao menu", Center, new Vector2(470f, -330f), new Vector2(430f, 112f), race.BackToMenu);

        pause.gameObject.SetActive(false);
        resultOverlay.gameObject.SetActive(false);
        countdown.gameObject.SetActive(false);
    }
}
