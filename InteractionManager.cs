using System.Collections;
using UnityEngine;
using static PieceManager;

public class InteractionManager : MonoBehaviour
{
    [SerializeField] private GridGenerator gridGenerator;
    private PieceManager selectedPiece;


public void HandlePieceClick(PieceManager clickedPiece)
    {


        Debug.Log($"HandlePieceClick викликано для: {clickedPiece.name}, Тип: {clickedPiece.pieceType}");

        if (selectedPiece == null)
        {
            selectedPiece = clickedPiece;
            selectedPiece.Highlight();
        }
        else
        {
            if (selectedPiece == clickedPiece)
            {
                selectedPiece.Unhighlight();
                selectedPiece = null;
                return;
            }

            if (AreAdjacent(selectedPiece, clickedPiece))
            {
                gridGenerator.SwapPieces(selectedPiece, clickedPiece);
                gridGenerator.CheckForMatchesAfterSwap(selectedPiece, clickedPiece); // ✅ Залишаємо лише один виклик

                // ⬇️ Ось тут додаємо ↓↓↓
                FindObjectOfType<TimerManager>()?.DecrementMove();

                selectedPiece.Unhighlight();
                selectedPiece = null;
            }
            else
            {
                selectedPiece.Unhighlight();
                selectedPiece = clickedPiece;
                selectedPiece.Highlight();
            }
        }
    }

    private bool AreAdjacent(PieceManager piece1, PieceManager piece2)
    {
        int rowDiff = Mathf.Abs(piece1.Row - piece2.Row);
        int colDiff = Mathf.Abs(piece1.Col - piece2.Col);
        return (rowDiff == 1 && colDiff == 0) || (rowDiff == 0 && colDiff == 1);
    }



}