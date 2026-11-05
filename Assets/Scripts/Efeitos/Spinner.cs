using UnityEngine;

// Gira o objeto devagar (carro da vitrine na oficina e no menu).
public class Spinner : MonoBehaviour
{
    [Tooltip("Graus por segundo.")]
    public float speed = 25f;

    void Update()
    {
        transform.Rotate(0f, 0f, speed * Time.deltaTime);
    }
}
