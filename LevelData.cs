using System;
using System.Collections.Generic;

[Serializable]
public class LevelData
{
    public string levelName;                          // Назва рівня або страви
    public int moves = 0;                             // Кількість ходів (0 = не використовується)
    public int timeLimit = 0;                         // Обмеження по часу в секундах (0 = не використовується)

    public Dictionary<string, int> goals = new Dictionary<string, int>(); // Цілі — скільки яких фруктів зібрати

    public List<List<string>> grid = new List<List<string>>();            // Сітка рівня, кожен елемент — символ

    public string introDialogue = "";                 // Текст репліки при старті рівня

    public bool hasStickyTiles = false;               // Наприклад, для липкої карамелі
    public bool hasDough = false;                     // Для тіста
    public bool hasRotten = false;                    // Гнилі продукти, що поширюються

    public List<string> startBonuses = new List<string>(); // Наприклад: ["Spoon", "Ladle", "Vinegar"]
}
