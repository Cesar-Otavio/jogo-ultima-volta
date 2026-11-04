using UnityEngine;

// Conta os danos do carro, troca o sprite e avisa quando ele quebra.
[RequireComponent(typeof(SpriteRenderer))]
public class CarDamage : MonoBehaviour
{
    public RaceManager raceManager;

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
        if (!raceManager.IsRacing || Time.time < invulnerableUntil)
            return;

        if (IsValidHit(collision))
            TakeDamage();
    }

    // Obstáculos sempre causam dano. Nas paredes, conta só a velocidade na
    // direção da parede: bater de frente causa dano, raspar de lado não.
    bool IsValidHit(Collision2D collision)
    {
        if (collision.collider.GetComponent<Obstacle>() != null)
            return true;

        Vector2 wallDirection = collision.GetContact(0).normal;
        float impactSpeed = Mathf.Abs(Vector2.Dot(collision.relativeVelocity, wallDirection));
        return impactSpeed >= minWallImpactSpeed;
    }

    void TakeDamage()
    {
        damageCount++;
        invulnerableUntil = Time.time + invulnerabilityTime;
        spriteRenderer.sprite = damageSprites[damageCount];
        Debug.Log("Dano " + damageCount + " de " + DamageLimit + ".");

        if (damageCount >= DamageLimit)
            raceManager.CarBroken();
    }
}
