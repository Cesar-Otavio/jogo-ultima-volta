using UnityEngine;

// Cronômetro regressivo: só conta enquanto a corrida está em andamento.
// Também guarda o tempo de prova, usado no recorde da pista.
public class RaceTimer : MonoBehaviour
{
    public RaceManager raceManager;

    [Tooltip("Tempo restante, só para consulta durante o Play.")]
    [SerializeField] private float timeLeft;
    [SerializeField] private float elapsed;

    public float TimeLeft => timeLeft;
    public float Elapsed => elapsed;

    void Start()
    {
        timeLeft = raceManager.track.timeLimit;
    }

    void Update()
    {
        if (!raceManager.IsRacing)
            return;

        timeLeft = Mathf.Max(0f, timeLeft - Time.deltaTime);
        elapsed += Time.deltaTime;

        if (timeLeft <= 0f)
            raceManager.TimeUp();
    }

    // Texto no formato 03:00. Arredonda para cima para mostrar 03:00 no
    // início e 00:00 somente quando o tempo acabar de verdade.
    public string FormattedTime()
    {
        return Format(Mathf.CeilToInt(timeLeft));
    }

    public static string Format(float seconds)
    {
        int total = Mathf.Max(0, Mathf.FloorToInt(seconds));
        return (total / 60).ToString("00") + ":" + (total % 60).ToString("00");
    }

    // Com décimos, para tempos de prova e recordes: 01:23.4
    public static string FormatPrecise(float seconds)
    {
        int tenths = Mathf.Max(0, Mathf.FloorToInt(seconds * 10f));
        return Format(tenths / 10) + "." + (tenths % 10);
    }
}
