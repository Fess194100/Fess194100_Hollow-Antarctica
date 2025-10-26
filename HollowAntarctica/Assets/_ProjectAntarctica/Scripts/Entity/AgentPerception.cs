using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.InputSystem.LowLevel;

namespace AdaptivEntityAgent
{
    [Serializable]
    public class VisionSettings
    {
        [Header("Detection Settings")]
        public float visionRange = 24f;
        public float visionAngle = 90f;
        public LayerMask visionObstacleMask = ~0;
        public LayerMask visionTargetMask = ~0;
        public float peripheralVisionRange = 5f;
        public float targetSwitchRadius = 5f;
        public int maxTargets = 3;

        [Header("Detection Frequency")]
        public AnimationCurve detectionFrequencyByDistance = new AnimationCurve(
            new Keyframe(5f, 0.1f),
            new Keyframe(30f, 1f)
        );
    }

    [Serializable]
    public class HearingSettings
    {
        public float hearingRange = 15f;
        public LayerMask soundObstacleMask = ~0;
    }

    [Serializable]
    public struct PotentialTarget
    {
        public GameObject target;
        public float distance;
        public bool isVisible;

        public PotentialTarget(GameObject target, float distance = float.MaxValue)
        {
            this.target = target;
            this.distance = distance;
            this.isVisible = false;
        }
    }

    public class AgentPerception : MonoBehaviour
    {
        #region Variables
        [Header("Vision Settings")]
        [SerializeField] private VisionSettings visionSettings = new VisionSettings();

        [Header("Agent Events")]
        public AgentEventsPerception perceptionEvents = new AgentEventsPerception();

        [Header("Debug")]
        [SerializeField] private bool debugMode = false;
        [SerializeField] private bool drawDebugGizmo = false;
        #endregion

        #region Private Variables
        private GameObject currentTarget;
        private Vector3 lastKnownTargetPosition;
        private Coroutine perceptionCoroutine;
        private EntityType currentEntityType;
        private List<PotentialTarget> potentialTargets;
        private float currentTargetDistance;
        #endregion

        #region Public Properties
        public int CountPotentialTargets => potentialTargets.Count;
        public float CurrentTargetDistance => currentTargetDistance;
        public bool HasTarget => currentTarget != null;
        public GameObject CurrentTarget => currentTarget;
        public Vector3 LastKnownTargetPosition => lastKnownTargetPosition;
        #endregion

        #region System Functions
        private void Start()
        {
            InitializePerception();
        }

        private void InitializePerception()
        {
            potentialTargets = new List<PotentialTarget>();
            currentTarget = null;
            currentTargetDistance = float.MaxValue;
            perceptionCoroutine = StartCoroutine(PerceptionUpdate());
        }

        private void OnDestroy()
        {
            if (perceptionCoroutine != null)
                StopCoroutine(perceptionCoroutine);
        }

        private void OnDrawGizmosSelected()
        {
            if (!drawDebugGizmo) return;

            DrawVisionGizmos();
            DrawTargetGizmos();
        }
        #endregion

        #region Private Methods
        /*public void OnStateChanged(AgentState newState)
        {
            if (debugMode) Debug.Log($"AgentPerception - OnStateChanged.RemoveCurrentTarget XXX = {newState}");
            if (newState == AgentState.Dead) RemoveCurrentTarget();
        }*/

        
        private IEnumerator PerceptionUpdate()
        {
            while (true)
            {
                UpdatePotentialTargets();
                UpdateCurrentTarget();
                UpdateCurrentTargetDistance();
                UpdateVision();

                yield return new WaitForSeconds(GetCheckFrequency());
            }
        }

        private float GetCheckFrequency()
        {
            if (currentTarget != null)
            {
                return visionSettings.detectionFrequencyByDistance.Evaluate(currentTargetDistance);
            }
            else
            {
                return visionSettings.detectionFrequencyByDistance.Evaluate(visionSettings.visionRange);
            }
        }

        //------------------------------------------------------------------------------------------------

