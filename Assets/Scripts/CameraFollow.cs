using UnityEngine;

// Segue o carro com suavização, sem mostrar o que está fora do mapa.
[RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviour
{
    public Transform target;

    [Tooltip("Quanto maior, mais rápido a câmera alcança o carro.")]
    public float smoothSpeed = 5f;

    [Tooltip("Sprite do chão: os limites dele são os limites do mapa.")]
    public SpriteRenderer mapGround;

    private Camera cam;

    void Awake()
    {
        cam = GetComponent<Camera>();
    }

    // Começa já sobre o carro, sem deslizar no início da corrida.
    void Start()
    {
        transform.position = DesiredPosition();
    }

    // LateUpdate roda depois do movimento do quadro, o que evita tremores.
    void LateUpdate()
    {
        // Esta fórmula dá a mesma suavização em qualquer taxa de quadros.
        float t = 1f - Mathf.Exp(-smoothSpeed * Time.deltaTime);
        transform.position = Vector3.Lerp(transform.position, DesiredPosition(), t);
    }

    Vector3 DesiredPosition()
    {
        Bounds map = mapGround.bounds;
        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;

        float x = ClampToMap(target.position.x, map.min.x + halfWidth, map.max.x - halfWidth);
        float y = ClampToMap(target.position.y, map.min.y + halfHeight, map.max.y - halfHeight);

        return new Vector3(x, y, transform.position.z);
    }

    // Se o mapa for menor que a tela neste eixo, a câmera fica no centro dele.
    float ClampToMap(float value, float min, float max)
    {
        return min > max ? (min + max) / 2f : Mathf.Clamp(value, min, max);
    }
}
