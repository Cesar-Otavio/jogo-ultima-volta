using UnityEngine;
using UnityEngine.InputSystem;

// Movimento do carro do jogador.
// A variável speed guarda a velocidade na direção da frente do carro.
// A física (Rigidbody2D) resolve as batidas nas paredes.
[RequireComponent(typeof(Rigidbody2D))]
public class CarController : MonoBehaviour
{
    [Header("Movimento (unidades/s e unidades/s²)")]
    public float acceleration = 8f;
    public float maxSpeed = 8f;
    public float reverseSpeed = 3f;
    public float brakeForce = 12f;
    [Tooltip("Desaceleração quando o jogador solta o acelerador.")]
    public float coastDeceleration = 2f;

    [Header("Direção")]
    [Tooltip("Giro máximo, em graus por segundo.")]
    public float turnSpeed = 180f;
    [Tooltip("Velocidade a partir da qual o giro chega ao máximo.")]
    public float fullTurnAtSpeed = 2f;

    [Header("Corrida")]
    public RaceManager raceManager;

    private Rigidbody2D rb;
    private float speed;
    private float throttleInput;
    private float steeringInput;
    private bool brakeInput;
    private bool hitSomething;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // O teclado é lido a cada quadro; a física roda no FixedUpdate.
    void Update()
    {
        ReadInput();
    }

    void FixedUpdate()
    {
        // Fora da corrida (antes de começar ou depois do resultado), o carro fica travado.
        if (!raceManager.IsRacing)
        {
            StopCar();
            return;
        }

        LoseSpeedOnContact();
        UpdateSpeed();
        ApplyMovement();
    }

    // Só colisões físicas passam por aqui. Triggers, como a linha de chegada, não.
    void OnCollisionEnter2D(Collision2D collision)
    {
        hitSomething = true;
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        hitSomething = true;
    }

    void StopCar()
    {
        speed = 0f;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }

    void ReadInput()
    {
        throttleInput = 0f;
        steeringInput = 0f;
        brakeInput = false;

        // Sem teclado conectado, Keyboard.current é nulo.
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
            return;

        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
            throttleInput += 1f;
        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
            throttleInput -= 1f;

        // Na Unity, rotação positiva é para a esquerda (sentido anti-horário).
        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
            steeringInput += 1f;
        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
            steeringInput -= 1f;

        brakeInput = keyboard.spaceKey.isPressed;
    }

    // Depois de uma batida, a física já reduziu a velocidade real do carro.
    // Copiar esse valor evita o erro do projeto antigo: segurar W contra a
    // parede e sair em velocidade máxima ao se soltar.
    void LoseSpeedOnContact()
    {
        if (!hitSomething)
            return;

        hitSomething = false;

        float realSpeed = Vector2.Dot(rb.linearVelocity, Forward());
        if (Mathf.Abs(realSpeed) < Mathf.Abs(speed))
            speed = realSpeed;
    }

    void UpdateSpeed()
    {
        float dt = Time.fixedDeltaTime;

        if (brakeInput)
            speed = Mathf.MoveTowards(speed, 0f, brakeForce * dt);
        else if (throttleInput > 0f)
            speed += acceleration * dt;
        else if (throttleInput < 0f)
            speed -= (speed > 0.1f ? brakeForce : acceleration) * dt; // freia e depois dá ré
        else
            speed = Mathf.MoveTowards(speed, 0f, coastDeceleration * dt);

        speed = Mathf.Clamp(speed, -reverseSpeed, maxSpeed);
    }

    void ApplyMovement()
    {
        // Virar só funciona em movimento: o giro cresce com a velocidade
        // e inverte na ré, como em um carro real.
        float turnFactor = Mathf.Clamp(speed / fullTurnAtSpeed, -1f, 1f);
        rb.angularVelocity = steeringInput * turnSpeed * turnFactor;

        rb.linearVelocity = Forward() * speed;
    }

    // Frente do carro segundo a física. O Transform não serve aqui porque,
    // com Interpolate ligado, ele guarda a posição suavizada para a tela.
    Vector2 Forward()
    {
        return rb.GetRelativeVector(Vector2.up);
    }
}
