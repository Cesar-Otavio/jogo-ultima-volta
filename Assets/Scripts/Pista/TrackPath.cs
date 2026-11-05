using UnityEngine;

// Linha central da pista: uma lista de pontos em ordem, formando um laço.
// Serve para três coisas: contar o progresso de cada carro (voltas e
// posições), dizer em que piso o carro está e guiar os bots.
public class TrackPath : MonoBehaviour
{
    [Tooltip("Pontos da linha central, no sentido da corrida. O último liga no primeiro.")]
    public Vector2[] points;

    [Tooltip("Piso de cada trecho: um valor por ponto.")]
    public SurfaceType[] surfaces;

    [Tooltip("Metade da largura da pista, em unidades.")]
    public float roadHalfWidth = 3.5f;

    [Tooltip("Piso fora da pista.")]
    public SurfaceType offRoadSurface = SurfaceType.Grama;

    [Tooltip("A volta só conta depois de passar por todos os setores, em ordem.")]
    public int sectorCount = 8;

    private float[] cumulative;
    private float length;

    public float Length => length;
    public int Count => points.Length;

    void Awake()
    {
        Build();
    }

    // Calcula a distância acumulada até cada ponto.
    public void Build()
    {
        cumulative = new float[points.Length + 1];
        for (int i = 0; i < points.Length; i++)
            cumulative[i + 1] = cumulative[i] + Vector2.Distance(points[i], points[Next(i)]);
        length = cumulative[points.Length];
    }

    public int Next(int i) => (i + 1) % points.Length;

    public float DistanceAtPoint(int i) => cumulative[i];

    // Procura o ponto mais próximo só perto do último conhecido. Assim, um
    // trecho da pista que passa ao lado não confunde a contagem.
    public int FindNearest(Vector2 position, int hint, int window)
    {
        int best = hint;
        float bestDistance = float.MaxValue;
        for (int offset = -window; offset <= window; offset++)
        {
            int i = Wrap(hint + offset);
            float distance = (points[i] - position).sqrMagnitude;
            if (distance < bestDistance)
            {
                bestDistance = distance;
                best = i;
            }
        }
        return best;
    }

    public int FindNearestGlobal(Vector2 position)
    {
        return FindNearest(position, 0, points.Length / 2);
    }

    // Distância ao longo da pista do ponto do carro projetado no trecho certo.
    public float ProjectDistance(Vector2 position, int index)
    {
        // O carro pode estar no trecho que sai deste ponto ou no que chega nele.
        int previous = Wrap(index - 1);
        float t = SegmentT(position, index);
        if (t >= 0f)
            return Mathf.Repeat(cumulative[index] + Mathf.Min(t, 1f) * SegmentLength(index), length);

        float tPrevious = SegmentT(position, previous);
        return Mathf.Repeat(cumulative[previous] + Mathf.Clamp01(tPrevious) * SegmentLength(previous), length);
    }

    // Distância lateral até a linha central: positiva à esquerda.
    public float LateralOffset(Vector2 position, int index)
    {
        Vector2 direction = (points[Next(index)] - points[index]).normalized;
        Vector2 left = new Vector2(-direction.y, direction.x);
        return Vector2.Dot(position - points[index], left);
    }

    public SurfaceType SurfaceAt(Vector2 position, int index)
    {
        if (Mathf.Abs(LateralOffset(position, index)) > roadHalfWidth)
            return offRoadSurface;
        return surfaces != null && index < surfaces.Length ? surfaces[index] : SurfaceType.Asfalto;
    }

    public Vector2 PositionAt(float distance)
    {
        int i = IndexAt(distance, out float t);
        return Vector2.Lerp(points[i], points[Next(i)], t);
    }

    public Vector2 DirectionAt(float distance)
    {
        int i = IndexAt(distance, out _);
        return (points[Next(i)] - points[i]).normalized;
    }

    // Quanto a pista vira, em graus, entre duas distâncias. Os bots usam
    // isso para frear antes das curvas.
    public float TurnBetween(float from, float to)
    {
        float total = 0f;
        Vector2 previous = DirectionAt(from);
        for (float d = from + 2f; d <= to; d += 2f)
        {
            Vector2 current = DirectionAt(d);
            total += Mathf.Abs(Vector2.SignedAngle(previous, current));
            previous = current;
        }
        return total;
    }

    public int SectorOf(float distance)
    {
        return Mathf.Clamp((int)(distance / length * sectorCount), 0, sectorCount - 1);
    }

    int IndexAt(float distance, out float t)
    {
        distance = Mathf.Repeat(distance, length);
        // Busca binária na distância acumulada.
        int low = 0, high = points.Length - 1;
        while (low < high)
        {
            int middle = (low + high + 1) / 2;
            if (cumulative[middle] <= distance) low = middle;
            else high = middle - 1;
        }
        float segment = SegmentLength(low);
        t = segment > 0f ? (distance - cumulative[low]) / segment : 0f;
        return low;
    }

    float SegmentLength(int i) => cumulative[i + 1] - cumulative[i];

    float SegmentT(Vector2 position, int i)
    {
        Vector2 a = points[i];
        Vector2 ab = points[Next(i)] - a;
        float lengthSquared = ab.sqrMagnitude;
        return lengthSquared > 0f ? Vector2.Dot(position - a, ab) / lengthSquared : 0f;
    }

    int Wrap(int i)
    {
        int n = points.Length;
        return ((i % n) + n) % n;
    }

    // Desenha a linha central na janela Scene, para conferir a pista.
    void OnDrawGizmosSelected()
    {
        if (points == null || points.Length < 2)
            return;
        Gizmos.color = Color.cyan;
        for (int i = 0; i < points.Length; i++)
            Gizmos.DrawLine(points[i], points[(i + 1) % points.Length]);
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(points[0], 0.6f);
    }
}
