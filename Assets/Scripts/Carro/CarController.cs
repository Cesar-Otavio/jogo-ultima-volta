using System.Collections.Generic;
using UnityEngine;

// Movimento de qualquer carro, do jogador ou dos bots.
// Quem decide o que fazer é o piloto (PlayerDriver ou BotDriver), que
// chama SetInput a cada quadro. A física (Rigidbody2D) resolve as batidas.
[RequireComponent(typeof(Rigidbody2D))]
public class CarController : MonoBehaviour
{
    [Header("Desempenho (unidades/s e unidades/s²)")]
    public float maxSpeed = 10f;
    public float acceleration = 8.5f;
    public float reverseSpeed = 3.5f;
    public float brakeForce = 14f;
    [Tooltip("Desaceleração quando o piloto solta o acelerador.")]
    public float coastDeceleration = 2f;

    [Header("Direção")]
    [Tooltip("Giro máximo, em graus por segundo.")]
    public float turnSpeed = 175f;
    [Tooltip("Velocidade a partir da qual o giro chega ao máximo.")]
    public float fullTurnAtSpeed = 2f;

    [Header("Aderência e drift")]
    [Tooltip("Quanto maior, mais rápido o carro para de escorregar de lado.")]
    public float grip = 10f;
    [Tooltip("Aderência com o freio de mão puxado.")]
    public float driftGrip = 1.5f;
    [Tooltip("No drift, o carro gira mais rápido do que anda: é isso que faz ele escorregar.")]
    public float driftTurnBoost = 1.4f;
    public float handbrakeDeceleration = 4f;
    [Tooltip("Aderência no óleo.")]
    public float oilGrip = 0.8f;
    [Tooltip("Desaceleração ao entrar em um piso mais lento, como a grama.")]
    public float offRoadDeceleration = 9f;

    [Header("Nitro")]
    [Tooltip("Duração do tanque cheio, em segundos.")]
    public float nitroCapacity = 2.5f;
    public float nitroSpeedBoost = 1.35f;
    public float nitroAcceleration = 12f;
    [Tooltip("Quanto o tanque enche a cada segundo de drift (1 = tanque cheio).")]
    public float nitroPerDriftSecond = 0.3f;
    [Tooltip("Quanto o tanque enche sozinho por segundo.")]
    public float nitroPassiveRecharge = 0.02f;

    [Header("Corrida")]
    public RaceManager raceManager;
    public RaceCar raceCar;

    // Estado lido pelo HUD, pelos efeitos e pelos sons.
    public float ForwardSpeed { get; private set; }
    public float LateralSpeed { get; private set; }
    public float SpeedPercent => Mathf.Clamp01(Mathf.Abs(ForwardSpeed) / maxSpeed);
    public bool IsDrifting { get; private set; }
    public bool IsBraking { get; private set; }
    public bool IsNitroActive { get; private set; }
    public float Nitro { get; private set; } = 0.35f;
    public int NitroUses { get; private set; }
    public float DriftTime { get; private set; }
    public float ThrottleInput => throttleInput;
    public bool IsAirborne => airTimeLeft > 0f;
    public float AirProgress => airDuration > 0f ? 1f - airTimeLeft / airDuration : 0f;
    public SurfaceType Surface { get; private set; }

