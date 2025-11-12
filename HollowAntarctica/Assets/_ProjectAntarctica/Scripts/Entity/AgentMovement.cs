using UnityEngine;
using System.Collections;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace AdaptivEntityAgent
{
    public class AgentMovement : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private bool debugMode;
        [SerializeField] private float patrolSpeed = 2f;
        [SerializeField] private float combatSpeed = 3.5f;
        [SerializeField] private float fleeSpeed = 4f;
        [SerializeField] private float rotationSpeed = 120f;

#if UNITY_EDITOR_RUS
        [Tooltip("ћинимальное рассто€ние до цели дл€ обновлени€ пути. ѕри изменени€х меньше этого значени€ цель не обновл€етс€")]
#else
        [Tooltip("Minimum distance to target for path update. Target won't update for changes smaller than this value")]
#endif
        [SerializeField] private float minDistanceThreshold = 0.3f;

        [Header("Patrol Settings")]
        [SerializeField] private Transform[] patrolPoints;
        [SerializeField] private float waitTimeAtPoints = 2f;
        [SerializeField] private float pointReachedThreshold = 0.5f;

        [Header("Interact Settings")]
        [SerializeField] private bool interactAfterPatrol;
        [SerializeField] private Transform interactPoint;

        public AgentEventsMovement eventsMovement;

        private NavMeshAgent navMeshAgent;
        private int currentPatrolIndex = 0;
        private Vector3 targetPositionToMove;
        private Coroutine patrolCoroutine;
        private AgentStateController stateController;

        #region Public Property
        public Vector3 TargetPositionToMove => targetPositionToMove;
        #endregion

        private void Awake()
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
            navMeshAgent.angularSpeed = rotationSpeed;
            stateController = GetComponent<AgentStateController>();
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
        }

        public void OnStateChanged(AgentState newState)
        {
            // ≈сли выходим из состо€ни€ взаимодействи€ - вызываем событие окончани€
            if (stateController.GetPreviousState() == AgentState.Interact && newState != AgentState.Interact)
            {
                EndInteraction();
            }

            // Ќастройка скорости в зависимости от состо€ни€
            switch (newState)
            {
                case AgentState.Patrol:
                    navMeshAgent.speed = patrolSpeed;
                    StartPatrol();
                    break;
                case AgentState.Combat:
                case AgentState.Investigate:
                case AgentState.Alert:
                    navMeshAgent.speed = combatSpeed;
                    StopPatrol();
                    break;
                case AgentState.Flee:
                    navMeshAgent.speed = fleeSpeed;
                    StopPatrol();
                    break;
                case AgentState.Follow:
                    navMeshAgent.speed = patrolSpeed;
                    StopPatrol();
                    break;
                case AgentState.Interact:
                    navMeshAgent.speed = patrolSpeed;
                    StartInteract();
                    break;
                case AgentState.Idle:
                    navMeshAgent.isStopped = true;
                    StopPatrol();
                    break;
            }

            navMeshAgent.isStopped = false;
        }

        //----------------------------------------------------------------------------------

        private void StartPatrol()
        {
            if (patrolPoints == null || patrolPoints.Length == 0) return;

            StopPatrol();
            patrolCoroutine = StartCoroutine(PatrolRoutine());
        }

        private void StopPatrol()
        {
            if (patrolCoroutine != null)
            {
                StopCoroutine(patrolCoroutine);
                patrolCoroutine = null;
            }
        }

        private IEnumerator PatrolRoutine()
        {
            while (true)
            {
                if (patrolPoints.Length > 0)
                {
                    Vector3 targetPoint = patrolPoints[currentPatrolIndex].position;
                    navMeshAgent.SetDestination(targetPoint);
                    targetPositionToMove = targetPoint;
                    eventsMovement.OnMoveToNextPosition?.Invoke(targetPositionToMove);

                    yield return new WaitUntil(() =>
                        !navMeshAgent.pathPending &&
                        navMeshAgent.remainingDistance <= pointReachedThreshold);

                    if (interactAfterPatrol && IsLastPatrolPoint())
                    {
                        currentPatrolIndex = Random.Range(0, patrolPoints.Length - 1);
                        if (stateController != null)
                        {
                            stateController.ForceStateChange(AgentState.Interact);
                        }
                        yield break;
                    }

                    yield return new WaitForSeconds(waitTimeAtPoints);
                    currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
                }
                else
                {
                    yield return new WaitForSeconds(1f);
                }
            }
        }

        private bool IsLastPatrolPoint()
        {
            return patrolPoints.Length > 0 && currentPatrolIndex == patrolPoints.Length - 1;
        }

        //----------------------------------------------------------------------------------

        private void StartInteract()
        {
            if (interactPoint == null)
            {
                if (stateController != null)
                {
                    stateController.ForceStateChange(AgentState.Patrol);
                }
                return;
            }

            StopPatrol();
            patrolCoroutine = StartCoroutine(InteractRoutine());
        }

        private void StopInteract()
        {
            if (patrolCoroutine != null)
            {
                StopCoroutine(patrolCoroutine);
                patrolCoroutine = null;
            }
        }

        private IEnumerator InteractRoutine()
        {
            MoveToPosition(interactPoint.position, interactPoint.position);

            yield return new WaitUntil(() => navMeshAgent.hasPath && navMeshAgent.velocity.magnitude > 0.1f);

            yield return new WaitUntil(() => navMeshAgent.remainingDistance <= pointReachedThreshold && navMeshAgent.velocity.magnitude < 0.1f);

            eventsMovement.OnStartInteract?.Invoke();
        }

        private void EndInteraction()
        {
            eventsMovement.OnEndInteract?.Invoke();
            StopInteract();
        }

        private IEnumerator CalculateAndSetPathRoutine(Vector3 targetPosition)
        {
            NavMeshPath newPath = new NavMeshPath();
            NavMesh.CalculatePath(transform.position, targetPosition, NavMesh.AllAreas, newPath);

            yield return null;
            yield return null;

            if (newPath.status == NavMeshPathStatus.PathComplete ||
                newPath.status == NavMeshPathStatus.PathPartial)
            {
                while (navMeshAgent.pathPending)
                {
                    yield return null;
                }

                navMeshAgent.SetPath(newPath);
                if (navMeshAgent.isStopped) navMeshAgent.isStopped = false;
            }
            else
            {
                if (debugMode) Debug.LogWarning("PathInvalid - the agent continues the current route");
            }
        }
        #region Public API дл€ управлени€ движением

        public void MoveToPosition(Vector3 position, Vector3 originPositionTarget)
        {
            if (Vector3.Distance(navMeshAgent.destination, position) <= minDistanceThreshold) return;

            targetPositionToMove = originPositionTarget;
            eventsMovement.OnMoveToNextPosition?.Invoke(targetPositionToMove);

            StartCoroutine(CalculateAndSetPathRoutine(position));
        }

        public void StopMovement()
        {
            navMeshAgent.isStopped = true;
        }

        public void UpdateRotation(bool updateRotation = true) => navMeshAgent.updateRotation = updateRotation;

        public bool HasReachedDestination()
        {
            return !navMeshAgent.pathPending && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance;
        }

        public void SetPatrolPoints(Transform[] points)
        {
            patrolPoints = points;
            currentPatrolIndex = 0;
        }

        public void SetMovementSpeed(float speed)
        {
            navMeshAgent.speed = speed;
        }

        public float GetRemainingDistance()
        {
            return navMeshAgent.remainingDistance;
        }

        public void SetInteractPoint(Transform point)
        {
            interactPoint = point;
        }

        public void SetInteractAfterPatrol(bool enable)
        {
            interactAfterPatrol = enable;
        }
        #endregion
    }
}