        private void UpdatePotentialTargets()
        {
            if (potentialTargets.Count < visionSettings.maxTargets)
            {
                Collider[] targetsInRange = Physics.OverlapSphere(
                transform.position,
                visionSettings.visionRange,
                visionSettings.visionTargetMask
            );

                foreach (Collider target in targetsInRange)
                {
                    if (IsValidTarget(target.gameObject) && !IsTargetInList(target.gameObject) && potentialTargets.Count < visionSettings.maxTargets)
                    {
                        float distance = Vector3.Distance(transform.position, target.transform.position);
                        potentialTargets.Add(new PotentialTarget(target.gameObject, distance));

                        //if (debugMode) Debug.Log($"Added potential target: {target.gameObject.name}");
                    }
                }
            }
            else
            {
                // ��������� ��������� ��� ���� ������������ �����
                for (int i = 0; i < potentialTargets.Count; i++)
                {
                    if (potentialTargets[i].target != null)
                    {
                        var target = potentialTargets[i];

                        if (target.target == currentTarget) target.distance = currentTargetDistance;
                        else target.distance = Vector3.Distance(transform.position, target.target.transform.position);

                        potentialTargets[i] = target;
                    }
                }
            }

            // ������� ����, ������� ����� �� ������� ��� ����� null
            potentialTargets.RemoveAll(target => target.target == null || target.distance > visionSettings.visionRange);
        }

        //------------------------------------------------------------------------------------------------
        private void UpdateCurrentTarget()
        {
            // ���� ������� ���� ��� - ������� ���������
            if (currentTarget == null && potentialTargets.Count > 0)
            {
                GameObject closestTarget = FindClosestTarget();
                if (closestTarget != null)
                {
                    //SetCurrentTarget(closestTarget);
                    currentTarget = closestTarget;
                    if (debugMode) Debug.Log($" currentTarget = closestTarget NOT at Event OnChengetTarget");
                }
                return;
            }

            // ���� ������� ���� ����, ��������� ����� �� ������� ��� ������� ���� ��� ������.
            if (currentTarget != null)
            {
                if (potentialTargets.Count > 0)
                {
                    if (currentTargetDistance > visionSettings.targetSwitchRadius)
                    {
                        GameObject closestTarget = FindClosestTarget();
                        if (closestTarget != null && closestTarget != currentTarget)
                        {
                            SetCurrentTarget(closestTarget);
                        }
                    }

                    if(!IsValidTarget(currentTarget)) RemoveCurrentTarget();
                    return;
                }

                if (potentialTargets.Count == 0 && currentTargetDistance > visionSettings.visionRange)
                {
                    if (currentTarget != null) lastKnownTargetPosition = currentTarget.transform.position;
                    LostCurrentTarget();
                } 
            }
        }

        private GameObject FindClosestTarget()
        {
            if (potentialTargets.Count == 0) return null;

            GameObject closestTarget = null;
            float closestDistance = float.MaxValue;

            foreach (var potentialTarget in potentialTargets)
            {
                if (potentialTarget.target != null && potentialTarget.distance < closestDistance)
                {
                    closestDistance = potentialTarget.distance;
                    closestTarget = potentialTarget.target;
                }
            }

            return closestTarget;
        }

        private void SetCurrentTarget(GameObject newTarget = null)
        {
            if (currentTarget != newTarget)
            {
                if (debugMode)
                {
                    string previousName = currentTarget != null ? currentTarget.name : "null";
                    string currentName = newTarget != null ? newTarget.name : "null";
                    Debug.Log($"Current target changed from {previousName} to: {currentName}");
                }

                if (currentTarget != null) lastKnownTargetPosition = currentTarget.transform.position;
                currentTarget = newTarget;

                UpdateCurrentTargetDistance();
                perceptionEvents.OnTargetChanged?.Invoke(currentTarget);
            }
        }

        //------------------------------------------------------------------------------------------------

        private void UpdateCurrentTargetDistance()
        {
            if (currentTarget != null)
            {
                currentTargetDistance = Vector3.Distance(transform.position, currentTarget.transform.position);
                if (TargetTooFar(currentTargetDistance)) RemoveTargetIfTooFar(currentTarget);
            }
            else
            {
                currentTargetDistance = float.MaxValue;
            }
        }

