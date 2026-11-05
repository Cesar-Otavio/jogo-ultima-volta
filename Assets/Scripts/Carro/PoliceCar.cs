using UnityEngine;

// Viatura da polícia: patrulha a pista devagar. Quando o jogador passa
// perto em alta velocidade, liga a sirene e tenta fechar o caminho dele
// por alguns segundos. Depois desiste e volta a patrulhar.
[RequireComponent(typeof(BotDriver))]
public class PoliceCar : MonoBehaviour
{
    public RaceCar player;
    [Tooltip("Animator com o parâmetro bool \"Ligada\".")]
    public Animator siren;

    [Tooltip("Distância em que a viatura percebe o jogador.")]
    public float detectDistance = 14f;
    [Tooltip("Velocidade mínima do jogador para chamar a atenção.")]
    public float minPlayerSpeed = 6f;
    public float chaseTime = 4f;
    public float cooldown = 6f;

    [Tooltip("Velocidade máxima patrulhando e perseguindo.")]
    public float patrolSpeed = 6.5f;
    public float chaseSpeed = 11f;

    [Header("Estado (só para consulta durante o Play)")]
    [SerializeField] private float chaseLeft;
    [SerializeField] private float cooldownLeft;

    private BotDriver bot;
    private CarController car;
    private Rigidbody2D playerBody;

    public bool IsChasing => chaseLeft > 0f;

    void Awake()
    {
        bot = GetComponent<BotDriver>();
        car = GetComponent<CarController>();
    }

    void Start()
    {
        playerBody = player.GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (chaseLeft > 0f)
        {
            chaseLeft -= Time.deltaTime;
            if (chaseLeft <= 0f)
            {
                bot.chaseTarget = null;
                cooldownLeft = cooldown;
            }
        }
        else if (cooldownLeft > 0f)
        {
            cooldownLeft -= Time.deltaTime;
        }
        else if (player.raceManager.IsRacing && SeesPlayer())
        {
            chaseLeft = chaseTime;
            bot.chaseTarget = playerBody;
        }

        siren.SetBool("Ligada", IsChasing);

        // Patrulhando, anda devagar (os corredores a alcançam); perseguindo, acelera.
        car.maxSpeed = IsChasing ? chaseSpeed : patrolSpeed;
    }

    bool SeesPlayer()
    {
        float distance = Vector2.Distance(playerBody.position, transform.position);
        return distance < detectDistance && playerBody.linearVelocity.magnitude > minPlayerSpeed;
    }
}
