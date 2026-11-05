using UnityEngine;

// Conta os danos do carro do jogador, troca o sprite e avisa quando ele quebra.
public class CarDamage : MonoBehaviour
{
    public RaceManager raceManager;
    public AudioManager audioManager;
    public CarEffects effects;
    public CameraFollow cameraFollow;

    [Tooltip("Sprite da carroceria, que muda a cada dano.")]
    public SpriteRenderer body;

    [Tooltip("Do carro intacto ao quebrado.")]
    public Sprite[] damageSprites;

    [Tooltip("Quantos danos o carro aguenta. A blindagem da oficina aumenta este número.")]
    public int damageLimit = 4;

    [Tooltip("Segundos sem receber novo dano depois de uma batida.")]
    public float invulnerabilityTime = 1f;

    [Tooltip("Velocidade mínima contra uma parede para a batida contar como dano.")]
    public float minWallImpactSpeed = 3f;

    [Tooltip("Danos atuais, só para consulta durante o Play.")]
    [SerializeField] private int damageCount;

    private float invulnerableUntil;

    public int DamageCount => damageCount;
    public int DamageLimit => damageLimit;
    public int HeartsLeft => damageLimit - damageCount;
    public bool IsInvulnerable => Time.time < invulnerableUntil;

    // OnCollisionEnter2D só roda no começo do contato:
    // encostar e continuar encostado conta como um dano só.
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!raceManager.IsRacing)
            return;

        // Conta só a velocidade na direção do que foi atingido:
        // bater de frente é forte, raspar de lado é fraco.
        ContactPoint2D contact = collision.GetContact(0);
        float impactSpeed = Mathf.Abs(Vector2.Dot(collision.relativeVelocity, contact.normal));

        audioManager.PlayCollision(impactSpeed);
        if (effects != null)
            effects.Crash(contact.point, impactSpeed);
        if (cameraFollow != null)
            cameraFollow.Shake(impactSpeed / 10f);

        if (IsInvulnerable)
            return;

        // Obstáculos sempre causam dano. Paredes e outros carros, só em batidas fortes.
        bool isObstacle = collision.collider.GetComponent<Obstacle>() != null;
        if (isObstacle || impactSpeed >= minWallImpactSpeed)
            TakeDamage();
    }

    void TakeDamage()
    {
        damageCount++;
        invulnerableUntil = Time.time + invulnerabilityTime;
        UpdateSprite();

        if (damageCount >= damageLimit)
            raceManager.CarBroken();
    }

    // Chamado pela chave de conserto da pista.
    public bool Repair()
    {
        if (damageCount == 0)
            return false;

        damageCount--;
        UpdateSprite();
        return true;
    }

    // Com mais corações (blindagem), os 5 sprites são distribuídos pelos danos.
    void UpdateSprite()
    {
        int last = damageSprites.Length - 1;
        int index = damageCount >= damageLimit
            ? last
            : Mathf.Min(last - 1, Mathf.CeilToInt((float)damageCount / damageLimit * last));
        body.sprite = damageSprites[index];
    }
}
