using UnityEngine;
using System.Collections;
using SimpleCharController;
using UnityEngine.AI;

namespace AdaptivEntityAgent
{
    [System.Serializable]
    public class AttackSettings
    {
        public float attackRate = 1f;
        public float optimalDistance = 5f;
        public float optimalAngle = 15f;
    }

    public class AgentCombat : MonoBehaviour
    {
        #region Variables
        [SerializeField] private bool debug = false;
        [SerializeField] private bool drawDebugGizmo = false;

        [Space(10)]
        [SerializeField] private float combatUpdateFrequency = 0.2f;
        [Tooltip(" 0 = melee, 1 = ranged")]
        [Range(0f, 1f)]
        [SerializeField] private float attackTypeWeight = 0.5f;
        [SerializeField] private float rotationSpeed = 120f;

        [Space(10)]
        [Tooltip("Additional distance beyond optimal melee range at which the agent will still close in for a hand-to-hand attack.")]
        [SerializeField] private float meleeApproachThreshold = 0.5f;
        [SerializeField] private AttackSettings meleeAttackSettings = new AttackSettings();
        [SerializeField] private AttackSettings rangedAttackSettings = new AttackSettings();

        [Space(10)]
        public AgentEventsCombat agentEventsCombat;
        #endregion

        #region Private Variables
        private AgentPerception perception;
        private AgentMovement agentMovement;
        private EssenceHealth targetHealth;

        private bool hashComponents;
        private bool canAttack = true;
        private bool isAiming = false;
        private bool canRotation = false;
        private bool isFlee = false;
        private Vector3 attackPosition;
        private Quaternion targetRotation;
        private AttackType currentAttackType;
        private GameObject currentTargetAttack;
        private Coroutine combatCoroutine;
        #endregion

        #region Public Properties
        public float AttackTypeWeight => attackTypeWeight;
        #endregion

        #region System Functions
        private void Awake()
        {
            perception = GetComponent<AgentPerception>();
            agentMovement = GetComponent<AgentMovement>();

            if (agentMovement != null && perception != null) hashComponents = true;
        }

        private void FixedUpdate()
        {
            UpdateRotation();
        }

        private void OnDestroy()
        {
            StopCombatState();
        }

        private void OnDrawGizmosSelected()
        {
            if (!drawDebugGizmo) return;

            DrawCombatGizmos();
        }
        #endregion

        #region State Management
        public void OnStateChanged(AgentState newState)
        {
            if (debug) Debug.Log($"AgentCombat - AgentState = {newState}");
            if (newState != AgentState.Combat)
            {
                StopCombatState();
            }
            else if (newState == AgentState.Combat)
            {
                if (debug) Debug.Log($"AgentCombat - StartCombatState");
                if (hashComponents) StartCombatState();
            }
        }

        private void StartCombatState()
        {
            combatCoroutine = StartCoroutine(CombatRoutine());
        }

        private void StopCombatState()
        {
            if (debug) Debug.Log($"AgentCombat - StopCombatState()");

            if (combatCoroutine != null)
            {
                StopCoroutine(combatCoroutine);
                combatCoroutine = null;
            }

            canAttack = true;
            isAiming = false;
            canRotation = false;
            isFlee = false;
        }
        #endregion

        #region Combat Process
        private IEnumerator CombatRoutine()
        {
            while (true)
            {
                if (perception.HasTarget)
                {
                    if (debug) Debug.Log($"AgentCombat - CombatRoutine. HasTarget = true");
                    ProcessCombat();
                }
                else
                {
                    if (debug) Debug.Log($"AgentCombat - CombatRoutine. HasTarget = false");
                    canRotation = false;
                }
                yield return new WaitForSeconds(combatUpdateFrequency);
            }
        }

        private void ProcessCombat()
        {
            GameObject target = perception.CurrentTarget;
            float distance = perception.CurrentTargetDistance;

            if (target == null) return;
            UpdateAttackTarget(target);

            if (!isAiming)
            {
                HandleMovementPhase(target, distance);
            }
            else
            {
                HandleAimingPhase(target);
            }

            if (debug) Debug.Log($"AgentCombat - ProcessCombat = {target} ||| isAiming = {isAiming}");
        }

        //---------------------------------------------------------------------------------------

