using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public Canvas canvas;
    public Sprite cellSprite;
    public Sprite[] pieceSprites;
    public int rows = 9;
    public int columns = 9;

    private float cellSize;
    private RectTransform gridContainer;
    private Piece selectedPiece = null;
    private bool isCheckingMatches = false;

    public static GridManager Instance;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        GenerateGrid();
    }

    public void HandlePieceClick(Piece piece)
    {
        if (selectedPiece == null)
        {
            selectedPiece = piece;
            piece.Select();
        }
        else
        {
            if (IsAdjacent(selectedPiece, piece))
            {
                SwapPieces(selectedPiece, piece);
                selectedPiece.Deselect();
                selectedPiece = null;

                StartCoroutine(CheckForMatchesAfterMove());
            }
            else
            {
                selectedPiece.Deselect();
                selectedPiece = piece;
                piece.Select();
            }
        }
    }

    private bool IsAdjacent(Piece piece1, Piece piece2)
    {
        int dx = Mathf.Abs(piece1.position.x - piece2.position.x);
        int dy = Mathf.Abs(piece1.position.y - piece2.position.y);
        return (dx + dy == 1);
    }

    private void SwapPieces(Piece piece1, Piece piece2)
    {
        Vector2Int tempPosition = piece1.position;
        piece1.position = piece2.position;
        piece2.position = tempPosition;

        Transform parent1 = piece1.transform.parent;
        Transform parent2 = piece2.transform.parent;

        piece1.transform.SetParent(parent2, false);
        piece2.transform.SetParent(parent1, false);
    }

    private List<Piece> FindMatches()
    {
        List<Piece> matchedPieces = new List<Piece>();
        // Перевірка горизонтальних збігів
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns - 2; col++)
            {
                List<Piece> currentMatch = new List<Piece>();
                for (int i = 0; i < columns - col; i++)
                {
                    Piece current = GetPieceAt(row, col + i);
                    if (current != null && currentMatch.Count == 0 ||
                        (current != null && current.pieceType == currentMatch[0].pieceType))
                    {
                        currentMatch.Add(current);
                    }
                    else
                    {
                        break;
                    }
                }
                if (currentMatch.Count >= 3)
                {
                    matchedPieces.AddRange(currentMatch);
                }
            }
        }
        // Перевірка вертикальних збігів
        for (int col = 0; col < columns; col++)
        {
            for (int row = 0; row < rows - 2; row++)
            {
                List<Piece> currentMatch = new List<Piece>();
                for (int i = 0; i < rows - row; i++)
                {
                    Piece current = GetPieceAt(row + i, col);
                    if (current != null && currentMatch.Count == 0 ||
                        (current != null && current.pieceType == currentMatch[0].pieceType))
                    {
                        currentMatch.Add(current);
                    }
                    else
                    {
                        break;
                    }
                }
                if (currentMatch.Count >= 3)
                {
                    matchedPieces.AddRange(currentMatch);
                }
            }
        }
        return matchedPieces;
    }

    private IEnumerator CheckForMatchesAfterMove()
    {
        if (isCheckingMatches)
            yield break;

        isCheckingMatches = true;

        while (true)
        {
            List<Piece> matchedPieces = FindMatches();

            if (matchedPieces.Count >= 3)
            {
                DestroyMatchedPieces(matchedPieces);
                yield return new WaitForSeconds(0.5f);
                FillEmptyCells();
                yield return new WaitForSeconds(0.5f);
            }
            else
            {
                break;
            }
        }

        isCheckingMatches = false;
    }

    private Piece GetPieceAt(int row, int col)
    {
        foreach (Transform child in gridContainer)
        {
            Cell cell = child.GetComponent<Cell>();
            if (cell != null && cell.position.x == row && cell.position.y == col)
            {
                return cell.currentPiece?.GetComponent<Piece>();
            }
        }
        return null;
    }

    private void DestroyMatchedPieces(List<Piece> matchedPieces)
    {
        foreach (Piece piece in matchedPieces)
        {
            if (piece != null)
            {
                Cell parentCell = piece.transform.parent.GetComponent<Cell>();
                if (parentCell != null)
                {
                    parentCell.currentPiece = null;
                }
                Destroy(piece.gameObject);
            }
        }
    }

    public void FillEmptyCells()
    {
        for (int row = rows - 1; row >= 0; row--)
        {
            for (int col = 0; col < columns; col++)
            {
                Cell cell = GetCellAt(row, col);

                if (cell != null && cell.currentPiece == null)
                {
                    DropPiecesAbove(cell);
                }
            }
        }
    }


    private void DropPiecesAbove(Cell emptyCell)
    {
        for (int row = emptyCell.position.x - 1; row >= 0; row--)
        {
            Cell aboveCell = GetCellAt(row, emptyCell.position.y);

            if (aboveCell != null && aboveCell.currentPiece != null)
            {
                Piece piece = aboveCell.currentPiece.GetComponent<Piece>();
                piece.transform.SetParent(emptyCell.transform, false);
                piece.position = emptyCell.position;

                emptyCell.currentPiece = aboveCell.currentPiece;
                aboveCell.currentPiece = null;

                return;
            }
        }

        AddPieceToCell(emptyCell);
    }

    public void GenerateGrid()
    {
        if (gridContainer == null)
        {
            GameObject container = new GameObject("GridContainer");
            container.transform.SetParent(canvas.transform, false);
            gridContainer = container.AddComponent<RectTransform>();
            gridContainer.anchorMin = new Vector2(0.5f, 0.5f);
            gridContainer.anchorMax = new Vector2(0.5f, 0.5f);
            gridContainer.pivot = new Vector2(0.5f, 0.5f);
        }

        foreach (Transform child in gridContainer)
        {
            Destroy(child.gameObject);
        }

        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        float canvasWidth = canvasRect.rect.width;
        float canvasHeight = canvasRect.rect.height;

        float maxCellWidth = canvasWidth / columns;
        float maxCellHeight = canvasHeight / rows;
        cellSize = Mathf.Min(maxCellWidth, maxCellHeight);

        float gridWidth = columns * cellSize;
        float gridHeight = rows * cellSize;

        gridContainer.sizeDelta = new Vector2(gridWidth, gridHeight);

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                GameObject cellObject = CreateUIElement($"Cell {row},{col}", cellSprite, gridContainer);
                RectTransform rectTransform = cellObject.GetComponent<RectTransform>();

                rectTransform.sizeDelta = new Vector2(cellSize, cellSize);
                float posX = col * cellSize - gridWidth / 2 + cellSize / 2;
                float posY = -row * cellSize + gridHeight / 2 - cellSize / 2;
                rectTransform.anchoredPosition = new Vector2(posX, posY);

                Cell cell = cellObject.AddComponent<Cell>();
                cell.position = new Vector2Int(row, col);

                AddPieceToCell(cell);
            }
        }
    }

    private void AddPieceToCell(Cell cell)
    {
        int randomIndex = Random.Range(0, pieceSprites.Length);
        Sprite pieceSprite = pieceSprites[randomIndex];

        GameObject pieceObject = CreateUIElement("Piece", pieceSprite, cell.transform);
        RectTransform rectTransform = pieceObject.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(cellSize, cellSize);

        Piece piece = pieceObject.AddComponent<Piece>();
        piece.Initialize(pieceSprite.name);

        piece.position = cell.position;

        cell.currentPiece = pieceObject;
    }

    private GameObject CreateUIElement(string name, Sprite sprite, Transform parent)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);

        Image image = obj.AddComponent<Image>();
        image.sprite = sprite;

        return obj;
    }

    private Cell GetCellAt(int row, int col)
    {
        foreach (Transform child in gridContainer)
        {
            Cell cell = child.GetComponent<Cell>();
            if (cell != null && cell.position.x == row && cell.position.y == col)
            {
                return cell;
            }
        }
        return null;
    }
}
