using UnityEngine;

// Mancha de óleo: faz o carro perder aderência por um tempo.
// Precisa de um Collider2D com "Is Trigger" marcado neste mesmo objeto.
public class OilSlick : MonoBehaviour
{
    [Tooltip("Por quantos segundos o carro escorrega depois de passar no óleo.")]
    public float slipDuration = 1.5f;

    void OnTriggerEnter2D(Collider2D other)
    {
        CarController car = other.GetComponent<CarController>();
        if (car != null)
            car.Slip(slipDuration);
    }
}
