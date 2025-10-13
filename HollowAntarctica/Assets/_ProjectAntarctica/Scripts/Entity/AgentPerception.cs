using UnityEngine;
using System.Collections;
using System;

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

        // События восприятия
        public event Action<GameObject> OnTargetSpotted;    // Обнаружен враг
        public event Action<GameObject> OnTargetLost;       // Враг потерян
        public event Action<Vector3> OnSoundHeard;          // Услышан звук

        private GameObject currentTarget;
        private Vector3 lastKnownTargetPosition;
        private Coroutine perceptionCoroutine;
        private EntityType currentEntityType;

        private void Start()
        {
            perceptionCoroutine = StartCoroutine(PerceptionUpdate());
        }

        private IEnumerator PerceptionUpdate()
        {
            while (true)
            {
                UpdateVision();
                yield return new WaitForSeconds(0.1f); // Частота проверки зрения
            }
        }

        private void UpdateVision()
        {
            // Поиск целей в радиусе зрения
            Collider[] targetsInRange = Physics.OverlapSphere(transform.position, visionSettings.visionRange);

            foreach (Collider target in targetsInRange)
            {
                if (IsValidTarget(target.gameObject))
                {
                    if (debugMode) Debug.Log($"IsValidTarget: {target.gameObject.name}");
                    if (IsTargetVisible(target.gameObject))
                    {
                        if (debugMode) Debug.Log($"IsTargetVisible: {target.gameObject.name}");
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

            // Проверка потери цели
            if (currentTarget != null && !IsTargetVisible(currentTarget))
            {
                OnTargetLost?.Invoke(currentTarget);
                if (debugMode) Debug.Log($"Target lost: {currentTarget.name}");
                currentTarget = null;
            }
        }

        private bool IsValidTarget(GameObject target)
        {
            FlagFaction flagFaction = target.GetComponent<FlagFaction>();
            if (flagFaction != null)
            {
                if (flagFaction.flagFaction == currentEntityType || flagFaction.flagFaction == EntityType.Neutral) return false;
                else return true;
            }
            else return false;
        }

        private bool IsTargetVisible(GameObject target)
        {
            Vector3 directionToTarget = (target.transform.position - transform.position).normalized;
            float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);

            // Проверка угла обзора
            float angleToTarget = Vector3.Angle(transform.forward, directionToTarget);
            if (debugMode) Debug.Log($"Angle To Target: {angleToTarget}");

            if (angleToTarget > visionSettings.visionAngle / 2f &&
                distanceToTarget > visionSettings.peripheralVisionRange)
            {                
                return false;
            }

            // Проверка препятствий
            if (Physics.Raycast(transform.position, directionToTarget, out RaycastHit hit,
                distanceToTarget, visionSettings.visionObstacleMask))
            {
                return hit.collider.gameObject == target;
            }

            return true;
        }

        // Метод для регистрации звуков (вызывается извне)
        public void RegisterSound(Vector3 soundPosition, float soundVolume)
        {
            float distanceToSound = Vector3.Distance(transform.position, soundPosition);
            float audibleRange = hearingSettings.hearingRange * soundVolume;

            if (distanceToSound <= audibleRange)
            {
                // Проверка препятствий для звука
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
        public void SetCurrentEntityType(EntityType entityType)
        {
            currentEntityType = entityType;
        }
        private void OnDrawGizmosSelected()
        {
            if (!debugMode) return;

            // Визуализация зрения
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, visionSettings.visionRange);

            // Визуализация угла обзора
            Vector3 leftBoundary = Quaternion.Euler(0, -visionSettings.visionAngle / 2, 0) * transform.forward * visionSettings.visionRange;
            Vector3 rightBoundary = Quaternion.Euler(0, visionSettings.visionAngle / 2, 0) * transform.forward * visionSettings.visionRange;

            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(transform.position, leftBoundary);
            Gizmos.DrawRay(transform.position, rightBoundary);

            // Визуализация слуха
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