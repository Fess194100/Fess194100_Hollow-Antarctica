using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Events;

public enum Difficulty { Low, Medium, High }
public enum TileType { Empty, Wall, Box1, Box2, Box3, Target1, Target2, Target3, Player }

public class MiniGame_Chest : MonoBehaviour
{
    [Header("Game Settings")]
    public Difficulty difficulty = Difficulty.Low;
    public Vector2Int gridSize = new Vector2Int(5, 5);
    public int boxesLow = 1;
    public int boxesMedium = 2;
    public int boxesHigh = 3;

    [Header("Additional Walls")]
    [Range(0, 10)]
    public int wallOutcropsCount = 3; // Количество выступов у стен
    [Space(6)]
    [Range(0, 10)]
    public int centralWallsCount = 3; // Количество центральных стен

    [Header("Prefabs")]
    public GameObject cellPrefab;
    public Image floorPrefab;
    public Image wallPrefab;
    public Image playerPrefab;
    public Image[] boxPrefabs; // Должно быть 3 элемента
    public Image[] targetPrefabs; // Должно быть 3 элемента
    public Image[] boxOnTargetPrefabs; // Должно быть 3 элемента

    [Header("References")]
    public GridLayoutGroup gridLayout;
    public Button restartButton;

    [Space(10)]
    [Header("Events")]
    public UnityEvent OnMovePlayer; // Когда игрок просто двигается
    public UnityEvent OnMoveBox;    // Когда игрок толкает ящик
    public UnityEvent OnBoxInTarget; // Когда ящик попадает на цель
    public UnityEvent OnWin;        // Когда уровень пройден

    private GameObject[,] cellObjects;
    private CellContent[,] cells;
    private Vector2Int playerPosition;
    private TileType[,] grid;
    private List<Vector2Int> targetPositions = new List<Vector2Int>();
    private bool win = false;

    void Start()
    {
        restartButton.onClick.AddListener(RestartLevel);
        GenerateLevel();
    }

    void InitializeGrid()
    {
        // Настройка GridLayout
        gridLayout.constraintCount = gridSize.x;
        RectTransform rt = gridLayout.GetComponent<RectTransform>();
        float cellSize = Mathf.Min(rt.rect.width / gridSize.x, rt.rect.height / gridSize.y);
        gridLayout.cellSize = new Vector2(cellSize, cellSize);

        // Создание массивов
        cellObjects = new GameObject[gridSize.x, gridSize.y];
        cells = new CellContent[gridSize.x, gridSize.y];
        grid = new TileType[gridSize.x, gridSize.y];
    }

    void GenerateLevel()
    {
        // Очистка предыдущего уровня
        foreach (Transform child in gridLayout.transform)
            Destroy(child.gameObject);

        InitializeGrid();
        targetPositions.Clear();

        // Создание ячеек
        for (int y = 0; y < gridSize.y; y++)
        {
            for (int x = 0; x < gridSize.x; x++)
            {
                CreateCell(x, y);
            }
        }

        // Размещение игрока
        PlacePlayer(new Vector2Int(1, 1));

        // Генерация выступов
        GenerateWallOutcrops();

        // Генерация центральных стен
        GenerateCentralWalls();

        // Размещение целей и ящиков
        int boxCount = GetBoxCountForCurrentDifficulty();
        int typesCount = GetBoxTypesCountForCurrentDifficulty();

        for (int typeIndex = 0; typeIndex < typesCount; typeIndex++)
        {
            for (int i = 0; i < boxCount; i++)
            {
                PlaceTarget(typeIndex);
                PlaceBox(typeIndex);
            }
        }
    }

    void CreateCell(int x, int y)
    {
        GameObject cellObj = Instantiate(cellPrefab, gridLayout.transform);
        cellObjects[x, y] = cellObj;
        cells[x, y] = cellObj.GetComponent<CellContent>();

        // Стены по краям, пол внутри
        if (x == 0 || y == 0 || x == gridSize.x - 1 || y == gridSize.y - 1)
        {
            cells[x, y].wall = Instantiate(wallPrefab, cellObj.transform);
            grid[x, y] = TileType.Wall;
        }
        else
        {
            cells[x, y].floor = Instantiate(floorPrefab, cellObj.transform);
            grid[x, y] = TileType.Empty;
        }
    }

    #region Additional Wall

    void GenerateWallOutcrops()
    {
        if (wallOutcropsCount <= 0) return;

        for (int i = 0; i < wallOutcropsCount; i++)
        {
            Vector2Int pos = GetRandomWallAdjacentPosition();
            if (pos == Vector2Int.zero) continue; // Если не нашли подходящую позицию

            // Очищаем ячейку от всего (кроме стен)
            ClearCell(pos);

            // Создаем выступ
            cells[pos.x, pos.y].wall = Instantiate(wallPrefab, cellObjects[pos.x, pos.y].transform);
            grid[pos.x, pos.y] = TileType.Wall;
        }
    }