    private Rigidbody2D rb;
    private float throttleInput;
    private float steeringInput;
    private bool handbrakeInput;
    private bool nitroInput;
    private float slipTimeLeft;
    private float airTimeLeft;
    private float airDuration;
    private float currentGrip;
    private readonly List<SurfaceZone> zones = new List<SurfaceZone>();

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        currentGrip = grip;
    }

    public void SetInput(float throttle, float steering, bool handbrake, bool nitro)
    {
        throttleInput = Mathf.Clamp(throttle, -1f, 1f);
        steeringInput = Mathf.Clamp(steering, -1f, 1f);
        handbrakeInput = handbrake;
        nitroInput = nitro;
    }

    void FixedUpdate()
    {
        RaceState state = raceManager.State;

        // Na contagem regressiva, todos os carros esperam parados.
        if (state == RaceState.Countdown)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            return;
        }

        // Quem terminou ou perdeu deixa de acelerar e vai parando.
        bool canDrive = raceManager.CanDrive(raceCar);
        if (!canDrive)
            SetInput(0f, 0f, false, false);

        float dt = Time.fixedDeltaTime;
        if (airTimeLeft > 0f)
            airTimeLeft -= dt;
        if (slipTimeLeft > 0f)
            slipTimeLeft -= dt;

        Surface = CurrentSurface();

        Vector2 forward = rb.GetRelativeVector(Vector2.up);
        Vector2 right = rb.GetRelativeVector(Vector2.right);
        Vector2 velocity = rb.linearVelocity;

        // A velocidade real vem da física: depois de uma batida ela já
        // chega reduzida, e o carro não sai disparado ao se soltar da parede.
        float forwardSpeed = Vector2.Dot(velocity, forward);
        float lateralSpeed = Vector2.Dot(velocity, right);

        UpdateNitro(dt, canDrive);
        forwardSpeed = UpdateForwardSpeed(forwardSpeed, dt, canDrive);
        lateralSpeed = UpdateLateralSpeed(lateralSpeed, dt);

        rb.linearVelocity = forward * forwardSpeed + right * lateralSpeed;
        ApplySteering(forwardSpeed);

        ForwardSpeed = forwardSpeed;
        LateralSpeed = lateralSpeed;
        IsDrifting = Mathf.Abs(lateralSpeed) > 1.5f && Mathf.Abs(forwardSpeed) > 3f && !IsAirborne;
        IsBraking = canDrive && (handbrakeInput || (throttleInput < 0f && forwardSpeed > 0.5f));

        if (IsDrifting)
        {
            DriftTime += dt;
            Nitro = Mathf.Min(1f, Nitro + nitroPerDriftSecond * dt);
        }
    }

    float UpdateForwardSpeed(float speed, float dt, bool canDrive)
    {
        float topSpeed = maxSpeed * SurfaceInfo.SpeedFactor(Surface);
        float accel = acceleration;
        if (IsNitroActive)
        {
            topSpeed = maxSpeed * nitroSpeedBoost;
            accel += nitroAcceleration;
        }

        if (!canDrive)
            return Mathf.MoveTowards(speed, 0f, brakeForce * 0.5f * dt);

        if (handbrakeInput && throttleInput > 0f)
            speed = Mathf.MoveTowards(speed, topSpeed * 0.8f, accel * 0.5f * dt); // drift acelerando
        else if (handbrakeInput)
            speed = Mathf.MoveTowards(speed, 0f, handbrakeDeceleration * dt);
        else if (throttleInput > 0f || IsNitroActive)
            speed = speed < topSpeed ? Mathf.Min(speed + accel * dt, topSpeed) : speed;
        else if (throttleInput < 0f)
            speed -= (speed > 0.1f ? brakeForce : acceleration) * dt; // freia e depois dá ré
        else
            speed = Mathf.MoveTowards(speed, 0f, coastDeceleration * dt);

        // Acima do limite (por exemplo, ao entrar na grama), o carro perde velocidade.
        if (speed > topSpeed)
            speed = Mathf.MoveTowards(speed, topSpeed, offRoadDeceleration * dt);

        return Mathf.Max(speed, -reverseSpeed);
    }

    // O deslizamento lateral diminui a cada passo. Com pouca aderência
    // (freio de mão, óleo, terra), ele dura mais: é o drift.
    float UpdateLateralSpeed(float lateral, float dt)
    {
        float targetGrip = grip * SurfaceInfo.GripFactor(Surface);
        if (handbrakeInput)
            targetGrip = driftGrip;
        if (slipTimeLeft > 0f)
            targetGrip = oilGrip;

        // A aderência volta aos poucos, para o fim do drift ser suave.
        currentGrip = targetGrip < currentGrip ? targetGrip : Mathf.MoveTowards(currentGrip, targetGrip, 30f * dt);
        return lateral * Mathf.Exp(-currentGrip * dt);
    }

    void ApplySteering(float forwardSpeed)
    {
        // Virar só funciona em movimento: o giro cresce com a velocidade
        // e inverte na ré, como em um carro real.
        float turnFactor = Mathf.Clamp(forwardSpeed / fullTurnAtSpeed, -1f, 1f);
        float turn = turnSpeed * (handbrakeInput ? driftTurnBoost : 1f);
        if (IsAirborne)
            turn *= 0.3f;
        rb.angularVelocity = steeringInput * turn * turnFactor;
    }

    void UpdateNitro(float dt, bool canDrive)
    {
        bool wantsNitro = canDrive && nitroInput && throttleInput >= 0f;
        bool starting = wantsNitro && !IsNitroActive && Nitro > 0.1f;

        if (starting)
            NitroUses++;

        IsNitroActive = wantsNitro && (IsNitroActive || starting) && Nitro > 0f;

        if (IsNitroActive)
            Nitro = Mathf.Max(0f, Nitro - dt / nitroCapacity);
        else if (canDrive)
            Nitro = Mathf.Min(1f, Nitro + nitroPassiveRecharge * dt);
    }

    SurfaceType CurrentSurface()
    {
        if (IsAirborne)
            return SurfaceType.Asfalto;
        if (zones.Count > 0)
            return zones[zones.Count - 1].surface;
        return raceCar != null ? raceCar.Surface : SurfaceType.Asfalto;
    }

    // Chamado pela mancha de óleo.
    public void Slip(float duration)
    {
        if (!IsAirborne)
            slipTimeLeft = Mathf.Max(slipTimeLeft, duration);
    }

    // Chamado pela rampa: o carro voa por alguns instantes.
    public void Jump(float duration)
    {
        airDuration = duration;
        airTimeLeft = duration;
    }

    public void AddNitro(float amount)
    {
        Nitro = Mathf.Clamp01(Nitro + amount);
    }

    // Usado quando um bot fica preso: volta para a pista.
    public void PlaceAt(Vector2 position, float angle)
    {
        rb.position = position;
        rb.rotation = angle;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        transform.SetPositionAndRotation(position, Quaternion.Euler(0f, 0f, angle));
    }

    public void EnterZone(SurfaceZone zone)
    {
        if (!zones.Contains(zone))
            zones.Add(zone);
    }

    public void ExitZone(SurfaceZone zone)
    {
        zones.Remove(zone);
    }
}
