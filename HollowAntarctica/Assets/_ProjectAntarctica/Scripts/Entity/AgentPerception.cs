using UnityEngine;
using System.Collections;
using System;
using SimpleCharController;

namespace AdaptivEntityAgent
{
    [System.Serializable]
    public class VisionSettings
    {
        public float visionRange = 10f;
        public float visionAngle = 90f;
        public LayerMask visionObstacleMask = ~0;
        public float peripheralVisionRange = 5f;
    }

    [System.Serializable]
    public class HearingSettings
    {
        public float hearingRange = 15f;
        public LayerMask soundObstacleMask = ~0;
    }

    public class AgentPerception : MonoBehaviour
    {
        [Header("Vision Settings")]
        [SerializeField] private VisionSettings visionSettings = new VisionSettings();

        [Header("Hearing Settings")]
        [SerializeField] private HearingSettings hearingSettings = new HearingSettings();

        [Header("Debug")]
        [SerializeField] private bool debugMode = false;

        // ������� ����������
        public event Action<GameObject> OnTargetSpotted;    // ��������� ����
        public event Action<GameObject> OnTargetLost;       // ���� �������
        public event Action<Vector3> OnSoundHeard;          // ������� ����

        private GameObject currentTarget;
        private Vector3 lastKnownTargetPosition;
        private Coroutine perceptionCoroutine;

        private void Start()
        {
            perceptionCoroutine = StartCoroutine(PerceptionUpdate());
        }

        private IEnumerator PerceptionUpdate()
        {
            while (true)
            {
                UpdateVision();
                yield return new WaitForSeconds(0.1f); // ������� �������� ������
            }
        }

        private void UpdateVision()
        {
            // ����� ����� � ������� ������
            Collider[] targetsInRange = Physics.OverlapSphere(transform.position, visionSettings.visionRange);

            foreach (Collider target in targetsInRange)
            {
                if (IsValidTarget(target.gameObject))
                {
                    if (IsTargetVisible(target.gameObject))
                    {
                        if (currentTarget != target.gameObject)
                        {
                            currentTarget = target.gameObject;
                            lastKnownTargetPosition = currentTarget.transform.position;
                            OnTargetSpotted?.Invoke(currentTarget);

                            if (debugMode) Debug.Log($"Target spotted: {currentTarget.name}");
                        }
                        else
                        {
                            lastKnownTargetPosition = currentTarget.transform.position;
                        }
                    }
                }
            }

            // �������� ������ ����
            if (currentTarget != null && !IsTargetVisible(currentTarget))
            {
                OnTargetLost?.Invoke(currentTarget);
                if (debugMode) Debug.Log($"Target lost: {currentTarget.name}");
                currentTarget = null;
            }
        }

        private bool IsValidTarget(GameObject target)
        {
            // ���������, �������� �� ������ ������������� �����
            // ����� �������� �������� �� �����, ����������� � �.�.
            return target.GetComponent<EssenceHealth>() != null && target != gameObject;
        }

        private bool IsTargetVisible(GameObject target)
        {
            Vector3 directionToTarget = (target.transform.position - transform.position).normalized;
            float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);

            // �������� ���� ������
            float angleToTarget = Vector3.Angle(transform.forward, directionToTarget);

            if (angleToTarget > visionSettings.visionAngle / 2f &&
                distanceToTarget > visionSettings.peripheralVisionRange)
            {
                return false;
            }

            // �������� �����������
            if (Physics.Raycast(transform.position, directionToTarget, out RaycastHit hit,
                distanceToTarget, visionSettings.visionObstacleMask))
            {
                return hit.collider.gameObject == target;
            }

            return true;
        }

        // ����� ��� ����������� ������ (���������� �����)
        public void RegisterSound(Vector3 soundPosition, float soundVolume)
        {
            float distanceToSound = Vector3.Distance(transform.position, soundPosition);
            float audibleRange = hearingSettings.hearingRange * soundVolume;

            if (distanceToSound <= audibleRange)
            {
                // �������� ����������� ��� �����
                if (!Physics.Linecast(transform.position, soundPosition, hearingSettings.soundObstacleMask))
                {
                    OnSoundHeard?.Invoke(soundPosition);

                    if (debugMode)
                        Debug.Log($"Sound heard at position: {soundPosition}");
                }
            }
        }

        public GameObject GetCurrentTarget() => currentTarget;
        public Vector3 GetLastKnownTargetPosition() => lastKnownTargetPosition;
        public bool HasTarget() => currentTarget != null;

        private void OnDrawGizmosSelected()
        {
            if (!debugMode) return;

            // ������������ ������
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, visionSettings.visionRange);

            // ������������ ���� ������
            Vector3 leftBoundary = Quaternion.Euler(0, -visionSettings.visionAngle / 2, 0) * transform.forward * visionSettings.visionRange;
            Vector3 rightBoundary = Quaternion.Euler(0, visionSettings.visionAngle / 2, 0) * transform.forward * visionSettings.visionRange;

            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(transform.position, leftBoundary);
            Gizmos.DrawRay(transform.position, rightBoundary);

            // ������������ �����
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, hearingSettings.hearingRange);
        }

        private void OnDestroy()
        {
            if (perceptionCoroutine != null)
                StopCoroutine(perceptionCoroutine);
        }
    }
}