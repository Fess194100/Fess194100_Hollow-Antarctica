using UnityEngine;

namespace AdaptivEntityAgent
{
    public class AgentEnemy : AgentStateController
    {
        [Header("Enemy Specific Settings")]
        [SerializeField] private float fleeHealthThreshold = 0.3f;
        [SerializeField] private float investigationTime = 5f;

        private float investigationTimer = 0f;

        // Правильное переопределение Start
        protected override void Start()
        {
            // Сначала получаем компоненты (они уже protected в родителе)
            // perception и movement уже инициализированы в Awake родителя

            // Вызываем базовый Start
            base.Start();

            // Теперь безопасно подписываемся на события
            if (perception != null)
            {
                perception.OnTargetSpotted += OnTargetSpotted;
                perception.OnTargetLost += OnTargetLost;
                perception.OnSoundHeard += OnSoundHeard;
            }
        }

        // Альтернативный вариант - инициализация в Awake
        protected override void Awake()
        {
            // Сначала вызываем базовый Awake (который инициализирует компоненты)
            base.Awake();

            // Здесь perception и movement уже инициализированы
            // Можно выполнить дополнительную настройку специфичную для врага
        }

        protected override void UpdatePatrolState()
        {
            // Базовая логика патрулирования уже в AgentMovement
            // Можно добавить вражескую специфику
        }

        protected override void UpdateCombatState()
        {
            if (perception == null || !perception.HasTarget())
            {
                ChangeState(AgentState.Alert);
                return;
            }

            // Проверка необходимости отступления
            if (health != null && health.GetHealthPercentage() < fleeHealthThreshold)
            {
                ChangeState(AgentState.Flee);
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

            // Если увидели цель во время исследования - переходим в бой
            if (perception != null && perception.HasTarget())
            {
                investigationTimer = 0f;
                ChangeState(AgentState.Combat);
            }
        }

        protected override void UpdateFleeState()
        {
            // Логика бегства - движение в случайном направлении от цели
            if (perception != null && perception.HasTarget() && movement != null)
            {
                Vector3 fleeDirection = (transform.position - perception.GetCurrentTarget().transform.position).normalized;
                Vector3 fleePosition = transform.position + fleeDirection * 10f;
                movement.MoveToPosition(fleePosition);
            }

            // Возврат к патрулированию через некоторое время
            if (health != null && health.GetHealthPercentage() > 0.6f)
            {
                ChangeState(AgentState.Patrol);
            }
        }

        protected override void UpdateAlertState()
        {
            // Повышенная бдительность - поиск целей
            if (perception != null && perception.HasTarget())
            {
                ChangeState(AgentState.Combat);
            }
        }

        // Обработчики событий восприятия
        private void OnTargetSpotted(GameObject target)
        {
            ChangeState(AgentState.Combat);
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

        protected override void OnDestroy()
        {
            // Отписываемся от событий
            if (perception != null)
            {
                perception.OnTargetSpotted -= OnTargetSpotted;
                perception.OnTargetLost -= OnTargetLost;
                perception.OnSoundHeard -= OnSoundHeard;
            }

            // Вызываем базовый OnDestroy
            base.OnDestroy();
        }
    }
}