using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Um cartão da tela de pistas. Só guarda as referências dos textos.
[System.Serializable]
public class TrackCard
{
    public Image background;
    public TMP_Text title;
    public TMP_Text details;
    public TMP_Text record;
    public GameObject lockOverlay;
    public TMP_Text lockText;
    public Button playButton;
}

// Tela de escolha de pista: mostra regras, recorde, desafios e bloqueio.
public class TrackSelectScreen : MonoBehaviour
{
    public MenuController menu;
    public TrackCard[] cards;

    public void Refresh()
    {
        GameCatalog catalog = menu.catalog;
        SaveData data = menu.Data;

        for (int i = 0; i < cards.Length; i++)
        {
            TrackCard card = cards[i];
            TrackInfo track = catalog.tracks[i];
            bool unlocked = data.IsUnlocked(track.id);

            card.background.color = Color.Lerp(Color.white, track.themeColor, 0.35f);
            card.title.text = track.displayName + "\n<size=60%>" + track.country + "</size>";

            string rivals = track.botCount > 0 ? track.botCount + " rivais" : "sem rivais";
            card.details.text = track.description + "\n\n" + track.laps + " voltas · " + rivals
                + "\nTempo limite: " + RaceTimer.Format(track.timeLimit);

            int done = 0;
            for (int c = 0; c < track.challenges.Length; c++)
                if (data.HasChallenge(track.ChallengeId(c)))
                    done++;
            TrackRecord record = data.Record(track.id);
            string best = record != null && record.bestTime > 0f
                ? "Recorde: " + RaceTimer.FormatPrecise(record.bestTime) + (track.botCount > 0 ? " · melhor: " + record.bestPosition + "º" : "")
                : "Sem recorde ainda";
            card.record.text = best + "\nDesafios: " + done + "/" + track.challenges.Length;

            card.lockOverlay.SetActive(!unlocked);
            card.playButton.interactable = unlocked;
            if (!unlocked && i > 0)
                card.lockText.text = "Bloqueada\n\n<size=70%>Termine no pódio em " + catalog.tracks[i - 1].displayName + " para liberar</size>";
        }
    }

    public void Play(int index)
    {
        TrackInfo track = menu.catalog.tracks[index];
        if (!menu.Data.IsUnlocked(track.id))
        {
            menu.PlayError();
            return;
        }

        menu.Data.selectedTrack = track.id;
        menu.SaveProgress();
        SceneManager.LoadScene(track.sceneName);
    }
}
