using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MiniGame_Line : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Vector2Int _gridSize = new Vector2Int(5, 5);
    [SerializeField] private Vector2Int _sourcePosition;
    [SerializeField] private Vector2Int _targetPosition;

    [Header("Prefabs")]
    [SerializeField] private MiniGame_Line_Cell _cellPrefab;

    [Header("Sprites")]
    [SerializeField] private MiniGame_Line_Sprites _spritesFilled;
    [SerializeField] private MiniGame_Line_Sprites _spritesUnfilled;

    [Header("Grid Settings")]
    [SerializeField] private GridLayoutGroup _gridLayout;
    [SerializeField] private RectTransform _gridContainer;

    private MiniGame_Line_Cell[,] _grid;
    private HashSet<Vector2Int> _filledCells = new HashSet<Vector2Int>();

    private void Start()
    {
        if (!ValidateComponents()) return;
        ConfigureGrid();
        GenerateGrid();
        UpdatePipeSystem();
    }

    public void UpdatePipeSystem()
    {
        ClearFilledCells();
        FillConnectedCells(_sourcePosition);
        CheckWinCondition();
    }

    private void FillConnectedCells(Vector2Int startPosition)
    {
        if (!IsPositionValid(startPosition)) return;
        if (_filledCells.Contains(startPosition)) return;

        var cell = _grid[startPosition.x, startPosition.y];
        cell.SetFilled(true, _spritesFilled);
        _filledCells.Add(startPosition);

        foreach (var direction in cell.GetConnectionDirections())
        {
            var neighborPos = startPosition + direction;
            if (!IsPositionValid(neighborPos)) continue;

            var neighborCell = _grid[neighborPos.x, neighborPos.y];
            if (IsConnected(cell, neighborCell))
            {
                FillConnectedCells(neighborPos);
            }
        }
    }

    private bool IsConnected(MiniGame_Line_Cell from, MiniGame_Line_Cell to)
    {
        var fromPos = from.GridPosition;
        var toPos = to.GridPosition;
        var direction = toPos - fromPos;

        foreach (var fromDirection in from.GetConnectionDirections())
        {
            if (fromDirection == direction)
            {
                foreach (var toDirection in to.GetConnectionDirections())
                {
                    if (toDirection == -direction)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private void CheckWinCondition()
    {
        if (_filledCells.Contains(_targetPosition))
        {
            Debug.Log("Победа! Трубопровод соединен!");
            // Здесь можно добавить визуальные эффекты победы
        }
    }

    private void ClearFilledCells()
    {
        foreach (var pos in _filledCells)
        {
            _grid[pos.x, pos.y].SetFilled(false, _spritesUnfilled);
        }
        _filledCells.Clear();
    }

    private bool IsPositionValid(Vector2Int position)
    {
        return position.x >= 0 && position.x < _gridSize.x &&
               position.y >= 0 && position.y < _gridSize.y;
    }

    private bool ValidateComponents()
    {
        if (_cellPrefab == null)
        {
            Debug.LogError("Cell prefab is not assigned!");
            return false;
        }

        if (_gridLayout == null || _gridContainer == null)
        {
            Debug.LogError("Grid components are not assigned!");
            return false;
        }

        return true;
    }

    private void ConfigureGrid()
    {
        float containerWidth = _gridContainer.rect.width;
        float containerHeight = _gridContainer.rect.height;

        float cellSize = Mathf.Min(
            containerWidth / _gridSize.x,
            containerHeight / _gridSize.y
        );

        _gridLayout.cellSize = new Vector2(cellSize, cellSize);
        _gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        _gridLayout.constraintCount = Mathf.Min(_gridSize.x, _gridSize.y);
    }

    private void GenerateGrid()
    {
        _grid = new MiniGame_Line_Cell[_gridSize.x, _gridSize.y];

        for (int x = 0; x < _gridSize.x; x++)
        {
            for (int y = 0; y < _gridSize.y; y++)
            {
                var cell = Instantiate(_cellPrefab, _gridContainer.transform);
                cell.Initialize(new Vector2Int(x, y), _spritesUnfilled, this);
                _grid[x, y] = cell;
            }
        }
    }
}