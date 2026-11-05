using UnityEngine;

// Segue o carro com suavização, olha um pouco à frente, afasta o zoom em
// alta velocidade e treme nas batidas. Nunca mostra o que está fora do mapa.
[RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Rigidbody2D targetBody;

    [Tooltip("Quanto maior, mais rápido a câmera alcança o carro.")]
    public float smoothSpeed = 5f;

    [Tooltip("Sprite do chão: os limites dele são os limites do mapa.")]
    public SpriteRenderer mapGround;

    [Header("Olhar à frente e zoom")]
    [Tooltip("Segundos de movimento que a câmera antecipa.")]
    public float lookAhead = 0.35f;
    public float baseSize = 7.69f;
    [Tooltip("Quanto o zoom se afasta na velocidade máxima.")]
    public float speedZoom = 1.5f;
    public float maxSpeedForZoom = 12f;

    [Header("Tremor")]
    public float shakeStrength = 0.6f;
    public float shakeDecay = 2.5f;

    private Camera cam;
    private Vector3 smoothPosition;
    private float trauma;

    void Awake()
    {
        cam = GetComponent<Camera>();
    }

    // Começa já sobre o carro, sem deslizar no início da corrida.
    void Start()
    {
        cam.orthographicSize = baseSize;
        smoothPosition = DesiredPosition();
        transform.position = smoothPosition;
    }

    // LateUpdate roda depois do movimento do quadro, o que evita tremores.
    void LateUpdate()
    {
        float speed = targetBody != null ? targetBody.linearVelocity.magnitude : 0f;
        float targetSize = baseSize + speedZoom * Mathf.Clamp01(speed / maxSpeedForZoom);
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetSize, 1f - Mathf.Exp(-2f * Time.deltaTime));

        // Esta fórmula dá a mesma suavização em qualquer taxa de quadros.
        float t = 1f - Mathf.Exp(-smoothSpeed * Time.deltaTime);
        smoothPosition = Vector3.Lerp(smoothPosition, DesiredPosition(), t);

        trauma = Mathf.Max(0f, trauma - shakeDecay * Time.deltaTime);
        float shake = trauma * trauma * shakeStrength;
        Vector3 offset = new Vector3(
            (Mathf.PerlinNoise(Time.time * 25f, 0f) - 0.5f) * 2f * shake,
            (Mathf.PerlinNoise(0f, Time.time * 25f) - 0.5f) * 2f * shake,
            0f);

        transform.position = smoothPosition + offset;
    }

    // Chamado nas batidas. De 0 (nada) a 1 (batida forte).
    public void Shake(float amount)
    {
        trauma = Mathf.Clamp01(trauma + amount);
    }

    Vector3 DesiredPosition()
    {
        Vector2 point = target.position;
        if (targetBody != null)
            point += targetBody.linearVelocity * lookAhead;

        Bounds map = mapGround.bounds;
        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;

        float x = ClampToMap(point.x, map.min.x + halfWidth, map.max.x - halfWidth);
        float y = ClampToMap(point.y, map.min.y + halfHeight, map.max.y - halfHeight);

        return new Vector3(x, y, transform.position.z);
    }

    // Se o mapa for menor que a tela neste eixo, a câmera fica no centro dele.
    float ClampToMap(float value, float min, float max)
    {
        return min > max ? (min + max) / 2f : Mathf.Clamp(value, min, max);
    }
}
