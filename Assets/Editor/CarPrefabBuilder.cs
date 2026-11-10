using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

// Monta os prefabs dos carros (jogador e bot) com todas as partes:
// carroceria, sombra, luzes com Animator, chamas, marcas de pneu,
// partículas, faróis, sons e o ícone do minimapa.
// Menu: Última Volta > 2. Criar prefabs dos carros
public static class CarPrefabBuilder
{
    public const string PlayerPrefabPath = "Assets/Prefabs/CarroJogador.prefab";
    public const string BotPrefabPath = "Assets/Prefabs/CarroBot.prefab";

    // Ordem de desenho: quanto maior, mais na frente.
    public const int OrderSkid = 5, OrderDust = 6, OrderShadow = 7, OrderNitro = 8, OrderBody = 9,
        OrderBrake = 10, OrderSmoke = 11, OrderSpark = 12, OrderMinimap = 50;

    [MenuItem("Última Volta/2. Criar prefabs dos carros")]
    public static void BuildAll()
    {
        Build(true);
        Build(false);
        AssetDatabase.SaveAssets();
        Debug.Log("Prefabs dos carros criados.");
    }

    static void Build(bool isPlayer)
    {
        GameCatalog catalog = AssetSetup.Load<GameCatalog>(AssetSetup.CatalogPath);
        Material spriteLit = AssetSetup.Load<Material>("Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Lit-Default.mat");
        Material spriteUnlit = AssetSetup.Load<Material>("Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Unlit-Default.mat");

        GameObject root = new GameObject(isPlayer ? "CarroJogador" : "CarroBot");
        if (isPlayer)
            root.tag = "Player";

        Rigidbody2D rb = root.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        rb.linearDamping = 0f;
        rb.angularDamping = 0f;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.sleepMode = RigidbodySleepMode2D.NeverSleep;

        BoxCollider2D box = root.AddComponent<BoxCollider2D>();
        box.sharedMaterial = AssetSetup.Load<PhysicsMaterial2D>("Assets/Fisica/SemAtrito.physicsMaterial2D");
        box.size = new Vector2(1.54f, 3.37f);

        CarController controller = root.AddComponent<CarController>();
        RaceCar raceCar = root.AddComponent<RaceCar>();
        raceCar.isPlayer = isPlayer;
        controller.raceCar = raceCar;

        // Carroceria e sombra
        SpriteRenderer shadow = Sprite(root.transform, "Sombra", catalog.cars[0].damageSprites[0], OrderShadow, spriteLit);
        shadow.color = new Color(0f, 0f, 0f, 0.35f);
        shadow.transform.localPosition = new Vector3(0.15f, -0.2f, 0f);
        SpriteRenderer body = Sprite(root.transform, "Carroceria", catalog.cars[0].damageSprites[0], OrderBody,
            AssetSetup.Load<Material>(AssetSetup.MaterialsFolder + "/Carro.mat"));

        // Estados do próprio carro: Parado, Andando e Derrapando.
        Animator bodyAnimator = body.gameObject.AddComponent<Animator>();
        bodyAnimator.runtimeAnimatorController = AssetSetup.Load<RuntimeAnimatorController>(AssetSetup.AnimationsFolder + "/Carro.controller");

        CarSetup setup = root.AddComponent<CarSetup>();
        setup.catalog = catalog;
        setup.usePlayerChoices = isPlayer;
        setup.body = body;
        setup.bodyCollider = box;
        setup.carId = catalog.cars[0].id;
        setup.colorId = "original";
        setup.engineId = "esportivo";
        setup.exhaustId = "padrao";

        // Luzes com estados
        SpriteRenderer brake = Sprite(root.transform, "LuzFreio", Load("Efeitos/Stop_Signals_000"), OrderBrake, spriteUnlit);
        brake.transform.localScale = Vector3.one * 0.45f;
        Animator brakeAnimator = brake.gameObject.AddComponent<Animator>();
        brakeAnimator.runtimeAnimatorController = AssetSetup.Load<RuntimeAnimatorController>(AssetSetup.AnimationsFolder + "/LuzFreio.controller");

        SpriteRenderer nitro = Sprite(root.transform, "Nitro", Load("Efeitos/Nitro_ON_000"), OrderNitro, spriteUnlit);
        nitro.transform.localScale = Vector3.one * 0.3f;
        Animator nitroAnimator = nitro.gameObject.AddComponent<Animator>();
        nitroAnimator.runtimeAnimatorController = AssetSetup.Load<RuntimeAnimatorController>(AssetSetup.AnimationsFolder + "/Nitro.controller");

        SpriteRenderer flame = Sprite(root.transform, "ChamaEscapamento", Load("Efeitos/Nitro_Low_000"), OrderNitro, spriteUnlit);
        flame.transform.localScale = Vector3.one * 0.18f;

        // Marcas de pneu
        Material skidMaterial = AssetSetup.Load<Material>(AssetSetup.MaterialsFolder + "/MarcaPneu.mat");
        TrailRenderer left = Trail(root.transform, "MarcaPneuEsquerda", skidMaterial);
        TrailRenderer right = Trail(root.transform, "MarcaPneuDireita", skidMaterial);

        // Partículas
        Material smoke = AssetSetup.Load<Material>(AssetSetup.MaterialsFolder + "/Fumaca.mat");
        Material glow = AssetSetup.Load<Material>(AssetSetup.MaterialsFolder + "/Brilho.mat");
        ParticleSystem tireSmoke = Particles(root.transform, "FumacaPneus", smoke, new Color(0.92f, 0.92f, 0.92f, 0.55f),
            new Vector2(0.6f, 1f), new Vector2(0.8f, 1.6f), new Vector2(0.2f, 0.8f), OrderSmoke, 0.5f);
        ParticleSystem dust = Particles(root.transform, "Poeira", smoke, new Color(0.7f, 0.55f, 0.35f, 0.5f),
            new Vector2(0.6f, 1f), new Vector2(0.9f, 1.6f), new Vector2(0.2f, 0.8f), OrderDust, 0.6f);
        ParticleSystem damageSmoke = Particles(root.transform, "FumacaDano", smoke, new Color(0.25f, 0.25f, 0.25f, 0.7f),
            new Vector2(1.2f, 1.8f), new Vector2(0.8f, 1.5f), new Vector2(0.2f, 0.6f), OrderSmoke, 0.3f);
        ParticleSystem fire = Particles(root.transform, "Fogo", glow, new Color(1f, 0.55f, 0.15f, 0.9f),
            new Vector2(0.35f, 0.55f), new Vector2(0.5f, 0.9f), new Vector2(0.3f, 1f), OrderSpark, 0.3f);
        ParticleSystem sparks = Particles(root.transform, "Faiscas", glow, new Color(1f, 0.85f, 0.35f, 1f),
            new Vector2(0.2f, 0.4f), new Vector2(0.12f, 0.25f), new Vector2(3f, 8f), OrderSpark, 0.1f);
        ParticleSystemRenderer sparkRenderer = sparks.GetComponent<ParticleSystemRenderer>();
        sparkRenderer.renderMode = ParticleSystemRenderMode.Stretch;
        sparkRenderer.velocityScale = 0.06f;
        sparkRenderer.lengthScale = 1f;

        // Faróis: só ligam nas pistas noturnas
        GameObject headlights = new GameObject("Farois");
        headlights.transform.SetParent(root.transform, false);
        Light2D light = headlights.AddComponent<Light2D>();
        light.lightType = Light2D.LightType.Point;
        light.pointLightOuterRadius = 16f;
        light.pointLightInnerRadius = 1f;
        light.pointLightOuterAngle = 80f;
        light.pointLightInnerAngle = 35f;
        light.intensity = 1.3f;
        light.color = new Color(1f, 0.95f, 0.8f);

        // Luz fraca em volta do carro, para ele aparecer no escuro.
        GameObject glowObject = new GameObject("LuzCarro");
        glowObject.transform.SetParent(headlights.transform, false);
        glowObject.transform.localPosition = new Vector3(0f, -1.7f, 0f);
        Light2D glowLight = glowObject.AddComponent<Light2D>();
        glowLight.lightType = Light2D.LightType.Point;
        glowLight.pointLightOuterRadius = 4.5f;
        glowLight.pointLightInnerRadius = 0.5f;
        glowLight.intensity = 0.8f;
        headlights.SetActive(false);

        // Ícone do minimapa
        SpriteRenderer icon = Sprite(root.transform, "IconeMinimapa", UIBuilder.Generated("Disco"), OrderMinimap, spriteUnlit);
        icon.gameObject.layer = AssetSetup.MinimapLayer;
        icon.transform.localScale = Vector3.one * (isPlayer ? 9f : 7f);
        icon.color = isPlayer ? new Color(1f, 0.9f, 0.2f) : new Color(1f, 0.35f, 0.3f);

        // Sons
        CarSounds sounds = root.AddComponent<CarSounds>();
        sounds.car = controller;
        sounds.setup = setup;
        sounds.engine = Audio(root.transform, "Motor", AssetSetup.Load<AudioClip>("Assets/Audio/Efeitos/motor_loop_0.wav"), isPlayer);
        sounds.engineFilter = sounds.engine.gameObject.AddComponent<AudioLowPassFilter>();
        sounds.skid = Audio(root.transform, "Pneus", AssetSetup.Load<AudioClip>("Assets/Audio/Efeitos/derrapagem_tires_squal_loop.wav"), isPlayer);
        sounds.oneShots = Audio(root.transform, "SonsCarro", null, isPlayer);
        sounds.nitroClip = AssetSetup.Load<AudioClip>("Assets/Audio/Efeitos/nitro_boost.mp3");
        sounds.popClips = new[]
        {
            AssetSetup.Load<AudioClip>("Assets/Audio/Efeitos/escapamento_shot_01.ogg"),
            AssetSetup.Load<AudioClip>("Assets/Audio/Efeitos/escapamento_shot_02.ogg"),
            AssetSetup.Load<AudioClip>("Assets/Audio/Efeitos/escapamento_shot_03.ogg"),
        };
        sounds.engineVolume = isPlayer ? 0.5f : 0.35f;
        sounds.skidVolume = isPlayer ? 0.35f : 0.25f;

        CarEffects effects = root.AddComponent<CarEffects>();
        effects.car = controller;
        effects.setup = setup;
        effects.sounds = sounds;
        effects.body = body;
        effects.shadow = shadow;
        effects.skidTrails = new[] { left, right };
        effects.tireSmoke = tireSmoke;
        effects.dust = dust;
        effects.damageSmoke = damageSmoke;
        effects.fire = fire;
        effects.sparks = sparks;
        effects.exhaustFlame = flame;
        effects.bodyAnimator = bodyAnimator;
        effects.brakeLights = brakeAnimator;
        effects.nitroFlame = nitroAnimator;
        effects.headlights = headlights;

        if (isPlayer)
        {
            root.AddComponent<PlayerDriver>();
            CarDamage damage = root.AddComponent<CarDamage>();
            damage.body = body;
            damage.damageSprites = catalog.cars[0].damageSprites;
            damage.effects = effects;
            effects.damage = damage;
        }
        else
        {
            BotDriver bot = root.AddComponent<BotDriver>();
            bot.raceCar = raceCar;
        }

        PrefabUtility.SaveAsPrefabAsset(root, isPlayer ? PlayerPrefabPath : BotPrefabPath);
        Object.DestroyImmediate(root);
    }

