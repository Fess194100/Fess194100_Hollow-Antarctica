using UnityEngine;
using System.Collections;
using System;
using UnityEngine.AI;

namespace AdaptivEntityAgent
{
    public class AgentStateController : MonoBehaviour
    {
        #region Variables
        [Header("Agent Configuration")]
        [SerializeField] private EntityType entityType = EntityType.Enemy;
        [SerializeField] private float stateUpdateFrequency = 0.2f;
        [SerializeField] private float criticalHealthThreshold = 0.3f;

        [Header("Debug")]
        public bool debugMode = false;
        [SerializeField] private AgentState currentState = AgentState.Idle;
        #endregion

        #region Public Property
        public float CriticalHealthThreshold => criticalHealthThreshold;
        #endregion

        #region Private Variables
        protected float currentHealth = 100f;
        protected NavMeshAgent navMeshAgent;
        protected AgentPerception perception;
        protected AgentCombat combat;
        protected AgentMovement movement;
        protected AgentState previousState;
        //protected Coroutine stateUpdateCoroutine;
        #endregion

        #region Events
        public event Action<AgentState> OnStateChanged;
        public event Action<AgentState, AgentState> OnStateTransition;
        #endregion

        #region System Functions
        protected virtual void Awake()
        {
            InitializeComponents();
        }

        protected virtual void Start()
        {
            StartStateMachine();
        }

        protected virtual void OnDestroy()
        {
            /*if (stateUpdateCoroutine != null)
                StopCoroutine(stateUpdateCoroutine);*/
            StopCoroutine(StateUpdateLoop());
        }

        protected void InitializeComponents()
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
            perception = GetComponent<AgentPerception>();
            combat = GetComponent<AgentCombat>();
            movement = GetComponent<AgentMovement>();

            if (perception == null) perception = gameObject.AddComponent<AgentPerception>();
            if (combat == null) combat = gameObject.AddComponent<AgentCombat>();
            if (movement == null) movement = gameObject.AddComponent<AgentMovement>();
        }

        protected void StartStateMachine()
        {
            SetInitialState();
            //stateUpdateCoroutine = StartCoroutine(StateUpdateLoop());
            StartCoroutine(StateUpdateLoop());
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
        #endregion

        #region State Management
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
            if (currentState == AgentState.Dead)
            {
                navMeshAgent.isStopped = true;
                return;
            }

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

        // Update States
        protected virtual void UpdateIdleState() { }
        protected virtual void UpdatePatrolState() { }
        protected virtual void UpdateInvestigateState() { }
        protected virtual void UpdateCombatState() { }
        protected virtual void UpdateFleeState() { }
        protected virtual void UpdateFollowState() { }
        protected virtual void UpdateInteractState() { }
        protected virtual void UpdateAlertState() { }
        protected virtual void OnCriticalHealth() { }
        #endregion

        #region Private Methods
        private void OnDeath()
        {
            //if (stateUpdateCoroutine != null) StopCoroutine(stateUpdateCoroutine);
            ChangeState(AgentState.Dead);
            StopCoroutine(StateUpdateLoop());
            navMeshAgent.isStopped = true;
        }

        private void RebirthAgent()
        {
            //if (stateUpdateCoroutine != null) StopCoroutine(stateUpdateCoroutine);
            UpdateHealth(100f);
            StartStateMachine();
            navMeshAgent.isStopped = false;
        }
        #endregion

        #region Public API
        public AgentState GetCurrentState() => currentState;
        public AgentState GetPreviousState() => previousState;
        public EntityType GetEntityType() => entityType;
        public void ForceStateChange(AgentState newState) => ChangeState(newState);
        public void ReturnToPreviousState() => ChangeState(previousState);

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

        public void UpdateHealth(float health)
        {
            if (currentHealth == health) return;
            currentHealth = health;

            if (currentHealth <= 0f)
            {
                OnDeath();
                return;
            }

            if (currentHealth < criticalHealthThreshold)
            {
                OnCriticalHealth();
            }
        }

        public void RespawnAgent()
        {
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
            RebirthAgent();
        }
        #endregion



    }
}