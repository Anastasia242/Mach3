using System.Collections;
using System.Collections.Generic;
using UnityEditor.EditorTools;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [Header("Обов’язкові компоненти")]
    public LevelLoader levelLoader;
    public GridGenerator gridGenerator;
    public GoalManager goalManager;
    public TimerManager timerManager;
    public DialogueUI dialogueUI;
    public BonusManager bonusManager;
    public EndLevelUIManager endLevelUIManager;
    public SettingsUIManager settingsUIManager;


    [Header("Дані рівня")]
    public TextAsset jsonFile;

    private LevelData currentLevelData;

    private void Start()
    {
        LoadLevel();
    }

    public void LoadLevel()
    {
        if (jsonFile == null)
        {
            Debug.LogError("❌ JSON-файл не призначений у LevelManager!");
            return;
        }

        currentLevelData = levelLoader.LoadLevelFromText(jsonFile.text);

        if (currentLevelData == null)
        {
            Debug.LogError("❌ Дані рівня не завантажено!");
            return;
        }

        // 1️⃣ Генеруємо сітку
        gridGenerator.useLevelData = true;
        string[][] gridArray = ConvertGrid(currentLevelData.grid);
        gridGenerator.GenerateLevelFromData(gridArray);

        // 🔁 Передаємо мапу символів і спрайтів у GoalManager
        var spriteMapping = gridGenerator.GetSymbolSpriteMapping();
        goalManager.InitializeSprites(spriteMapping);
        goalManager.levelManager = this;
        gridGenerator.goalManager = goalManager;


        // 2️⃣ Встановлюємо цілі
        goalManager.SetGoals(currentLevelData.goals);

        // 3️⃣ Обмеження (ходи або таймер)
        if (currentLevelData.timeLimit > 0)
        {
            timerManager.StartCountdown(currentLevelData.timeLimit);
        }
        else
        {
            timerManager.SetMoves(currentLevelData.moves);
        }

        // 🧩 Активуємо відповідний UI
        if (timerManager.timerText != null)
            timerManager.timerText.gameObject.SetActive(currentLevelData.timeLimit > 0);

        if (timerManager.moveText != null)
            timerManager.moveText.gameObject.SetActive(currentLevelData.timeLimit == 0);

        // 4️⃣ Показуємо вступний діалог, якщо є
        if (!string.IsNullOrEmpty(currentLevelData.introDialogue))
        {
            dialogueUI.ShowDialogue(currentLevelData.introDialogue);
        }

        // 5️⃣ Спавнимо стартові бонуси, якщо вони є
        if (currentLevelData.startBonuses != null && currentLevelData.startBonuses.Count > 0)
        {
            bonusManager.SpawnBonuses(currentLevelData.startBonuses);
        }

        timerManager.OnMovesOver += OnLevelLose;
        timerManager.OnTimeOver += OnLevelLose;

    }

    public void OnLevelWin()
    {
        Debug.Log("✅ Ви виграли рівень!");
        timerManager.StopAll(); // 🛑 Зупиняємо таймер
        StartCoroutine(ShowWinPanelWithDelay());

    }
    private IEnumerator ShowWinPanelWithDelay()
    {
        yield return new WaitForSeconds(2f); // або більше, якщо потрібно
        endLevelUIManager?.ShowWinPanel();  // поміняй місцями
        settingsUIManager.PauseGameExternally(); // ховає сітку
    }

    public void OnLevelLose()
    {
        Debug.Log("❌ Ви програли рівень!");
        timerManager.StopAll(); // 🛑 Зупиняємо таймер
        settingsUIManager.PauseGameExternally(); // ховає сітку
        endLevelUIManager?.ShowLosePanel();
    }



    private string[][] ConvertGrid(List<List<string>> listGrid)
    {
        return listGrid.ConvertAll(row => row.ToArray()).ToArray();
    }
}
