using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
[RequireComponent(typeof(RectTransform))]
public class CellContent : MonoBehaviour
{
    public Image floor;
    public Image wall;
    public Image player;
    public Image[] boxes = new Image[3]; // 3 типа ящиков
    public Image[] targets = new Image[3]; // 3 типа целей
}