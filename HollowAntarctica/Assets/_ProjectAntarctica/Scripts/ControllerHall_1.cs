using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

public class ControllerHall_1 : MonoBehaviour
{
    public string validSign = "132"; // ������ ������������ ����� � ����������� �������

    private List<int> enteredDigits = new List<int>();
    private HashSet<int> validDigits = new HashSet<int>();

    public UnityEvent OnComplete; // ��������� UnityEvent ��� ��������� ����������
    public UnityEvent OnOff;      // ��������� UnityEvent ��� ������������

    public void SetValidSign(string _validSign)
    {
        validSign = _validSign;
        // �������������� ������ ���������� ����
        foreach (char c in validSign)
        {
            if (char.IsDigit(c))
            {
                validDigits.Add(int.Parse(c.ToString()));
            }
        }

        // ���������, ��� validSign �������� ����� 3 ���������� �����
        if (validDigits.Count != 3)
        {
            Debug.LogError("validSign must contain exactly 3 unique digits!");
        }
    }

    // ��������� ����� � ����������� int
    public void CheckDigit(int digit)
    {
        enteredDigits.Add(digit);

        // ���������, ����� ������� 3 �����
        if (enteredDigits.Count == 3)
        {
            CheckCombination();
        }
    }

    private void CheckCombination()
    {
        // ���������, ��� ��� ��������� ����� ���������� � validSign
        bool isValid = true;
        foreach (int digit in enteredDigits)
        {
            if (!validDigits.Contains(digit))
            {
                isValid = false;
                break;
            }
        }

        // ���������, ��� ��� ����� �� validSign ���� �������
        if (isValid)
        {
            foreach (int validDigit in validDigits)
            {
                if (!enteredDigits.Contains(validDigit))
                {
                    isValid = false;
                    break;
                }
            }
        }

        // �������� ��������������� �����
        if (isValid)
        {
            OnComplete?.Invoke();
        }
        else
        {
            OnOff?.Invoke();
        }

        // ������� ������ ��� ��������� ��������
        enteredDigits.Clear();
    }
}