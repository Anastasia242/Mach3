using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class PageData
{
    public string levelName;
    public GameObject pageObject; // UI-сторінка з рецептом
}

[System.Serializable]
public class Chapter
{
    public string chapterName;
    public List<PageData> pages;
}

public class RecipeBookManager : MonoBehaviour
{
    public List<Chapter> chapters;
    private int currentChapterIndex = 0;
    private int currentPageIndex = 0;

    public void NextPage()
    {
        if (currentPageIndex < chapters[currentChapterIndex].pages.Count - 1)
        {
            ShowPage(++currentPageIndex);
        }
    }

    public void PreviousPage()
    {
        Debug.Log("Спроба перейти на попередню сторінку. Поточна сторінка: " + currentPageIndex);

        if (currentPageIndex > 0)
        {
            currentPageIndex--;
            Debug.Log("Переходимо на сторінку: " + currentPageIndex);
            ShowPage(currentPageIndex);
        }
        else
        {
            Debug.Log("Вже на першій сторінці. Назад не можна.");
        }
    }


    public void NextChapter()
    {
        if (currentChapterIndex < chapters.Count - 1)
        {
            currentChapterIndex++;
            currentPageIndex = 0;

            // Перевірка: якщо в новій главі немає сторінок — просто пропустити
            if (chapters[currentChapterIndex].pages.Count > 0)
                ShowPage(currentPageIndex);
        }
    }


    public void PreviousChapter()
    {
        if (currentChapterIndex > 0)
        {
            currentChapterIndex--;
            currentPageIndex = 0;
            ShowPage(currentPageIndex);
        }
    }

    public void ShowPage(int index)
    {
        var currentChapter = chapters[currentChapterIndex];

        if (currentChapter.pages == null || currentChapter.pages.Count == 0)
        {
            Debug.LogWarning($"⚠️ Глава '{currentChapter.chapterName}' не має сторінок!");
            return;
        }

        if (index < 0 || index >= currentChapter.pages.Count)
        {
            Debug.LogWarning($"⚠️ Індекс сторінки {index} виходить за межі списку в главі '{currentChapter.chapterName}'!");
            return;
        }

        foreach (var page in currentChapter.pages)
            page.pageObject.SetActive(false);

        currentChapter.pages[index].pageObject.SetActive(true);
    }


    public void LoadCurrentLevel()
    {
        string levelToLoad = chapters[currentChapterIndex].pages[currentPageIndex].levelName;
        if (!string.IsNullOrEmpty(levelToLoad))
        {
            SceneManager.LoadScene(levelToLoad);
        }
    }

    public void OnNextPageButtonClick()
    {
        NextPage();
    }

    public void OnPreviousPageButtonClick()
    {
        Debug.Log("Натиснули кнопку ЛІВА СТОРІНКА. Поточна сторінка: " + currentPageIndex);
        PreviousPage();
    }


    public void OnNextChapterButtonClick()
    {
        NextChapter();
    }

    public void OnPreviousChapterButtonClick()
    {
        PreviousChapter();
    }

}
