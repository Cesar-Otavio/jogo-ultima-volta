using UnityEngine;

// Acompanha um carro na pista: em que ponto ele está, quantas voltas
// completou e em que piso anda. Todo carro da corrida tem este componente.
public class RaceCar : MonoBehaviour
{
    public string driverName = "Você";
    public bool isPlayer;
    [Tooltip("Desligado na viatura da polícia: ela anda na pista, mas não disputa a corrida.")]
    public bool countsInRace = true;
    [Tooltip("Cor do carro no minimapa e na tabela de resultado.")]
    public Color mapColor = Color.yellow;

    public RaceManager raceManager;
    public TrackPath track;

    [Header("Estado (só para consulta durante o Play)")]
    [SerializeField] private int laps;
    [SerializeField] private int nextSector = 1;
    [SerializeField] private float lapDistance;
    [SerializeField] private int position = 1;
    [SerializeField] private bool finished;

    private int index;
    private float wrongWayTime;
    private Rigidbody2D rb;

    public int Laps => laps;
    public float LapDistance => lapDistance;
    public int Position { get => position; set => position = value; }
    public bool Finished => finished;
    public float FinishTime { get; private set; }
    public int PathIndex => index;
    public SurfaceType Surface { get; private set; }
    public float CenterOffset { get; private set; }
    public bool WrongWay => wrongWayTime > 1f;

    // Progresso total, usado para ordenar as posições. Antes de cruzar a
    // largada pela primeira vez, o carro ainda está na "volta zero".
    public float Progress
    {
        get
        {
            float progress = laps * track.Length + lapDistance;
            if (nextSector <= 1 && lapDistance > track.Length * 0.5f)
                progress -= track.Length;
            return progress;
        }
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        index = track.FindNearestGlobal(rb.position);
        lapDistance = track.ProjectDistance(rb.position, index);
        UpdateSurface();
    }

    void FixedUpdate()
    {
        Vector2 position2D = rb.position;
        int newIndex = track.FindNearest(position2D, index, 25);
        float newDistance = track.ProjectDistance(position2D, newIndex);

        // Cruzou a linha de chegada indo para a frente: a distância "voltou" a zero.
        if (newDistance - lapDistance < -track.Length * 0.5f)
        {
            if (nextSector >= track.sectorCount)
            {
                laps++;
                nextSector = 1;
                if (countsInRace)
                    raceManager.LapCompleted(this);
            }
        }

        index = newIndex;
        lapDistance = newDistance;

        // Os setores precisam ser visitados em ordem: atalho ou ré não contam.
        if (nextSector < track.sectorCount && track.SectorOf(lapDistance) == nextSector)
            nextSector++;

        UpdateSurface();
        UpdateWrongWay();
    }

    void UpdateSurface()
    {
        CenterOffset = track.LateralOffset(rb.position, index);
        Surface = track.SurfaceAt(rb.position, index);
    }

    // Andando ao contrário por mais de 1 segundo, o HUD avisa.
    void UpdateWrongWay()
    {
        float along = Vector2.Dot(rb.linearVelocity, track.DirectionAt(lapDistance));
        if (along < -2f)
            wrongWayTime += Time.fixedDeltaTime;
        else
            wrongWayTime = 0f;
    }

    public void MarkFinished(float time)
    {
        finished = true;
        FinishTime = time;
    }
}
