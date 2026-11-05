using UnityEngine;

// Item de conserto: devolve um coração ao jogador. Só some se consertar algo.
public class RepairPickup : MonoBehaviour
{
    public RaceManager raceManager;
    public AudioManager audioManager;

    private Vector3 baseScale;

    void Awake()
    {
        baseScale = transform.localScale;
    }

    // Pulsa devagar para ser notado de longe.
    void Update()
    {
        transform.localScale = baseScale * (1f + 0.12f * Mathf.Sin(Time.time * 4f));
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        RaceCar car = other.GetComponent<RaceCar>();
        if (car == null || !car.isPlayer || !raceManager.IsRacing)
            return;

        if (raceManager.playerDamage.Repair())
        {
            audioManager.PlayRepair();
            gameObject.SetActive(false);
        }
    }
}
