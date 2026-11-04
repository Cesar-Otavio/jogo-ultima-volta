using UnityEngine;
using UnityEngine.SceneManagement;

// Botões do menu inicial. Cada função é ligada a um botão no Inspector.
public class MenuController : MonoBehaviour
{
    public GameObject mainScreen;
    public GameObject instructionsScreen;

    public void PlayGame()
    {
        SceneManager.LoadScene("Corrida");
    }

    public void ShowInstructions()
    {
        mainScreen.SetActive(false);
        instructionsScreen.SetActive(true);
    }

    public void HideInstructions()
    {
        instructionsScreen.SetActive(false);
        mainScreen.SetActive(true);
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
