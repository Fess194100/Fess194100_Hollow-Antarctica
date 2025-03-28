using UnityEngine;
using UnityEngine.Animations;

public class AddSourcesLookAtConstraint : MonoBehaviour
{
    public LookAtConstraint lookAtConstraint;

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

        // ������� ConstraintSource � ��������� ������
        ConstraintSource source = new ConstraintSource
        {
            sourceTransform = Camera.main.transform,
            weight = 1f // ��� ������� (1 = ������ �������)
        };

        lookAtConstraint.AddSource(source);
        lookAtConstraint.constraintActive = true; // ���������� �����������
    }
}
