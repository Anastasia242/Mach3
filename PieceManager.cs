using DG.Tweening;
using UnityEngine;

public class PieceManager : MonoBehaviour
{
    public int Row { get; private set; }
    public int Col { get; private set; }

private GridGenerator gridGenerator;
    private SpriteRenderer spriteRenderer;

    public enum PieceType
    {
        Normal,
        Knife,
        Fork,
        AreaBomb,
        ColorBomb,
        Ingredient,
        Rotten

    }

    public PieceType pieceType = PieceType.Normal;
    public Sprite specialColorSprite; // Колір спеціального елемента
    public bool wasSwapped = false;
    public string assignedName; // 🔥 Назва кольору (наприклад, "Apple", "Banana")
    public bool isLocked = false;
    public string baseNameForRotten; // наприклад, "Apple"



    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Initialize(int row, int col, GridGenerator generator, PieceType type = PieceType.Normal, Sprite colorSprite = null)
    {
        Row = row;
        Col = col;
        gridGenerator = generator;

        // Отримуємо спрайт, якщо не передано
        if (colorSprite == null)
        {
            Debug.LogWarning($"⚠️ `colorSprite` == null для [{row}, {col}]. Спробуємо знайти за спрайтом...");
            colorSprite = GetComponent<SpriteRenderer>().sprite;
        }

        // Запасний варіант, якщо навіть тоді null
        if (colorSprite == null)
        {
            Debug.LogError($"❌ `colorSprite` все ще null для [{row}, {col}]. Використовується `defaultSprite`!");
            colorSprite = generator.defaultSprite;
        }

        // ✅ Автоматичне визначення типу за спрайтом, якщо тип = Normal
        pieceType = (type == PieceType.Normal)
            ? generator.DetectPieceTypeBySprite(colorSprite)
            : type;

        spriteRenderer.sprite = colorSprite;
        assignedName = gridGenerator.GetFruitNameBySprite(colorSprite);
        Debug.Log($"🎯 Ініціалізація: [{row}, {col}] => {assignedName} ({pieceType})");
    }

    public void SetSprite(Sprite sprite)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = sprite;
        }
    }

    public void UpdatePosition(int newRow, int newCol)
    {
        Row = newRow;
        Col = newCol;
        transform.position = gridGenerator.GetPosition(newRow, newCol);
    }

    public void Highlight()
    {
        spriteRenderer.color = Color.gray; // Виділення
    }

    public void Unhighlight()
    {
        spriteRenderer.color = Color.white; // Зняття виділення
    }

    public void AnimateSpawn()
    {
        if (transform == null) return;

        try
        {
            transform.localScale = Vector3.zero;
            transform.DOScale(Vector3.one * 0.04f, 0.3f)
                     .SetEase(Ease.OutBack)
                     .SetUpdate(true); // ✅ дозволяє анімації працювати навіть при TimeScale = 0
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ AnimateSpawn error: {ex.Message}");
        }
    }

    public void AnimateDestroy()
    {
        transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack)
                 .OnComplete(() => Destroy(gameObject));
    }

    public void MoveToPosition(Vector3 targetPosition)
    {
        float delay = (8 - Row) * 0.02f; // Можна адаптувати, якщо у тебе не 8 рядків
        transform.DOMove(targetPosition, 0.3f)
                 .SetEase(Ease.InOutQuad)
                 .SetDelay(delay);
    }

    public void AnimateBlinkAndFade(System.Action onComplete = null)
    {
        Sequence seq = DOTween.Sequence();
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();

        seq.Append(renderer.DOFade(0.3f, 0.1f).SetLoops(4, LoopType.Yoyo)) // блимає
           .Join(transform.DOPunchScale(Vector3.one * 0.08f, 0.3f, 5, 0.05f)) // 🔧 м'якша пульсація
           .Append(renderer.DOFade(0f, 0.2f))
           .OnComplete(() =>
           {
               onComplete?.Invoke();
               Destroy(gameObject);
           });
    }

    private void OnMouseDown()
    {
        Debug.Log($"🖱️ Клік по {name}, Тип: {pieceType}");

        InteractionManager interactionManager = FindObjectOfType<InteractionManager>();
        if (interactionManager != null)
        {
            interactionManager.HandlePieceClick(this);
        }
    }

}