        private void HandleMovementPhase(GameObject target, float distance)
        {
            canRotation = false;
            currentAttackType = ChooseAttackType(distance);
            attackPosition = CalculateAttackPosition(target, currentAttackType);

            if (debug) Debug.Log($"AgentCombat - HandleMovementPhase.attackPosition = {attackPosition}");

            if (attackPosition == Vector3.zero)
            {
                if (debug) Debug.Log("No valid attack position found, fleeing!");
                agentMovement.MoveToPosition(target.transform.position);
                return;
            }

            MoveToAttackPosition(target);
        }

        private void MoveToAttackPosition(GameObject target)
        {
            float distanceToAttackPosition = Vector3.Distance(transform.position, attackPosition);
            float distanceToSwitchAttackPosition = meleeApproachThreshold;
            if (debug) Debug.Log($"AgentCombat - distanceToAttackPosition = {distanceToAttackPosition}");

            if (currentAttackType == AttackType.Ranged)
            {
                distanceToSwitchAttackPosition = rangedAttackSettings.optimalDistance / 2.3f;
            }
            else
            {
                if (distanceToAttackPosition < distanceToSwitchAttackPosition + meleeAttackSettings.optimalDistance / 2)
                {
                    HandleAimingPhase(target, false);
                }
            }

            if (distanceToAttackPosition > distanceToSwitchAttackPosition)
            {
                agentMovement.MoveToPosition(attackPosition);
                if (debug) Debug.Log($"Moving to attack position for {currentAttackType} attack, Distance: {distanceToAttackPosition:F1}");

                if (currentAttackType == AttackType.Ranged && perception.CurrentTargetDistance < distanceToAttackPosition && !isFlee)
                {
                    // Agent is Flee
                    isFlee = true;
                    agentEventsCombat.OnFleeAgent.Invoke();
                    if (debug) Debug.Log("AgentCombat - MoveToAttackPosition - <color=yellow>FLEE</color>");
                }
            }
            else
            {
                agentMovement.StopMovement();
                isAiming = true;
                isFlee = false;
                if (debug) Debug.Log($"Starting aim for {currentAttackType} attack");
            }
        }

        //---------------------------------------------------------------------------------------

        private void HandleAimingPhase(GameObject target, bool canAim = true)
        {
            if (canAim) AimAtTarget(target);

            if (IsReadyToAttack(target) && canAttack) PerformAttack(target, currentAttackType);

            if (debug) Debug.Log($"AgentCombat - HandleAimingPhase = {target}");
        }

        private void UpdateRotation()
        {
            if (canRotation)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
            }
        }

        private void UpdateAttackTarget(GameObject target)
        {
            if (currentTargetAttack != target)
            {
                currentTargetAttack = target;
                canAttack = true;
                targetHealth = currentTargetAttack.GetComponent<FlagFaction>().EssenceHealth;
                if (debug) Debug.Log($"AgentCombat - UpdateAttackTarget_____Update EssenceHealth ++++++");
            }
        }

        private void AimAtTarget(GameObject target)
        {
            if (target == null) return;

            Vector3 direction = (target.transform.position - transform.position).normalized;
            direction.y = 0;

            if (direction != Vector3.zero)
            {
                targetRotation = Quaternion.LookRotation(direction);
                canRotation = true;
            }
            else
            {
                canRotation = false;
            }
        }
        #endregion

        #region Attack Logic
        private void PerformAttack(GameObject target, AttackType attackType)
        {
            AttackSettings settings = GetAttackSettings(attackType);
            if (debug) Debug.Log($"AgentCombat - PerformAttack = {target}");

            if (targetHealth == null || targetHealth.IsDead()) return;

            if(debug) LogAttack(attackType, target);

            switch (attackType)
            {
                case AttackType.Melee:
                    agentEventsCombat.OnAttackMelle.Invoke();
                    break;
                case AttackType.Ranged:
                    agentEventsCombat.OnAttackRange.Invoke();
                    break;
            }

            StartCoroutine(AttackCooldown(settings));
        }

        private IEnumerator AttackCooldown(AttackSettings settings)
        {
            canAttack = false;
            isAiming = false;

            if (debug) Debug.Log($"Attack cooldown: {1f / settings.attackRate}s");
            yield return new WaitForSeconds(1f / settings.attackRate);

            if (!targetHealth.IsDead()) canAttack = true;
            else perception.RemoveCurrentTarget();
        }
        #endregion

