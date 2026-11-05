using UnityEngine;

// Área com piso diferente do resto da pista, como a água de um rio.
// Precisa de um Collider2D marcado como Is Trigger.
public class SurfaceZone : MonoBehaviour
{
    public SurfaceType surface = SurfaceType.Agua;

    void OnTriggerEnter2D(Collider2D other)
    {
        CarController car = other.GetComponent<CarController>();
        if (car != null)
            car.EnterZone(this);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        CarController car = other.GetComponent<CarController>();
        if (car != null)
            car.ExitZone(this);
    }
}
