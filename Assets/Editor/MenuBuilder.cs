using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using UImage = UnityEngine.UI.Image;

// Monta a cena do menu: tela principal, pistas, oficina (com vitrine),
// desafios, instruções, créditos e "Sobre".
// Menu: Última Volta > 5. Construir o menu
public static class MenuBuilder
{
    const string ScenePath = "Assets/Scenes/Menu.unity";
    static readonly Vector2 Center = new Vector2(0.5f, 0.5f), Top = new Vector2(0.5f, 1f), Bottom = new Vector2(0.5f, 0f),
        TopRight = new Vector2(1f, 1f), Left = new Vector2(0f, 0.5f), Right = new Vector2(1f, 0.5f);

    public const string Members =
        "Amanda Pazold dos Santos\nCesar Otavio da Silva Boiani\nGiovana Giraldeli\nGiovani Nogueira Pires\nMarcus Vinicius da Silva Capeteruchi";

    [MenuItem("Última Volta/5. Construir o menu")]
    public static void Build()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        GameCatalog catalog = AssetSetup.Load<GameCatalog>(AssetSetup.CatalogPath);

        // Câmera principal (só fundo) e luz para o shader do carro.
        GameObject cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        Camera cam = cameraObject.AddComponent<Camera>();
        cam.orthographic = true;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.1f, 0.2f, 0.12f);
        cam.cullingMask = 0;
        cameraObject.transform.position = new Vector3(0f, 0f, -10f);
        cameraObject.AddComponent<AudioListener>();
        cameraObject.AddComponent<UniversalAdditionalCameraData>();
        // A luz precisa estar numa camada que a câmera da vitrine enxerga.
        Light2D light = new GameObject("LuzGlobal").AddComponent<Light2D>();
        light.lightType = Light2D.LightType.Global;
        light.gameObject.layer = AssetSetup.GarageLayer;

        // Vitrine: um carro girando, visto por uma câmera que desenha numa textura.
        RenderTexture showroomTexture = ShowroomTexture();
        GameObject showroomCamera = new GameObject("CameraOficina");
        Camera garageCam = showroomCamera.AddComponent<Camera>();
        garageCam.orthographic = true;
        garageCam.orthographicSize = 5f;
        garageCam.clearFlags = CameraClearFlags.SolidColor;
        garageCam.backgroundColor = new Color(0f, 0f, 0f, 0f);
        garageCam.cullingMask = 1 << AssetSetup.GarageLayer;
        garageCam.targetTexture = showroomTexture;
        showroomCamera.transform.position = new Vector3(1000f, 0f, -10f);
        showroomCamera.AddComponent<UniversalAdditionalCameraData>();

        GameObject previewObject = new GameObject("CarroVitrine");
        previewObject.layer = AssetSetup.GarageLayer;
        previewObject.transform.position = new Vector3(1000f, 0f, 0f);
        SpriteRenderer preview = previewObject.AddComponent<SpriteRenderer>();
        preview.sprite = catalog.cars[0].damageSprites[0];
        preview.sharedMaterial = AssetSetup.Load<Material>(AssetSetup.MaterialsFolder + "/Carro.mat");
        previewObject.AddComponent<Spinner>().speed = 30f;

        // Sons do menu
        GameObject audioObject = new GameObject("Audio");
        AudioSource music = audioObject.AddComponent<AudioSource>();
        music.clip = AssetSetup.Load<AudioClip>("Assets/Audio/Musica/racing_game_menu_bpm165.ogg");
        music.loop = true;
        music.volume = 0.4f;
        music.playOnAwake = true;
        AudioSource effects = new GameObject("Efeitos").AddComponent<AudioSource>();
        effects.transform.SetParent(audioObject.transform, false);
        effects.playOnAwake = false;
        AudioSource engine = new GameObject("MotorOficina").AddComponent<AudioSource>();
        engine.transform.SetParent(audioObject.transform, false);
        engine.clip = AssetSetup.Load<AudioClip>("Assets/Audio/Efeitos/motor_loop_0.wav");
        engine.loop = true;
        engine.playOnAwake = false;
        AudioLowPassFilter engineFilter = engine.gameObject.AddComponent<AudioLowPassFilter>();

        // Interface
        Canvas canvas = UIBuilder.Canvas("Telas", 0);
        Transform root = canvas.transform;
        MenuController menu = canvas.gameObject.AddComponent<MenuController>();
        menu.catalog = catalog;
        menu.effects = effects;
        menu.unlockClip = AssetSetup.Load<AudioClip>("Assets/Audio/Efeitos/desbloqueio_Upper01.aif");
        menu.errorClip = AssetSetup.Load<AudioClip>("Assets/Audio/Efeitos/erro_Downer01.aif");

        UImage background = UIBuilder.Image(root, "Fundo", AssetSetup.Load<Sprite>("Assets/Sprites/Fundo/Grass_Tile.png"), Color.white, Center, Vector2.zero, Vector2.zero);
        background.type = UImage.Type.Tiled;
        background.preserveAspect = false;
        RectTransform backgroundRect = background.rectTransform;
        backgroundRect.anchorMin = Vector2.zero;
        backgroundRect.anchorMax = Vector2.one;
        backgroundRect.sizeDelta = Vector2.zero;

        menu.mainScreen = MainScreen(root, menu, showroomTexture);
        menu.tracksScreen = TracksScreen(root, menu, catalog);
        menu.garageScreen = GarageScreenUI(root, menu, showroomTexture, preview, engine, engineFilter);
        menu.challengesScreen = ChallengesScreenUI(root, menu);
        menu.instructionsScreen = TextScreen(root, menu, "TelaInstrucoes", "Como jogar", InstructionsText, 30f, false);
        menu.creditsScreen = TextScreen(root, menu, "TelaCreditos", "Créditos", CreditsText, 26f, true);
        menu.aboutScreen = TextScreen(root, menu, "TelaSobre", "Sobre o jogo", AboutText, 32f, false);

        // Moedas no canto (aparece em todas as telas) e aviso temporário.
        UIBuilder.Image(root, "IconeMoedas", UIBuilder.Generated("Moeda"), Color.white, TopRight, new Vector2(-230f, -34f), new Vector2(70f, 70f));
        menu.coinsText = UIBuilder.Text(root, "Moedas", "0", 56f, TopRight, new Vector2(-30f, -30f), new Vector2(190f, 76f), TextAlignmentOptions.Left);
        menu.coinsText.fontStyle = FontStyles.Bold;
        menu.coinsText.color = UIBuilder.Gold;
        UImage toastBack = UIBuilder.Image(root, "Aviso", UIBuilder.Generated("Pixel"), new Color(0f, 0f, 0f, 0.75f), Bottom, new Vector2(0f, 150f), new Vector2(1300f, 80f));
        menu.toast = toastBack.gameObject;
        menu.toastText = UIBuilder.Text(toastBack.transform, "Texto", "", 36f, Center, Vector2.zero, new Vector2(1280f, 80f));
        menu.toastText.color = UIBuilder.Gold;

        UIBuilder.EventSystem();
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), ScenePath);
        SceneList.Update();
        Debug.Log("Menu construído.");
    }

    static RenderTexture ShowroomTexture()
    {
        const string path = "Assets/Texturas/Oficina.renderTexture";
        RenderTexture texture = AssetSetup.Load<RenderTexture>(path);
        if (texture == null)
        {
            texture = new RenderTexture(512, 512, 24, RenderTextureFormat.ARGB32);
            AssetDatabase.CreateAsset(texture, path);
        }
        return texture;
    }

    static RawImage Showroom(Transform parent, RenderTexture texture, Vector2 anchor, Vector2 position, float size)
    {
        RectTransform rect = UIBuilder.Rect(parent, "Vitrine", anchor, position, new Vector2(size, size));
        RawImage image = rect.gameObject.AddComponent<RawImage>();
        image.texture = texture;
        image.raycastTarget = false;
        return image;
    }

    static GameObject Screen(Transform root, string name, string title)
    {
        RectTransform screen = UIBuilder.Fill(root, name);
        if (title != null)
        {
            UIBuilder.Overlay(screen, "Escurecer", 0.55f);
            UImage band = UIBuilder.Image(screen, "FaixaTitulo", UIBuilder.UISprite("Faixa"), Color.white, Top, new Vector2(0f, -24f), new Vector2(760f, 130f), true);
            TMP_Text text = UIBuilder.Text(band.transform, "Titulo", title, 64f, Center, Vector2.zero, new Vector2(740f, 120f));
            text.fontStyle = FontStyles.Bold;
            text.color = UIBuilder.Gold;
        }
        return screen.gameObject;
    }

    static Button Back(Transform parent, MenuController menu)
    {
        return UIBuilder.Button(parent, "BotaoVoltar", "Voltar", new Vector2(0f, 0f), new Vector2(40f, 36f), new Vector2(320f, 100f), menu.ShowMain);
    }

    // ---------- Tela principal ----------

    static GameObject MainScreen(Transform root, MenuController menu, RenderTexture showroom)
    {
        GameObject screen = Screen(root, "TelaPrincipal", null);
        Transform t = screen.transform;

        UImage band = UIBuilder.Image(t, "FaixaTitulo", UIBuilder.UISprite("Faixa"), Color.white, Top, new Vector2(0f, -40f), new Vector2(1000f, 200f), true);
        TMP_Text title = UIBuilder.Text(band.transform, "Titulo", "Última Volta", 110f, Center, Vector2.zero, new Vector2(980f, 190f));
        title.fontStyle = FontStyles.Bold;
        UIBuilder.Text(t, "Subtitulo", "Desenvolvimento de Jogos Digitais · Ciência da Computação", 32f, Top, new Vector2(0f, -250f), new Vector2(1400f, 50f));

        string[] labels = { "Jogar", "Oficina", "Desafios", "Instruções", "Créditos", "Sobre", "Sair" };
        UnityEngine.Events.UnityAction[] actions = { menu.ShowTracks, menu.ShowGarage, menu.ShowChallenges, menu.ShowInstructions, menu.ShowCredits, menu.ShowAbout, menu.QuitGame };
        for (int i = 0; i < labels.Length; i++)
        {
            Button button = UIBuilder.Button(t, "Botao" + labels[i], labels[i], Left, new Vector2(140f, 200f - i * 100f), new Vector2(500f, 90f), actions[i], 40f);
            if (i == 0)
                button.GetComponent<UImage>().color = new Color(0.75f, 1f, 0.75f);
        }

        Showroom(t, showroom, Right, new Vector2(-220f, -90f), 560f);
        UIBuilder.Text(t, "Dica", "W A S D: dirigir · Espaço: drift · Shift: nitro · Esc: pausar", 28f, new Vector2(1f, 0f), new Vector2(-60f, 36f), new Vector2(1000f, 44f), TextAlignmentOptions.Right);
        return screen;
    }

    // ---------- Pistas ----------

    static GameObject TracksScreen(Transform root, MenuController menu, GameCatalog catalog)
    {
        GameObject screen = Screen(root, "TelaPistas", "Escolha a pista");
        TrackSelectScreen select = screen.AddComponent<TrackSelectScreen>();
        select.menu = menu;
        menu.trackSelect = select;
        select.cards = new TrackCard[catalog.tracks.Length];

        for (int i = 0; i < catalog.tracks.Length; i++)
        {
            float x = (i - (catalog.tracks.Length - 1) / 2f) * 362f;
            UImage card = UIBuilder.Image(screen.transform, "Cartao" + (i + 1), UIBuilder.UISprite("Faixa"), Color.white, Center, new Vector2(x, -10f), new Vector2(345f, 690f), true);
            Transform c = card.transform;
            TrackCard view = new TrackCard
            {
                background = card,
                title = UIBuilder.Text(c, "Nome", "", 42f, Top, new Vector2(0f, -28f), new Vector2(310f, 110f)),
                details = UIBuilder.Text(c, "Detalhes", "", 23f, Top, new Vector2(0f, -150f), new Vector2(295f, 330f), TextAlignmentOptions.TopLeft),
                record = UIBuilder.Text(c, "Recorde", "", 23f, Top, new Vector2(0f, -470f), new Vector2(295f, 80f), TextAlignmentOptions.TopLeft),
            };
            view.title.fontStyle = FontStyles.Bold;
            view.record.color = UIBuilder.Gold;
            view.playButton = UIBuilder.Button(c, "BotaoCorrer", "Correr", Bottom, new Vector2(0f, 34f), new Vector2(260f, 88f), null, 36f);
            view.playButton.GetComponent<UImage>().color = new Color(0.75f, 1f, 0.75f);
            UIBuilder.AddCall(view.playButton, select.Play, i);

            UImage lockOverlay = UIBuilder.Image(c, "Bloqueio", UIBuilder.Generated("Pixel"), new Color(0f, 0f, 0f, 0.85f), Center, Vector2.zero, new Vector2(330f, 676f));
            lockOverlay.raycastTarget = true;
            view.lockOverlay = lockOverlay.gameObject;
            view.lockText = UIBuilder.Text(lockOverlay.transform, "Texto", "Bloqueada", 36f, Center, Vector2.zero, new Vector2(300f, 400f));
            select.cards[i] = view;
        }

        Back(screen.transform, menu);
        return screen;
    }

    // ---------- Oficina ----------

    static GameObject GarageScreenUI(Transform root, MenuController menu, RenderTexture showroom, SpriteRenderer preview,
        AudioSource engine, AudioLowPassFilter engineFilter)
    {
        GameObject screen = Screen(root, "TelaOficina", "Oficina");
        Transform t = screen.transform;
        GarageScreen garage = screen.AddComponent<GarageScreen>();
        garage.menu = menu;
        garage.previewCar = preview;
        garage.enginePreview = engine;
        garage.enginePreviewFilter = engineFilter;
        menu.garage = garage;

        Showroom(t, showroom, Center, new Vector2(-560f, 120f), 520f);
        garage.statsText = UIBuilder.Text(t, "Numeros", "", 28f, Center, new Vector2(-560f, -300f), new Vector2(560f, 260f), TextAlignmentOptions.TopLeft);

        string[] categories = { "Modelo", "Pintura", "Som do motor", "Escapamento" };
        garage.rows = new GarageRow[4];
        for (int row = 0; row < 4; row++)
        {
            float y = 340f - row * 128f;
            TMP_Text label = UIBuilder.Text(t, "Categoria" + row, categories[row], 28f, Center, new Vector2(0f, y), new Vector2(210f, 50f), TextAlignmentOptions.Left);
            label.color = UIBuilder.Gold;

            Button left = Arrow(t, "Esquerda" + row, "Left", new Vector2(150f, y));
            UIBuilder.AddCall(left, garage.Previous, row);
            Button right = Arrow(t, "Direita" + row, "Right", new Vector2(530f, y));
            UIBuilder.AddCall(right, garage.Next, row);

            GarageRow view = new GarageRow
            {
                value = UIBuilder.Text(t, "Valor" + row, "", 36f, Center, new Vector2(340f, y + 10f), new Vector2(320f, 56f)),
                description = UIBuilder.Text(t, "Descricao" + row, "", 20f, Center, new Vector2(340f, y - 38f), new Vector2(330f, 50f)),
            };
            view.value.fontStyle = FontStyles.Bold;
            view.actionButton = UIBuilder.Button(t, "Acao" + row, "", Center, new Vector2(750f, y), new Vector2(330f, 86f), null, 28f);
            view.actionLabel = view.actionButton.GetComponentInChildren<TMP_Text>();
            UIBuilder.AddCall(view.actionButton, garage.Action, row);
            garage.rows[row] = view;
        }
        UIBuilder.Button(t, "BotaoOuvir", "Ouvir", Center, new Vector2(-40f, 340f - 2 * 128f - 52f), new Vector2(130f, 52f), garage.ListenEngine, 22f);

        TMP_Text upgradesTitle = UIBuilder.Text(t, "TituloMelhorias", "Melhorias", 34f, Center, new Vector2(200f, -170f), new Vector2(400f, 50f), TextAlignmentOptions.Left);
        upgradesTitle.color = UIBuilder.Gold;
        upgradesTitle.fontStyle = FontStyles.Bold;
        garage.upgradeRows = new UpgradeRow[5];
        for (int type = 0; type < 5; type++)
        {
            float y = -225f - type * 64f;
            UpgradeRow view = new UpgradeRow
            {
                level = UIBuilder.Text(t, "Melhoria" + type, "", 26f, Center, new Vector2(290f, y), new Vector2(580f, 64f), TextAlignmentOptions.Left),
            };
            view.buyButton = UIBuilder.Button(t, "Comprar" + type, "", Center, new Vector2(750f, y), new Vector2(330f, 58f), null, 24f);
            view.buyLabel = view.buyButton.GetComponentInChildren<TMP_Text>();
            UIBuilder.AddCall(view.buyButton, garage.BuyUpgrade, type);
            garage.upgradeRows[type] = view;
        }

        Back(t, menu);
        return screen;
    }

    static Button Arrow(Transform parent, string name, string sprite, Vector2 position)
    {
        UImage image = UIBuilder.Image(parent, name, UIBuilder.UISprite(sprite), Color.white, Center, position, new Vector2(54f, 100f));
        image.raycastTarget = true;
        return image.gameObject.AddComponent<Button>();
    }

    // ---------- Desafios ----------

    static GameObject ChallengesScreenUI(Transform root, MenuController menu)
    {
        GameObject screen = Screen(root, "TelaDesafios", "Desafios");
        ChallengesScreen challenges = screen.AddComponent<ChallengesScreen>();
        challenges.menu = menu;
        menu.challenges = challenges;

        UIBuilder.Image(screen.transform, "Estrela", UIBuilder.UISprite("Star_Gold"), Color.white, Center, new Vector2(-620f, 330f), new Vector2(90f, 86f));
        challenges.summaryText = UIBuilder.Text(screen.transform, "Resumo", "", 34f, Center, new Vector2(40f, 330f), new Vector2(1200f, 100f), TextAlignmentOptions.Left);
        challenges.listText = ScrollText(screen.transform, new Vector2(0f, -60f), new Vector2(1500f, 640f), 26f);

        Button reset = UIBuilder.Button(screen.transform, "BotaoZerar", "Zerar progresso", new Vector2(1f, 0f), new Vector2(-40f, 36f), new Vector2(560f, 100f), challenges.ResetProgress, 32f);
        reset.GetComponent<UImage>().color = new Color(1f, 0.75f, 0.7f);
        challenges.resetLabel = reset.GetComponentInChildren<TMP_Text>();
        Back(screen.transform, menu);
        return screen;
    }

    // Texto longo dentro de uma área com rolagem (roda do mouse).
    static TMP_Text ScrollText(Transform parent, Vector2 position, Vector2 size, float fontSize)
    {
        UImage area = UIBuilder.Image(parent, "Rolagem", UIBuilder.Generated("Pixel"), new Color(0f, 0f, 0f, 0.35f), Center, position, size);
        area.raycastTarget = true;
        area.gameObject.AddComponent<RectMask2D>();
        ScrollRect scroll = area.gameObject.AddComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 40f;

        TMP_Text text = UIBuilder.Text(area.transform, "Conteudo", "", fontSize, Top, Vector2.zero, new Vector2(size.x - 60f, size.y), TextAlignmentOptions.TopLeft);
        RectTransform content = text.rectTransform;
        content.pivot = new Vector2(0.5f, 1f);
        content.anchoredPosition = new Vector2(0f, -20f);
        ContentSizeFitter fitter = text.gameObject.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        scroll.content = content;
        return text;
    }

    // ---------- Telas de texto ----------

    static GameObject TextScreen(Transform root, MenuController menu, string name, string title, string body, float fontSize, bool scroll)
    {
        GameObject screen = Screen(root, name, title);
        if (scroll)
        {
            TMP_Text text = ScrollText(screen.transform, new Vector2(0f, -40f), new Vector2(1560f, 760f), fontSize);
            text.text = body;
            UIBuilder.Text(screen.transform, "DicaRolagem", "Use a roda do mouse para rolar", 24f, Bottom, new Vector2(0f, 60f), new Vector2(800f, 40f));
        }
        else
        {
            UImage panel = UIBuilder.Image(screen.transform, "Painel", UIBuilder.Generated("Pixel"), new Color(0f, 0f, 0f, 0.35f), Center, new Vector2(0f, -40f), new Vector2(1560f, 760f));
            UIBuilder.Text(panel.transform, "Texto", body, fontSize, Center, Vector2.zero, new Vector2(1480f, 720f), TextAlignmentOptions.TopLeft);
        }
        Back(screen.transform, menu);
        return screen;
    }

    const string InstructionsText =
        "<b><color=#FFD95A>Objetivo</color></b>\n" +
        "Complete as voltas antes do tempo acabar, sem quebrar o carro. Termine no pódio (até 3º lugar) para liberar a próxima pista.\n\n" +
        "<b><color=#FFD95A>Controles</color></b>\n" +
        "W ou ↑: acelerar     S ou ↓: frear e dar ré     A e D, ou ← e →: virar\n" +
        "Espaço: freio de mão (drift)     Shift: nitro     Esc: pausar\n\n" +
        "<b><color=#FFD95A>Dicas</color></b>\n" +
        "• Segure Espaço numa curva para derrapar. O drift enche a barra de nitro.\n" +
        "• Barris sempre causam dano. Paredes e outros carros, só em batidas fortes.\n" +
        "• O óleo faz o carro escorregar. Grama, areia e água deixam o carro lento.\n" +
        "• Rampas fazem o carro pular: na Amazônia, dá para passar por cima do rio.\n" +
        "• O item HP+ conserta um dano.\n" +
        "• Moedas da pista, colocação e desafios dão dinheiro para gastar na Oficina.\n" +
        "• Em Nevada, a viatura da polícia persegue quem passa rápido perto dela!";

    const string AboutText =
        "<b><color=#FFD95A>Última Volta</color></b> é um jogo de corrida 2D com vista de cima, feito como trabalho da disciplina " +
        "<b>Desenvolvimento de Jogos Digitais</b>, do curso de Ciência da Computação (P2), com entrega em 19/11/2026.\n\n" +
        "<b><color=#FFD95A>Integrantes</color></b>\n" + Members + "\n\n" +
        "O jogo tem 5 pistas (Treino, Rio de Janeiro, Amazônia, Nevada e Tóquio), rivais controlados pelo computador, drift, nitro, " +
        "uma oficina com peças e desafios que dão moedas.\n\n" +
        "Feito em Unity 6 com C#, com apoio de IA (permitido pela disciplina).";

    const string CreditsText =
        "<b><color=#FFD95A>Integrantes</color></b>\n" + Members + "\n\n" +
        "<b><color=#FFD95A>Arte</color></b>\n" +
        "Sprites: CraftPix.net, pacotes gratuitos \"Free Racing Game Kit\" e \"Free Race Track 2D Game Tile Set\" (licença Freebie: craftpix.net/file-licenses)\n" +
        "Moeda, círculos e marcas de pneu: desenhados por código no próprio projeto\n\n" +
        "<b><color=#FFD95A>Músicas</color></b> (todas de MintoDog, licença CC0, OpenGameArt.org)\n" +
        "Pure Raceway (Treino) · Sunny City (Rio de Janeiro) · Joyful Jungle (Amazônia)\n" +
        "Hot Roadway [Remake] (Nevada) · Neon sign Circuit [Remake] (Tóquio)\n" +
        "Racing Game Menu (menu) · Racing Game Finish & Jingle (vitória e derrota)\n\n" +
        "<b><color=#FFD95A>Efeitos sonoros</color></b>\n" +
        "Motor: \"racing car engine sound loops\", de domasx2 (CC0, OpenGameArt.org)\n" +
        "Batidas: \"Impact Sounds\", de Kenney (CC0, kenney.nl)\n" +
        "Fogos e escapamento: \"25 CC0 bang / firework SFX\", de rubberduck (CC0, OpenGameArt.org)\n" +
        "Moeda, compra e erro: \"Level up, power up, Coin get\", de wobbleboxx (CC0, OpenGameArt.org)\n" +
        "Nitro: \"Boost or Launch or Thruster\", de EZduzziteh (CC0, OpenGameArt.org)\n" +
        "Largada: \"Race Start Countdown\", de kheetor (CC0, OpenGameArt.org)\n" +
        "Derrapagem: \"Car tire squeal skid loop\", de audible-edge (Tom Haigh), licença CC-BY 3.0 (creativecommons.org/licenses/by/3.0)\n\n" +
        "<b><color=#FFD95A>Ferramentas</color></b>\n" +
        "Unity 6 (6000.6.7f1), Universal Render Pipeline 2D, Input System e TextMesh Pro\n" +
        "Fonte Liberation Sans (SIL Open Font License)\n" +
        "Desenvolvido com apoio do assistente de programação Claude, da Anthropic. A disciplina permite o uso de IA.";
}
