using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MiniGame_Line_Cell : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private MiniGame_Line_Tile _tilePrefab;

    private MiniGame_Line_Tile _currentTile;
    private MiniGame_Line _gameController;
    private Vector2Int _gridPosition;
    private bool _isFilled;
    private bool _isSource;
    private bool _isSpecialTile;

    [Header("Debug")]
    [SerializeField] private TextMeshProUGUI _debugText;
    private bool _debugEnabled = true;

    public void Initialize(Vector2Int gridPosition, MiniGame_Line_Sprites sprites, MiniGame_Line gameController, bool debugMode)
    {
        _gridPosition = gridPosition;
        _gameController = gameController;
        _isSpecialTile = false;
        _isSource = false;

        _currentTile = Instantiate(_tilePrefab, transform);
        _currentTile.transform.localPosition = Vector3.zero;
        _currentTile.transform.localScale = Vector3.one;
        _currentTile.Initialize(GetRandomTileType(), sprites, this);

        _debugEnabled = debugMode;
        if (_debugEnabled)
        {
            CreateDebugText(gridPosition);
            UpdateDebugText();
        }
    }

    private void CreateDebugText(Vector2Int gridPosition)
    {
        GameObject debugObj = new GameObject("DebugText");
        debugObj.transform.SetParent(transform);
        debugObj.transform.SetAsLastSibling();
        debugObj.transform.localPosition = Vector3.zero;

        _debugText = debugObj.AddComponent<TextMeshProUGUI>();
        _debugText.alignment = TextAlignmentOptions.Center;
        _debugText.fontSize = 12;
        _debugText.raycastTarget = false;
    }

    public void UpdateDebugText()
    {
        if (!_debugEnabled || _debugText == null) return;

        string tileType = _currentTile != null ? _currentTile.Type.ToString() : "None";
        string fillStatus = _isFilled ? "Filled" : "Empty";
        int angle = _currentTile != null ? _currentTile.RotationAngle : 0;

        _debugText.text = $"Pos: {_gridPosition.x},{_gridPosition.y}\n" +
                         $"Type: {tileType}\n" +
                         $"Angle: {angle}°\n" +
                         $"State: {fillStatus}";
    }

    public void ConvertToSpecialTile(Sprite sprite, bool isSource)
    {
        _isSpecialTile = true;
        _isSource = isSource;

        if (_currentTile != null)
            Destroy(_currentTile.gameObject);

        _currentTile = Instantiate(_tilePrefab, transform);
        _currentTile.transform.localPosition = Vector3.zero;
        _currentTile.transform.localScale = Vector3.one;

        if (isSource)
            _currentTile.InitializeAsSource(sprite, this);
        else
            _currentTile.InitializeAsStatic(sprite);
    }

    public void OnTileRotated()
    {
        if (!_isSpecialTile || _isSource)
            _gameController.UpdatePipeSystem();
    }

    public void SetFilledState(bool filled, MiniGame_Line_Sprites sprites)
    {
        if (_isSpecialTile && !_isSource) return;

        _isFilled = filled;
        if (_currentTile != null)
        {
            _currentTile.UpdateVisuals(sprites);
        }

        UpdateDebugText();

    }

    public Vector2Int[] GetConnectionDirections()
    {
        // Специальные тайлы (кроме источников) не имеют соединений
        if (_isSpecialTile && !_isSource)
            return new Vector2Int[0];

        if (_currentTile == null)
            return new Vector2Int[0];

        return _currentTile.GetConnectionDirections();
    }

    private MiniGame_Line_Tile.TileType GetRandomTileType()
    {
        float r = Random.value;
        return r < 0.5f ? MiniGame_Line_Tile.TileType.Line :
               r < 0.8f ? MiniGame_Line_Tile.TileType.Corner :
               MiniGame_Line_Tile.TileType.CrossT;
    }

    // Properties
    public Vector2Int GridPosition => _gridPosition;
    public bool IsFilled => _isFilled;
    public bool IsSource => _isSource;
    public bool IsSpecialTile => _isSpecialTile;
}