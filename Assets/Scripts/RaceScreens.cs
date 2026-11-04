using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

// Mostra a tela certa para o estado atual da corrida.
// Só existe uma tela de resultado, com o texto trocado: assim vitória e
// derrota nunca aparecem juntas.
public class RaceScreens : MonoBehaviour
{
    public RaceManager raceManager;
    public RaceTimer raceTimer;

    [Header("Telas")]
    public GameObject hud;
    public GameObject pauseScreen;
    public GameObject resultScreen;

    [Header("Textos do resultado")]
    public TMP_Text resultTitle;
    public TMP_Text resultMessage;

    void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
            raceManager.TogglePause();

        bool finished = raceManager.State == RaceState.Won || raceManager.State == RaceState.Lost;

        hud.SetActive(!finished);
        pauseScreen.SetActive(raceManager.State == RaceState.Paused);
        resultScreen.SetActive(finished);

        if (finished)
            ShowResult();
    }

    void ShowResult()
    {
        string laps = raceManager.CompletedLaps + " de " + raceManager.totalLaps + " voltas";

        if (raceManager.State == RaceState.Won)
        {
            resultTitle.text = "Vitória!";
            resultMessage.text = "Você completou as " + raceManager.totalLaps + " voltas com "
                + raceTimer.FormattedTime() + " de sobra.";
        }
        else if (raceManager.Reason == LossReason.TimeUp)
        {
            resultTitle.text = "Tempo esgotado";
            resultMessage.text = "O tempo acabou antes da chegada. Você completou " + laps + ".";
        }
        else
        {
            resultTitle.text = "Carro quebrado";
            resultMessage.text = "O carro não aguentou as batidas. Você completou " + laps + ".";
        }
    }
}