        private void RemoveTargetIfTooFar(GameObject target)
        {
            if (target == null) return;
            if (target == currentTarget)
            {
                if (potentialTargets.Count > 1)
                {
                    RemoveTargetFromList(target);
                    UpdateCurrentTarget();
                }
                else
                {
                    RemoveCurrentTarget();
                }
            }
            else
            {
                RemoveTargetFromList(target);
            }
        }

        private bool TargetTooFar(float targetDistance)
        {
            return targetDistance > visionSettings.visionRange;
        }

        //------------------------------------------------------------------------------------------------

        private void UpdateVision()
        {
            if (currentTarget == null) return;

            bool isCurrentlyVisible = IsTargetVisible(currentTarget);
            bool targetFromListVisible = GetTargetFromList(currentTarget).isVisible;
            lastKnownTargetPosition = currentTarget.transform.position;

            if (isCurrentlyVisible)
            {
                // ���� ���� ������ ��� ����� ������� (���� �������� � ������)
                if (!targetFromListVisible)
                {
                    perceptionEvents.OnTargetSpotted?.Invoke(currentTarget);
                    UpdateTargetInList(currentTarget, isCurrentlyVisible);
                    if (debugMode) Debug.Log($" Target spotted: {currentTarget.name}");
                }
            }
            else
            {
                // ���� ���� ������ ��� �������� �� ���� (���� ������ � ������)
                if (targetFromListVisible)
                {
                    UpdateTargetInList(currentTarget, isCurrentlyVisible);

                    //�������� ����� ���� ���� ���� ������ ������������� ����
                    if (potentialTargets.Count >= 2)
                    {
                        RemoveTargetFromList(currentTarget);
                        GameObject newTarget = FindClosestTarget();
                        SetCurrentTarget(newTarget);
                    }
                    else
                    {
                        LostCurrentTarget();
                    }
                }
                else
                {
                    UpdateTargetInList(currentTarget, isCurrentlyVisible);
                    //SetCurrentTarget();

                    currentTarget = null;
                    if (debugMode) Debug.Log($" currentTarget = null NOT at Event OnLostTarget");
                }
            }
        }

        private bool IsTargetVisible(GameObject target)
        {
            if (!CanSeeTarget(target)) return false;

            Vector3 directionToTarget = (target.transform.position - transform.position).normalized;
            float angleToTarget = Vector3.Angle(transform.forward, directionToTarget);
            float distanceToTarget = (target == currentTarget) ? currentTargetDistance : Vector3.Distance(transform.position, target.transform.position);

            // �������� ���� ������ ������ ����
            if (angleToTarget > visionSettings.visionAngle / 2f && distanceToTarget > visionSettings.peripheralVisionRange) return false;
            return true;
        }

        private bool CanSeeTarget(GameObject target)
        {
            Vector3 directionToTarget = ((target.transform.position + Vector3.up) - transform.position).normalized;
            float distanceToTarget = (target == currentTarget) ? currentTargetDistance : Vector3.Distance(transform.position, target.transform.position);
            Vector3 rayStart = transform.position + Vector3.up * 1.8f;

            // �������� �����������
            if (Physics.Raycast(rayStart, directionToTarget, out RaycastHit hit, distanceToTarget, visionSettings.visionObstacleMask))
            {
#if UNITY_EDITOR
                if (debugMode)
                {
                    Debug.DrawRay(rayStart, directionToTarget * hit.distance, Color.blue, 0.1f);
                    Debug.Log($"RayVision - Hit = {hit.collider.gameObject.name}, Layer = {LayerMask.LayerToName(hit.collider.gameObject.layer)}");
                }
#endif
                return false;
            }

            return true;
        }

        private void LostCurrentTarget()
        {
            perceptionEvents.OnTargetLost?.Invoke(currentTarget);
            if (debugMode) Debug.Log($" Target lost: {currentTarget.name}");
            currentTarget = null;
            currentTargetDistance = float.MaxValue;
        }

