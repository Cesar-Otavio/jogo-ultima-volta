using UnityEngine;

// Monta o carro antes da corrida: modelo, pintura, números e peças.
// No jogador, usa o que foi escolhido na oficina. Nos bots, usa os
// campos preenchidos no Inspector.
public class CarSetup : MonoBehaviour
{
    public GameCatalog catalog;

    [Tooltip("Ligado no carro do jogador: usa as escolhas salvas na oficina.")]
    public bool usePlayerChoices = true;

    [Header("Escolhas dos bots")]
    public string carId;
    public string colorId;
    public string engineId;
    public string exhaustId;
    [Range(0f, 1f)] public float botSkill = 0.5f;

    [Header("Partes do carro")]
    public SpriteRenderer body;
    public BoxCollider2D bodyCollider;

    public CarModel Model { get; private set; }
    public ColorOption Paint { get; private set; }
    public EngineOption Engine { get; private set; }
    public ExhaustOption Exhaust { get; private set; }
    public CarStats Stats { get; private set; }
    // Metade do comprimento e da largura do carro, para posicionar luzes e efeitos.
    public float HalfLength { get; private set; }
    public float HalfWidth { get; private set; }

    void Awake()
    {
        Apply();
    }

    public void Apply()
    {
        if (usePlayerChoices)
        {
            SaveData save = SaveSystem.Load(catalog);
            Model = catalog.CarById(save.car);
            Paint = catalog.ColorById(save.color);
            Engine = catalog.EngineById(save.engine);
            Exhaust = catalog.ExhaustById(save.exhaust);
            Stats = catalog.StatsFor(save);
        }
        else
        {
            Model = catalog.CarById(carId);
            Paint = catalog.ColorById(colorId);
            Engine = catalog.EngineById(engineId);
            Exhaust = catalog.ExhaustById(exhaustId);
            Stats = BotStats();
        }

        ApplyBody();
        ApplyStats();
    }

    // Bots mais habilidosos são mais rápidos. A física é a mesma do jogador.
    CarStats BotStats()
    {
        CarStats stats = Model.stats.Copy();
        stats.maxSpeed = 8.6f + 2.8f * botSkill;
        stats.acceleration = 7.5f + 2.5f * botSkill;
        stats.grip = 11f + 3f * botSkill;
        return stats;
    }

    void ApplyBody()
    {
        body.sprite = Model.damageSprites[0];
        body.transform.localScale = Vector3.one * Model.scale;

        // A caixa de colisão acompanha o tamanho do sprite do modelo.
        Vector2 spriteSize = Model.damageSprites[0].bounds.size * Model.scale;
        bodyCollider.size = new Vector2(spriteSize.x * 0.88f, spriteSize.y * 0.95f);
        bodyCollider.offset = Vector2.zero;
        HalfLength = spriteSize.y / 2f;
        HalfWidth = spriteSize.x / 2f;

        ApplyPaint(body, Model, Paint);

        CarDamage damage = GetComponent<CarDamage>();
        if (damage != null)
        {
            damage.damageSprites = Model.damageSprites;
            damage.damageLimit = Stats.hearts;
        }
    }

    void ApplyStats()
    {
        CarController car = GetComponent<CarController>();
        car.maxSpeed = Stats.maxSpeed;
        car.acceleration = Stats.acceleration;
        car.brakeForce = Stats.brakeForce;
        car.turnSpeed = Stats.turnSpeed;
        car.grip = Stats.grip;
        car.nitroCapacity = Stats.nitroCapacity;
        GetComponent<Rigidbody2D>().mass = Stats.mass;
    }

    // A pintura é feita pelo shader do carro (Assets/Materiais/Carro.shader):
    // ele gira a cor das partes coloridas e tinge as partes brancas.
    public static void ApplyPaint(Renderer renderer, CarModel model, ColorOption paint)
    {
        float hue = 0f, saturation = 1f, brightness = 1f;
        Color tint = Color.white;

        if (!paint.original)
        {
            Color.RGBToHSV(paint.color, out float h, out float s, out float v);
            if (model.whiteBody)
                tint = paint.color;
            else if (s < 0.15f)
            {
                saturation = 0f;
                brightness = Mathf.Lerp(0.35f, 1.6f, v);
            }
            else
                hue = h * 360f - model.baseHue;
        }

        MaterialPropertyBlock block = new MaterialPropertyBlock();
        renderer.GetPropertyBlock(block);
        block.SetFloat("_Hue", Mathf.Repeat(hue, 360f));
        block.SetFloat("_Saturation", saturation);
        block.SetFloat("_Brightness", brightness);
        block.SetColor("_Tint", tint);
        renderer.SetPropertyBlock(block);
    }
}