    void GenerateCentralWalls()
    {
        if (centralWallsCount <= 0) return;

        int maxDistance = CalculateMaxSafeDistance();
        if (maxDistance < 3) return; // Минимальная дистанция 3 клетки

        for (int i = 0; i < centralWallsCount; i++)
        {
            // Выбираем случайное расстояние от 3 до maxDistance
            int distance = Random.Range(3, maxDistance + 1);

            Vector2Int pos = GetRandomEmptyPosition(true, distance);
            if (pos == Vector2Int.zero) continue; // Если не нашли подходящую позицию

            // Очищаем ячейку и создаем стену
            ClearCell(pos);
            cells[pos.x, pos.y].wall = Instantiate(wallPrefab, cellObjects[pos.x, pos.y].transform);
            grid[pos.x, pos.y] = TileType.Wall;
        }
    }

    Vector2Int GetRandomWallAdjacentPosition()
    {
        int attempts = 0;
        int maxAttempts = 100;

        while (attempts < maxAttempts)
        {
            // Выбираем случайную сторону (0-верх, 1-право, 2-низ, 3-лево)
            int side = Random.Range(0, 4);
            Vector2Int pos = Vector2Int.zero;

            switch (side)
            {
                case 0: // Верх
                    pos = new Vector2Int(Random.Range(1, gridSize.x - 1), gridSize.y - 2);
                    break;
                case 1: // Право
                    pos = new Vector2Int(gridSize.x - 2, Random.Range(1, gridSize.y - 1));
                    break;
                case 2: // Низ
                    pos = new Vector2Int(Random.Range(1, gridSize.x - 1), 1);
                    break;
                case 3: // Лево
                    pos = new Vector2Int(1, Random.Range(1, gridSize.y - 1));
                    break;
            }

            // Проверяем, что ячейка пустая и рядом есть стена
            if (grid[pos.x, pos.y] == TileType.Empty && IsAdjacentToWall(pos))
                return pos;

            attempts++;
        }

        return Vector2Int.zero;
    }

    bool IsAdjacentToWall(Vector2Int pos)
    {
        // Проверяем только 4 направления (не диагонали)
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };

        foreach (var dir in directions)
        {
            Vector2Int checkPos = pos + dir;
            if (checkPos.x >= 0 && checkPos.y >= 0 &&
                checkPos.x < gridSize.x && checkPos.y < gridSize.y &&
                grid[checkPos.x, checkPos.y] == TileType.Wall)
            {
                return true;
            }
        }

