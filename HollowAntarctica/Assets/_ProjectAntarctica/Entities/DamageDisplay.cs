using UnityEngine;
using TMPro;
using SimpleCharController;
using HutongGames.PlayMaker.Actions;
using static UnityEngine.Rendering.DebugUI;

public class DamageDisplay : MonoBehaviour
{
    [Header("��������� ����������� �����")]
    public TMP_Text damageText;    // ������ �� TextMeshPro

    [Header("������ �� ��������")]
    public EssenceHealth healthComponent; // ������ �� ��������� ��������

    private float _totalDamage;
    void Start()
    {
        // ��������� � ����������� ����������
        if (damageText == null)
        {
            damageText = GetComponent<TMP_Text>();
            if (damageText == null)
            {
                Debug.LogError("�� �������� TextMeshPro ���������!");
                return;
            }
        }

        // ���� ��������� �������� ���� �� ��������
        if (healthComponent == null)
        {
            healthComponent = GetComponentInParent<EssenceHealth>();
            if (healthComponent == null)
            {
                Debug.LogError("�� ������ ��������� EssenceHealth!");
                return;
            }
        }

        // ������������� �� �������
        healthComponent.OnDamageTaken.AddListener(OnDamageTakenHandler);
    }

    // ����� ��� �������� �� ������� �����
    public void OnDamageTakenHandler(float damageAmount)
    {
        DisplayDamage(damageAmount);
    }

    void DisplayDamage(float damageAmount)
    {
        _totalDamage += damageAmount;
        int damageIndex = Mathf.RoundToInt(_totalDamage);

        // ������������� ����� � ������ �����
        damageText.text = $"- {damageIndex:F1}";
    }

    public void ResetTotalDamage()
    {
        _totalDamage = 0;
        DisplayDamage(_totalDamage);
    }
    void OnDestroy()
    {
        // ������������ �� ������� ��� �����������
        if (healthComponent != null)
        {
            healthComponent.OnDamageTaken.RemoveListener(OnDamageTakenHandler);
        }
    }

    void OnDisable()
    {
        // ������������ �� ������� ��� ����������
        if (healthComponent != null)
        {
            healthComponent.OnDamageTaken.RemoveListener(OnDamageTakenHandler);
        }
    }
}