using UnityEngine;

// Piloto controlado pelo computador. Segue a linha central da pista
// (TrackPath) com um deslocamento lateral próprio, freia antes das curvas,
// usa nitro nas retas e se recupera quando fica preso.
[RequireComponent(typeof(CarController))]
public class BotDriver : MonoBehaviour
{
    public TrackPath track;
    public RaceCar raceCar;

    [Tooltip("Habilidade de 0 (fácil) a 1 (difícil): curvas mais rápidas e mais nitro.")]
    [Range(0f, 1f)] public float skill = 0.5f;

    [Tooltip("Faixa preferida: distância lateral da linha central.")]
    public float laneOffset;

    [Tooltip("Carro do jogador, para ajustar o ritmo e manter a corrida disputada.")]
    public RaceCar player;

    [Tooltip("Quando preenchido, o bot vai atrás deste alvo em vez de seguir a pista (viatura).")]
    public Rigidbody2D chaseTarget;

    private CarController car;
    private Rigidbody2D rb;
    private float stuckTime;
    private float reverseTimeLeft;
    private float laneShift;

    void Awake()
    {
        car = GetComponent<CarController>();
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        if (!car.raceManager.CanDrive(raceCar))
        {
            car.SetInput(0f, 0f, false, false);
            return;
        }

        if (HandleStuck())
            return;

        float speed = car.ForwardSpeed;
        float distance = raceCar.LapDistance;

        // Mira um ponto à frente na pista: quanto mais rápido, mais longe.
        float lookAhead = 4f + Mathf.Max(0f, speed) * 0.55f;
        Vector2 direction = track.DirectionAt(distance + lookAhead);
        Vector2 left = new Vector2(-direction.y, direction.x);
        Vector2 target = track.PositionAt(distance + lookAhead) + left * (laneOffset + laneShift);

        // Perseguição: mira um pouco à frente de onde o alvo vai estar.
        if (chaseTarget != null)
        {
            target = chaseTarget.position + chaseTarget.linearVelocity * 0.4f;
            float chaseAngle = Vector2.SignedAngle(rb.GetRelativeVector(Vector2.up), target - rb.position);
            car.SetInput(1f, Mathf.Clamp(chaseAngle / 30f, -1f, 1f), false, false);
            return;
        }

        Vector2 forward = rb.GetRelativeVector(Vector2.up);
        float angle = Vector2.SignedAngle(forward, target - rb.position);
        float steering = Mathf.Clamp(angle / 30f, -1f, 1f);

        // Velocidade segura: quanto mais a pista vira à frente, mais devagar.
        float turnAhead = track.TurnBetween(distance + 2f, distance + 8f + Mathf.Max(0f, speed) * 1.3f);
        float cornerSpeed = Mathf.Lerp(4.5f, 7.5f, skill);
        float safeSpeed = Mathf.Lerp(car.maxSpeed, cornerSpeed, Mathf.Clamp01(turnAhead / 100f));
        safeSpeed *= PaceFactor();

        float throttle = speed < safeSpeed ? 1f : (speed > safeSpeed + 1.2f ? -1f : 0f);
        bool nitro = skill > 0.4f && turnAhead < 12f && car.Nitro > 0.5f && speed > car.maxSpeed * 0.7f;

        AvoidCarAhead();
        car.SetInput(throttle, steering, false, nitro);
    }

    // Se um carro está logo à frente na mesma faixa, desvia para o lado.
    void AvoidCarAhead()
    {
        Vector2 forward = rb.GetRelativeVector(Vector2.up);
        RaycastHit2D[] hits = Physics2D.CircleCastAll(rb.position + forward * 2f, 0.8f, forward, 3f);
        bool blocked = false;
        foreach (RaycastHit2D hit in hits)
            if (hit.rigidbody != null && hit.rigidbody != rb && hit.collider.GetComponent<RaceCar>() != null)
                blocked = true;

        float target = blocked ? (laneOffset >= 0f ? -2f : 2f) : 0f;
        laneShift = Mathf.MoveTowards(laneShift, target, Time.fixedDeltaTime * 2f);
    }

    // Se o bot está muito à frente do jogador, alivia um pouco; se está muito
    // atrás, força um pouco. Isso mantém os carros perto uns dos outros.
    float PaceFactor()
    {
        if (player == null)
            return 1f;
        float gap = raceCar.Progress - player.Progress;
        return Mathf.Clamp(1f - gap / 400f, 0.88f, 1.08f);
    }

    // Parado contra algo por muito tempo: dá ré. Preso de vez: volta para a pista.
    bool HandleStuck()
    {
        if (reverseTimeLeft > 0f)
        {
            reverseTimeLeft -= Time.fixedDeltaTime;
            float side = Vector2.SignedAngle(rb.GetRelativeVector(Vector2.up), track.DirectionAt(raceCar.LapDistance));
            car.SetInput(-1f, side > 0f ? -1f : 1f, false, false);
            return true;
        }

        if (Mathf.Abs(car.ForwardSpeed) < 0.8f)
            stuckTime += Time.fixedDeltaTime;
        else
            stuckTime = 0f;

        if (stuckTime > 5f || Mathf.Abs(raceCar.CenterOffset) > track.roadHalfWidth + 12f)
        {
            Respawn();
            return true;
        }

        if (stuckTime > 1.2f)
        {
            reverseTimeLeft = 1f;
            stuckTime = 0f;
            return true;
        }
        return false;
    }

    void Respawn()
    {
        float distance = raceCar.LapDistance;
        Vector2 direction = track.DirectionAt(distance);
        float angle = Vector2.SignedAngle(Vector2.up, direction);
        car.PlaceAt(track.PositionAt(distance), angle);
        stuckTime = 0f;
    }
}
