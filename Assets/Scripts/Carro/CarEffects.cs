using UnityEngine;

// Efeitos visuais do carro. Só lê o estado do CarController e do CarDamage;
// não muda nada na física.
public class CarEffects : MonoBehaviour
{
    public CarController car;
    public CarSetup setup;
    [Tooltip("Só no carro do jogador.")]
    public CarDamage damage;
    public CarSounds sounds;
    public SpriteRenderer body;
    public SpriteRenderer shadow;

    [Header("Marcas de pneu e fumaça")]
    public TrailRenderer[] skidTrails;
    public ParticleSystem tireSmoke;
    public ParticleSystem dust;

    [Header("Danos")]
    public ParticleSystem damageSmoke;
    public ParticleSystem fire;
    public ParticleSystem sparks;

    [Header("Escapamento")]
    public SpriteRenderer exhaustFlame;
    public float flameDuration = 0.09f;

    [Header("Carro com estados (Animator)")]
    [Tooltip("Parâmetros \"Velocidade\" (float) e \"Derrapando\" (bool): Parado, Andando e Derrapando.")]
    public Animator bodyAnimator;

    [Header("Luzes com estados (Animator)")]
    [Tooltip("Parâmetro bool \"Freando\".")]
    public Animator brakeLights;
    [Tooltip("Parâmetro bool \"Ativo\".")]
    public Animator nitroFlame;

    [Header("Faróis (só nas pistas noturnas)")]
    public GameObject headlights;

    private float lastThrottle;
    private float flameTimeLeft;
    private Vector3 bodyScale;
    private Vector3 shadowOffset;

    void Start()
    {
        PlaceParts();
        bodyScale = body.transform.localScale;
        if (shadow != null)
        {
            shadow.sprite = body.sprite;
            shadow.transform.localScale = bodyScale;
            shadowOffset = shadow.transform.localPosition;
        }
        if (exhaustFlame != null)
        {
            exhaustFlame.color = setup.Exhaust.flameColor;
            exhaustFlame.enabled = false;
        }
        if (headlights != null)
            headlights.SetActive(car.raceManager.track.isNight);
    }

    // Cada modelo tem um tamanho: luzes, chamas e marcas vão para a traseira certa.
    void PlaceParts()
    {
        float back = -setup.HalfLength;
        float front = setup.HalfLength;
        float side = setup.HalfWidth * 0.62f;

        SetY(brakeLights, 0f, back + 0.12f);
        SetY(nitroFlame, 0f, back - 0.75f);
        SetY(exhaustFlame, side * 0.5f, back - 0.3f);
        SetY(tireSmoke, 0f, back * 0.6f);
        SetY(dust, 0f, back * 0.6f);
        SetY(damageSmoke, 0f, front * 0.5f);
        SetY(fire, 0f, front * 0.5f);
        SetY(headlights, 0f, front);
        if (skidTrails.Length == 2)
        {
            SetY(skidTrails[0], -side, back * 0.55f);
            SetY(skidTrails[1], side, back * 0.55f);
        }
    }

    static void SetY(Component part, float x, float y)
    {
        if (part != null)
            part.transform.localPosition = new Vector3(x, y, 0f);
    }

    static void SetY(GameObject part, float x, float y)
    {
        if (part != null)
            part.transform.localPosition = new Vector3(x, y, 0f);
    }

    void Update()
    {
        bool sliding = (car.IsDrifting || (car.IsBraking && car.ForwardSpeed > 5f)) && !car.IsAirborne;

        foreach (TrailRenderer trail in skidTrails)
            trail.emitting = sliding;

        SetEmission(tireSmoke, car.IsDrifting ? 28f : 0f);
        bool dusty = SurfaceInfo.MakesDust(car.Surface) && Mathf.Abs(car.ForwardSpeed) > 3f;
        SetEmission(dust, dusty ? 25f * car.SpeedPercent : 0f);

        // Os estados ficam nos Animators: aqui só mudamos os parâmetros.
        if (bodyAnimator != null)
        {
            bodyAnimator.SetFloat("Velocidade", Mathf.Abs(car.ForwardSpeed));
            bodyAnimator.SetBool("Derrapando", car.IsDrifting);
        }
        if (brakeLights != null)
            brakeLights.SetBool("Freando", car.IsBraking);
        if (nitroFlame != null)
            nitroFlame.SetBool("Ativo", car.IsNitroActive);

        UpdateExhaust();
        UpdateDamageEffects();
        UpdateJump();
        UpdateBlink();
    }

    // Tirar o pé do acelerador em alta rotação pode dar um estouro com chama.
    void UpdateExhaust()
    {
        float throttle = car.ThrottleInput;
        if (lastThrottle > 0.5f && throttle <= 0f && car.SpeedPercent > 0.55f
            && Random.value < setup.Exhaust.popChance)
        {
            flameTimeLeft = flameDuration;
            if (sounds != null)
                sounds.PlayPop();
        }
        lastThrottle = throttle;

        if (exhaustFlame != null)
        {
            flameTimeLeft -= Time.deltaTime;
            exhaustFlame.enabled = flameTimeLeft > 0f;
        }
    }

    // Com poucos corações, o motor solta fumaça; no último, pega fogo.
    void UpdateDamageEffects()
    {
        if (damage == null)
            return;

        int heartsLeft = damage.HeartsLeft;
        SetEmission(damageSmoke, heartsLeft <= 2 ? 10f : 0f);
        SetEmission(fire, heartsLeft <= 1 ? 18f : 0f);
    }

    // No pulo, o carro cresce e a sombra se afasta, como se ele subisse.
    void UpdateJump()
    {
        float height = car.IsAirborne ? Mathf.Sin(car.AirProgress * Mathf.PI) : 0f;
        body.transform.localScale = bodyScale * (1f + 0.3f * height);

        if (shadow != null)
        {
            shadow.sprite = body.sprite;
            shadow.transform.localScale = bodyScale * (1f + 0.1f * height);
            shadow.transform.localPosition = shadowOffset + new Vector3(0.5f, -0.5f, 0f) * height;
        }
    }

    // O carro pisca enquanto está protegido depois de um dano.
    void UpdateBlink()
    {
        if (damage == null)
            return;

        Color color = body.color;
        color.a = damage.IsInvulnerable && Mathf.Repeat(Time.time * 10f, 1f) < 0.5f ? 0.45f : 1f;
        body.color = color;
    }

    // Chamado pelo CarDamage: faíscas no ponto da batida.
    public void Crash(Vector2 point, float impactSpeed)
    {
        if (sparks == null)
            return;

        sparks.transform.position = point;
        sparks.Emit(Mathf.RoundToInt(Mathf.Lerp(4f, 30f, impactSpeed / 10f)));
    }

    static void SetEmission(ParticleSystem particles, float rate)
    {
        if (particles == null)
            return;
        ParticleSystem.EmissionModule emission = particles.emission;
        emission.rateOverTime = rate;
    }
}
