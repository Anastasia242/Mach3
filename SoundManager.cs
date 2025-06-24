using UnityEngine;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Звукові ефекти")]
    public AudioSource sfxSource;

    [Header("Звуки подій")]
    public AudioClip swapSound;
    public AudioClip matchSound;
    public AudioClip specialCreateSound;
    public AudioClip specialExplodeSound;
    public AudioClip colorBombSound;
    public AudioClip fallSound;
    public AudioClip clickSound;

    [Header("Фонові налаштування")]
    public AudioSource musicSource;
    public AudioClip backgroundMusic;


    private bool isMusicEnabled = true;
    private bool isSfxEnabled = true;


    private AudioSource audioSource;

    protected void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject); // збереження між сценами (за потреби)

        // Завантаження налаштувань
        isMusicEnabled = PlayerPrefs.GetInt("Music", 1) == 1;
        isSfxEnabled = PlayerPrefs.GetInt("Sound", 1) == 1;

        // Запуск фонової музики
        if (musicSource != null && backgroundMusic != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.loop = true;
            musicSource.playOnAwake = false;

            if (isMusicEnabled)
                musicSource.Play();
        }
    }

    // 🎵 Увімкнення/вимкнення музики
    public void EnableMusic(bool enable)
    {
        isMusicEnabled = enable;
        PlayerPrefs.SetInt("Music", enable ? 1 : 0);
        PlayerPrefs.Save(); // 👈 додай після цього

        if (musicSource == null) return;

        if (enable)
            musicSource.Play();
        else
            musicSource.Pause();
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
        if (musicSource != null && isMusicEnabled && !musicSource.isPlaying)
        {
            musicSource.Play();
        }
    }



    // 🔊 Увімкнення/вимкнення ефектів
    public void EnableSFX(bool enable)
    {
        isSfxEnabled = enable;
        PlayerPrefs.SetInt("Sound", enable ? 1 : 0);
    }

    // 🔁 Програвання одного звуку
    public void PlaySound(AudioClip clip)
    {
        if (clip != null && isSfxEnabled && sfxSource != null)
            sfxSource.PlayOneShot(clip);
    }

    // Для зручності: можна створити короткі методи
    public void PlaySwap() => PlaySound(swapSound);
    public void PlayMatch() => PlaySound(matchSound);
    public void PlayClick() => PlaySound(clickSound);
    public void PlayFall() => PlaySound(fallSound);
    public void PlaySpecialCreate() => PlaySound(specialCreateSound);
    public void PlaySpecialExplode() => PlaySound(specialExplodeSound);
    public void PlayColorBomb() => PlaySound(colorBombSound);
}
