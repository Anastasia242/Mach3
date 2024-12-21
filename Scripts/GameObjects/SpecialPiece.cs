using UnityEngine;

public class SpecialPiece : MonoBehaviour
{
    public string specialType;

    public void Activate()
    {
        Debug.Log($"Special Piece Activated: {specialType}");
        // Логіка для спеціальної фігурки
    }
}
