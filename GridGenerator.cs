using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.EditorTools;
using UnityEngine;
using static PieceManager;

public class GridGenerator : MonoBehaviour
{
    int rows = 9, cols = 9;
    public GameObject cellPrefab;
    public GameObject piecePrefab;
    public Transform gridParent;
    public List<Sprite> pieceSprites = new List<Sprite>();

public RectTransform GridAreaRect; // прив’язати фон сітки з Canvas

    public Sprite knifeSprite;
    public Sprite forkSprite;
    public Sprite areaBombSprite;
    public Sprite colorBombSprite;



// **Додані змінні для спрайтів фруктів**
public Sprite appleSprite;
    public Sprite bananaSprite;
    public Sprite bilberrySprite;
    public Sprite pearSprite;
    public Sprite defaultSprite;

    // 🔥 Додаємо змінні для унікальних спрайтів спеціальних елементів
    public Sprite redKnifeSprite;
    public Sprite yellowKnifeSprite;
    public Sprite blueKnifeSprite;
    public Sprite greenKnifeSprite;


public Sprite redForkSprite;
    public Sprite yellowForkSprite;
    public Sprite blueForkSprite;
    public Sprite greenForkSprite;

    public Sprite redBombSprite;
    public Sprite yellowBombSprite;
    public Sprite blueBombSprite;
    public Sprite greenBombSprite;

    private Sprite[,] gridSprites;
    public PieceManager[,] gridPieces;
    private bool[,] reservedPositions; // Масив для резервування позицій
    public PieceManager selectedPiece = null; // ✅ Зберігає вибраний елемент для обміну


    public bool useLevelData = false; // Якщо true, завантажується рівень, якщо false – генерується випадкова сітка
    public bool IsGridIdle = false;
    public GoalManager goalManager; // ← додай у GridGenerator.cs
    [SerializeField] private GameObject doughTilePrefab; // призначити в інспекторі
    private ObstacleTile[,] obstacleTiles;
    public Sprite ingredientSprite; 
    public Sprite rottenAppleSprite;
    public Sprite rottenBananaSprite;
    public Sprite rottenBilberrySprite;
    public Sprite rottenPearSprite;
    //private RottenFruitTile[,] rottenTiles;
    private List<RottenFruitTile> rottenFruits = new List<RottenFruitTile>();




    private Dictionary<string, (Sprite sprite, PieceType type)> pieceMapping = new Dictionary<string, (Sprite, PieceType)>();

    private void Awake()
    {
        InitializePieceMapping();
    }

    //Ініціалізує масиви сітки та резерву позицій і викликає метод генерації сітки.
    private void Start()
    {
        gridSprites = new Sprite[rows, cols]; // Ініціалізуємо масив спрайтів
        gridPieces = new PieceManager[rows, cols];
        reservedPositions = new bool[rows, cols]; // Ініціалізуємо масив для резервування позицій
        IsGridIdle = true;
        //rottenTiles = new RottenFruitTile[rows, cols];


        if (!useLevelData)
        {
            GenerateGrid(); // Використовуємо випадкову генерацію лише якщо не завантажуємо рівень
        }
    }

    // -------------------------
    // Ініціалізація та розміщення фігурок
    // -------------------------

    //Повертає фігурку, розташовану в заданій позиції сітки.
    public PieceManager GetPiece(int row, int col)
    {
        return gridPieces[row, col];
    }

    //Встановлює фігурку в зазначену позицію сітки.
    public void SetPiece(int row, int col, PieceManager piece)
    {
        gridPieces[row, col] = piece;
    }

