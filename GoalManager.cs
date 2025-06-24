using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GoalManager : MonoBehaviour
{
    [Header("UI елемент для виводу цілей")]
    public Transform goalContainer; // Батьківський об’єкт для елементів цілей

    private Dictionary<string, int> targetGoals = new Dictionary<string, int>();
    private Dictionary<string, int> currentProgress = new Dictionary<string, int>();
    private Dictionary<string, TextMeshProUGUI> goalTexts = new Dictionary<string, TextMeshProUGUI>();
    private Dictionary<string, Sprite> symbolToSprite;
    public LevelManager levelManager;


    public void InitializeSprites(Dictionary<string, Sprite> mapping)
    {
        symbolToSprite = mapping;
    }

    public List<Transform> goalItems; // Признач у інспекторі по черзі

    public void SetGoals(Dictionary<string, int> goals)
    {
        targetGoals = new Dictionary<string, int>();
        currentProgress = new Dictionary<string, int>();
        goalTexts = new Dictionary<string, TextMeshProUGUI>();

        int index = 0;

        foreach (var goal in goals)
        {
            if (index >= goalItems.Count)
            {
                Debug.LogWarning("⚠️ Недостатньо вручну доданих елементів у goalItems!");
                break;
            }

            Transform item = goalItems[index];
            var goalText = item.Find("Text (TMP)")?.GetComponent<TextMeshProUGUI>();
            var icon = item.Find("Image")?.GetComponent<UnityEngine.UI.Image>();

            string symbol = goal.Key;
            int amount = goal.Value;

            // 🧠 Перетворюємо символ на assignedName
            string name = SymbolToName(symbol);

            if (goalText == null)
            {
                Debug.LogError($"❌ Не знайдено Text (TMP) у {item.name}!");
                continue;
            }

            if (icon == null)
            {
                Debug.LogWarning($"⚠️ Не знайдено Image у {item.name}!");
            }

            currentProgress[name] = 0;
            targetGoals[name] = amount;
            goalText.text = $"0/{amount}";
            goalTexts[name] = goalText;

            if (symbolToSprite != null && symbolToSprite.TryGetValue(symbol, out var sprite) && icon != null)
            {
                icon.sprite = sprite;
            }

            index++;
        }
    }


    public bool HasGoal(string symbol)
    {
        string name = SymbolToName(symbol); // 🔄
        return targetGoals.ContainsKey(name);
    }


    private string SymbolToName(string symbol)
    {
        return symbol switch
        {
            "A" => "Apple",
            "B" => "Banana",
            "P" => "Pear",
            "L" => "Bilberry",
            "I" => "Ingredient", // 🆕 додано
            _ => symbol // якщо щось інше, залишаємо як є
        };
    }




    public void AddProgress(string symbol)
    {
        if (!currentProgress.ContainsKey(symbol))
        {
            Debug.LogWarning($"⚠️ AddProgress: {symbol} не знайдено серед цілей");
            return;
        }

        currentProgress[symbol]++;
        goalTexts[symbol].text = $"{currentProgress[symbol]} / {targetGoals[symbol]}";
        Debug.Log($"✅ Прогрес оновлено для {symbol}: {currentProgress[symbol]} / {targetGoals[symbol]}");

        if (AreGoalsCompleted())
        {
            levelManager?.OnLevelWin();
        }
    }



    public bool AreGoalsCompleted()
    {
        foreach (var goal in targetGoals)
        {
            string symbol = goal.Key;
            int required = goal.Value;

            if (!currentProgress.ContainsKey(symbol) || currentProgress[symbol] < required)
                return false;
        }

        return true;
    }

    private void ClearUI()
    {
        foreach (Transform child in goalContainer)
        {
            Destroy(child.gameObject);
        }
    }
}
