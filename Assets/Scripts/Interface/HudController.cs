using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Mostra tempo, volta, posição, moedas, velocidade, nitro e corações.
public class HudController : MonoBehaviour
{
    public RaceManager raceManager;
    public RaceTimer raceTimer;
    public CarDamage carDamage;
    public CarController car;

    [Header("Textos")]
    public TMP_Text timeText;
    public TMP_Text lapText;
    public TMP_Text positionText;
    public TMP_Text coinsText;
    public TMP_Text speedText;
    public TMP_Text messageText;

    [Header("Barras")]
    [Tooltip("Corações cheios. Cada um é filho do seu fundo vazio.")]
    public Image[] hearts;
    public Image nitroFill;

    [Tooltip("Tempo que os avisos (volta, última volta) ficam na tela.")]
    public float messageDuration = 2.5f;

    private int lastLaps;
    private string message = "";
    private float messageUntil;

    void Update()
    {
        RaceCar player = raceManager.player;

        timeText.text = raceTimer.FormattedTime();
        timeText.color = raceTimer.TimeLeft < 20f ? new Color(1f, 0.4f, 0.35f) : Color.white;
        lapText.text = "Volta " + raceManager.PlayerLap + "/" + raceManager.TotalLaps;

        int carCount = raceManager.Cars.Count;
        positionText.text = carCount > 1 ? player.Position + "º/" + carCount : "";

        coinsText.text = raceManager.CoinsCollected.ToString();
        // 10 unidades/s aparecem como 180 km/h.
        speedText.text = Mathf.RoundToInt(Mathf.Abs(car.ForwardSpeed) * 18f) + " km/h";

        UpdateHearts();

        nitroFill.fillAmount = car.Nitro;
        nitroFill.color = car.IsNitroActive ? new Color(0.6f, 0.95f, 1f) : new Color(0.2f, 0.65f, 1f);

        UpdateMessage(player);
    }

    // Mostra só os corações que o carro tem; os perdidos ficam vazios.
    void UpdateHearts()
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            hearts[i].transform.parent.gameObject.SetActive(i < carDamage.DamageLimit);
            hearts[i].enabled = i < carDamage.HeartsLeft;
        }
    }

    void UpdateMessage(RaceCar player)
    {
        if (player.Laps != lastLaps)
        {
            lastLaps = player.Laps;
            if (raceManager.IsRacing)
                Show(raceManager.IsLastLap ? "ÚLTIMA VOLTA!" : "Volta " + raceManager.PlayerLap);
        }

        if (player.WrongWay && raceManager.IsRacing)
            messageText.text = "CONTRAMÃO!";
        else
            messageText.text = Time.time < messageUntil ? message : "";
    }

    void Show(string text)
    {
        message = text;
        messageUntil = Time.time + messageDuration;
    }
}
