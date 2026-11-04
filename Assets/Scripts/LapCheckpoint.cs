using UnityEngine;

// Ponto de controle invisível, na metade do percurso.
// Precisa de um Collider2D com "Is Trigger" marcado neste mesmo objeto.
public class LapCheckpoint : MonoBehaviour
{
    public RaceManager raceManager;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            raceManager.PassCheckpoint();
    }
}
