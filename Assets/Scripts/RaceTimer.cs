using UnityEngine;

// Cronômetro regressivo: só conta enquanto a corrida está em andamento.
public class RaceTimer : MonoBehaviour
{
    public RaceManager raceManager;

    [Tooltip("Duração da corrida, em segundos.")]
    public float duration = 180f;

    [Tooltip("Tempo restante, só para consulta durante o Play.")]
    [SerializeField] private float timeLeft;

    public float TimeLeft => timeLeft;

    void Awake()
    {
        timeLeft = duration;
    }

    void Update()
    {
        if (!raceManager.IsRacing)
            return;

        timeLeft = Mathf.Max(0f, timeLeft - Time.deltaTime);

        if (timeLeft <= 0f)
            raceManager.TimeUp();
    }

    // Texto no formato 03:00. Arredonda para cima para mostrar 03:00 no
    // início e 00:00 somente quando o tempo acabar de verdade.
    public string FormattedTime()
    {
        int seconds = Mathf.CeilToInt(timeLeft);
        return (seconds / 60).ToString("00") + ":" + (seconds % 60).ToString("00");
    }
}
