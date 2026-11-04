using UnityEngine;

// Linha de chegada: avisa o RaceManager quando o carro do jogador cruza.
// Precisa de um Collider2D com "Is Trigger" marcado neste mesmo objeto.
public class FinishLine : MonoBehaviour
{
    public RaceManager raceManager;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            raceManager.CrossFinishLine();
    }
}
