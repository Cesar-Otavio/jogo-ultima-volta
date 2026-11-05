using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

// Menu inicial: troca entre as telas e guarda o progresso carregado.
// Cada botão chama uma função pública daqui (ligada no Inspector).
public class MenuController : MonoBehaviour
{
    public GameCatalog catalog;

    [Header("Telas")]
    public GameObject mainScreen;
    public GameObject tracksScreen;
    public GameObject garageScreen;
    public GameObject challengesScreen;
    public GameObject instructionsScreen;
    public GameObject creditsScreen;
    public GameObject aboutScreen;

    [Header("Partes")]
    public TrackSelectScreen trackSelect;
    public GarageScreen garage;
    public ChallengesScreen challenges;
    public TMP_Text coinsText;
    [Tooltip("Aviso temporário (fundo + texto) no pé da tela.")]
    public GameObject toast;
    public TMP_Text toastText;

    [Header("Sons")]
    public AudioSource effects;
    public AudioClip clickClip;
    public AudioClip unlockClip;
    public AudioClip errorClip;

    private float toastUntil;

    public SaveData Data { get; private set; }

    void Awake()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        Data = SaveSystem.Load(catalog);
    }

    void Start()
    {
        ShowMain();
    }

    void Update()
    {
        coinsText.text = Data.coins.ToString();
        toast.SetActive(Time.unscaledTime < toastUntil);

        // Modo apresentação: libera todas as pistas e dá moedas.
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard.f9Key.wasPressedThisFrame)
            UnlockEverything();
    }

    public void SaveProgress()
    {
        SaveSystem.Save(Data);
    }

    // Recarrega do arquivo (usado depois de zerar o progresso).
    public void ReloadProgress()
    {
        Data = SaveSystem.Load(catalog);
    }

    public void ShowToast(string text)
    {
        toastText.text = text;
        toastUntil = Time.unscaledTime + 2.5f;
    }

    public void PlayClick() => Play(clickClip, 0.5f);
    public void PlayUnlock() => Play(unlockClip, 0.6f);
    public void PlayError() => Play(errorClip, 0.6f);

    void Play(AudioClip clip, float volume)
    {
        if (clip != null)
            effects.PlayOneShot(clip, volume);
    }

    void UnlockEverything()
    {
        foreach (TrackInfo track in catalog.tracks)
            Data.Unlock(track.id);
        Data.coins += 5000;
        SaveProgress();
        PlayUnlock();
        ShowToast("Modo apresentação: todas as pistas liberadas e +5000 moedas");
        trackSelect.Refresh();
        garage.Refresh();
    }

    // ---------- Navegação ----------

    void Show(GameObject screen)
    {
        mainScreen.SetActive(screen == mainScreen);
        tracksScreen.SetActive(screen == tracksScreen);
        garageScreen.SetActive(screen == garageScreen);
        challengesScreen.SetActive(screen == challengesScreen);
        instructionsScreen.SetActive(screen == instructionsScreen);
        creditsScreen.SetActive(screen == creditsScreen);
        aboutScreen.SetActive(screen == aboutScreen);
    }

    public void ShowMain() => Show(mainScreen);
    public void ShowInstructions() => Show(instructionsScreen);
    public void ShowCredits() => Show(creditsScreen);
    public void ShowAbout() => Show(aboutScreen);

    public void ShowTracks()
    {
        trackSelect.Refresh();
        Show(tracksScreen);
    }

    public void ShowGarage()
    {
        garage.Refresh();
        Show(garageScreen);
    }

    public void ShowChallenges()
    {
        challenges.Refresh();
        Show(challengesScreen);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        // Dentro do Editor não há executável para fechar: apenas sai do Play.
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
