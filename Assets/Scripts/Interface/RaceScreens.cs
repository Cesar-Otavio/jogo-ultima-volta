using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Mostra a tela certa para o estado atual da corrida: contagem, HUD,
// pausa ou resultado. Só existe uma tela de resultado, com o texto
// trocado: assim vitória e derrota nunca aparecem juntas.
public class RaceScreens : MonoBehaviour
{
    public RaceManager raceManager;
    public RaceTimer raceTimer;

    [Header("Telas")]
    public GameObject hud;
    public GameObject countdownScreen;
    public GameObject pauseScreen;
    public GameObject resultScreen;

    [Header("Contagem")]
    public TMP_Text countdownText;
    [Tooltip("As 3 luzes do semáforo, da esquerda para a direita.")]
    public Image[] countdownLights;

    [Header("Resultado")]
    public TMP_Text resultTitle;
    public TMP_Text resultMessage;
    public TMP_Text rankingText;
    public TMP_Text rewardsText;
    public GameObject nextTrackButton;
    [Tooltip("Espera antes do resultado, para ver a chegada e os fogos.")]
    public float resultDelay = 1.5f;

    private bool resultFilled;

    static readonly Color LightOff = new Color(0.25f, 0.25f, 0.25f);
    static readonly Color LightRed = new Color(1f, 0.2f, 0.15f);
    static readonly Color LightGreen = new Color(0.3f, 1f, 0.35f);

    void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
            raceManager.TogglePause();

        bool showResult = raceManager.IsOver && Time.time >= raceManager.EndTime + resultDelay;

        hud.SetActive(!showResult);
        pauseScreen.SetActive(raceManager.State == RaceState.Paused);
        resultScreen.SetActive(showResult);
        UpdateCountdown();

        if (showResult && !resultFilled)
        {
            resultFilled = true;
            FillResult();
        }
    }

    // 3 luzes vermelhas acendem uma a uma; no "VAI!", todas ficam verdes.
    void UpdateCountdown()
    {
        float left = raceManager.CountdownLeft;
        bool counting = raceManager.State == RaceState.Countdown && left <= raceManager.countdownDuration;
        bool go = raceManager.IsRacing && Time.time - raceManager.RaceStartTime < 0.8f;

        countdownScreen.SetActive(counting || go);
        if (!counting && !go)
            return;

        int number = Mathf.CeilToInt(left);
        countdownText.text = go ? "VAI!" : number.ToString();
        countdownText.color = go ? LightGreen : Color.white;

        for (int i = 0; i < countdownLights.Length; i++)
        {
            bool lit = i < countdownLights.Length - number + 1;
            countdownLights[i].color = go ? LightGreen : (lit ? LightRed : LightOff);
        }
    }

    void FillResult()
    {
        RaceResult result = raceManager.Result;
        RaceSummary summary = result.summary;
        TrackInfo track = raceManager.track;

        if (raceManager.State == RaceState.Finished)
        {
            if (summary.carCount <= 1)
                resultTitle.text = "Corrida concluída!";
            else if (summary.position == 1)
                resultTitle.text = "Vitória! 1º lugar";
            else
                resultTitle.text = summary.position + "º lugar";

            resultMessage.text = "Tempo de prova " + RaceTimer.FormatPrecise(summary.raceTime)
                + ", com " + raceTimer.FormattedTime() + " de sobra.";
        }
        else
        {
            resultTitle.text = raceManager.Reason == LossReason.TimeUp ? "Tempo esgotado" : "Carro quebrado";
            string reason = raceManager.Reason == LossReason.TimeUp
                ? "O tempo acabou antes da chegada."
                : "O carro não aguentou as batidas.";
            resultMessage.text = reason + " Você completou " + raceManager.player.Laps
                + " de " + raceManager.TotalLaps + " voltas.";
        }

        rankingText.text = BuildRanking();
        rewardsText.text = BuildRewards(result, track);

        TrackInfo next = raceManager.catalog.NextTrack(track);
        SaveData save = SaveSystem.Load(raceManager.catalog);
        nextTrackButton.SetActive(next != null && save.IsUnlocked(next.id));
    }

    string BuildRanking()
    {
        if (raceManager.Cars.Count <= 1)
            return "";

        StringBuilder text = new StringBuilder("<b>Classificação</b>\n");
        foreach (RaceCar car in raceManager.Cars)
        {
            string name = car.isPlayer ? "<color=#FFE066>" + car.driverName + "</color>" : car.driverName;
            string time = car.Finished ? RaceTimer.FormatPrecise(car.FinishTime) : "correndo";
            text.Append(car.Position + "º<pos=12%>" + name + "<pos=68%>" + time + "\n");
        }
        return text.ToString();
    }

    string BuildRewards(RaceResult result, TrackInfo track)
    {
        StringBuilder text = new StringBuilder("<b>Recompensas</b>\n");
        if (result.positionCoins > 0)
            text.Append("Colocação: +" + result.positionCoins + "\n");
        if (result.pickupCoins > 0)
            text.Append("Moedas da pista: +" + result.pickupCoins + "\n");

        for (int i = 0; i < track.challenges.Length; i++)
        {
            string status;
            if (result.completedNow[i])
                status = "<color=#7CFC00>completo! +" + track.challenges[i].reward + "</color>";
            else if (result.completedBefore[i])
                status = "<color=#9BC4FF>já completo</color>";
            else
                status = "<color=#FF8C7A>não completo</color>";
            text.Append("Desafio: " + track.challenges[i].Description() + " (" + status + ")\n");
        }

        if (result.newRecord)
            text.Append("<color=#FFE066>Novo recorde da pista!</color>\n");
        if (result.unlockedTrack != null)
            text.Append("<color=#7CFC00>Nova pista liberada: " + result.unlockedTrack.displayName + "!</color>\n");
        else if (raceManager.State == RaceState.Finished && result.summary.position > 3)
            text.Append("Termine no pódio para liberar a próxima pista.\n");

        text.Append("<b>Total: +" + result.TotalCoins + " moedas</b>");
        return text.ToString();
    }
}
