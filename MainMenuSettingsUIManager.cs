using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuSettingsUIManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject settingsPanel;
    public Button musicButton;
    public Button soundButton;
    public Button closeButton;

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
        closeButton.onClick.AddListener(() => settingsPanel.SetActive(false));
    }

    public void OpenSettingsPanel()
    {
        settingsPanel.SetActive(true);
    }

    private void ToggleMusic()
    {
        isMusicOn = !isMusicOn;
        PlayerPrefs.SetInt("Music", isMusicOn ? 1 : 0);
        MainMenuSoundManager.Instance?.EnableMusic(isMusicOn);
        UpdateMusicButtonUI();
    }

    private void ToggleSound()
    {
        isSoundOn = !isSoundOn;
        PlayerPrefs.SetInt("Sound", isSoundOn ? 1 : 0);
        MainMenuSoundManager.Instance?.EnableSFX(isSoundOn);
        UpdateSoundButtonUI();
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

    public void OnBookButtonPressed()
    {
        string sceneName = "Lavls"; // заміни на точну назву сцени
        SceneManager.LoadScene(sceneName);
    }
}
