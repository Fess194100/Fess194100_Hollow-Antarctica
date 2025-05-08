using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MiniGame_Line : MonoBehaviour
{
    [Header("Game Settings")]
    [SerializeField] private Vector2Int _gridSize = new Vector2Int(5, 5);
    [SerializeField] private int _sourceCount = 1;
    [SerializeField] private int _emptyTilesCount = 3;
    [SerializeField] private bool _randomPlacement = true;
    [SerializeField] private bool _generateWalls = true;

    [Header("Tile Prefabs")]
    [SerializeField] private MiniGame_Line_Cell _cellPrefab;
    [SerializeField] private Sprite _sourceSprite;
    [SerializeField] private Sprite _targetSprite;
    [SerializeField] private Sprite _wallSprite;
    [SerializeField] private Sprite _emptySprite;

    [Header("Tile Sprites")]
    [SerializeField] private MiniGame_Line_Sprites _filledSprites;
    [SerializeField] private MiniGame_Line_Sprites _unfilledSprites;

    [Header("UI Components")]
    [SerializeField] private GridLayoutGroup _gridLayout;
    [SerializeField] private RectTransform _gridContainer;

    [Header("Debug Settings")]
    [SerializeField] private bool _enableDebugText = true;

    private MiniGame_Line_Cell[,] _grid;
    private HashSet<Vector2Int> _filledCells = new HashSet<Vector2Int>();
    private List<Vector2Int> _sourcePositions = new List<Vector2Int>();
    private List<Vector2Int> _targetPositions = new List<Vector2Int>();

    private void Start()
    {
        InitializeGameGrid();
    }

    private void InitializeGameGrid()
    {
        _grid = new MiniGame_Line_Cell[_gridSize.x, _gridSize.y];
        _sourcePositions.Clear();
        _targetPositions.Clear();

        // 1. Create all cells first
        for (int x = 0; x < _gridSize.x; x++)
        {
            for (int y = 0; y < _gridSize.y; y++)
            {
                var cell = Instantiate(_cellPrefab, _gridContainer);
                cell.Initialize(new Vector2Int(x, y), _unfilledSprites, this, _enableDebugText);
                _grid[x, y] = cell;
            }
        }

        // 2. Place special tiles
        PlaceSpecialTiles();
        UpdatePipeSystem();
    }

    private void PlaceSpecialTiles()
    {
        var availablePositions = GetAllGridPositions();

        // Place sources and targets
        for (int i = 0; i < _sourceCount && availablePositions.Count >= 2; i++)
        {
            // Place source
            int sourceIdx = _randomPlacement ? Random.Range(0, availablePositions.Count) : 0;
            var sourcePos = availablePositions[sourceIdx];
            _grid[sourcePos.x, sourcePos.y].ConvertToSpecialTile(_sourceSprite, true);
            _sourcePositions.Add(sourcePos);
            availablePositions.RemoveAt(sourceIdx);

            // Place matching target
            int targetIdx = _randomPlacement ? Random.Range(0, availablePositions.Count) : availablePositions.Count - 1;
            var targetPos = availablePositions[targetIdx];
            _grid[targetPos.x, targetPos.y].ConvertToSpecialTile(_targetSprite, false);
            _targetPositions.Add(targetPos);
            availablePositions.RemoveAt(targetIdx);
        }

        // Place walls
        if (_generateWalls)
        {
            foreach (var wallPos in GetWallPositions())
            {
                if (!_sourcePositions.Contains(wallPos) && !_targetPositions.Contains(wallPos))
                {
                    _grid[wallPos.x, wallPos.y].ConvertToSpecialTile(_wallSprite, false);
                    availablePositions.Remove(wallPos);
                }
            }
        }

        // Place empty tiles
        for (int i = 0; i < _emptyTilesCount && availablePositions.Count > 0; i++)
        {
            int emptyIdx = Random.Range(0, availablePositions.Count);
            var emptyPos = availablePositions[emptyIdx];
            _grid[emptyPos.x, emptyPos.y].ConvertToSpecialTile(_emptySprite, false);
            availablePositions.RemoveAt(emptyIdx);
        }
    }

    public void UpdatePipeSystem()
    {
        // 1. Создаем временный набор для новых залитых ячеек
        HashSet<Vector2Int> newFilledCells = new HashSet<Vector2Int>();

        // 2. Для каждого источника выполняем поиск в ширину (BFS)
        foreach (var sourcePos in _sourcePositions)
        {
            if (!IsPositionValid(sourcePos)) continue;

            Queue<Vector2Int> cellsToCheck = new Queue<Vector2Int>();
            cellsToCheck.Enqueue(sourcePos);

            while (cellsToCheck.Count > 0)
            {
                var currentPos = cellsToCheck.Dequeue();

                // Если уже обработали эту ячейку - пропускаем
                if (newFilledCells.Contains(currentPos)) continue;

                var currentCell = _grid[currentPos.x, currentPos.y];
                if (currentCell == null) continue;

                // Добавляем в залитые (даже если это источник)
                newFilledCells.Add(currentPos);

                // Проверяем всех соседей
                foreach (var direction in currentCell.GetConnectionDirections())
                {
                    var neighborPos = currentPos + direction;

                    if (IsPositionValid(neighborPos) &&
                        !newFilledCells.Contains(neighborPos))
                    {
                        var neighborCell = _grid[neighborPos.x, neighborPos.y];
                        if (neighborCell != null &&
                            AreCellsConnected(currentCell, neighborCell))
                        {
                            cellsToCheck.Enqueue(neighborPos);
                        }
                    }
                }
            }
        }

        // 3. Обновляем визуальное состояние всех ячеек
        for (int x = 0; x < _gridSize.x; x++)
        {
            for (int y = 0; y < _gridSize.y; y++)
            {
                var pos = new Vector2Int(x, y);
                var cell = _grid[x, y];

                if (cell != null)
                {
                    bool shouldBeFilled = newFilledCells.Contains(pos);
                    cell.SetFilledState(shouldBeFilled, shouldBeFilled ? _filledSprites : _unfilledSprites);

                }
            }
        }

        // 4. Сохраняем новые залитые ячейки
        _filledCells = newFilledCells;

        // ... после обновления заливки:
        if (_enableDebugText)
        {
            foreach (var cell in _grid)
            {
                cell?.UpdateDebugText();
            }
        }

        // 5. Проверяем победу
        CheckWinCondition();
    }

    private bool AreCellsConnected(MiniGame_Line_Cell from, MiniGame_Line_Cell to)
    {
        var fromPos = from.GridPosition;
        var toPos = to.GridPosition;
        var direction = toPos - fromPos;

        foreach (var fromDir in from.GetConnectionDirections())
        {
            if (fromDir == direction)
            {
                foreach (var toDir in to.GetConnectionDirections())
                {
                    if (toDir == -direction)
                        return true;
                }
            }
        }
        return false;
    }

    private void FillConnectedCells(Vector2Int position)
    {
        // Убедимся, что позиция валидна
        if (!IsPositionValid(position)) return;

        var cell = _grid[position.x, position.y];
        if (cell == null || cell.IsFilled || _filledCells.Contains(position)) return;

        // Помечаем ячейку как залитую
        cell.SetFilledState(true, _filledSprites);
        _filledCells.Add(position);

        // Распространяем заливку на соединенные тайлы
        foreach (var dir in cell.GetConnectionDirections())
        {
            var neighborPos = position + dir;
            if (IsPositionValid(neighborPos))
            {
                var neighbor = _grid[neighborPos.x, neighborPos.y];
                if (neighbor != null && AreCellsConnected(cell, neighbor))
                {
                    FillConnectedCells(neighborPos);
                }
            }
        }
    }

    private List<Vector2Int> GetAllGridPositions()
    {
        List<Vector2Int> positions = new List<Vector2Int>();
        for (int x = 0; x < _gridSize.x; x++)
        {
            for (int y = 0; y < _gridSize.y; y++)
            {
                positions.Add(new Vector2Int(x, y));
            }
        }
        return positions;
    }

    private List<Vector2Int> GetWallPositions()
    {
        List<Vector2Int> walls = new List<Vector2Int>();

        // Верхняя и нижняя границы
        for (int x = 0; x < _gridSize.x; x++)
        {
            walls.Add(new Vector2Int(x, 0));
            walls.Add(new Vector2Int(x, _gridSize.y - 1));
        }

        // Боковые границы (без углов)
        for (int y = 1; y < _gridSize.y - 1; y++)
        {
            walls.Add(new Vector2Int(0, y));
            walls.Add(new Vector2Int(_gridSize.x - 1, y));
        }

        return walls;
    }

    private void CheckWinCondition()
    {
        bool allTargetsReached = true;
        foreach (var targetPos in _targetPositions)
        {
            if (!_filledCells.Contains(targetPos))
            {
                allTargetsReached = false;
                break;
            }
        }

        if (allTargetsReached)
        {
            Debug.Log("Level Complete! All targets connected!");
            // Здесь можно добавить вызов события победы
        }
    }

    private bool IsPositionValid(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < _gridSize.x &&
               pos.y >= 0 && pos.y < _gridSize.y;
    }
}