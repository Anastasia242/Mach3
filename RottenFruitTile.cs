using UnityEngine;

public class RottenFruitTile : MonoBehaviour
{
    public int turnsBeforeSpread = 2;
    private int currentTurns;
    private GridGenerator gridGenerator;
    public int row, col;

    public void Initialize(GridGenerator generator, int r, int c)
    {
        gridGenerator = generator;
        row = r;
        col = c;
        currentTurns = turnsBeforeSpread;
    }

    public void OnTurnPassed()
    {
        currentTurns--;
        if (currentTurns <= 0)
        {
            SpreadRotten();
            currentTurns = turnsBeforeSpread;
        }
    }

    private void SpreadRotten()
    {
        Vector2Int[] directions = new Vector2Int[]
        {
        new Vector2Int(0, 1),
        new Vector2Int(0, -1),
        new Vector2Int(1, 0),
        new Vector2Int(-1, 0)
        };

        foreach (var dir in directions)
        {
            int newRow = row + dir.x;
            int newCol = col + dir.y;

            if (gridGenerator.IsValidPosition(newRow, newCol))
            {
                var target = gridGenerator.gridPieces[newRow, newCol];

                if (target != null &&
                    target.pieceType == PieceManager.PieceType.Normal) // тільки нормальні
                {
                    // Видаляємо старий фрукт
                    Destroy(target.gameObject);

                    // Отримуємо символ гнилого фрукта для цього кольору
                    string rottenSymbol = "R" + target.assignedName.Substring(0, 1).ToUpper();

                    // Створюємо гнилий фрукт на цьому місці
                    gridGenerator.CreatePieceFromSymbol(newRow, newCol, rottenSymbol);

                    Debug.Log($"☠️ Гниль поширилась на [{newRow},{newCol}]");
                    break; // поширюємось тільки на одного сусіда
                }
            }
        }
    }


    public void Clean()
    {
        Destroy(gameObject); // або якась анімація очищення
    }
}