    //Генерує початкову сітку фігурок із правильним розташуванням і без початкових збігів.
    public void GenerateGrid()
    {
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                // Створення клітинки
                GameObject cell = Instantiate(cellPrefab, gridParent);
                cell.transform.position = GetPosition(row, col);
                cell.transform.localScale = Vector3.one * 0.03f;

                // Створення фігурки
                GameObject piece = Instantiate(piecePrefab, gridParent);
                piece.transform.position = GetPosition(row, col);

                // Вибір допустимого спрайта
                Sprite validSprite = GetValidSprite(row, col);
                piece.GetComponent<SpriteRenderer>().sprite = validSprite;

                // Записуємо спрайт в масив
                gridSprites[row, col] = validSprite;

                // Масштаб фігурки
                piece.transform.localScale = Vector3.one * 0.04f;

                // Ініціалізуємо фігурку та додаємо її до масиву gridPieces
                PieceManager pieceManager = piece.GetComponent<PieceManager>();
                pieceManager.Initialize(row, col, this);
                gridPieces[row, col] = pieceManager; // Додаємо фігурку до масиву
            }
        }
    }

    //Вибирає спрайт для фігурки, уникаючи випадкових початкових збігів.
    private Sprite GetValidSprite(int row, int col)
    {
        List<Sprite> possibleSprites = new List<Sprite>(pieceSprites);

        if (possibleSprites.Count == 0)
        {
            Debug.LogError("❌ GetValidSprite: pieceSprites порожній! Переконайся, що всі спрайти додані в інспекторі.");
            return defaultSprite; // Запасний варіант
        }

        if (col > 1 && gridSprites[row, col - 1] == gridSprites[row, col - 2])
        {
            possibleSprites.Remove(gridSprites[row, col - 1]);
        }

        if (row > 1 && gridSprites[row - 1, col] == gridSprites[row - 2, col])
        {
            possibleSprites.Remove(gridSprites[row - 1, col]);
        }

        if (possibleSprites.Count == 0)
        {
            Debug.LogWarning($"⚠️ GetValidSprite: всі можливі варіанти видалені для [{row}, {col}]. Використовуємо defaultSprite.");
            return defaultSprite;
        }

        return possibleSprites[Random.Range(0, possibleSprites.Count)];
    }

    //Обчислює світові координати клітинки сітки на основі її рядка та стовпця.
    public Vector3 GetPosition(int row, int col)
    {
        float cellSize = 0.5f;
        float offsetX = -(cols - 1) * cellSize / 2;

        float verticalShift = 1f;
        float offsetY = (rows - 1) * cellSize / 2 + verticalShift;

        return new Vector3(col * cellSize + offsetX, -row * cellSize + offsetY, 0);
    }

    // -------------------------
    // завантаження рівнів
    // -------------------------

    // 1️⃣ Ініціалізація мапи відповідностей
    private void InitializePieceMapping()
    {
        pieceMapping.Add("A", (appleSprite, PieceType.Normal));
        pieceMapping.Add("B", (bananaSprite, PieceType.Normal));
        pieceMapping.Add("L", (bilberrySprite, PieceType.Normal));
        pieceMapping.Add("P", (pearSprite, PieceType.Normal));
        pieceMapping.Add("K", (knifeSprite, PieceType.Knife));
        pieceMapping.Add("F", (forkSprite, PieceType.Fork));
        pieceMapping.Add("X", (areaBombSprite, PieceType.AreaBomb));
        pieceMapping.Add("C", (colorBombSprite, PieceType.ColorBomb));
        pieceMapping.Add("RA", (rottenAppleSprite, PieceType.Rotten));
        pieceMapping.Add("RB", (rottenBananaSprite, PieceType.Rotten));
        pieceMapping.Add("RL", (rottenBilberrySprite, PieceType.Rotten));
        pieceMapping.Add("RP", (rottenPearSprite, PieceType.Rotten));
        pieceMapping["I"] = (ingredientSprite, PieceType.Ingredient);
    }

    // 2️⃣ Отримання спрайта та типу через один метод
    private (Sprite, PieceType) GetPieceData(string symbol)
    {
        if (pieceMapping.TryGetValue(symbol, out var data))
        {
            return data;
        }

        Debug.LogWarning($"⚠️ Невідомий символ '{symbol}', використовується значення за замовчуванням.");
        return (defaultSprite, PieceType.Normal);
    }

    // 3️⃣ Використання нового підходу у створенні об'єкта
    public void CreatePieceFromSymbol(int row, int col, string symbol)
    {
        if (string.IsNullOrEmpty(symbol))
        {
            Debug.LogError($"❌ Порожній символ на позиції [{row}, {col}]!");
            return;
        }

        // 🧱 Якщо це тісто: D1 або D2
        if (symbol.StartsWith("D"))
        {
            // 🎮 Спочатку створюємо випадковий фрукт під тістом
            Sprite newSprite = GetValidSprite(row, col);
            GameObject piece = Instantiate(piecePrefab, gridParent);
            piece.transform.position = GetPosition(row, col);
            piece.transform.localScale = Vector3.one * 0.04f;
            piece.GetComponent<SpriteRenderer>().sprite = newSprite;

            PieceManager pieceManager = piece.GetComponent<PieceManager>();
            pieceManager.Initialize(row, col, this, PieceType.Normal, newSprite);
            pieceManager.isLocked = true; // 🔒 фрукти під тістом заблоковані
            gridPieces[row, col] = pieceManager;

            // 🍞 Тепер створюємо тісто зверху
            if (!int.TryParse(symbol.Substring(1), out int layerCount))
            {
                Debug.LogError($"❌ Неправильний формат для DoughTile: '{symbol}'");
                return;
            }

            GameObject doughTile = Instantiate(doughTilePrefab, gridParent);
            doughTile.transform.position = GetPosition(row, col);
            doughTile.transform.localScale = Vector3.one * 0.04f;

            var tile = doughTile.GetComponent<DoughTile>();
            tile.layers = layerCount;
            tile.UpdateSprite();
            tile.gridGenerator = this;
            tile.row = row;
            tile.col = col;


            obstacleTiles[row, col] = tile;
            return;
        }

        // ☠️ Гнилі фрукти: RA, RB, RL, RP
        if (symbol.StartsWith("R") && symbol.Length == 2)
        {
            (Sprite rottenSprite, PieceType rottenType) = GetPieceData(symbol);

            GameObject rottenPiece = Instantiate(piecePrefab, gridParent);
            rottenPiece.transform.position = GetPosition(row, col);
            rottenPiece.transform.localScale = Vector3.one * 0.04f;
            rottenPiece.GetComponent<SpriteRenderer>().sprite = rottenSprite;

            PieceManager manager = rottenPiece.GetComponent<PieceManager>();
            manager.Initialize(row, col, this, rottenType, rottenSprite);

            // Додаємо компонент гнилі
            RottenFruitTile rot = rottenPiece.AddComponent<RottenFruitTile>();
            rot.Initialize(this, row, col);
            rottenFruits.Add(rot); // ✅ додаємо до списку

            gridPieces[row, col] = manager;
            return;
        }


        // 🧂 Якщо це інгредієнт
        if (symbol == "I")
        {
            GameObject ingredient = Instantiate(piecePrefab, gridParent);
            ingredient.transform.position = GetPosition(row, col);
            ingredient.transform.localScale = Vector3.one * 0.04f;

            ingredient.GetComponent<SpriteRenderer>().sprite = ingredientSprite; // 🧠 або GetPieceData("I").Item1

            PieceManager pieceManager = ingredient.GetComponent<PieceManager>();
            pieceManager.Initialize(row, col, this, PieceType.Ingredient, ingredientSprite);

            gridPieces[row, col] = pieceManager;
            return;
        }


        // 🎮 Інакше — звичайна фігурка
        GameObject normalPiece = Instantiate(piecePrefab, gridParent);
        normalPiece.transform.position = GetPosition(row, col);
        normalPiece.transform.localScale = Vector3.one * 0.04f;

        (Sprite sprite, PieceType type) = GetPieceData(symbol);
        if (sprite == null) sprite = defaultSprite;

        normalPiece.GetComponent<SpriteRenderer>().sprite = sprite;
        PieceManager normalManager = normalPiece.GetComponent<PieceManager>();
        normalManager.Initialize(row, col, this, type, sprite);


        gridPieces[row, col] = normalManager;
    }


    //тест
    public Dictionary<string, Sprite> GetSymbolSpriteMapping()
    {
        Dictionary<string, Sprite> result = new();

        foreach (var pair in pieceMapping)
        {
            result[pair.Key] = pair.Value.Item1;
        }

        return result;
    }



    public void GenerateLevelFromData(string[][] levelData)
    {
        if (levelData == null || levelData.Length == 0 || levelData[0].Length == 0)
        {
            Debug.LogError("❌ Передані дані рівня пусті або некоректні!");
            return;
        }

        Debug.Log($"✅ Генеруємо рівень із JSON-даних: {levelData.Length} x {levelData[0].Length}");

        rows = levelData.Length;
        cols = levelData[0].Length;

        gridSprites = new Sprite[rows, cols];
        gridPieces = new PieceManager[rows, cols];
        reservedPositions = new bool[rows, cols];
        obstacleTiles = new ObstacleTile[rows, cols]; // ⬅️ нове

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                string symbol = levelData[row][col];
                if (string.IsNullOrEmpty(symbol))
                {
                    Debug.LogError($"❌ Порожнє значення у JSON на позиції [{row}, {col}]!");
                    continue;
                }

                CreatePieceFromSymbol(row, col, symbol);
            }
        }

        Invoke(nameof(CheckForMatchesAfterFill), 0.5f);
    }




    // Видаляє старі елементи, щоб уникнути дублювання при завантаженні рівня
    private void ClearGrid()
    {
        foreach (Transform child in gridParent)
        {
            Destroy(child.gameObject);
        }
    }

    // -------------------------
    // Обробка обміну фігурок
    // -------------------------

    //Міняє місцями дві фігурки в сітці й оновлює їхні позиції.
    public void SwapPieces(PieceManager piece1, PieceManager piece2)
    {
        IsGridIdle = false;
        Debug.Log("SwapPieces викликано для фігурок: " + piece1.name + " та " + piece2.name);

        // ✅ Граємо звук обміну
        SoundManager.Instance?.PlaySwap();

        // Позначаємо, що спеціальний елемент був обміняний
        if (piece1.pieceType != PieceType.Normal)
            piece1.wasSwapped = true;

        if (piece2.pieceType != PieceType.Normal)
            piece2.wasSwapped = true;

        // Зберігаємо початкові позиції
        int row1 = piece1.Row, col1 = piece1.Col;
        int row2 = piece2.Row, col2 = piece2.Col;

        // Міняємо місцями в масиві
        gridPieces[row1, col1] = piece2;
        gridPieces[row2, col2] = piece1;

        // Оновлюємо позиції в об'єктах
        piece1.UpdatePosition(row2, col2);
        piece2.UpdatePosition(row1, col1);
    }

    public void TrySwap(PieceManager piece1, PieceManager piece2)
    {
        IsGridIdle = false;
        if (piece1 == piece2) return;

        Debug.Log($"🔄 Обмін {piece1.name} ↔ {piece2.name}");

        // ✅ Позначаємо, що спеціальний елемент був обміняний
        if (piece1.pieceType != PieceType.Normal) piece1.wasSwapped = true;
        if (piece2.pieceType != PieceType.Normal) piece2.wasSwapped = true;

        // Виконуємо сам обмін
        SwapPieces(piece1, piece2);

        // Після обміну шукаємо збіги
        CheckForMatchesAfterSwap(piece1, piece2);
    }

    //Анімує обертання фігурки при скасуванні невдалого ходу або обміні.
    private IEnumerator RotatePiece(GameObject piece, float duration, float angle)
    {
        IsGridIdle = false;
        float elapsedTime = 0;
        Quaternion startRotation = piece.transform.rotation; // Початкова орієнтація
        Quaternion targetRotation = Quaternion.Euler(0, 0, angle); // Обертання на певний кут

        while (elapsedTime < duration / 2) // Обертаємо до цільового кута
        {
            piece.transform.rotation = Quaternion.Lerp(startRotation, targetRotation, elapsedTime / (duration / 2));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        elapsedTime = 0;
        while (elapsedTime < duration / 2) // Повертаємо фігурку назад
        {
            piece.transform.rotation = Quaternion.Lerp(targetRotation, startRotation, elapsedTime / (duration / 2));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        piece.transform.rotation = startRotation; // Переконуємося, що повернулися в початковий стан
    }

    //Повертає фігурки на початкові місця, якщо збігів після обміну не знайдено.
    private IEnumerator HandleInvalidSwap(PieceManager piece1, PieceManager piece2)
    {
        // Анімація обертання
        yield return StartCoroutine(RotatePiece(piece1.gameObject, 0.3f, 15f));
        yield return StartCoroutine(RotatePiece(piece2.gameObject, 0.3f, -15f));

        // Після завершення обертання повертаємо фігурки назад
        SwapPieces(piece1, piece2);
        IsGridIdle = true;
    }

    // -------------------------
    // Пошук і обробка збігів
    // -------------------------

    public void CheckForMatchesAfterSwap(PieceManager piece1, PieceManager piece2)
    {
        IsGridIdle = false;
        Debug.Log("▶️ CheckForMatchesAfterSwap()");
        StartCoroutine(DelayedCheckAfterSwap(piece1, piece2));
    }

    private IEnumerator DelayedCheckAfterSwap(PieceManager piece1, PieceManager piece2)
    {


        IsGridIdle = false;
        yield return new WaitForSeconds(0.5f);

        // 🧺 Обмін між двома інгредієнтами (книгами) — не дозволено
        if (piece1.pieceType == PieceType.Ingredient && piece2.pieceType == PieceType.Ingredient)
        {
            Debug.Log("📚 Обмін між двома книгами — скасовується");
            StartCoroutine(HandleInvalidSwap(piece1, piece2));
            yield break;
        }


        // 🌌 Дві ColorBomb — знищуємо всю сітку
        if (piece1.pieceType == PieceType.ColorBomb && piece2.pieceType == PieceType.ColorBomb)
        {
            Debug.Log("🌌 Дві ColorBomb обмінялись — знищуємо всю сітку!");
            StartCoroutine(DestroyEntireGrid());
            yield break;
        }

        // 🎆 Якщо одна з ColorBomb — активуємо
        if (piece1.pieceType == PieceType.ColorBomb)
        {
            Debug.Log("🎆 ColorBomb активується після обміну (piece1)");
            StartCoroutine(ActivateColorBombWithDelay(piece1, piece2));
            yield break;
        }

        if (piece2.pieceType == PieceType.ColorBomb)
        {
            Debug.Log("🎆 ColorBomb активується після обміну (piece2)");
            StartCoroutine(ActivateColorBombWithDelay(piece2, piece1));
            yield break;
        }

        // 💥 Два спецелементи — комбінація
        if (piece1.pieceType != PieceType.Normal && piece2.pieceType != PieceType.Normal)
        {
            Debug.Log("💥 Два спецелементи обмінялись — активуємо комбінацію!");
            StartCoroutine(ActivateSpecialCombo(piece1, piece2));
            yield break;
        }

        // 🔍 Перевірка на матчі
        List<List<PieceManager>> matchGroups = FindMatchGroups();

        if (matchGroups.Count > 0)
        {
            Debug.Log($"📊 Нові комбінації знайдені після обміну: {matchGroups.Count}");
            StartCoroutine(HandleMatchesWithDelay(matchGroups, piece1, piece2));
        }
        else
        {
            Debug.Log("❌ Комбінацій після обміну не знайдено — повертаємо назад");
            StartCoroutine(HandleInvalidSwap(piece1, piece2));
        }
    }

    private IEnumerator DestroyEntireGrid()
    {
        IsGridIdle = false;
        Debug.Log("💣 DestroyEntireGrid: пульсація + хвиля знищення");

        SoundManager.Instance?.PlayColorBomb();
        yield return new WaitForSeconds(0.2f);

        // 1️⃣ ПУЛЬСАЦІЯ ВСІХ ФІШОК (одночасно)
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                var piece = gridPieces[row, col];
                if (piece != null)
                {
                    piece.transform.DOScale(0.05f, 0.15f).SetLoops(2, LoopType.Yoyo); // пульс
                }
            }
        }

        yield return new WaitForSeconds(0.4f); // час на пульсацію

        // 2️⃣ ХВИЛЯ ЗНИЩЕННЯ рядками зверху вниз
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                var piece = gridPieces[row, col];
                if (piece != null)
                {
                    piece.AnimateBlinkAndFade(); // або AnimateDestroy()
                    gridPieces[row, col] = null;
                }
            }

            yield return new WaitForSeconds(0.06f); // хвиля по рядках
        }

        yield return new WaitForSeconds(0.4f); // анімація згасання

        DropPieces(); // технічно не обов’язково, але для стабільності
        yield return new WaitForSeconds(0.3f);

        FillEmptySpaces();
        yield return new WaitForSeconds(0.3f);

        yield return StartCoroutine(CheckForMatchesAfterFill());
    }

    private IEnumerator ActivateColorBombWithDelay(PieceManager colorBomb, PieceManager otherPiece)
    {
        IsGridIdle = false;
        Debug.Log("🚀 Активуємо ColorBomb через затримку...");

        yield return new WaitForSeconds(0.25f); // Коротка пауза після обміну

        if (otherPiece == null)
        {
            Debug.LogWarning("⚠️ Інший елемент дорівнює null — переривання активації ColorBomb.");
            yield break;
        }

        // 🔍 Якщо інший елемент — спецелемент
        if (otherPiece.pieceType != PieceType.Normal)
        {
            Debug.Log($"🎯 ColorBomb обміняна зі спецелементом: {otherPiece.pieceType}");
            ActivateAllOfType(otherPiece.pieceType); // 🔥 Активуємо всі спецелементи цього типу
        }
        else
        {
            // 🎨 Якщо інший елемент — звичайна фішка
            Sprite targetSprite = otherPiece.GetComponent<SpriteRenderer>().sprite;

            if (targetSprite == null)
            {
                Debug.LogWarning("⚠️ Інший елемент не має спрайта!");
                yield break;
            }

            Debug.Log($"🌈 ColorBomb активується проти кольору: {targetSprite.name}");
            DestroyPiecesByColor(targetSprite); // Знищення всіх фішок цього кольору
        }

        // 🔊 Звук вибуху
        SoundManager.Instance?.PlayColorBomb();

        // ❌ Не видаляємо вручну colorBomb — її має прибрати логіка HandleMatches або DestroyPiecesByColor, якщо вона була включена
        // Це усуває баг накладання

        yield return new WaitForSeconds(0.3f);
        DropPieces();
        yield return new WaitForSeconds(0.3f);
        yield return StartCoroutine(CheckForMatchesAfterFill());

    }

    private void ActivateAllOfType(PieceType typeToActivate)
    {
        IsGridIdle = false;
        Debug.Log($"🔥 Активуємо всі елементи типу: {typeToActivate}");

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                var piece = gridPieces[row, col];
                if (piece != null && piece.pieceType == typeToActivate)
                {
                    ActivateSpecialPiece(piece);
                }
            }
        }
    }

    private IEnumerator FillEmptySpacesWithDelay()
    {
        IsGridIdle = false;
        yield return new WaitForSeconds(0.1f);
        FillEmptySpaces(); // метод, що заповнює порожні клітинки
    }

    private IEnumerator HandleMatchesWithDelay(List<List<PieceManager>> matchGroups, PieceManager piece1, PieceManager piece2)
    {
        IsGridIdle = false;
        // 1️⃣ Обробляємо знайдені комбінації
        HandleMatches(matchGroups);

        yield return new WaitForSeconds(0.5f);
        DropPieces();
        yield return new WaitForSeconds(0.5f);
        CheckIngredientsOnLastRow();

        // 2️⃣ Шукаємо нові збіги після падіння
        List<List<PieceManager>> newMatchGroups = FindMatchGroups();

        if (newMatchGroups.Count > 0)
        {
            HandleMatches(newMatchGroups);
        }

        // 3️⃣ Примусово активуємо ВСІ спецелементи в комбінаціях
        HashSet<PieceManager> allMatched = new HashSet<PieceManager>(newMatchGroups.SelectMany(g => g));
        foreach (var piece in allMatched)
        {
            if (piece != null && piece.pieceType != PieceType.Normal)
            {
                Debug.Log($"🔥 Примусова активація спецелемента: {piece.name} [{piece.pieceType}]");
                ActivateSpecialPiece(piece);
            }
        }
        // ☠️ Оновлюємо гнилі фрукти
        AdvanceRottenFruits();

        IsGridIdle = true;

    }

    // Пошук горизонтальних та вертикальних збігів
    private List<List<PieceManager>> FindMatchGroups()
    {
        IsGridIdle = false;
        Debug.Log("▶️ FindMatchGroups() запущено");
        List<List<PieceManager>> allMatches = new List<List<PieceManager>>();
        bool[,] visited = new bool[rows, cols];

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                if (visited[row, col] || gridPieces[row, col] == null)
                    continue;

                List<PieceManager> horizontalMatch = FindMatch(row, col, 0, 1);
                List<PieceManager> verticalMatch = FindMatch(row, col, 1, 0);

                if (horizontalMatch.Count >= 3)
                {
                    allMatches.Add(horizontalMatch);
                    MarkVisited(horizontalMatch, visited);
                }
                if (verticalMatch.Count >= 3)
                {
                    allMatches.Add(verticalMatch);
                    MarkVisited(verticalMatch, visited);
                }
            }
        }
        return allMatches;
    }
    private void MarkVisited(List<PieceManager> match, bool[,] visited)
    {
        foreach (PieceManager piece in match)
        {
            visited[piece.Row, piece.Col] = true;
        }
    }

    private List<PieceManager> FindMatch(int row, int col, int rowDir, int colDir)
    {
        IsGridIdle = false;
        List<PieceManager> match = new List<PieceManager>();
        PieceManager startPiece = gridPieces[row, col];

        if (startPiece == null)
        {
            return match;
        }

        Sprite startSprite = startPiece.GetComponent<SpriteRenderer>().sprite;

        while (IsValidPosition(row, col) && gridPieces[row, col] != null)
        {
            PieceManager current = gridPieces[row, col];

            if (current == null)
            {
                break;
            }

            if (IsSameColorOrType(startPiece, current))
            {
                match.Add(current);
            }
            else
            {
                break;
            }

            row += rowDir;
            col += colDir;
        }

        return match;
    }

    private bool IsSameColorOrType(PieceManager piece1, PieceManager piece2)
    {
        if (piece1 == null || piece2 == null) return false;

        // 🔒 Забороняємо комбінацію, якщо хоча б один заблокований
        if (piece1.isLocked || piece2.isLocked) return false;

        // ❌ Забороняємо комбінацію з інгредієнтами
        if (piece1.pieceType == PieceType.Ingredient || piece2.pieceType == PieceType.Ingredient)
            return false;

        // 🔒 Забороняємо гнилі фрукти в комбінаціях
        //if (piece1.pieceType == PieceType.Rotten || piece2.pieceType == PieceType.Rotten)
         //   return false;


        // ✅ Звичайна комбінація
        if (piece1.pieceType == PieceType.Normal && piece2.pieceType == PieceType.Normal)
        {
            return piece1.GetComponent<SpriteRenderer>().sprite == piece2.GetComponent<SpriteRenderer>().sprite;
        }

        // ✅ Спецелементи одного кольору
        if (piece1.pieceType != PieceType.Normal || piece2.pieceType != PieceType.Normal)
        {
            return piece1.assignedName == piece2.assignedName;
        }

        return false;
    }



    void DamageAdjacentDough(int row, int col)
    {
        Vector2Int[] directions = new Vector2Int[]
        {
        new Vector2Int(0, 1),   // up
        new Vector2Int(0, -1),  // down
        new Vector2Int(1, 0),   // right
        new Vector2Int(-1, 0),  // left
        };

        foreach (var dir in directions)
        {
            int r = row + dir.x;
            int c = col + dir.y;

            if (IsInsideGrid(r, c))
            {
                var tile = obstacleTiles[r, c];
                if (tile != null)
                {
                    tile.Hit(this, r, c);
                    if (tile.layers <= 0)
                    {
                        obstacleTiles[r, c] = null;
                    }
                }
            }
        }
    }

    private bool IsInsideGrid(int row, int col)
    {
        return row >= 0 && row < rows && col >= 0 && col < cols;
    }


    // Обробка знайдених збігів
    private void HandleMatches(List<List<PieceManager>> matches)
    {
        IsGridIdle = false;
        List<Vector2Int> specialPiecesToCreate = new List<Vector2Int>();
        List<PieceType> specialPieceTypes = new List<PieceType>();
        List<Sprite> specialColors = new List<Sprite>();
        List<PieceManager> specialPiecesToActivate = new List<PieceManager>();

        foreach (List<PieceManager> match in matches)
        {
            SoundManager.Instance?.PlayMatch();

            Vector2Int position = FindBestCenter(match);
            Sprite matchColor = match[0].GetComponent<SpriteRenderer>().sprite;

            bool isL = IsLShape(position.x, position.y);
            bool isT = IsTShape(position.x, position.y);

            if (match.Count >= 5)
            {
                specialPiecesToCreate.Add(position);
                specialPieceTypes.Add(PieceType.ColorBomb);
                specialColors.Add(null);
            }
            else if (isL || isT)
            {
                specialPiecesToCreate.Add(position);
                specialPieceTypes.Add(PieceType.AreaBomb);
                specialColors.Add(matchColor);
            }
            else if (match.Count == 4)
            {
                PieceType specialType = IsHorizontalMatch(match) ? PieceType.Knife : PieceType.Fork;
                specialPiecesToCreate.Add(position);
                specialPieceTypes.Add(specialType);
                specialColors.Add(matchColor);
            }

            foreach (PieceManager piece in match)
            {
                if (piece != null && piece.gameObject != null)
                {
                    // 🎯 Прогрес цілі
                    if (goalManager != null)
                    {
                        if (goalManager.HasGoal(piece.assignedName))
                            goalManager.AddProgress(piece.assignedName);
                    }
                    // 🍞 Розбиваємо тісто поруч
                    DamageAdjacentDough(piece.Row, piece.Col);

                    // 🍞 Перешкода: тісто
                    ObstacleTile obstacle = obstacleTiles[piece.Row, piece.Col];
                    if (obstacle != null)
                    {
                        Debug.Log($"🥖 Удар по тісту на [{piece.Row}, {piece.Col}]");
                        obstacle.Hit(this, piece.Row, piece.Col);

                    }

                    // ⛔️ Гнилі фрукти не знищуються звичайними матчами
                    if (piece.pieceType == PieceManager.PieceType.Rotten)
                    {
                        Debug.Log($"⛔️ Гнилий фрукт [{piece.Row},{piece.Col}] ігнорується у звичайному матчі");
                        continue;
                    }

                    // 💥 Якщо це спецелемент — активуємо
                    if (piece.pieceType != PieceType.Normal)
                        specialPiecesToActivate.Add(piece);

                    // 🧨 Знищення фігури
                    piece.AnimateDestroy();
                    gridPieces[piece.Row, piece.Col] = null;
                }
            }
        }


        // 🎯 Створюємо нові спецелементи
        StartCoroutine(UpdateGridAndCreateSpecial(specialPiecesToCreate, specialPieceTypes, specialColors));

        // 🔥 Активуємо спецелементи
        foreach (PieceManager specialPiece in specialPiecesToActivate)
        {
            ActivateSpecialPiece(specialPiece);
        }

        // ⬇️ Падіння і заповнення
        DropPieces();
        CheckIngredientsOnLastRow();
        StartCoroutine(WaitAndFillEmptySpaces());
    }

    public void AdvanceRottenFruits()
    {
        foreach (var rotten in rottenFruits)
        {
            if (rotten != null)
            {
                rotten.OnTurnPassed();
            }
        }
    }


    public void UnlockPieceBelow(int row, int col)
    {
        var piece = gridPieces[row, col];
        if (piece != null && piece.isLocked)
        {
            piece.isLocked = false;
            reservedPositions[row, col] = false;

            Debug.Log($"🔓 Розблоковано фрукт на [{row}, {col}]");
        }
    } 

    // Визначення кращого центру форми
    private Vector2Int FindBestCenter(List<PieceManager> match)
    {
        foreach (PieceManager piece in match)
        {
            if (IsCenterOfShape(piece.Row, piece.Col))
            {
                return new Vector2Int(piece.Row, piece.Col);
            }
        }
        return new Vector2Int(match[match.Count / 2].Row, match[match.Count / 2].Col);
    }

    // Функція перевірки, чи це центр форми
    private bool IsCenterOfShape(int row, int col)
    {
        return IsLShape(row, col) || IsTShape(row, col);
    }

    // Покращена перевірка L-форми
    private bool IsLShape(int row, int col)
    {
        int[,] lShapes = {



{ -1, 0, -2, 0, 0, 1, 0, 2 }, // Верхнє ліве L
{ -1, 0, -2, 0, 0, -1, 0, -2 }, // Верхнє праве L
{ 1, 0, 2, 0, 0, 1, 0, 2 }, // Нижнє ліве L
{ 1, 0, 2, 0, 0, -1, 0, -2 }  // Нижнє праве L

};
        return CheckShape(row, col, lShapes);
    }

