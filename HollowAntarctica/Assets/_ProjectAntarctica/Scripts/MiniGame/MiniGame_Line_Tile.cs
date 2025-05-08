using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MiniGame_Line_Tile : MonoBehaviour, IPointerClickHandler
{
    public enum TileType { Line, Corner, CrossT }

    private Image _image;
    private TileType _type;
    private int _rotationAngle;
    private MiniGame_Line_Cell _parentCell;
    private bool _isSource;
    private bool _isStatic;

    public void Initialize(TileType type, MiniGame_Line_Sprites sprites, MiniGame_Line_Cell parentCell)
    {
        _image = GetComponent<Image>();
        _type = type;
        _parentCell = parentCell;
        _isSource = false;
        _isStatic = false;
        UpdateVisuals(sprites);
    }

    public void InitializeAsSource(Sprite sprite, MiniGame_Line_Cell parentCell)
    {
        _image = GetComponent<Image>();
        _image.sprite = sprite;
        _parentCell = parentCell;
        _isSource = true;
        _isStatic = false;

        // Добавляем коллайдер если нужно
        if (GetComponent<Collider2D>() == null)
            gameObject.AddComponent<BoxCollider2D>();
    }

    public void InitializeAsStatic(Sprite sprite)
    {
        _image = GetComponent<Image>();
        _image.sprite = sprite;
        _isStatic = true;

        // Удаляем ненужные компоненты
        Destroy(GetComponent<EventTrigger>());
        Destroy(GetComponent<Collider2D>());
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!_isStatic && _parentCell != null)
        {
            Rotate();
            _parentCell.OnTileRotated();
        }
    }

    public void Rotate()
    {
        _rotationAngle = (_rotationAngle + 90) % 360;
        transform.rotation = Quaternion.Euler(0, 0, _rotationAngle);
    }

    public void UpdateVisuals(MiniGame_Line_Sprites sprites)
    {
        if (_isStatic || _isSource) return;

        var image = GetComponent<Image>();
        if (image == null) return;

        if (sprites != null)
        {
            switch (_type)
            {
                case TileType.Line: image.sprite = sprites.line; break;
                case TileType.Corner: image.sprite = sprites.corner; break;
                case TileType.CrossT: image.sprite = sprites.crossT; break;
            }
            return;
        }

        /*switch (_type)
        {
            case TileType.Line: image.sprite = sprites.line; break;
            case TileType.Corner: image.sprite = sprites.corner; break;
            case TileType.CrossT: image.sprite = sprites.crossT; break;
        }*/
    }

    public Vector2Int[] GetConnectionDirections()
    {
        if (_isStatic) return new Vector2Int[0];

        // Для источника - соединение во все стороны
        if (_isSource) return new[] {
        Vector2Int.up, Vector2Int.down,
        Vector2Int.left, Vector2Int.right
    };

        // Для обычных тайлов учитываем поворот
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

    public TileType Type => _type;
    public int RotationAngle => _rotationAngle;
}