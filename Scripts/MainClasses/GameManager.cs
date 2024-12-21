// GameManager.cs
using UnityEngine;

public class GameManager : MonoBehaviour
{
    void Start()
    {
        Debug.Log("Гра почалася, генеруємо сітку...");
        GridManager.Instance.GenerateGrid();
    }

    public void OnMatchFound(Piece[] matchedPieces)
    {
        Debug.Log("Комбінація знайдена! Кількість фігурок: " + matchedPieces.Length);

        // Видаляємо всі знайдені фігурки
        foreach (Piece piece in matchedPieces)
        {
            if (piece != null)
            {
                Destroy(piece.gameObject);
                Debug.Log($"Фігурка {piece.pieceType} видалена.");
            }
        }

        // Заповнюємо порожні клітинки
        Debug.Log("Заповнюємо порожні клітинки...");
        GridManager.Instance.FillEmptyCells();
    }
}