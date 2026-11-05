using System.Collections;
using UnityEngine;

// Fogos de artifício quando o jogador termina no pódio.
public class Fireworks : MonoBehaviour
{
    public RaceManager raceManager;
    public Camera cam;
    [Tooltip("Sistema de partículas que explode em várias direções.")]
    public ParticleSystem burst;
    public AudioSource audioSource;
    public AudioClip[] clips;
    public int rockets = 10;

    private bool started;

    void Update()
    {
        if (started || raceManager.State != RaceState.Finished || raceManager.player.Position > 3)
            return;

        started = true;
        StartCoroutine(Launch());
    }

    IEnumerator Launch()
    {
        for (int i = 0; i < rockets; i++)
        {
            // Espalha os fogos pela área visível da câmera.
            float height = cam.orthographicSize;
            float width = height * cam.aspect;
            Vector3 position = cam.transform.position + new Vector3(
                Random.Range(-width * 0.8f, width * 0.8f),
                Random.Range(-height * 0.6f, height * 0.8f),
                10f);

            ParticleSystem.MainModule main = burst.main;
            main.startColor = Color.HSVToRGB(Random.value, 0.8f, 1f);
            burst.transform.position = position;
            burst.Emit(70);

            if (clips.Length > 0)
                audioSource.PlayOneShot(clips[Random.Range(0, clips.Length)], 0.6f);

            yield return new WaitForSeconds(Random.Range(0.25f, 0.55f));
        }
    }
}
