using System.Text;
using TMPro;
using UnityEngine;

// Lista todos os desafios do jogo, com a recompensa e se já foram feitos.
// Também tem o botão de zerar o progresso (precisa de dois cliques).
public class ChallengesScreen : MonoBehaviour
{
    public MenuController menu;
    public TMP_Text summaryText;
    public TMP_Text listText;
    public TMP_Text resetLabel;

    private bool confirmReset;

    public void Refresh()
    {
        GameCatalog catalog = menu.catalog;
        SaveData data = menu.Data;
        StringBuilder text = new StringBuilder();

        foreach (TrackInfo track in catalog.tracks)
        {
            text.Append("<b><color=#FFD95A>" + track.displayName + "</color></b>"
                        + (data.IsUnlocked(track.id) ? "" : " <color=#AAAAAA>(bloqueada)</color>") + "\n");
            for (int i = 0; i < track.challenges.Length; i++)
            {
                ChallengeInfo challenge = track.challenges[i];
                bool done = data.HasChallenge(track.ChallengeId(i));
                string status = done ? "<color=#7CFC00>completo</color>" : "<color=#FF9C8A>a fazer</color>";
                text.Append("   " + challenge.Description() + "  ·  " + challenge.reward + " moedas  ·  " + status + "\n");
            }
        }

        listText.text = text.ToString();
        summaryText.text = "Completos: " + data.completedChallenges.Count + " de " + catalog.TotalChallenges()
                           + "\n<size=75%>Alguns itens da oficina só são liberados com desafios completos.</size>";
        confirmReset = false;
        resetLabel.text = "Zerar progresso";
    }

    public void ResetProgress()
    {
        if (!confirmReset)
        {
            confirmReset = true;
            resetLabel.text = "Clique de novo para confirmar";
            return;
        }

        SaveSystem.Delete();
        menu.ReloadProgress();
        menu.ShowToast("Progresso zerado");
        Refresh();
    }
}
