using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Renderer))]
public class MPB_ColorHDR : MonoBehaviour
{
    [Header("Color Settings")]
    [ColorUsage(true, true)]
    public Color decalColor = Color.white;

    [Tooltip("Shader property name (default: _BaseColor)")]
    public string colorProperty = "_BaseColor";

    private Renderer rend;
    private MaterialPropertyBlock propBlock;
    private Color lastAppliedColor;

    void OnEnable()
    {
        UpdateColor();
    }

    void OnValidate()
    {
        // Автоматически обновляем при изменении в инспекторе
        if (isActiveAndEnabled && Application.isEditor)
        {
            UpdateColor();
        }
    }

    void UpdateColor()
    {
        // Пропускаем если цвет не изменился
        if (lastAppliedColor == decalColor)
            return;

        if (rend == null)
            rend = GetComponent<Renderer>();

        if (propBlock == null)
            propBlock = new MaterialPropertyBlock();

        // Получаем текущие свойства
        rend.GetPropertyBlock(propBlock);

        // Устанавливаем новый цвет
        propBlock.SetColor(colorProperty, decalColor);

        // Применяем обратно
        rend.SetPropertyBlock(propBlock);

        lastAppliedColor = decalColor;
    }

    // Метод для изменения цвета из кода
    public void ChangeColor(Color newColor)
    {
        decalColor = newColor;
        UpdateColor();
    }
}