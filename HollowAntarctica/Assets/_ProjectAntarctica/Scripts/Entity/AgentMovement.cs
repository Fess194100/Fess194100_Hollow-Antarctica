using UnityEngine;
using System.Collections;
using UnityEngine.AI;

namespace AdaptivEntityAgent
{
    public class AgentMovement : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float patrolSpeed = 2f;
        [SerializeField] private float combatSpeed = 3.5f;
        [SerializeField] private float fleeSpeed = 4f;
        [SerializeField] private float rotationSpeed = 120f;

        [Header("Patrol Settings")]
        [SerializeField] private Transform[] patrolPoints;
        [SerializeField] private float waitTimeAtPoints = 2f;
        [SerializeField] private float pointReachedThreshold = 0.5f;

        private NavMeshAgent navMeshAgent;
        private int currentPatrolIndex = 0;
        private bool isMovingToPoint = false;
        private Coroutine patrolCoroutine;

        private void Awake()
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
            navMeshAgent.angularSpeed = rotationSpeed;
        }

        private void OnDestroy()
        {
            StopPatrol();
        }

        public void OnStateChanged(AgentState newState)
        {
            // Настройка скорости в зависимости от состояния
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
                case AgentState.Idle:
                    navMeshAgent.isStopped = true;
                    StopPatrol();
                    break;
            }

            navMeshAgent.isStopped = false;
        }

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
                    isMovingToPoint = true;

                    // Ждем достижения точки
                    yield return new WaitUntil(() =>
                        !navMeshAgent.pathPending &&
                        navMeshAgent.remainingDistance <= pointReachedThreshold);

                    isMovingToPoint = false;

                    // Ждем на точке
                    yield return new WaitForSeconds(waitTimeAtPoints);

                    // Следующая точка патруля
                    currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
                }
                else
                {
                    yield return new WaitForSeconds(1f);
                }
            }
        }

        #region Public API для управления движением

        public void MoveToPosition(Vector3 position)
        {
            navMeshAgent.SetDestination(position);
            navMeshAgent.isStopped = false;
        }

        public void StopMovement()
        {
            navMeshAgent.isStopped = true;
        }

        public void UpdateRotation(bool updateRotation = true) => navMeshAgent.updateRotation = updateRotation;

        public bool HasReachedDestination()
        {
            return !navMeshAgent.pathPending &&
                   navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance;
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
        #endregion

        
    }
}