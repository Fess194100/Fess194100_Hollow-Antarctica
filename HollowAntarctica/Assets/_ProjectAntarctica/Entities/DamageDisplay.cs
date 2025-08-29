using UnityEngine;
using TMPro;
using SimpleCharController;
using HutongGames.PlayMaker.Actions;
using static UnityEngine.Rendering.DebugUI;

public class DamageDisplay : MonoBehaviour
{
    [Header("Настройки отображения урона")]
    public TMP_Text damageText;    // Ссылка на TextMeshPro

    [Header("Ссылка на здоровье")]
    public EssenceHealth healthComponent; // Ссылка на компонент здоровья

    private float _totalDamage;
    void Start()
    {
        // Проверяем и настраиваем компоненты
        if (damageText == null)
        {
            damageText = GetComponent<TMP_Text>();
            if (damageText == null)
            {
                Debug.LogError("Не назначен TextMeshPro компонент!");
                return;
            }
        }

        // Ищем компонент здоровья если не назначен
        if (healthComponent == null)
        {
            healthComponent = GetComponentInParent<EssenceHealth>();
            if (healthComponent == null)
            {
                Debug.LogError("Не найден компонент EssenceHealth!");
                return;
            }
        }

        // Подписываемся на событие
        healthComponent.OnDamageTaken.AddListener(OnDamageTakenHandler);
    }

    // Метод для подписки на событие урона
    public void OnDamageTakenHandler(float damageAmount)
    {
        DisplayDamage(damageAmount);
    }

    void DisplayDamage(float damageAmount)
    {
        _totalDamage += damageAmount;
        int damageIndex = Mathf.RoundToInt(_totalDamage);

        // Устанавливаем текст с знаком минус
        damageText.text = $"- {damageIndex:F1}";
    }

    public void ResetTotalDamage()
    {
        _totalDamage = 0;
        DisplayDamage(_totalDamage);
    }
    void OnDestroy()
    {
        // Отписываемся от события при уничтожении
        if (healthComponent != null)
        {
            healthComponent.OnDamageTaken.RemoveListener(OnDamageTakenHandler);
        }
    }

    void OnDisable()
    {
        // Отписываемся от события при отключении
        if (healthComponent != null)
        {
            healthComponent.OnDamageTaken.RemoveListener(OnDamageTakenHandler);
        }
    }
}