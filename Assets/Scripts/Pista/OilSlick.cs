using UnityEngine;

// Mancha de óleo: quem passa por cima escorrega por alguns instantes.
// Vale para o jogador e para os bots.
public class OilSlick : MonoBehaviour
{
    [Tooltip("Tempo, em segundos, que o carro fica escorregando.")]
    public float slipDuration = 1.5f;

    void OnTriggerEnter2D(Collider2D other)
    {
        CarController car = other.GetComponent<CarController>();
        if (car != null)
            car.Slip(slipDuration);
    }
}
