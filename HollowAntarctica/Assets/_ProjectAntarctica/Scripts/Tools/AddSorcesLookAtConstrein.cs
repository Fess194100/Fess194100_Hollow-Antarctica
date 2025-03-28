using UnityEngine;
using UnityEngine.Animations;

public class AddSourcesLookAtConstraint : MonoBehaviour
{
    public LookAtConstraint lookAtConstraint;

    void Start()
    {
        // Проверяем, что компонент LookAtConstraint есть
        if (lookAtConstraint == null)
        {
            lookAtConstraint = GetComponent<LookAtConstraint>();
            if (lookAtConstraint == null)
            {
                Debug.LogError("LookAtConstraint component not found!");
                return;
            }
        }

        // Создаем ConstraintSource и добавляем камеру
        ConstraintSource source = new ConstraintSource
        {
            sourceTransform = Camera.main.transform,
            weight = 1f // Вес влияния (1 = полное влияние)
        };

        lookAtConstraint.AddSource(source);
        lookAtConstraint.constraintActive = true; // Активируем ограничение
    }
}