        #region Deterministic Functions

        private AttackType ChooseAttackType(float distance)
        {
            if (attackTypeWeight <= 0.02f) return AttackType.Melee;
            if (attackTypeWeight >= 0.98f) return AttackType.Ranged;

            float meleeMaxDistance = meleeAttackSettings.optimalDistance * 1.5f;
            float rangeMinDistance = rangedAttackSettings.optimalDistance * 1.3f;

            if (distance <= meleeMaxDistance) return AttackType.Melee;
            if (distance >= rangeMinDistance) return AttackType.Ranged;

            float normalizedDistance = Mathf.InverseLerp(meleeMaxDistance, rangeMinDistance, distance);
            float decisionThreshold = 0.5f + (0.5f - attackTypeWeight) * 0.4f;

            if (debug) Debug.Log($"AgentCombat - ChooseAttackType || normalizedDistance = {normalizedDistance} ||| decisionThreshold = {decisionThreshold}");
            return normalizedDistance > decisionThreshold ? AttackType.Ranged : AttackType.Melee;
        }

        private bool IsReadyToAttack(GameObject target)
        {
            if (target == null) return false;

            Vector3 directionToTarget = (target.transform.position - transform.position).normalized;
            directionToTarget.y = 0f;
            float angleToTarget = Vector3.Angle(transform.forward, directionToTarget);

            float currentOptimalAngle = currentAttackType switch
            {
                AttackType.Melee => meleeAttackSettings.optimalAngle,
                AttackType.Ranged => rangedAttackSettings.optimalAngle,
                _ => 15f
            };

            if (debug) Debug.Log($"AgentCombat - IsReadyToAttack ||| angleToTarget = {angleToTarget}");
            return angleToTarget < currentOptimalAngle;
        }

        private Vector3 CalculateAttackPosition(GameObject target, AttackType attackType)
        {
            AttackSettings settings = GetAttackSettings(attackType);
            Vector3 targetPosition = target.transform.position;
            float optimalDistance = settings.optimalDistance;

            Vector3 directionToTarget = (targetPosition - transform.position).normalized;
            Vector3 desiredPosition = targetPosition - directionToTarget * optimalDistance;
            desiredPosition.y = Mathf.Clamp(desiredPosition.y, transform.position.y - 2.1f, transform.position.y + 2.1f);

            NavMeshHit hit;
            if (NavMesh.SamplePosition(desiredPosition, out hit, 5f, NavMesh.AllAreas))
            {
                // Для дальней атаки избегаем слишком близких позиций
                if (attackType == AttackType.Ranged)
                {
                    float distanceToTargetFromPosition = Vector3.Distance(hit.position, targetPosition);
                    if (distanceToTargetFromPosition < meleeAttackSettings.optimalDistance * 1.5f)
                    {
                        Vector3 retreatDirection = (transform.position - targetPosition).normalized;
                        Vector3 fallbackPosition = transform.position + retreatDirection * 3f;
                        if (NavMesh.SamplePosition(fallbackPosition, out NavMeshHit fallbackHit, 3f, NavMesh.AllAreas))
                        {
                            return fallbackHit.position;
                        }
                    }
                }
                return hit.position;
            }

            return targetPosition;
        }

        private AttackSettings GetAttackSettings(AttackType attackType)
        {
            return attackType == AttackType.Melee ? meleeAttackSettings : rangedAttackSettings;
        }
        #endregion

        #region Utility Methods

        private void LogAttack(AttackType attackType, GameObject target)
        {
            string attackTypeString = attackType == AttackType.Melee ? "MELEE" : "RANGED";
            string debugMessage = $"<color=red><size=16><b>{attackTypeString} ATTACK!</b></size></color>\n" +
                                $"<color=yellow>Attacker: {gameObject}</color>\n" +
                                $"<color=cyan>Target: {target}</color>\n";

            Debug.Log(debugMessage);
        }

        private void DrawCombatGizmos()
        {
            Gizmos.DrawSphere(attackPosition, 0.1f);
        }
        #endregion

        #region Public API
        public void SetAttackTypeWeight(float weight)
        {
            attackTypeWeight = Mathf.Clamp01(weight);
        }

        public void SetRotationSpeed(float speed)
        {
            rotationSpeed = speed;
        }
        #endregion
    }
}