    static Sprite Load(string path) => AssetSetup.Load<Sprite>("Assets/Sprites/" + path + ".png");

    static SpriteRenderer Sprite(Transform parent, string name, Sprite sprite, int order, Material material)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = order;
        renderer.sharedMaterial = material;
        return renderer;
    }

    static TrailRenderer Trail(Transform parent, string name, Material material)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        TrailRenderer trail = go.AddComponent<TrailRenderer>();
        trail.sharedMaterial = material;
        trail.time = 4f;
        trail.minVertexDistance = 0.2f;
        trail.widthMultiplier = 0.32f;
        trail.emitting = false;
        trail.sortingOrder = OrderSkid;
        trail.alignment = LineAlignment.TransformZ;
        trail.textureMode = LineTextureMode.Stretch;
        trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new[] { new GradientColorKey(new Color(0.08f, 0.08f, 0.08f), 0f), new GradientColorKey(new Color(0.08f, 0.08f, 0.08f), 1f) },
            new[] { new GradientAlphaKey(0.55f, 0f), new GradientAlphaKey(0.45f, 0.7f), new GradientAlphaKey(0f, 1f) });
        trail.colorGradient = gradient;
        return trail;
    }

    public static ParticleSystem Particles(Transform parent, string name, Material material, Color color,
        Vector2 lifetime, Vector2 size, Vector2 speed, int order, float radius)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        ParticleSystem particles = go.AddComponent<ParticleSystem>();
        particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        ParticleSystem.MainModule main = particles.main;
        main.loop = true;
        main.duration = 1f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(lifetime.x, lifetime.y);
        main.startSize = new ParticleSystem.MinMaxCurve(size.x, size.y);
        main.startSpeed = new ParticleSystem.MinMaxCurve(speed.x, speed.y);
        main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        main.startColor = color;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 300;
        main.playOnAwake = true;

        ParticleSystem.EmissionModule emission = particles.emission;
        emission.rateOverTime = 0f;

        ParticleSystem.ShapeModule shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = radius;

        ParticleSystem.ColorOverLifetimeModule colorOverLifetime = particles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient fade = new Gradient();
        fade.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0.8f, 0.4f), new GradientAlphaKey(0f, 1f) });
        colorOverLifetime.color = fade;

        ParticleSystem.SizeOverLifetimeModule sizeOverLifetime = particles.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 0.6f, 1f, 1.4f));

        ParticleSystemRenderer renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.sharedMaterial = material;
        renderer.sortingOrder = order;
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        return particles;
    }

    static AudioSource Audio(Transform parent, string name, AudioClip clip, bool isPlayer)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        AudioSource source = go.AddComponent<AudioSource>();
        source.clip = clip;
        source.loop = clip != null;
        source.playOnAwake = false;
        source.dopplerLevel = 0f;
        // Bots: som 3D, mais alto quando passam perto da câmera.
        source.spatialBlend = isPlayer ? 0f : 1f;
        source.rolloffMode = AudioRolloffMode.Linear;
        source.minDistance = 10f;
        source.maxDistance = 40f;
        return source;
    }
}
