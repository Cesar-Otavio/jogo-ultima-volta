using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum RaceState { Countdown, Racing, Paused, Finished, Lost }
public enum LossReason { None, TimeUp, CarBroken }

// Único lugar que decide o estado da corrida.
// Os outros scripts avisam o que aconteceu e consultam o estado por aqui.
public class RaceManager : MonoBehaviour
{
    public GameCatalog catalog;
    public TrackInfo track;
    public TrackPath path;
    public RaceTimer timer;

    [Header("Jogador")]
    public RaceCar player;
    public CarController playerCar;
    public CarDamage playerDamage;

    [Header("Largada")]
    [Tooltip("Duração da contagem 3, 2, 1.")]
    public float countdownDuration = 3f;
    [Tooltip("Espera antes do 3, para a cena terminar de carregar.")]
    public float countdownDelay = 0.6f;

    [Header("Estado atual (só para consulta durante o Play)")]
    [SerializeField] private RaceState state = RaceState.Countdown;
    [SerializeField] private LossReason lossReason = LossReason.None;
    [SerializeField] private int coinsCollected;

    private readonly List<RaceCar> cars = new List<RaceCar>();
    private readonly List<RaceCar> finishOrder = new List<RaceCar>();
    private float countdownLeft;

    public RaceState State => state;
    public LossReason Reason => lossReason;
    public bool IsRacing => state == RaceState.Racing;
    public bool IsOver => state == RaceState.Finished || state == RaceState.Lost;
    public int TotalLaps => track.laps;
    public int PlayerLap => Mathf.Clamp(player.Laps + 1, 1, TotalLaps);
    public bool IsLastLap => player.Laps == TotalLaps - 1;
    public float CountdownLeft => countdownLeft;
    public float RaceStartTime { get; private set; } = -1f;
    public float EndTime { get; private set; } = -1f;
    public int CoinsCollected => coinsCollected;
    public IReadOnlyList<RaceCar> Cars => cars;
    public RaceResult Result { get; private set; }

    void Awake()
    {
        foreach (RaceCar car in FindObjectsByType<RaceCar>())
            if (car.countsInRace)
                cars.Add(car);
        countdownLeft = countdownDelay + countdownDuration;
        SetPaused(false);
    }

    void Update()
    {
        if (state == RaceState.Countdown)
        {
            countdownLeft -= Time.deltaTime;
            if (countdownLeft <= 0f)
            {
                state = RaceState.Racing;
                RaceStartTime = Time.time;
            }
        }

        UpdatePositions();
    }

    // Quem pode acelerar agora. Depois que o jogador termina, os bots
    // continuam até a chegada.
    public bool CanDrive(RaceCar car)
    {
        if (car == null || car.Finished)
            return false;
        if (car.isPlayer)
            return state == RaceState.Racing;
        return state == RaceState.Racing || IsOver;
    }

    // Time.timeScale = 0 para o cronômetro e a física de uma vez só.
    // AudioListener.pause faz o mesmo com os sons.
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

    // Carregar a cena de novo restaura tempo, danos, voltas e itens da pista.
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

    public void GoToNextTrack()
    {
        TrackInfo next = catalog.NextTrack(track);
        if (next == null)
        {
            BackToMenu();
            return;
        }

        SaveData save = SaveSystem.Load(catalog);
        save.selectedTrack = next.id;
        SaveSystem.Save(save);
        SetPaused(false);
        SceneManager.LoadScene(next.sceneName);
    }

    void SetPaused(bool paused)
    {
        Time.timeScale = paused ? 0f : 1f;
        AudioListener.pause = paused;
    }

    // Chamado pelo RaceCar sempre que um carro completa uma volta.
    public void LapCompleted(RaceCar car)
    {
        if (car.Laps < TotalLaps || car.Finished)
            return;
        if (car.isPlayer && state != RaceState.Racing)
            return;

        // O relógio da prova continua depois que o jogador termina, para os bots.
        car.MarkFinished(Time.time - RaceStartTime);
        finishOrder.Add(car);

        if (car.isPlayer)
        {
            UpdatePositions();
            state = RaceState.Finished;
            EndRace(true);
        }
    }

    // Quem terminou fica na ordem de chegada; os outros, pelo progresso.
    void UpdatePositions()
    {
        cars.Sort(CompareCars);
        for (int i = 0; i < cars.Count; i++)
            cars[i].Position = i + 1;
    }

    int CompareCars(RaceCar a, RaceCar b)
    {
        if (a.Finished && b.Finished)
            return finishOrder.IndexOf(a).CompareTo(finishOrder.IndexOf(b));
        if (a.Finished != b.Finished)
            return a.Finished ? -1 : 1;
        return b.Progress.CompareTo(a.Progress);
    }

    // Conta quantas moedas o jogador pegou. Cada uma vale RaceRewards.CoinValue.
    public void AddCoin()
    {
        if (IsRacing)
            coinsCollected++;
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
        EndRace(false);
    }

    // Calcula moedas, desafios e desbloqueios, e grava o progresso.
    void EndRace(bool finished)
    {
        EndTime = Time.time;

        RaceSummary summary = new RaceSummary
        {
            finished = finished,
            position = player.Position,
            carCount = cars.Count,
            raceTime = timer.Elapsed,
            timeLeft = timer.TimeLeft,
            damage = playerDamage.DamageCount,
            coins = coinsCollected,
            driftTime = playerCar.DriftTime,
            nitroUses = playerCar.NitroUses
        };

        Result = RaceRewards.Apply(catalog, track, summary);
    }
}
