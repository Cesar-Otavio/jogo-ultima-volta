using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Mostra o tempo, a volta atual e o estado do carro durante a corrida.
public class HudController : MonoBehaviour
{
    public RaceManager raceManager;
    public RaceTimer raceTimer;
    public CarDamage carDamage;

    public TMP_Text timeText;
    public TMP_Text lapText;

    [Tooltip("Corações cheios do HUD. Um some a cada dano.")]
    public Image[] hearts;

    void Update()
    {
        timeText.text = raceTimer.FormattedTime();

        int currentLap = Mathf.Min(raceManager.CompletedLaps + 1, raceManager.totalLaps);
        lapText.text = "Volta " + currentLap + "/" + raceManager.totalLaps;

        int heartsLeft = carDamage.DamageLimit - carDamage.DamageCount;
        for (int i = 0; i < hearts.Length; i++)
            hearts[i].enabled = i < heartsLeft;
    }
}
