using UnityEngine;
using UnityEngine.UI;

public class Piece : MonoBehaviour
{
    public Vector2Int position;
    public string pieceType;

    private Image image;

    void Awake()
    {
        image = GetComponent<Image>();
        gameObject.AddComponent<Button>().onClick.AddListener(OnClick);
    }

    public void Initialize(string type)
    {
        pieceType = type;
    }

    public void OnClick()
    {
        Debug.Log($"Piece clicked: Type = {pieceType}, Position = {position}");
        GridManager.Instance.HandlePieceClick(this);
    }

    public void Select()
    {
        image.color = Color.yellow;
    }

    public void Deselect()
    {
        image.color = Color.white;
    }
}


