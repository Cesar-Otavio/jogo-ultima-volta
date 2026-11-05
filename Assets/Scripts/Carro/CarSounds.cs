using UnityEngine;

// Sons do próprio carro: motor, pneus cantando, nitro e estouros.
// Nos bots, as fontes de som são 3D: ficam mais altas quando eles passam perto.
public class CarSounds : MonoBehaviour
{
    public CarController car;
    public CarSetup setup;

    [Header("Fontes de som")]
    public AudioSource engine;
    public AudioSource skid;
    public AudioSource oneShots;
    public AudioLowPassFilter engineFilter;

    [Header("Efeitos")]
    public AudioClip nitroClip;
    public AudioClip[] popClips;

    [Header("Volumes")]
    public float engineVolume = 0.5f;
    public float skidVolume = 0.35f;

    private bool wasNitro;

    void Start()
    {
        engineFilter.cutoffFrequency = setup.Engine.lowPassCutoff;
        engine.volume = engineVolume;
        engine.Play();
        skid.volume = 0f;
        skid.Play();
    }

    void Update()
    {
        EngineOption option = setup.Engine;

        // O tom do motor sobe com a velocidade; o nitro dá um empurrão extra.
        float pitch = Mathf.Lerp(option.minPitch, option.maxPitch, car.SpeedPercent);
        if (car.IsNitroActive)
            pitch += 0.15f;
        engine.pitch = pitch;

        // Depois da corrida, o motor do jogador vai sumindo.
        bool raceOver = car.raceManager.IsOver && car.raceCar.isPlayer;
        float engineTarget = raceOver ? 0f : engineVolume;
        engine.volume = Mathf.MoveTowards(engine.volume, engineTarget, Time.deltaTime);

        bool sliding = (car.IsDrifting || (car.IsBraking && car.ForwardSpeed > 5f)) && !car.IsAirborne;
        float skidTarget = sliding && !raceOver ? skidVolume : 0f;
        skid.volume = Mathf.MoveTowards(skid.volume, skidTarget, 3f * Time.deltaTime);

        if (car.IsNitroActive && !wasNitro && nitroClip != null)
            oneShots.PlayOneShot(nitroClip, 0.7f);
        wasNitro = car.IsNitroActive;
    }

    public void PlayPop()
    {
        if (popClips.Length > 0)
            oneShots.PlayOneShot(popClips[Random.Range(0, popClips.Length)], 0.35f);
    }
}
