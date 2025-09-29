using UnityEngine;
using TMPro;
using SimpleCharController;

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
    public void OnDamageTakenHandler(float damageAmount, BodyPart bodyPart)
    {
        DisplayDamage(damageAmount, bodyPart);
    }

    void DisplayDamage(float damageAmount, BodyPart bodyPart)
    {
        _totalDamage += damageAmount;
        int damageIndex = Mathf.RoundToInt(_totalDamage);

        // ������������� ����� � ������ �����
        damageText.text = $"- {damageIndex}";
    }

    public void ResetTotalDamage()
    {
        _totalDamage = 0;
        DisplayDamage(_totalDamage, BodyPart.Body);
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