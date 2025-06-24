using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class SettingsUIManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject settingsPanel;
    public Button musicButton;
    public Button soundButton;
    public Button homeButton;
    public Button restartButton;

    public GameObject gridParent;
    public GridGenerator gridGenerator;
    public FadeManager fadeManager; 


    [Header("Icon Images (Child objects)")]
    public Image musicIconImage;
    public Image soundIconImage;

    [Header("Icons")]
    public Sprite musicOnIcon;
    public Sprite musicOffIcon;
    public Sprite soundOnIcon;
    public Sprite soundOffIcon;

    private bool isMusicOn;
    private bool isSoundOn;

    private void Start()
    {
        settingsPanel.SetActive(false);

        isMusicOn = PlayerPrefs.GetInt("Music", 1) == 1;
        isSoundOn = PlayerPrefs.GetInt("Sound", 1) == 1;

        UpdateMusicButtonUI();
        UpdateSoundButtonUI();

        musicButton.onClick.AddListener(ToggleMusic);
        soundButton.onClick.AddListener(ToggleSound);
        homeButton.onClick.AddListener(OnHomePressed);
        restartButton.onClick.AddListener(OnRestartPressed);

    }

    public void ToggleSettingsPanel()
    {
        StartCoroutine(OpenSettingsWithDelay());
    }

    private IEnumerator OpenSettingsWithDelay()
    {
        while (!gridGenerator.IsGridIdle)
            yield return null;

        yield return new WaitForSeconds(0.2f);

        if (gridParent != null)
            gridParent.SetActive(false);

        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);

        if (gridParent != null)
            gridParent.SetActive(true);
    }

    private void ToggleMusic()
    {
        isMusicOn = !isMusicOn;
        PlayerPrefs.SetInt("Music", isMusicOn ? 1 : 0);
        SoundManager.Instance?.EnableMusic(isMusicOn);
        UpdateMusicButtonUI();
    }

    private void ToggleSound()
    {
        isSoundOn = !isSoundOn;
        PlayerPrefs.SetInt("Sound", isSoundOn ? 1 : 0);
        SoundManager.Instance?.EnableSFX(isSoundOn);
        UpdateSoundButtonUI();
    }

    private void OnHomePressed()
    {
        string sceneName = "mainManu"; // ✅ правильна назва

        if (Application.CanStreamedLevelBeLoaded(sceneName))
        {
            if (fadeManager != null)
            {
                fadeManager.FadeToScene(sceneName);
            }
            else
            {
                Debug.LogWarning("⚠️ FadeManager не призначений у інспекторі. Виконуємо прямий перехід.");
                SceneManager.LoadScene(sceneName);
            }
        }
        else
        {
            Debug.LogError($"❌ Сцена '{sceneName}' не знайдена. Перевір, чи вона додана в Build Settings.");
        }
    }



    private void OnRestartPressed()
    {
        Debug.Log("🔁 Перезапуск сцени...");

        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        var fadeManager = FindObjectOfType<FadeManager>();
        if (fadeManager != null)
        {
            fadeManager.FadeToScene(sceneName);
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        }
    }

    public void PauseGameExternally()
    {
        if (gridParent != null)
            gridParent.SetActive(false);
    }



    private void UpdateMusicButtonUI()
    {
        if (musicIconImage != null)
            musicIconImage.sprite = isMusicOn ? musicOnIcon : musicOffIcon;
    }

    private void UpdateSoundButtonUI()
    {
        if (soundIconImage != null)
            soundIconImage.sprite = isSoundOn ? soundOnIcon : soundOffIcon;
    }
}
