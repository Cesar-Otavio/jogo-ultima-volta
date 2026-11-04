using UnityEngine;

// Conta os danos do carro, troca o sprite e avisa quando ele quebra.
[RequireComponent(typeof(SpriteRenderer))]
public class CarDamage : MonoBehaviour
{
    public RaceManager raceManager;
    public AudioManager audioManager;

    [Tooltip("Do carro intacto ao quebrado. O limite de danos é o último sprite da lista.")]
    public Sprite[] damageSprites;

    [Tooltip("Segundos sem receber novo dano depois de uma batida.")]
    public float invulnerabilityTime = 1f;

    [Tooltip("Velocidade mínima contra uma parede para a batida contar como dano.")]
    public float minWallImpactSpeed = 3f;

    [Tooltip("Danos atuais, só para consulta durante o Play.")]
    [SerializeField] private int damageCount;

    private SpriteRenderer spriteRenderer;
    private float invulnerableUntil;

    public int DamageCount => damageCount;
    public int DamageLimit => damageSprites.Length - 1;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // OnCollisionEnter2D só roda no começo do contato:
    // encostar e continuar encostado conta como um dano só.
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!raceManager.IsRacing)
            return;

        // Conta só a velocidade na direção do que foi atingido:
        // bater de frente é forte, raspar de lado é fraco.
        Vector2 hitDirection = collision.GetContact(0).normal;
        float impactSpeed = Mathf.Abs(Vector2.Dot(collision.relativeVelocity, hitDirection));

        audioManager.PlayCollision(impactSpeed);

        if (Time.time < invulnerableUntil)
            return;

        // Obstáculos sempre causam dano. Paredes, só em batidas fortes.
        bool isObstacle = collision.collider.GetComponent<Obstacle>() != null;
        if (isObstacle || impactSpeed >= minWallImpactSpeed)
            TakeDamage();
    }

    void TakeDamage()
    {
        damageCount++;
        invulnerableUntil = Time.time + invulnerabilityTime;
        spriteRenderer.sprite = damageSprites[damageCount];

        if (damageCount >= DamageLimit)
            raceManager.CarBroken();
    }
}
