using UnityEngine;
using UnityEngine.SceneManagement;

public enum RaceState { Waiting, Racing, Paused, Won, Lost }
public enum LossReason { None, TimeUp, CarBroken }

// Único lugar que decide o estado da corrida.
// Os outros scripts avisam o que aconteceu e consultam o estado por aqui.
public class RaceManager : MonoBehaviour
{
    [Tooltip("Voltas completas necessárias para vencer.")]
    public int totalLaps = 5;

    [Header("Estado atual (só para consulta durante o Play)")]
    [SerializeField] private RaceState state = RaceState.Waiting;
    [SerializeField] private LossReason lossReason = LossReason.None;
    [SerializeField] private int completedLaps;
    [SerializeField] private bool checkpointPassed;

    public RaceState State => state;
    public LossReason Reason => lossReason;
    public int CompletedLaps => completedLaps;
    public bool IsRacing => state == RaceState.Racing;

    void Start()
    {
        StartRace();
    }

    public void StartRace()
    {
        if (state != RaceState.Waiting)
            return;

        SetPaused(false);
        state = RaceState.Racing;
    }

    // Time.timeScale = 0 para o cronômetro e a física de uma vez só.
    // AudioListener.pause faz o mesmo com os sons.
    // Depois do resultado, esta função não faz nada.
    public void TogglePause()
    {
        if (state == RaceState.Racing)
        {
            state = RaceState.Paused;
            SetPaused(true);
        }
        else if (state == RaceState.Paused)
        {
            state = RaceState.Racing;
            SetPaused(false);
        }
    }

    // Carregar a cena de novo restaura tempo, danos e voltas.
    public void RestartRace()
    {
        SetPaused(false);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void BackToMenu()
    {
        SetPaused(false);
        SceneManager.LoadScene("Menu");
    }

    // Tempo e som valem para o jogo inteiro, por isso precisam voltar
    // ao normal antes de trocar de cena.
    void SetPaused(bool paused)
    {
        Time.timeScale = paused ? 0f : 1f;
        AudioListener.pause = paused;
    }

    // Chamado pelo ponto de controle, na metade do percurso.
    public void PassCheckpoint()
    {
        if (IsRacing)
            checkpointPassed = true;
    }

    // Chamado pela linha de chegada. A volta só conta depois do ponto de
    // controle: assim a travessia logo após a largada não vale, nem dar ré
    // e cruzar a linha de novo.
    public void CrossFinishLine()
    {
        if (!IsRacing || !checkpointPassed)
            return;

        checkpointPassed = false;
        completedLaps++;

        if (completedLaps >= totalLaps)
            state = RaceState.Won;
    }

    public void TimeUp()
    {
        Lose(LossReason.TimeUp);
    }

    public void CarBroken()
    {
        Lose(LossReason.CarBroken);
    }

    void Lose(LossReason reason)
    {
        // Depois que a corrida acabou, nada muda o resultado.
        if (!IsRacing)
            return;

        lossReason = reason;
        state = RaceState.Lost;
    }
}
