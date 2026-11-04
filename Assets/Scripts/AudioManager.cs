using UnityEngine;

// Música e efeitos da corrida. Consulta o estado no RaceManager, como as telas.
public class AudioManager : MonoBehaviour
{
    public RaceManager raceManager;
    public CarController car;

    [Header("Fontes de som")]
    public AudioSource musicSource;
    public AudioSource engineSource;
    public AudioSource effectsSource;

    [Header("Músicas")]
    public AudioClip raceMusic;
    public AudioClip lastLapMusic;

    [Header("Efeitos")]
    public AudioClip[] collisionClips;
    public AudioClip winClip;
    public AudioClip loseClip;

    [Header("Motor")]
    public float engineMinPitch = 0.8f;
    public float engineMaxPitch = 1.8f;

    private bool resultPlayed;

    void Start()
    {
        musicSource.clip = raceMusic;
        musicSource.Play();
        engineSource.Play();
    }

    void Update()
    {
        if (raceManager.State == RaceState.Won || raceManager.State == RaceState.Lost)
        {
            PlayResultOnce();
            return;
        }

        UpdateMusic();

        // O tom do motor sobe com a velocidade do carro.
        engineSource.pitch = Mathf.Lerp(engineMinPitch, engineMaxPitch, car.SpeedPercent);
    }

    // Na última volta, troca para a música mais acelerada.
    void UpdateMusic()
    {
        bool lastLap = raceManager.CompletedLaps == raceManager.totalLaps - 1;

        if (lastLap && musicSource.clip != lastLapMusic)
        {
            musicSource.clip = lastLapMusic;
            musicSource.Play();
        }
    }

    void PlayResultOnce()
    {
        if (resultPlayed)
            return;

        resultPlayed = true;
        musicSource.Stop();
        engineSource.Stop();
        effectsSource.PlayOneShot(raceManager.State == RaceState.Won ? winClip : loseClip);
    }

    // Chamado pelo CarDamage. Quanto mais forte a batida, mais alto o som.
    public void PlayCollision(float impactSpeed)
    {
        AudioClip clip = collisionClips[Random.Range(0, collisionClips.Length)];
        float volume = Mathf.Lerp(0.25f, 1f, impactSpeed / car.maxSpeed);
        effectsSource.PlayOneShot(clip, volume);
    }
}
