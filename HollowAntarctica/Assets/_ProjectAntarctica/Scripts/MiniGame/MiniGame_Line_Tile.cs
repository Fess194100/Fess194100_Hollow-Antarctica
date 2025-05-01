using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MiniGame_Line_Tile : MonoBehaviour, IPointerClickHandler
{
    public enum TileType { Line, Corner, CrossT }

    [SerializeField] private Image _image;
    private TileType _type;
    private int _rotationAngle;
    private MiniGame_Line_Cell _parentCell;

    public void Initialize(TileType type, MiniGame_Line_Sprites sprites, MiniGame_Line_Cell parentCell)
    {
        _type = type;
        _parentCell = parentCell;
        UpdateSprite(sprites);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Rotate();
        _parentCell.OnTileRotated();
    }

    public void Rotate()
    {
        _rotationAngle = (_rotationAngle + 90) % 360;
        transform.rotation = Quaternion.Euler(0, 0, _rotationAngle);
    }

    public void UpdateSprite(MiniGame_Line_Sprites sprites)
    {
        switch (_type)
        {
            case TileType.Line: _image.sprite = sprites.line; break;
            case TileType.Corner: _image.sprite = sprites.corner; break;
            case TileType.CrossT: _image.sprite = sprites.crossT; break;
        }
    }

    public Vector2Int[] GetConnectionDirections()
    {
        // Возвращает направления соединений в зависимости от типа и поворота
        switch (_type)
        {
            case TileType.Line:
                return _rotationAngle % 180 == 0 ?
                    new[] { Vector2Int.up, Vector2Int.down } :
                    new[] { Vector2Int.left, Vector2Int.right };

            case TileType.Corner:
                if (_rotationAngle == 0) return new[] { Vector2Int.right, Vector2Int.up };
                if (_rotationAngle == 90) return new[] { Vector2Int.up, Vector2Int.left };
                if (_rotationAngle == 180) return new[] { Vector2Int.left, Vector2Int.down };
                return new[] { Vector2Int.down, Vector2Int.right };

            case TileType.CrossT:
                if (_rotationAngle == 0) return new[] { Vector2Int.left, Vector2Int.up, Vector2Int.right };
                if (_rotationAngle == 90) return new[] { Vector2Int.up, Vector2Int.right, Vector2Int.down };
                if (_rotationAngle == 180) return new[] { Vector2Int.right, Vector2Int.down, Vector2Int.left };
                return new[] { Vector2Int.down, Vector2Int.left, Vector2Int.up };
        }
        return new Vector2Int[0];
    }
}