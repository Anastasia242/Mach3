using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class FadeManager : MonoBehaviour
{
    public CanvasGroup fadeGroup;
    public float fadeDuration = 5f;

    private void Start()
    {
        // На старті з'являємось із чорного
        fadeGroup.alpha = 1;
        StartCoroutine(FadeIn());
    }

    public void FadeToScene(string sceneName)
    {
        StartCoroutine(FadeOutAndLoad(sceneName));
    }

    private IEnumerator FadeIn()
    {
        float t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            fadeGroup.alpha = 1 - t / fadeDuration;
            yield return null;
        }
        fadeGroup.alpha = 0;
    }

    private IEnumerator FadeOutAndLoad(string sceneName)
    {
        float t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            fadeGroup.alpha = t / fadeDuration;
            yield return null;
        }

        SceneManager.LoadScene(sceneName);
    }
}
