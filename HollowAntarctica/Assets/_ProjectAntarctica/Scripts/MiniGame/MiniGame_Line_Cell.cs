using UnityEngine;
using UnityEngine.UI;

public class MiniGame_Line_Cell : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MiniGame_Line_Tile _tilePrefab;

    private MiniGame_Line_Tile _currentTile;
    private MiniGame_Line _gameController;
    private Vector2Int _gridPosition;
    private bool _isFilled;

    public void Initialize(Vector2Int gridPosition, MiniGame_Line_Sprites sprites, MiniGame_Line gameController)
    {
        _gridPosition = gridPosition;
        _gameController = gameController;

        _currentTile = Instantiate(_tilePrefab, transform);
        _currentTile.transform.localPosition = Vector3.zero;
        _currentTile.Initialize(GetRandomTileType(), sprites, this);
    }

    public void OnTileRotated()
    {
        _gameController.UpdatePipeSystem();
    }

    public void SetFilled(bool filled, MiniGame_Line_Sprites sprites)
    {
        _isFilled = filled;
        _currentTile.UpdateSprite(sprites);
    }

    public Vector2Int[] GetConnectionDirections()
    {
        return _currentTile.GetConnectionDirections();
    }

    public Vector2Int GridPosition => _gridPosition;
    public bool IsFilled => _isFilled;

    private MiniGame_Line_Tile.TileType GetRandomTileType()
    {
        return (MiniGame_Line_Tile.TileType)Random.Range(0, 3);
    }
}