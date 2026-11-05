using UnityEngine;

// Rampa: o carro que passa rápido por cima voa por alguns instantes.
// No ar, ele não sente o piso (por exemplo, a água de um rio).
public class JumpPad : MonoBehaviour
{
    [Tooltip("Velocidade mínima para pular.")]
    public float minSpeed = 4f;
    [Tooltip("Tempo no ar, em segundos, na velocidade máxima.")]
    public float maxAirTime = 0.9f;

    void OnTriggerEnter2D(Collider2D other)
    {
        CarController car = other.GetComponent<CarController>();
        if (car == null || car.IsAirborne || car.ForwardSpeed < minSpeed)
            return;

        // Quanto mais rápido, mais longe o pulo.
        float airTime = Mathf.Lerp(0.4f, maxAirTime, car.ForwardSpeed / car.maxSpeed);
        car.Jump(airTime);
    }
}
