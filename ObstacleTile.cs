using UnityEngine;

public abstract class ObstacleTile : MonoBehaviour
{
    public int layers;
    public Sprite[] layerSprites;
    public GridGenerator gridGenerator;
    public int row;
    public int col;

    public virtual void Hit(GridGenerator gridGenerator, int row, int col)
    {
        layers--;

        if (layers > 0)
        {
            UpdateSprite();
        }
        else
        {
            Destroy(gameObject);

            // 🔓 Розблоковуємо фрукт під тістом
            if (gridGenerator != null)
            {
                var piece = gridGenerator.gridPieces[row, col];
                if (piece != null)
                {
                    piece.isLocked = false;
                    Debug.Log($"🔓 Розблоковано фрукт під тістом на [{row}, {col}]");
                }
            }
        }
    }

    public abstract void UpdateSprite();
}