        return false;
    }

    void ClearCell(Vector2Int pos)
    {
        GameObject cellObject = cellObjects[pos.x, pos.y];

        // Удаляем все дочерние объекты ячейки
        foreach (Transform child in cellObject.transform)
        {
            Destroy(child.gameObject);
        }
    }

    int CalculateMaxSafeDistance()
    {
        // Проверяем минимальный размер поля
        int minGridSize = Mathf.Min(gridSize.x, gridSize.y);
        if (minGridSize < 7) return 0; // Если хоть одна сторона меньше 7

        // Берем половину минимальной стороны и округляем вниз
        return Mathf.FloorToInt(minGridSize / 2f);
    }

    #endregion

    void PlacePlayer(Vector2Int pos)
    {
        CellContent cell = cells[pos.x, pos.y];
        cell.player = Instantiate(playerPrefab, cellObjects[pos.x, pos.y].transform);
        cell.player.transform.localPosition = Vector3.zero;
        playerPosition = pos;
        grid[pos.x, pos.y] = TileType.Player;
    }

    void PlaceTarget(int typeIndex)
    {
        Vector2Int pos = GetRandomEmptyPosition(false, 1);
        CellContent cell = cells[pos.x, pos.y];

        // Убедимся, что очищаем другие цели
        for (int i = 0; i < 3; i++)
            cell.targets[i] = null;

        cell.targets[typeIndex] = Instantiate(targetPrefabs[typeIndex], cell.transform);
        cell.targets[typeIndex].transform.localPosition = Vector3.zero;
        grid[pos.x, pos.y] = TileType.Target1 + typeIndex;
        targetPositions.Add(pos);
    }

    void PlaceBox(int typeIndex)
    {
        Vector2Int pos = GetRandomEmptyPosition(true, 2);
        CellContent cell = cells[pos.x, pos.y];

        cell.boxes[typeIndex] = Instantiate(boxPrefabs[typeIndex], cell.transform);
        cell.boxes[typeIndex].transform.localPosition = Vector3.zero;
        grid[pos.x, pos.y] = TileType.Box1 + typeIndex;
    }

    int GetBoxCountForCurrentDifficulty()
    {
        return difficulty switch
        {
            Difficulty.Low => boxesLow,
            Difficulty.Medium => boxesMedium,
            Difficulty.High => boxesHigh,
            _ => 1
        };
    }

    int GetBoxTypesCountForCurrentDifficulty()
    {
        return difficulty switch
        {
            Difficulty.Low => 1,    // Только тип 0 (Box1)
            Difficulty.Medium => 2,  // Типы 0 и 1 (Box1 и Box2)
            Difficulty.High => 3,    // Все три типа
            _ => 1
        };
    }

    void Update()
    {
        HandleInput();
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.W)) TryMovePlayer(Vector2Int.up);
        if (Input.GetKeyDown(KeyCode.S)) TryMovePlayer(Vector2Int.down);
        if (Input.GetKeyDown(KeyCode.A)) TryMovePlayer(Vector2Int.left);
        if (Input.GetKeyDown(KeyCode.D)) TryMovePlayer(Vector2Int.right);
    }

    void TryMovePlayer(Vector2Int direction)
    {
        if (CheckWinCondition()) return;

        Vector2Int newPos = playerPosition + direction;
        if (!IsPositionValid(newPos)) return;

        bool movedBox = false;
        bool correctBoxOnTarget = false;

        if (IsBoxAtPosition(newPos))
        {
            Vector2Int boxNewPos = newPos + direction;
            if (!CanMoveBox(boxNewPos)) return;

            // Запоминаем, был ли ящик уже на правильной цели
            bool wasOnCorrectTarget = IsBoxOnTarget(newPos, out _);

            // Перемещаем ящик
            int boxType = (int)grid[newPos.x, newPos.y] - (int)TileType.Box1;
            MoveBox(newPos, boxNewPos);
            movedBox = true;

            // Проверяем, попал ли ящик на правильную цель
            bool nowOnCorrectTarget = IsBoxOnTarget(boxNewPos, out _);
            correctBoxOnTarget = !wasOnCorrectTarget && nowOnCorrectTarget;
        }

        MovePlayer(newPos);

        // Вызываем события
        if (correctBoxOnTarget)
        {
            OnBoxInTarget.Invoke();
            CheckWinCondition();
        }
        else if (movedBox)
        {
            OnMoveBox.Invoke();
        }
        else
        {
            OnMovePlayer.Invoke();
        }
    }

    void MovePlayer(Vector2Int newPos)
    {
        // Получаем текущую и новую ячейки
        GameObject oldCell = cellObjects[playerPosition.x, playerPosition.y];
        GameObject newCell = cellObjects[newPos.x, newPos.y];

        // Получаем компоненты CellContent
        CellContent oldCellContent = oldCell.GetComponent<CellContent>();
        CellContent newCellContent = newCell.GetComponent<CellContent>();

        // Переносим игрока
        oldCellContent.player.transform.SetParent(newCell.transform); // Важно: newCell.transform, а не parent!
        oldCellContent.player.transform.localPosition = Vector3.zero;

        // Обновляем ссылки
        newCellContent.player = oldCellContent.player;
        oldCellContent.player = null;

        // Обновляем логическую сетку
        grid[playerPosition.x, playerPosition.y] = oldCellContent.floor ? TileType.Empty : TileType.Wall;
        playerPosition = newPos;
        grid[newPos.x, newPos.y] = TileType.Player;
    }

    void MoveBox(Vector2Int from, Vector2Int to)
    {
        // Определяем тип ящика (0, 1 или 2)
        int boxType = (int)grid[from.x, from.y] - (int)TileType.Box1;

        // Получаем ячейки
        GameObject fromCell = cellObjects[from.x, from.y];
        GameObject toCell = cellObjects[to.x, to.y];

        // Получаем компоненты CellContent
        CellContent fromCellContent = fromCell.GetComponent<CellContent>();
        CellContent toCellContent = toCell.GetComponent<CellContent>();

        // Переносим ящик
        Image box = fromCellContent.boxes[boxType];
        box.transform.SetParent(toCell.transform); // Важно: toCell.transform, а не parent!
        box.transform.localPosition = Vector3.zero;

        // Обновляем ссылки
        toCellContent.boxes[boxType] = box;
        fromCellContent.boxes[boxType] = null;

        // Обновляем логическую сетку
        grid[from.x, from.y] = fromCellContent.floor ? TileType.Empty : TileType.Wall;
        grid[to.x, to.y] = TileType.Box1 + boxType;

        // Обновляем спрайт (ящик на цели или нет)
        UpdateBoxSprite(to, boxType);
    }

    void UpdateBoxSprite(Vector2Int pos, int boxType)
    {
        bool isOnTarget = IsBoxOnTarget(pos, out int targetType);

        if (isOnTarget && boxType == targetType)
        {
            cells[pos.x, pos.y].boxes[boxType].sprite = boxOnTargetPrefabs[boxType].sprite;
        }
        else
        {
            cells[pos.x, pos.y].boxes[boxType].sprite = boxPrefabs[boxType].sprite;
        }
    }

    Vector2Int GetRandomEmptyPosition(bool awayFromWalls, int disFromWall)
    {
        Vector2Int pos;
        int attempts = 0;
        int maxAttempts = 300;

        // Если запрошенное расстояние больше возможного - уменьшаем
        int maxPossibleDistance = Mathf.Min(gridSize.x, gridSize.y) / 2 - 1;
        disFromWall = Mathf.Min(disFromWall, maxPossibleDistance);
        disFromWall = Mathf.Max(disFromWall, 1); // Минимум 1

        do
        {
            pos = new Vector2Int(
                Random.Range(disFromWall, gridSize.x - disFromWall),
                Random.Range(disFromWall, gridSize.y - disFromWall)
            );

            if (awayFromWalls && IsNearWall(pos))
                continue;

            attempts++;
            if (attempts >= maxAttempts)
                return new Vector2Int(1, 1);

        } while (grid[pos.x, pos.y] != TileType.Empty);

        return pos;
    }

    bool IsNearWall(Vector2Int pos)
    {
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                int checkX = pos.x + x;
                int checkY = pos.y + y;
                if (checkX >= 0 && checkY >= 0 && checkX < gridSize.x && checkY < gridSize.y)
                {
                    if (grid[checkX, checkY] == TileType.Wall)
                        return true;
                }
            }
        }
        return false;
    }

    bool IsPositionValid(Vector2Int pos)
    {
        return pos.x >= 0 && pos.y >= 0 &&
               pos.x < gridSize.x && pos.y < gridSize.y &&
               grid[pos.x, pos.y] != TileType.Wall;
    }

    bool IsBoxAtPosition(Vector2Int pos)
    {
        return grid[pos.x, pos.y] >= TileType.Box1 &&
               grid[pos.x, pos.y] <= TileType.Box3;
    }

    bool CanMoveBox(Vector2Int pos)
    {
        return IsPositionValid(pos) &&
               (grid[pos.x, pos.y] == TileType.Empty ||
                grid[pos.x, pos.y] >= TileType.Target1 && grid[pos.x, pos.y] <= TileType.Target3);
    }

    bool IsBoxOnTarget(Vector2Int pos, out int targetType)
    {
        targetType = -1;

        // Проверяем, что в этой клетке вообще есть ящик
        if (grid[pos.x, pos.y] < TileType.Box1 || grid[pos.x, pos.y] > TileType.Box3)
            return false;

        // Получаем тип ящика (0, 1 или 2)
        int boxType = (int)grid[pos.x, pos.y] - (int)TileType.Box1;

        // Проверяем все цели в этой клетке
        CellContent cell = cells[pos.x, pos.y];
        for (int i = 0; i < 3; i++)
        {
            if (cell.targets[i] != null)
            {
                targetType = i;
                return boxType == i; // Возвращаем true только если типы совпадают
            }
        }

        return false;
    }

    bool CheckWinCondition()
    {
        // 1. Собираем информацию о типах целей
        Dictionary<Vector2Int, int> realTargetTypes = new Dictionary<Vector2Int, int>();
        foreach (Vector2Int pos in targetPositions)
        {
            int foundType = -1;
            CellContent cell = cells[pos.x, pos.y];

            for (int i = 0; i < 3; i++)
            {
                if (cell.targets[i] != null)
                {
                    foundType = i;
                    break;
                }
            }

            realTargetTypes[pos] = foundType;
        }

        // 2. Проверяем ящики на целях
        int[] correctBoxes = new int[3];
        int[] totalBoxes = new int[3];

        for (int y = 0; y < gridSize.y; y++)
        {
            for (int x = 0; x < gridSize.x; x++)
            {
                if (grid[x, y] >= TileType.Box1 && grid[x, y] <= TileType.Box3)
                {
                    int boxType = (int)grid[x, y] - (int)TileType.Box1;
                    totalBoxes[boxType]++;

                    Vector2Int pos = new Vector2Int(x, y);
                    if (realTargetTypes.TryGetValue(pos, out int targetType) &&
                        targetType == boxType)
                    {
                        correctBoxes[boxType]++;
                    }
                }
            }
        }

        // 3. Проверяем победу
        win = true;
        for (int i = 0; i < 3; i++)
        {
            if (totalBoxes[i] > 0 && correctBoxes[i] != totalBoxes[i])
            {
                win = false;
            }
        }

        if (win)
        {
            OnWin.Invoke();
            return true;
        }
        return false;
    }

    public void RestartLevel()
    {
        win = false;
        GenerateLevel();
    }
}