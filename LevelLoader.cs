using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;

public class LevelLoader : MonoBehaviour
{
    /// <summary>
    /// Завантажує дані рівня з текстового JSON і повертає LevelData
    /// </summary>
    public LevelData LoadLevelFromText(string jsonText)
    {
        if (string.IsNullOrEmpty(jsonText))
        {
            Debug.LogError("❌ Порожній JSON!");
            return null;
        }

        Debug.Log("✅ Починаємо парсинг JSON...");
        Debug.Log($"📜 JSON Вміст:\n{jsonText}");

        try
        {
            LevelData levelData = JsonConvert.DeserializeObject<LevelData>(jsonText);

            if (levelData == null || levelData.grid == null || levelData.grid.Count == 0)
            {
                Debug.LogError("❌ JSON-файл порожній або некоректний!");
                return null;
            }

            Debug.Log($"✅ Рівень завантажено: {levelData.grid.Count}x{levelData.grid[0].Count}");
            return levelData;
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ Помилка парсингу JSON: {e.Message}");
            return null;
        }
    }
}
