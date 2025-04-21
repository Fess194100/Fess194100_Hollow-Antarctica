using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

public class ControllerHall_1 : MonoBehaviour
{
    public string validSign = "132"; // Пример трехзначного числа с уникальными цифрами

    private List<int> enteredDigits = new List<int>();
    private HashSet<int> validDigits = new HashSet<int>();

    public UnityEvent OnComplete; // Публичный UnityEvent для успешного совпадения
    public UnityEvent OnOff;      // Публичный UnityEvent для несовпадения

    public void SetValidSign(string _validSign)
    {
        validSign = _validSign;
        // Инициализируем список допустимых цифр
        foreach (char c in validSign)
        {
            if (char.IsDigit(c))
            {
                validDigits.Add(int.Parse(c.ToString()));
            }
        }

        // Проверяем, что validSign содержит ровно 3 уникальные цифры
        if (validDigits.Count != 3)
        {
            Debug.LogError("validSign must contain exactly 3 unique digits!");
        }
    }

    // Публичный метод с перегрузкой int
    public void CheckDigit(int digit)
    {
        enteredDigits.Add(digit);

        // Проверяем, когда набрано 3 цифры
        if (enteredDigits.Count == 3)
        {
            CheckCombination();
        }
    }

    private void CheckCombination()
    {
        // Проверяем, что все введенные цифры содержатся в validSign
        bool isValid = true;
        foreach (int digit in enteredDigits)
        {
            if (!validDigits.Contains(digit))
            {
                isValid = false;
                break;
            }
        }

        // Проверяем, что все цифры из validSign были введены
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

        // Вызываем соответствующий ивент
        if (isValid)
        {
            OnComplete?.Invoke();
        }
        else
        {
            OnOff?.Invoke();
        }

        // Очищаем список для следующей проверки
        enteredDigits.Clear();
    }
}