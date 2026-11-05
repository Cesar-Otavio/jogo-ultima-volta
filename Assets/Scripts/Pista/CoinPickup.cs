using UnityEngine;

// Moeda na pista. Só o jogador pega. Gira no lugar para chamar atenção.
public class CoinPickup : MonoBehaviour
{
    public RaceManager raceManager;
    public AudioManager audioManager;
    [Tooltip("Voltas por segundo do giro.")]
    public float spinSpeed = 1.2f;

    private Vector3 baseScale;

    void Awake()
    {
        baseScale = transform.localScale;
    }

    // Encolher e esticar a largura dá a impressão de uma moeda girando.
    void Update()
    {
        float width = Mathf.Abs(Mathf.Cos(Time.time * spinSpeed * Mathf.PI));
        transform.localScale = new Vector3(baseScale.x * Mathf.Max(0.15f, width), baseScale.y, baseScale.z);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        RaceCar car = other.GetComponent<RaceCar>();
        if (car == null || !car.isPlayer || !raceManager.IsRacing)
            return;

        raceManager.AddCoin();
        audioManager.PlayCoin();
        gameObject.SetActive(false);
    }
}
