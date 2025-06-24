using DG.Tweening.Core.Easing;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndLevelUIManager : MonoBehaviour
{
    [Header("UI панелі")]
    public GameObject winPanel;
    public GameObject losePanel;
    public FadeManager fadeManager;

    public void ShowWinPanel()
    {
        if (winPanel != null)
        {
            winPanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("⚠️ WinPanel не призначено!");
        }
    }
    public void GoToRecipeBook()
    {
        string sceneName = "Lavls"; // Назва твоєї сцени з рецептами
        if (fadeManager != null)
        {
            fadeManager.FadeToScene(sceneName);
        }
        else
        {
            SceneManager.LoadScene(sceneName);
        }
    }

    public void ShowLosePanel()
    {
        if (losePanel != null)
        {
            losePanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("⚠️ LosePanel не призначено!");
        }
    }

    public void HideAllPanels()
    {
        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);
    }
}
