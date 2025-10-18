using UnityEngine;

namespace AdaptivEntityAgent
{
    public class AgentEnemy : AgentStateController
    {
        [Header("Enemy Specific Settings")]
        [SerializeField] private float fleeHealthThreshold = 0.3f;
        [SerializeField] private float investigationTime = 5f;
        [Range(0f, 1f)]
        [SerializeField] private float attackTypeWeight = 0.5f;

        private AgentCombat agentCombat;
        private float investigationTimer = 0f;
        private AgentState previousCombatState;

        protected override void Start()
        {
            base.Start();

            agentCombat = GetComponent<AgentCombat>();
            if (agentCombat != null)
            {
                agentCombat.SetAttackTypeWeight(attackTypeWeight);
            }

            if (perception != null)
            {
                perception.perceptionEvents.OnTargetSpotted.AddListener(OnTargetSpotted);
                perception.perceptionEvents.OnTargetLost.AddListener(OnTargetLost);
                perception.perceptionEvents.OnTargetChanged.AddListener(OnTargetChanged);
                perception.perceptionEvents.OnSoundHeard.AddListener(OnSoundHeard);
                perception.SetCurrentEntityType(GetEntityType());
            }
        }

        protected override void UpdateCombatState()
        {
            if (perception == null || !perception.HasTarget)
            {
                ChangeState(AgentState.Investigate);
                return;
            }

            // Проверка необходимости отступления по здоровью
            if (health != null && health.GetHealthPercentage() < fleeHealthThreshold)
            {
                ChangeState(AgentState.Flee);
                return;
            }

            // Для исключительно дальних бойцов - отступление при близкой цели
            if (attackTypeWeight >= 0.9f && agentCombat != null)
            {
                GameObject target = perception.GetCurrentTarget();
                float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);

                if (distanceToTarget < agentCombat.GetMeleeAttackSettings().optimalDistance)
                {
                    ChangeState(AgentState.Flee);
                    return;
                }
            }
        }

        protected override void UpdateInvestigateState()
        {
            investigationTimer += Time.deltaTime;

            if (investigationTimer >= investigationTime)
            {
                investigationTimer = 0f;
                ChangeState(AgentState.Patrol);
            }

            if (perception != null && perception.HasTarget)
            {
                investigationTimer = 0f;
                ChangeState(AgentState.Combat);
            }
        }

        protected override void UpdateFleeState()
        {
            // Логика бегства
            if (perception != null && perception.HasTarget && movement != null)
            {
                Vector3 fleeDirection = (transform.position - perception.GetCurrentTarget().transform.position).normalized;
                Vector3 fleePosition = transform.position + fleeDirection * 10f;
                movement.MoveToPosition(fleePosition);
            }

            // Возврат к патрулированию при восстановлении здоровья
            if (health != null && health.GetHealthPercentage() > 0.6f)
            {
                // Проверяем, нет ли целей рядом
                if (perception == null || !perception.HasTarget)
                {
                    ChangeState(AgentState.Patrol);
                }
            }
        }

        protected override void UpdateAlertState()
        {
            if (perception != null && perception.HasTarget)
            {
                ChangeState(AgentState.Combat);
            }
        }

        // Обработчики событий восприятия
        private void OnTargetSpotted(GameObject target)
        {
            // Запоминаем предыдущее состояние перед боем
            previousCombatState = GetCurrentState();

            if (previousCombatState == AgentState.Idle ||
                previousCombatState == AgentState.Investigate ||
                previousCombatState == AgentState.Patrol)
            {
                ChangeState(AgentState.Combat);
            }
            else
            {
                ChangeState(AgentState.Patrol);
            }
        }

        private void OnTargetChanged(GameObject target)
        {
            // Не реализованно
        }

        private void OnTargetLost(GameObject target)
        {
            ChangeState(AgentState.Investigate);
            if (movement != null && perception != null)
            {
                movement.MoveToPosition(perception.GetLastKnownTargetPosition());
            }
        }

        private void OnSoundHeard(Vector3 soundPosition)
        {
            AgentState currentState = GetCurrentState();
            if ((currentState == AgentState.Patrol || currentState == AgentState.Idle) && movement != null)
            {
                ChangeState(AgentState.Investigate);
                movement.MoveToPosition(soundPosition);
            }
        }

        public void SetAttackTypeWeight(float weight)
        {
            attackTypeWeight = Mathf.Clamp01(weight);
            if (agentCombat != null)
            {
                agentCombat.SetAttackTypeWeight(attackTypeWeight);
            }
        }

        protected override void OnDestroy()
        {
            if (perception != null)
            {
                perception.perceptionEvents.OnTargetSpotted.RemoveListener(OnTargetSpotted);
                perception.perceptionEvents.OnTargetLost.RemoveListener(OnTargetLost);
                perception.perceptionEvents.OnTargetChanged.RemoveListener(OnTargetChanged);
                perception.perceptionEvents.OnSoundHeard.RemoveListener(OnSoundHeard);
            }

            base.OnDestroy();
        }
    }
}