using UnityEngine;
using System.Collections;
using SimpleCharController;
using UnityEngine.AI;

namespace AdaptivEntityAgent
{
    public class AgentCombat : MonoBehaviour
    {
        [Header("Combat Settings")]
        [SerializeField] private float attackRange = 2f;
        [SerializeField] private float attackRate = 1f;
        [SerializeField] private float attackDamage = 10f;
        [SerializeField] private LayerMask targetMask = ~0;

        private EssenceHealth health;
        private AgentPerception perception;
        private NavMeshAgent navMeshAgent;
        private Coroutine combatCoroutine;
        private bool canAttack = true;

        private void Awake()
        {
            health = GetComponent<EssenceHealth>();
            perception = GetComponent<AgentPerception>();
            navMeshAgent = GetComponent<NavMeshAgent>();
        }

        public void OnStateChanged(AgentState newState)
        {
            // Остановка боевой корутины при выходе из состояния боя
            if (newState != AgentState.Combat && combatCoroutine != null)
            {
                StopCoroutine(combatCoroutine);
                combatCoroutine = null;
            }
            else if (newState == AgentState.Combat)
            {
                combatCoroutine = StartCoroutine(CombatRoutine());
            }
        }

        private IEnumerator CombatRoutine()
        {
            while (true)
            {
                if (perception.HasTarget())
                {
                    GameObject target = perception.GetCurrentTarget();
                    ProcessCombat(target);
                }
                yield return new WaitForSeconds(0.1f);
            }
        }

        private void ProcessCombat(GameObject target)
        {
            if (target == null) return;

            float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);

            // Движение к цели если она далеко
            if (distanceToTarget > attackRange)
            {
                navMeshAgent.SetDestination(target.transform.position);
            }
            else
            {
                // Атака если цель в радиусе
                navMeshAgent.isStopped = true;
                if (canAttack)
                {
                    AttackTarget(target);
                }
            }
        }

        private void AttackTarget(GameObject target)
        {
            EssenceHealth targetHealth = target.GetComponent<EssenceHealth>();
            if (targetHealth != null && !targetHealth.IsDead())
            {
                targetHealth.TakeDamage(attackDamage, ProjectileType.Green, 1, BodyPart.Body);
                StartCoroutine(AttackCooldown());
            }
        }

        private IEnumerator AttackCooldown()
        {
            canAttack = false;
            yield return new WaitForSeconds(1f / attackRate);
            canAttack = true;
        }

        public void SetAttackTarget(GameObject target)
        {
            // Принудительная установка цели для атаки
        }

        public bool IsInAttackRange(Vector3 targetPosition)
        {
            return Vector3.Distance(transform.position, targetPosition) <= attackRange;
        }

        private void OnDestroy()
        {
            if (combatCoroutine != null)
                StopCoroutine(combatCoroutine);
        }
    }
}