        //------------------------------------------------------------------------------------------------

        private bool IsValidTarget(GameObject target)
        {
            FlagFaction flagFaction = target.GetComponent<FlagFaction>();
            if (flagFaction != null && flagFaction.EssenceHealth != null && !flagFaction.EssenceHealth.IsDead())
            {
                if (flagFaction.flagFaction == currentEntityType || flagFaction.flagFaction == EntityType.Neutral)
                    return false;
                else
                    return true;
            }
            return false;
        }

        //------------------------------------------------------------------------------------------------

        private bool IsTargetInList(GameObject target)
        {
            return potentialTargets.Exists(t => t.target == target);
        }

        private PotentialTarget GetTargetFromList(GameObject target)
        {
            return potentialTargets.Find(t => t.target == target);
        }

        private void UpdateTargetInList(GameObject target, bool isVisible)
        {
            for (int i = 0; i < potentialTargets.Count; i++)
            {
                if (potentialTargets[i].target == target)
                {
                    var updatedTarget = potentialTargets[i];

                    if (target == currentTarget)
                    {
                        updatedTarget.distance = currentTargetDistance;
                    }
                    else
                    {
                        updatedTarget.distance = Vector3.Distance(transform.position, target.transform.position);
                    }

                    updatedTarget.isVisible = isVisible;
                    potentialTargets[i] = updatedTarget;
                    break;
                }
            }
        }

        private void RemoveTargetFromList(GameObject target)
        {
            potentialTargets.RemoveAll(t => t.target == target);
        }
        #endregion

        #region Gizmos
        private void DrawVisionGizmos()
        {
            Vector3 offset = Vector3.up * 0.1f;

            // ������������ ������
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position + offset, visionSettings.visionRange);

            // ������������ ���� ������
            Vector3 leftBoundary = Quaternion.Euler(0, -visionSettings.visionAngle / 2, 0) * transform.forward * visionSettings.visionRange;
            Vector3 rightBoundary = Quaternion.Euler(0, visionSettings.visionAngle / 2, 0) * transform.forward * visionSettings.visionRange;

            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(transform.position + offset, leftBoundary);
            Gizmos.DrawRay(transform.position + offset, rightBoundary);

            // ������������ ������� ����� ����
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position + offset, visionSettings.targetSwitchRadius);

            // ������������ ������� ������������� ������
            Gizmos.color = Color.blue;
            Vector3[] points = new Vector3[25];

            for (int i = 0; i < 24; i++)
            {
                float angle = i * (360f / 24) * Mathf.Deg2Rad;
                points[i] = transform.position + offset + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * visionSettings.peripheralVisionRange;
            }
            points[24] = points[0];

            for (int i = 0; i < points.Length - 1; i++) Gizmos.DrawLine(points[i], points[i + 1]);
        }

        private void DrawTargetGizmos()
        {
            // ������������ ������� ����
            if (currentTarget != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, currentTarget.transform.position);

                Gizmos.color = Color.blue;
                Gizmos.DrawLine(transform.position + Vector3.up * 1.8f, currentTarget.transform.position + Vector3.up);

                // ������������ ������������� �����
                foreach (var target in potentialTargets)
                {
                    if (target.target != null && target.target != currentTarget)
                    {
                        Gizmos.color = target.isVisible ? Color.green : Color.gray;
                        Gizmos.DrawLine(transform.position, target.target.transform.position);
                    }
                }
            }
        }
        #endregion

        #region Public Methods API

        public void SetCurrentEntityType(EntityType entityType)
        {
            currentEntityType = entityType;
        }

        public void RemoveCurrentTarget()
        {
            if (HasTarget) RemoveTargetFromList(currentTarget);
            SetCurrentTarget();
        }

        public void OnDeadAgent()
        {
            if (debugMode) Debug.Log($"AgentPerception - OnDeadAgent XXX RemoveCurrentTarget XXX");

            if (perceptionCoroutine != null) StopCoroutine(perceptionCoroutine);
            RemoveCurrentTarget();
        }

        public void OnRespawnAgent() => InitializePerception();
        #endregion
    }
}