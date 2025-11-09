using UnityEngine;

namespace AdaptivEntityAgent
{
    public class AgentEnemy : AgentStateController
    {
        [Header("Enemy Specific Settings")]
        [SerializeField] private float fleeOtimalDistantion = 15.0f;
        [SerializeField] private float investigationTime = 5f;

        private float attackTypeWeight = 0.5f;
        private float investigationTimer = 0f;
        private AgentCombat agentCombat;
        private AgentState previousCombatState;

        protected override void Start()
        {
            base.Start();

            agentCombat = GetComponent<AgentCombat>();
            if (agentCombat != null)
            {
                attackTypeWeight = agentCombat.AttackTypeWeight;
                agentCombat.agentEventsCombat.OnFleeAgent.AddListener(OnFleeAgent);
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
                ChangeState(AgentState.Combat);
                return;
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
                Vector3 fleeDirection = (transform.position - perception.CurrentTarget.transform.position).normalized;
                Vector3 fleePosition = transform.position + fleeDirection * fleeOtimalDistantion;
                movement.MoveToPosition(fleePosition);
            }

            if (movement.GetRemainingDistance() <= 0.5f)
            {
                switch (previousState)
                {
                    case AgentState.Idle:
                    case AgentState.Investigate:
                    case AgentState.Patrol:
                    case AgentState.Flee:
                        ChangeState(AgentState.Patrol);
                        break;

                    case AgentState.Combat:
                        SetInvestigateState();
                        break;
                    case AgentState.Follow:
                        break;
                    case AgentState.Interact:
                        ChangeState(AgentState.Combat);
                        break;
                    case AgentState.Alert:
                        ChangeState(AgentState.Combat);
                        break;
                    default:
                        ChangeState(AgentState.Patrol);
                        break;
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

        protected override void UpdateInteractState()
        {

        }

        protected override void OnCriticalHealth()
        {
            if (GetCurrentState() != AgentState.Flee) OnFleeAgent();
        }

        // Обработчики событий восприятия
        private void OnTargetSpotted(GameObject target)
        {
            // Запоминаем предыдущее состояние перед боем
            previousCombatState = GetCurrentState();

            switch (previousCombatState)
            {
                case AgentState.Idle:
                case AgentState.Investigate:
                case AgentState.Patrol:
                    ChangeState(AgentState.Combat);
                    break;

                case AgentState.Combat:
                    break;

                case AgentState.Flee:
                    break;
                case AgentState.Follow:
                    break;
                case AgentState.Interact:
                    ChangeState(AgentState.Combat);
                    break;
                case AgentState.Alert:
                    ChangeState(AgentState.Combat);
                    break;
                case AgentState.Dead:
                    break;
                default:
                    ChangeState(AgentState.Patrol);
                    break;
            }
        }

        private void OnTargetChanged(GameObject target)
        {
            previousCombatState = GetCurrentState();

            switch (previousCombatState)
            {
                case AgentState.Idle:
                case AgentState.Investigate:
                case AgentState.Patrol:
                    break;

                case AgentState.Combat:

                    if (target == null)
                    {
                        if (perception.CountPotentialTargets > 0) ChangeState(AgentState.Investigate);
                        else ChangeState(AgentState.Patrol);

                        break;
                    }

                    Debug.Log("ХУЙ ЗНАЕТ ЧТО ДЕЛАТЬ?????????????????????????????????????????");

                    break;

                case AgentState.Flee:
                    break;
                case AgentState.Follow:
                    break;
                case AgentState.Interact:
                    //ChangeState(AgentState.Combat);
                    break;
                case AgentState.Alert:
                    ChangeState(AgentState.Combat);
                    break;
                case AgentState.Dead:
                    break;
                default:
                    ChangeState(AgentState.Patrol);
                    break;
            }
        }

        private void OnTargetLost(GameObject target = null)
        {
            if (GetCurrentState() == AgentState.Flee) return;
            SetInvestigateState();
        }

        private void SetInvestigateState()
        {
            ChangeState(AgentState.Investigate);
            if (movement != null && perception != null)
            {
                movement.MoveToPosition(perception.LastKnownTargetPosition);
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

        private void OnFleeAgent() => ChangeState(AgentState.Flee);

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
            if (agentCombat != null)
            {
                agentCombat.agentEventsCombat.OnFleeAgent.RemoveListener(OnFleeAgent);
            }
            
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