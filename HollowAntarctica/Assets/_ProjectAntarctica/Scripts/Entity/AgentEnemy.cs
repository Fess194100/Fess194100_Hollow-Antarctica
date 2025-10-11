using UnityEngine;

namespace AdaptivEntityAgent
{
    public class AgentEnemy : AgentStateController
    {
        [Header("Enemy Specific Settings")]
        [SerializeField] private float fleeHealthThreshold = 0.3f;
        [SerializeField] private float investigationTime = 5f;

        private float investigationTimer = 0f;

        // ���������� ��������������� Start
        protected override void Start()
        {
            // ������� �������� ���������� (��� ��� protected � ��������)
            // perception � movement ��� ���������������� � Awake ��������

            // �������� ������� Start
            base.Start();

            // ������ ��������� ������������� �� �������
            if (perception != null)
            {
                perception.OnTargetSpotted += OnTargetSpotted;
                perception.OnTargetLost += OnTargetLost;
                perception.OnSoundHeard += OnSoundHeard;
            }
        }

        // �������������� ������� - ������������� � Awake
        protected override void Awake()
        {
            // ������� �������� ������� Awake (������� �������������� ����������)
            base.Awake();

            // ����� perception � movement ��� ����������������
            // ����� ��������� �������������� ��������� ����������� ��� �����
        }

        protected override void UpdatePatrolState()
        {
            // ������� ������ �������������� ��� � AgentMovement
            // ����� �������� ��������� ���������
        }

        protected override void UpdateCombatState()
        {
            if (perception == null || !perception.HasTarget())
            {
                ChangeState(AgentState.Alert);
                return;
            }

            // �������� ������������� �����������
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

            // ���� ������� ���� �� ����� ������������ - ��������� � ���
            if (perception != null && perception.HasTarget())
            {
                investigationTimer = 0f;
                ChangeState(AgentState.Combat);
            }
        }

        protected override void UpdateFleeState()
        {
            // ������ ������� - �������� � ��������� ����������� �� ����
            if (perception != null && perception.HasTarget() && movement != null)
            {
                Vector3 fleeDirection = (transform.position - perception.GetCurrentTarget().transform.position).normalized;
                Vector3 fleePosition = transform.position + fleeDirection * 10f;
                movement.MoveToPosition(fleePosition);
            }

            // ������� � �������������� ����� ��������� �����
            if (health != null && health.GetHealthPercentage() > 0.6f)
            {
                ChangeState(AgentState.Patrol);
            }
        }

        protected override void UpdateAlertState()
        {
            // ���������� ������������ - ����� �����
            if (perception != null && perception.HasTarget())
            {
                ChangeState(AgentState.Combat);
            }
        }

        // ����������� ������� ����������
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
            // ������������ �� �������
            if (perception != null)
            {
                perception.OnTargetSpotted -= OnTargetSpotted;
                perception.OnTargetLost -= OnTargetLost;
                perception.OnSoundHeard -= OnSoundHeard;
            }

            // �������� ������� OnDestroy
            base.OnDestroy();
        }
    }
}