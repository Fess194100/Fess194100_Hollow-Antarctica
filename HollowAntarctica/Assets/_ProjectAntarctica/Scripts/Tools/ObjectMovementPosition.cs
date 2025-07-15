using UnityEngine;
using Cinemachine;

public class ObjectMovementPosition : MonoBehaviour
{
    [SerializeField] private Transform _pointsParent;
    [SerializeField] private float _moveInterval = 1f;
    [SerializeField] private CinemachineVirtualCamera _virtualCamera;

    private Transform[] _movementPoints;
    private int _currentPointIndex = 0;
    private float _moveTimer = 0f;
    private bool _isActive = true;

    private void Start()
    {
        InitializeMovementPoints();
    }

    private void Update()
    {
        if (!_isActive || _movementPoints == null || _movementPoints.Length == 0)
            return;

        ProcessMovement();
    }

    private void InitializeMovementPoints()
    {
        if (_pointsParent != null && _pointsParent.childCount > 0)
        {
            _movementPoints = new Transform[_pointsParent.childCount];
            for (int i = 0; i < _pointsParent.childCount; i++)
            {
                _movementPoints[i] = _pointsParent.GetChild(i);
            }
            MoveToCurrentPoint();
        }
        else
        {
            _isActive = false;
        }
    }

    private void ProcessMovement()
    {
        _moveTimer += Time.deltaTime;

        if (_moveTimer >= _moveInterval)
        {
            _moveTimer = 0f;
            _currentPointIndex++;

            if (_currentPointIndex >= _movementPoints.Length)
            {
                CompleteMovement();
                return;
            }

            MoveToCurrentPoint();
        }
    }

    private void MoveToCurrentPoint()
    {
        transform.SetPositionAndRotation(
            _movementPoints[_currentPointIndex].position,
            _movementPoints[_currentPointIndex].rotation
        );
    }

    private void CompleteMovement()
    {
        _isActive = false;

        if (_virtualCamera != null)
        {
            _virtualCamera.Priority = 0;
        }
    }
}