// Покращена перевірка T-форми
private bool IsTShape(int row, int col)
    {
        int[,] tShapes = {



{ 0, -1, 0, 1, -1, 0, -2, 0 }, // Верхнє T
{ 0, -1, 0, 1, 1, 0, 2, 0 }, // Нижнє T
{ -1, 0, 1, 0, 0, -1, 0, -2 }, // Ліве T
{ -1, 0, 1, 0, 0, 1, 0, 2 }  // Праве T

};
        return CheckShape(row, col, tShapes);
    }

// Генералізована функція перевірки шаблонів
private bool CheckShape(int row, int col, int[,] shape)
    {
        for (int i = 0; i < shape.GetLength(0); i++)
        {
            if (IsMatch(row, col, shape[i, 0], shape[i, 1]) &&
            IsMatch(row, col, shape[i, 2], shape[i, 3]) &&
            IsMatch(row, col, shape[i, 4], shape[i, 5]) &&
            IsMatch(row, col, shape[i, 6], shape[i, 7]))
            {
                return true;
            }
        }
        return false;
    }



// Функція перевіряє, чи є сусідній елемент того ж типу
private bool IsMatch(int row, int col, int offsetX, int offsetY)
    {
        int newRow = row + offsetX;
        int newCol = col + offsetY;
        return IsValidPosition(newRow, newCol) &&
        gridPieces[newRow, newCol] != null &&
        gridPieces[row, col] != null &&
        gridPieces[newRow, newCol].GetComponent<SpriteRenderer>().sprite == gridPieces[row, col].GetComponent<SpriteRenderer>().sprite;
    }

