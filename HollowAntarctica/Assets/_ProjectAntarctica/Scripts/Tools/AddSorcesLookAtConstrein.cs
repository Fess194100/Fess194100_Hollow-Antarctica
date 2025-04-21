using UnityEngine;
using UnityEngine.Animations;

public class AddSourcesLookAtConstraint : MonoBehaviour
{
    public LookAtConstraint lookAtConstraint;
    public bool setMainCam = true;

    void Start()
    {
        // ���������, ��� ��������� LookAtConstraint ����
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
        // ������� ConstraintSource � ��������� ������
        ConstraintSource source = new ConstraintSource
        {
            sourceTransform = transform,
            weight = weight // ��� ������� (1 = ������ �������)
        };

        lookAtConstraint.AddSource(source);
        lookAtConstraint.constraintActive = true; // ���������� �����������
    }
}
