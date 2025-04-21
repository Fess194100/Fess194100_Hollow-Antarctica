using UnityEngine;
using UnityEngine.Animations;

public class AddSourcesLookAtConstraint : MonoBehaviour
{
    public LookAtConstraint lookAtConstraint;
    public bool setMainCam = true;

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

        if (setMainCam) AddSourceTransform(Camera.main.transform, 1f);
        
    }

    public void AddSourceTransform(Transform transform, float weight)
    {
        // Создаем ConstraintSource и добавляем камеру
        ConstraintSource source = new ConstraintSource
        {
            sourceTransform = transform,
            weight = weight // Вес влияния (1 = полное влияние)
        };

        lookAtConstraint.AddSource(source);
        lookAtConstraint.constraintActive = true; // Активируем ограничение
    }
}
