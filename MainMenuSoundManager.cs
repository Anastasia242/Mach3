using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuSoundManager : MonoBehaviour
{
    public static MainMenuSoundManager Instance;

    [Header("Фонові налаштування")]
    public AudioSource musicSource;
    public AudioClip mainMenuMusic;

    [Header("Звук кліку")]
    public AudioSource sfxSource;
    public AudioClip clickSound;

    private bool isMusicEnabled;
    private bool isSfxEnabled;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        LoadSoundSettings();
        InitMusic();
    }

    private void LoadSoundSettings()
    {
        isMusicEnabled = PlayerPrefs.GetInt("Music", 1) == 1;
        isSfxEnabled = PlayerPrefs.GetInt("Sound", 1) == 1;
    }

    private void InitMusic()
    {
        if (musicSource != null && mainMenuMusic != null)
        {
            musicSource.clip = mainMenuMusic;
            musicSource.loop = true;
            musicSource.playOnAwake = false;

            if (isMusicEnabled)
                musicSource.Play();
        }
    }

    public void PlayClickSound()
    {
        if (isSfxEnabled && clickSound != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clickSound);
        }
    }

    public void EnableMusic(bool enable)
    {
        isMusicEnabled = enable;
        PlayerPrefs.SetInt("Music", enable ? 1 : 0);
        PlayerPrefs.Save();

        if (musicSource == null) return;

        if (enable)
        {
            if (!musicSource.isPlaying)
                musicSource.Play();
        }
        else
        {
            musicSource.Pause();
        }
    }

    public void EnableSFX(bool enable)
    {
        isSfxEnabled = enable;
        PlayerPrefs.SetInt("Sound", enable ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Гарантує, що музика відновиться, якщо сцена перезавантажена
        if (musicSource != null && isMusicEnabled && !musicSource.isPlaying)
        {
            musicSource.Play();
        }
    }
}
