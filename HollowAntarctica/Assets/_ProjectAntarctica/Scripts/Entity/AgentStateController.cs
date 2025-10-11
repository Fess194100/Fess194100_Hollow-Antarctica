using UnityEngine;
using System.Collections;
using System;
using SimpleCharController;
using UnityEngine.AI;

namespace AdaptivEntityAgent
{
    public class AgentStateController : MonoBehaviour
    {
        [Header("Agent Configuration")]
        [SerializeField] private EntityType entityType = EntityType.Enemy;
        [SerializeField] private float stateUpdateFrequency = 0.2f;

        [Header("Debug")]
        [SerializeField] private bool debugMode = false;
        [SerializeField] private AgentState currentState = AgentState.Idle;

        // Компоненты (изменяем на protected для доступа в наследниках)
        protected NavMeshAgent navMeshAgent;
        protected EssenceHealth health;
        protected AgentPerception perception;
        protected AgentCombat combat;
        protected AgentMovement movement;

        // События для внешних систем
        public event Action<AgentState> OnStateChanged;
        public event Action<AgentState, AgentState> OnStateTransition;

        protected AgentState previousState;
        protected Coroutine stateUpdateCoroutine;

        // Изменяем Awake на virtual
        protected virtual void Awake()
        {
            InitializeComponents();
        }

        // Изменяем Start на virtual
        protected virtual void Start()
        {
            StartStateMachine();
        }

        protected void InitializeComponents()
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
            health = GetComponent<EssenceHealth>();
            perception = GetComponent<AgentPerception>();
            combat = GetComponent<AgentCombat>();
            movement = GetComponent<AgentMovement>();

            // Автоматическое создание обязательных компонентов
            if (perception == null) perception = gameObject.AddComponent<AgentPerception>();
            if (combat == null) combat = gameObject.AddComponent<AgentCombat>();
            if (movement == null) movement = gameObject.AddComponent<AgentMovement>();
        }

        protected void StartStateMachine()
        {
            // Подписка на события здоровья
            if (health != null)
            {
                health.OnDamageTaken.AddListener(OnDamageTaken);
                health.OnDeath.AddListener(OnDeath);
            }

            // Начальное состояние в зависимости от типа сущности
            SetInitialState();
            stateUpdateCoroutine = StartCoroutine(StateUpdateLoop());
        }

        private void SetInitialState()
        {
            switch (entityType)
            {
                case EntityType.Enemy:
                    ChangeState(AgentState.Patrol);
                    break;
                case EntityType.Ally:
                    ChangeState(AgentState.Follow);
                    break;
                case EntityType.Neutral:
                    ChangeState(AgentState.Idle);
                    break;
            }
        }

        private IEnumerator StateUpdateLoop()
        {
            while (true)
            {
                UpdateStateLogic();
                yield return new WaitForSeconds(stateUpdateFrequency);
            }
        }

        private void UpdateStateLogic()
        {
            if (health != null && health.IsDead())
            {
                // Обработка смерти - остановка всех действий
                navMeshAgent.isStopped = true;
                return;
            }

            // Логика принятия решений на основе состояния
            switch (currentState)
            {
                case AgentState.Idle:
                    UpdateIdleState();
                    break;
                case AgentState.Patrol:
                    UpdatePatrolState();
                    break;
                case AgentState.Investigate:
                    UpdateInvestigateState();
                    break;
                case AgentState.Combat:
                    UpdateCombatState();
                    break;
                case AgentState.Flee:
                    UpdateFleeState();
                    break;
                case AgentState.Follow:
                    UpdateFollowState();
                    break;
                case AgentState.Interact:
                    UpdateInteractState();
                    break;
                case AgentState.Alert:
                    UpdateAlertState();
                    break;
            }
        }

        public void ChangeState(AgentState newState)
        {
            if (currentState == newState) return;

            previousState = currentState;
            currentState = newState;

            OnStateTransition?.Invoke(previousState, currentState);
            OnStateChanged?.Invoke(currentState);

            if (debugMode)
            {
                Debug.Log($"{gameObject.name}: State changed from {previousState} to {currentState}");
            }

            // Уведомляем компоненты о смене состояния
            movement?.OnStateChanged(newState);
            combat?.OnStateChanged(newState);
        }

        // Методы обновления состояний (меняем на protected virtual)
        protected virtual void UpdateIdleState() { }
        protected virtual void UpdatePatrolState() { }
        protected virtual void UpdateInvestigateState() { }
        protected virtual void UpdateCombatState() { }
        protected virtual void UpdateFleeState() { }
        protected virtual void UpdateFollowState() { }
        protected virtual void UpdateInteractState() { }
        protected virtual void UpdateAlertState() { }

        // Обработчики событий здоровья
        private void OnDamageTaken(float damage, BodyPart bodyPart)
        {
            // Логика реакции на получение урона
            if (entityType == EntityType.Neutral && currentState != AgentState.Flee)
            {
                ChangeState(AgentState.Flee);
            }
            else if (health.GetHealthPercentage() < 0.3f && currentState != AgentState.Flee)
            {
                ChangeState(AgentState.Flee);
            }
        }

        private void OnDeath()
        {
            if (stateUpdateCoroutine != null)
                StopCoroutine(stateUpdateCoroutine);

            navMeshAgent.isStopped = true;
        }

        // Public API для внешнего управления
        public AgentState GetCurrentState() => currentState;
        public AgentState GetPreviousState() => previousState;
        public EntityType GetEntityType() => entityType;

        public void ForceStateChange(AgentState newState) => ChangeState(newState);
        public void ReturnToPreviousState() => ChangeState(previousState);

        protected virtual void OnDestroy()
        {
            if (stateUpdateCoroutine != null)
                StopCoroutine(stateUpdateCoroutine);
        }
    }
}