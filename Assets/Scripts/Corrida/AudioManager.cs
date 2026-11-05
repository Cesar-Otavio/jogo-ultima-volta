using UnityEngine;

// Música e efeitos gerais da corrida. Os sons de cada carro (motor, pneus,
// nitro) ficam no CarSounds de cada carro.
public class AudioManager : MonoBehaviour
{
    public RaceManager raceManager;

    [Header("Fontes de som")]
    public AudioSource musicSource;
    public AudioSource effectsSource;

    [Header("Efeitos")]
    public AudioClip[] collisionClips;
    public AudioClip winClip;
    public AudioClip loseClip;
    public AudioClip countdownClip;
    public AudioClip coinClip;
    public AudioClip repairClip;

    [Tooltip("Batidas nesta velocidade ou mais tocam no volume máximo.")]
    public float maxImpactSpeed = 10f;

    private bool resultPlayed;
    private bool countdownPlayed;

    void Start()
    {
        // A música vem dos dados da pista.
        musicSource.clip = raceManager.track.music;
        musicSource.Play();
    }

    void Update()
    {
        if (raceManager.IsOver)
        {
            PlayResultOnce();
            return;
        }

        // A contagem toca junto com o "3" na tela.
        if (!countdownPlayed && raceManager.State == RaceState.Countdown
            && raceManager.CountdownLeft <= raceManager.countdownDuration)
        {
            countdownPlayed = true;
            if (countdownClip != null)
                effectsSource.PlayOneShot(countdownClip, 0.8f);
        }

        UpdateMusic();
    }

    // Na última volta, troca para a música mais acelerada.
    void UpdateMusic()
    {
        AudioClip lastLapMusic = raceManager.track.lastLapMusic;
        if (raceManager.IsLastLap && lastLapMusic != null && musicSource.clip != lastLapMusic)
        {
            musicSource.clip = lastLapMusic;
            musicSource.Play();
        }
    }

    // Pódio toca a vinheta boa; o resto, a vinheta ruim.
    void PlayResultOnce()
    {
        if (resultPlayed)
            return;

        resultPlayed = true;
        musicSource.Stop();
        bool good = raceManager.State == RaceState.Finished && raceManager.player.Position <= 3;
        effectsSource.PlayOneShot(good ? winClip : loseClip);
    }

    // Chamado pelo CarDamage. Quanto mais forte a batida, mais alto o som.
    public void PlayCollision(float impactSpeed)
    {
        AudioClip clip = collisionClips[Random.Range(0, collisionClips.Length)];
        float volume = Mathf.Lerp(0.25f, 1f, impactSpeed / maxImpactSpeed);
        effectsSource.PlayOneShot(clip, volume);
    }

    public void PlayCoin()
    {
        effectsSource.PlayOneShot(coinClip, 0.45f);
    }

    public void PlayRepair()
    {
        effectsSource.PlayOneShot(repairClip, 0.6f);
    }
}