private bool IsHorizontalMatch(List<PieceManager> matches)
    {
        int row = matches[0].Row;
        foreach (PieceManager piece in matches)
        {
            if (piece.Row != row)
            {
                return false;
            }
        }
        return true;
    }

    public bool IsValidPosition(int row, int col)
    {
        return row >= 0 && row < rows && col >= 0 && col < cols;
    }

    // -------------------------
    // Створення спеціальних елементів
    // -------------------------

    private bool isSpecialPieceCreated = false;

    //Створює спеціальний елемент (ніж або виделку) у вказаній позиції сітки.

    // Метод, який повертає відповідний спрайт для спецелемента залежно від його типу та кольору
    public void CreateSpecialPiece(int row, int col, PieceType pieceType, Sprite colorSprite)
    {
        IsGridIdle = false;
        Debug.Log($"⚡ CreateSpecialPiece() викликано для ({row}, {col}) з типом {pieceType}");

        // Видаляємо старий об'єкт у цьому місці, якщо там щось є
        if (gridPieces[row, col] != null)
        {
            Debug.Log($"🗑 Видаляємо старий об'єкт у ({row}, {col}) перед створенням спец-елемента");
            Destroy(gridPieces[row, col].gameObject);
        }

        // Створюємо новий об'єкт
        GameObject specialPiece = Instantiate(piecePrefab, gridParent);
        specialPiece.transform.position = GetPosition(row, col);
        specialPiece.transform.localScale = Vector3.one * 0.04f;

        // Отримуємо PieceManager та SpriteRenderer
        PieceManager pieceManager = specialPiece.GetComponent<PieceManager>();
        SpriteRenderer spriteRenderer = specialPiece.GetComponent<SpriteRenderer>();

        // Ініціалізуємо PieceManager
        pieceManager.Initialize(row, col, this, pieceType, colorSprite);

        // ✅ Обробляємо ColorBomb окремо
        if (pieceType == PieceType.ColorBomb)
        {
            spriteRenderer.sprite = colorBombSprite; // Використовуємо загальний спрайт
            Debug.Log($"🎨 Встановлено спрайт для ColorBomb");
        }
        else
        {
            // **Перевіряємо, чи передається null в GetSpecialSprite**
            if (colorSprite == null)
            {
                Debug.LogError($"❌ CreateSpecialPiece: colorSprite == null для {pieceType}, вибираємо defaultSprite");
                colorSprite = defaultSprite;
            }

            // ✅ Встановлюємо правильний спрайт для спеціального елемента
            Sprite specialSprite = GetSpecialSprite(pieceType, colorSprite);
            if (specialSprite != null)
            {
                spriteRenderer.sprite = specialSprite;
            }
            else
            {
                Debug.LogError($"❌ Не знайдено спеціальний спрайт для {pieceType} з кольором {colorSprite?.name}");
            }
        }

        // Оновлюємо gridPieces
        gridPieces[row, col] = pieceManager;
        isSpecialPieceCreated = true; // ✅ Флаг, що спеціальний елемент створено
        Debug.Log($"✅ Спеціальний елемент {pieceType} створений у ({row}, {col})");
    }

    public Sprite GetSpecialSprite(PieceType pieceType, Sprite colorSprite)
    {
        IsGridIdle = false;
        if (pieceType == PieceType.ColorBomb)
        {
            return colorBombSprite; // ✅ Використовуємо загальний спрайт для ColorBomb
        }

        if (colorSprite == null)
        {
            Debug.LogError($"❌ GetSpecialSprite: colorSprite == null для {pieceType}, повертаємо null");
            return null;
        }

        string colorName = colorSprite.name.ToLower(); // Приводимо до нижнього регістру
        Debug.Log($"🔍 Шукаємо спеціальний спрайт для {pieceType} з кольором {colorName}");

        // Словник для відповідності
        Dictionary<string, Sprite> knifeSprites = new Dictionary<string, Sprite>()



{
            { "apple", redKnifeSprite },
{ "banana", yellowKnifeSprite },
{ "bilberry", blueKnifeSprite },
{ "pear", greenKnifeSprite }
        };


Dictionary<string, Sprite> forkSprites = new Dictionary<string, Sprite>()


{
            { "apple", redForkSprite },
{ "banana", yellowForkSprite },
{ "bilberry", blueForkSprite },
{ "pear", greenForkSprite }
        };


Dictionary<string, Sprite> bombSprites = new Dictionary<string, Sprite>()



{
            { "apple", redBombSprite },
{ "banana", yellowBombSprite },
{ "bilberry", blueBombSprite },
{ "pear", greenBombSprite }
        };

// Вибираємо правильний спрайт залежно від типу
switch (pieceType)
        {
            case PieceType.Knife:
                return knifeSprites.ContainsKey(colorName) ? knifeSprites[colorName] : null;

            case PieceType.Fork:
                return forkSprites.ContainsKey(colorName) ? forkSprites[colorName] : null;

            case PieceType.AreaBomb:
                return bombSprites.ContainsKey(colorName) ? bombSprites[colorName] : null;

            default:
                Debug.LogError($"❌ Немає мапінгу для {pieceType}");
                return null;
        }
    }

    public string GetFruitNameBySprite(Sprite sprite)
    {
        if (sprite == null)
        {
            Debug.LogError("❌ GetFruitNameBySprite: передано null!");
            return "Unknown";
        }

        string spriteName = sprite.name.ToLower(); // Приводимо до нижнього регістру
        Debug.Log($"🔎 Перевіряємо спрайт {spriteName}");

        if (sprite == appleSprite || spriteName.Contains("redbombcupcake")) return "Apple";
        if (sprite == bananaSprite || spriteName.Contains("yellowbombcupcake")) return "Banana";
        if (sprite == bilberrySprite || spriteName.Contains("bluebombcupcake")) return "Bilberry";
        if (sprite == pearSprite || spriteName.Contains("greenbombcupcake")) return "Pear";

        Debug.LogWarning($"⚠️ Невідомий спрайт: {spriteName}");
        return "Unknown";
    }
    public PieceType DetectPieceTypeBySprite(Sprite sprite)
    {
        if (sprite == null) return PieceType.Normal;

        if (sprite == redKnifeSprite || sprite == yellowKnifeSprite || sprite == blueKnifeSprite || sprite == greenKnifeSprite)
            return PieceType.Knife;

        if (sprite == redForkSprite || sprite == yellowForkSprite || sprite == blueForkSprite || sprite == greenForkSprite)
            return PieceType.Fork;

        if (sprite == redBombSprite || sprite == yellowBombSprite || sprite == blueBombSprite || sprite == greenBombSprite)
            return PieceType.AreaBomb;

        if (sprite == colorBombSprite)
            return PieceType.ColorBomb;

        return PieceType.Normal;
    }

    //Очищає масив резервів після завершення оновлення сітки.
    private void ClearAllReservedPositions()
    {
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                reservedPositions[row, col] = false;
            }
        }
    }

    //Обробляє створення спеціальних елементів після того, як сітка оновилася.
    private IEnumerator UpdateGridAndCreateSpecial(List<Vector2Int> specialPieces, List<PieceType> specialTypes, List<Sprite> specialColors)
    {
        IsGridIdle = false;
        // Одразу резервуємо всі позиції спеціальних елементів
        foreach (Vector2Int pos in specialPieces)
        {
            reservedPositions[pos.x, pos.y] = true;
            Debug.Log($"📍 Позиція зарезервована: ({pos.x}, {pos.y})");
        }

        yield return new WaitForSeconds(0.3f); // Чекаємо на падіння фігурок

        FillEmptySpaces(); // Заповнюємо порожні місця
        yield return new WaitForSeconds(0.3f); // Чекаємо на створення нових елементів

        isSpecialPieceCreated = false; // Спочатку флаг у false

        // Після заповнення обробляємо спеціальні елементи
        for (int i = 0; i < specialPieces.Count; i++)
        {
            Vector2Int pos = specialPieces[i];
            PieceType type = specialTypes[i];
            Sprite colorSprite = specialColors[i]; // ✅ Колір фігури, що створює спец-елемент

            // Перевіряємо, чи позиція все ще зарезервована та не зайнята новим елементом
            if (gridPieces[pos.x, pos.y] == null && reservedPositions[pos.x, pos.y])
            {
                Debug.Log($"✅ Створюємо спеціальний елемент {type} у ({pos.x}, {pos.y})");
                CreateSpecialPiece(pos.x, pos.y, type, colorSprite); // ✅ Передаємо колір
                reservedPositions[pos.x, pos.y] = false; // Звільняємо місце після створення
                isSpecialPieceCreated = true; // ✅ Дозволяємо падіння спец-елементу
            }
            else
            {
                Debug.LogWarning($"⚠️ Не можна створити спец-елемент {type} у ({pos.x}, {pos.y}), позиція зайнята!");
            }
        }

        yield return new WaitForSeconds(0.3f); // Додатковий час на обробку нових збігів
        StartCoroutine(CheckForMatchesAfterFill());

        // Очищаємо всі резерви після завершення оновлення сітки
        ClearAllReservedPositions();
        isSpecialPieceCreated = false; // Скидаємо флаг після повного оновлення
    }

    // -------------------------
    // Активація спеціальних елементів
    // -------------------------

    // Активує спеціальний елемент і викликає відповідну дію.
    public void ActivateSpecialPiece(PieceManager piece)
    {
        IsGridIdle = false;
        if (piece == null) return;

        Debug.Log($"🔥 Активація спецелемента {piece.pieceType} у ({piece.Row}, {piece.Col})");

        switch (piece.pieceType)
        {
            case PieceType.Knife:
                DestroyRow(piece.Row);
                break;
            case PieceType.Fork:
                DestroyColumn(piece.Col);
                break;
            case PieceType.AreaBomb:
                DestroyArea(piece.Row, piece.Col, 1);
                break;
            case PieceType.ColorBomb:
                Sprite colorToDestroy = piece.GetComponent<SpriteRenderer>().sprite;
                DestroyPiecesByColor(colorToDestroy);
                break;
        }

        piece.wasSwapped = false;

        DropPieces();
        StartCoroutine(CheckForMatchesAfterFill());
    }

    private IEnumerator ActivateSpecialCombo(PieceManager p1, PieceManager p2)
    {
        IsGridIdle = false;
        Debug.Log($"💥 Комбінація спецелементів: {p1.pieceType} + {p2.pieceType}");

        // 🔊 Звук вибуху спецелементів
        SoundManager.Instance?.PlaySpecialExplode();

        int row1 = p1.Row, col1 = p1.Col;
        int row2 = p2.Row, col2 = p2.Col;

        // ❌ Видаляємо спецелементи зі сітки ДО виклику ефектів
        gridPieces[row1, col1] = null;
        gridPieces[row2, col2] = null;

        Destroy(p1.gameObject);
        Destroy(p2.gameObject);

        yield return new WaitForSeconds(0.1f); // Дати Unity прибрати обʼєкти

        // 💣 Застосовуємо ефекти лише після знищення
        if ((p1.pieceType == PieceType.Knife && p2.pieceType == PieceType.Knife) ||
            (p1.pieceType == PieceType.Fork && p2.pieceType == PieceType.Fork))
        {
            DestroyRow(row1);
            DestroyRow(row2);
            DestroyColumn(col1);
            DestroyColumn(col2);
        }
        else if ((p1.pieceType == PieceType.Knife && p2.pieceType == PieceType.Fork) ||
                 (p1.pieceType == PieceType.Fork && p2.pieceType == PieceType.Knife))
        {
            DestroyRow(row1);
            DestroyColumn(col1);
        }
        else if (p1.pieceType == PieceType.AreaBomb && p2.pieceType == PieceType.AreaBomb)
        {
            DestroyArea(row1, col1, 2);
        }
        else if ((p1.pieceType == PieceType.AreaBomb && (p2.pieceType == PieceType.Knife || p2.pieceType == PieceType.Fork)) ||
                 ((p1.pieceType == PieceType.Knife || p1.pieceType == PieceType.Fork) && p2.pieceType == PieceType.AreaBomb))
        {
            DestroyArea(row1, col1, 1);
            DestroyRow(row1);
            DestroyColumn(col1);
        }

        yield return new WaitForSeconds(0.3f);
        DropPieces();
        yield return new WaitForSeconds(0.3f);

        yield return StartCoroutine(CheckForMatchesAfterFill());
    }

    // -------------------------
    // Знищення рядків, стовпців та областей
    // -------------------------



    //Видаляє всі фігурки в заданому рядку й оновлює сітку.
    private void DestroyRow(int row)
    {
        IsGridIdle = false;
        Debug.Log($"🧨 DestroyRow: Знищуємо рядок {row}");

        for (int col = 0; col < cols; col++)
        {
            var piece = gridPieces[row, col];
            if (CanBeDestroyed(piece))
            {
                Debug.Log($"❌ Видаляємо фішку на [{row}, {col}] → {piece.name}");
                piece.AnimateBlinkAndFade();
                gridPieces[row, col] = null;
            }
        }

        DropPieces();
        StartCoroutine(WaitAndFillEmptySpaces());
    }


    //Видаляє всі фігурки в заданому стовпці й оновлює сітку.
    private void DestroyColumn(int col)
    {
        IsGridIdle = false;
        Debug.Log($"🧨 DestroyColumn: Знищуємо стовпець {col}");

        for (int row = 0; row < rows; row++)
        {
            var piece = gridPieces[row, col];
            if (CanBeDestroyed(piece))
            {
                Debug.Log($"❌ Видаляємо фішку на [{row}, {col}] → {piece.name}");
                piece.AnimateBlinkAndFade();
                gridPieces[row, col] = null;
            }
        }

        DropPieces();
        StartCoroutine(WaitAndFillEmptySpaces());
    }


    private void DestroyArea(int centerRow, int centerCol, int radius)
    {
        IsGridIdle = false;
        Debug.Log($"💣 DestroyArea: Центр ({centerRow}, {centerCol}), радіус {radius}");

        for (int row = centerRow - radius; row <= centerRow + radius; row++)
        {
            for (int col = centerCol - radius; col <= centerCol + radius; col++)
            {
                if (IsValidPosition(row, col))
                {
                    var piece = gridPieces[row, col];
                    if (CanBeDestroyed(piece))
                    {
                        Debug.Log($"❌ Видаляємо фішку у зоні [{row}, {col}] → {piece.name}");
                        piece.AnimateBlinkAndFade();
                        gridPieces[row, col] = null;
                    }
                }
            }
        }

        DropPieces();
        StartCoroutine(WaitAndFillEmptySpaces());
    }

    private void DestroyPiecesByColor(Sprite colorSprite)
    {
        IsGridIdle = false;
        string targetColor = colorSprite.name.ToLower();
        Debug.Log($"🌈 DestroyPiecesByColor: Знищуємо всі фішки з кольором {targetColor}");

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                var piece = gridPieces[row, col];
                if (CanBeDestroyed(piece) && piece.GetComponent<SpriteRenderer>().sprite == colorSprite)
                {
                    Debug.Log($"❌ Видаляємо кольорову фішку [{row}, {col}] → {piece.name}");
                    Destroy(piece.gameObject);
                    gridPieces[row, col] = null;
                }
            }
        }

        DropPieces();
        StartCoroutine(WaitAndFillEmptySpaces());
    }


    private bool CanBeDestroyed(PieceManager piece)
    {
        return piece != null && piece.pieceType != PieceManager.PieceType.Ingredient;
    }



    // -------------------------
    // Опускання та заповнення сітки
    // -------------------------

    private bool IsPathBlockedByObstacle(int fromRow, int toRow, int col)
    {
        for (int r = fromRow + 1; r <= toRow; r++)
        {
            if (obstacleTiles[r, col] != null && obstacleTiles[r, col].layers > 0)
                return true;
        }
        return false;
    }




    //Опускає фігурки в порожні клітинки під ними, якщо місце не зарезервоване.
    public void DropPieces()
    {
        IsGridIdle = false;

        for (int col = 0; col < cols; col++)
        {
            for (int row = rows - 1; row >= 0; row--)
            {
                // Якщо вже є фігурка або активна перешкода — пропускаємо
                if (gridPieces[row, col] != null || (obstacleTiles[row, col]?.layers ?? 0) > 0)
                    continue;

                if (reservedPositions[row, col] && !isSpecialPieceCreated)
                    continue;

                for (int upperRow = row - 1; upperRow >= 0; upperRow--)
                {
                    var upperPiece = gridPieces[upperRow, col];
                    if (upperPiece != null)
                    {
                        if (upperPiece.isLocked)
                            continue;

                        if (IsPathBlockedByObstacle(upperRow, row, col))
                            break;

                        // Переміщення
                        gridPieces[row, col] = upperPiece;
                        gridPieces[upperRow, col] = null;

                        StartCoroutine(AnimatePieceDrop(upperPiece.gameObject, GetPosition(row, col), 0.3f));
                        upperPiece.UpdatePosition(row, col);
                        break;
                    }
                }

                // 🧃 Перевірка, чи інгредієнт досяг дна
                var pieceAtBottom = gridPieces[row, col];
                if (row == rows - 1 && pieceAtBottom != null && pieceAtBottom.pieceType == PieceManager.PieceType.Ingredient)
                {
                    Debug.Log($"🍯 Інгредієнт досяг дна на [{row}, {col}]");

                    // Додаємо до цілей
                    if (goalManager != null && goalManager.HasGoal("Ingredient"))
                    {
                        goalManager.AddProgress("Ingredient");
                    }

                    // Знищення з анімацією
                    pieceAtBottom.AnimateDestroy();
                    gridPieces[row, col] = null;
                }
            }
        }
    }



    //Чекає, поки фігурки впадуть, а потім заповнює порожні місця новими фігурками.
    private IEnumerator WaitAndFillEmptySpaces()
    {
        yield return new WaitForSeconds(0.3f); // Чекаємо, поки всі фігурки впадуть
        FillEmptySpaces();
        IsGridIdle = true;
    }

    //Заповнює порожні місця новими фігурками з випадковими спрайтами.
    private void FillEmptySpaces()
    {
        IsGridIdle = false;
        for (int col = 0; col < cols; col++)
        {
            for (int row = rows - 1; row >= 0; row--)
            {
                if (gridPieces[row, col] == null && !reservedPositions[row, col])
                {
                    // ❗ Перевіряємо, чи над цією клітинкою є активне тісто
                    // Якщо на самій клітинці є перешкода або над нею — пропускаємо
                    if ((obstacleTiles[row, col]?.layers ?? 0) > 0 || (gridPieces[row, col]?.isLocked ?? false))
                        continue;

                    GameObject newPiece = Instantiate(piecePrefab, gridParent);
                    Vector3 startPosition = GetPosition(-1, col);
                    newPiece.transform.position = startPosition;
                    newPiece.transform.localScale = Vector3.one * 0.04f;

                    Sprite newSprite = GetValidSprite(row, col);
                    if (newSprite == null)
                    {
                        newSprite = defaultSprite;
                    }

                    newPiece.GetComponent<SpriteRenderer>().sprite = newSprite;

                    PieceManager pieceManager = newPiece.GetComponent<PieceManager>();
                    pieceManager.Initialize(row, col, this, PieceType.Normal, newSprite);

                    gridPieces[row, col] = pieceManager;

                    pieceManager.AnimateSpawn();
                    StartCoroutine(AnimatePieceDrop(newPiece, GetPosition(row, col), 0.3f));

                    PrintFullGridToConsole();
                }
            }
        }
    }


    //Перевіряє, чи з’явилися нові збіги після заповнення порожніх місць.
    public IEnumerator CheckForMatchesAfterFill()
    {
        IsGridIdle = false;
        yield return new WaitForSeconds(0.3f); // Чекаємо, поки попередні анімації завершаться

        List<List<PieceManager>> matchGroups = FindMatchGroups();
        Debug.Log($"📊 Нові комбінації знайдені після падіння: {matchGroups.Count}");

        if (matchGroups.Count > 0)
        {
            Debug.Log($"📊 Нові комбінації знайдені: {matchGroups.Count}");
            HandleMatches(matchGroups);

            yield return new WaitForSeconds(0.3f);
            DropPieces();
            yield return new WaitForSeconds(0.3f);

            FillEmptySpaces();
            yield return new WaitForSeconds(0.4f); // 🕐 Додаткова пауза на анімацію падіння

            // 🔁 Повторна перевірка, якщо ще залишилися комбінації
            yield return StartCoroutine(CheckForMatchesAfterFill());
        }
        else
        {
            Debug.Log("✅ Нові комбінації не знайдено.");
            IsGridIdle = true;
        }
    }


    private void CheckIngredientsOnLastRow()
    {
        for (int col = 0; col < cols; col++)
        {
            int lastRow = rows - 1;
            var piece = gridPieces[lastRow, col];

            if (piece != null && piece.pieceType == PieceType.Ingredient && !piece.isLocked)
            {
                Debug.Log($"🥣 Інгредієнт досяг останнього рядка на [{lastRow}, {col}]");

                // Анімація зникнення
                piece.AnimateDestroy();

                // Врахування як виконаної мети (якщо є ціль на інгредієнти)
                if (goalManager != null && goalManager.HasGoal("I"))
                    goalManager.AddProgress("I");

                gridPieces[lastRow, col] = null;
            }
        }
    }



    //Анімує падіння фігурки з початкової до цільової позиції.
    private IEnumerator AnimatePieceDrop(GameObject piece, Vector3 targetPosition, float duration)
    {
        IsGridIdle = false;
        if (piece == null) yield break; // Перевіряємо, чи об'єкт існує

        Vector3 startPosition = piece.transform.position;
        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            if (piece == null) yield break; // Якщо об'єкт видалено, виходимо з корутини
            piece.transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (piece != null) // Переконуємося, що об'єкт існує
        {
            piece.transform.position = targetPosition;
        }
    }

    public void PrintFullGridToConsole()
    {
        string gridString = "\\\\n🧾 Повна сітка з типами:\\\\n";

        for (int row = 0; row < rows; row++)
        {
            string line = $"[{row}] ";
            for (int col = 0; col < cols; col++)
            {
                var piece = gridPieces[row, col];
                if (piece == null)
                {
                    line += " . ";
                }
                else
                {
                    string symbol = GetFruitSymbol(piece.GetComponent<SpriteRenderer>().sprite);
                    string typeLetter = GetPieceTypeSymbol(piece.pieceType);
                    line += $" {symbol}({typeLetter}) ";
                }
            }
            gridString += line + "\\\\n";
        }

        Debug.Log(gridString);
    }

    private string GetPieceTypeSymbol(PieceType type)
    {
        switch (type)
        {
            case PieceType.Normal: return "N";
            case PieceType.Knife: return "K";
            case PieceType.Fork: return "F";
            case PieceType.AreaBomb: return "A";
            case PieceType.ColorBomb: return "C";
            default: return "?";
        }
    }

    // Символи для кожного типу/фрукта
    private string GetSymbolForPiece(PieceManager piece)
    {
        if (piece == null) return ".";

        switch (piece.pieceType)
        {
            case PieceType.Normal:
                return GetFruitSymbol(piece.GetComponent<SpriteRenderer>().sprite);

            case PieceType.Knife: return "K";
            case PieceType.Fork: return "F";
            case PieceType.AreaBomb: return "B";
            case PieceType.ColorBomb: return "C";
            default: return "?";
        }
    }

    private string GetFruitSymbol(Sprite sprite)
    {
        if (sprite == appleSprite) return "A";
        if (sprite == bananaSprite) return "B";
        if (sprite == bilberrySprite) return "L";
        if (sprite == pearSprite) return "P";
        return "?";
    }



}