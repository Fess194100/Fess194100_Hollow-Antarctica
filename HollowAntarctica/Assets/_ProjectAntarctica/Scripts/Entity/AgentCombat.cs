using UnityEngine;
using System.Collections;
using SimpleCharController;
using UnityEngine.AI;
using UnityEngine.InputSystem.LowLevel;
using static Breeze.Core.BreezeMeleeWeapon.impactEffect;

namespace AdaptivEntityAgent
{
    [System.Serializable]
    public class AttackSettings
    {
        [Header("Basic Settings")]
        public float attackRange = 2f;
        public float attackDamage = 10f;
        public float attackRate = 1f;
        public float optimalDistance = 5f;

        [Header("Projectile Settings")]
        public GameObject projectilePrefab;
        public Transform attackStartPosition;

        [Header("Layers")]
        public LayerMask targetMask = ~0;
    }

    public class AgentCombat : MonoBehaviour
    {
        #region Variables
        [Header("Melee Attack Settings")]
        [SerializeField] private AttackSettings meleeAttackSettings = new AttackSettings();

        [Header("Ranged Attack Settings")]
        [SerializeField] private AttackSettings rangedAttackSettings = new AttackSettings();

        [Header("Attack Settings")]
        [SerializeField] private bool debug = false;

        [Space(10)]
        [Tooltip(" 0 = melee, 1 = ranged")]
        [Range(0f, 1f)]
        [SerializeField] private float attackTypeWeight = 0.5f;
        [SerializeField] private float rotationSpeed = 120f;

        [Space(10)]
        public AgentEventsCombat agentEventsCombat;
        #endregion

        #region Private Variables
        private EssenceHealth health;
        private AgentPerception perception;
        private AgentMovement agentMovement;
        private NavMeshAgent navMeshAgent;
        private Coroutine combatCoroutine;
        private EssenceHealth targetHealth;

        // Combat state variables
        private bool canAttack = true;
        private bool isAiming = false;
        private bool canRotation = false;
        private bool isFlee = false;
        private Vector3 attackPosition;
        private Quaternion targetRotation;
        private AttackType currentAttackType;
        private GameObject currentTargetAttack;
        #endregion

        #region Public Properties
        public float AttackTypeWeight => attackTypeWeight;
        public AttackType CurrentAttackType => currentAttackType;
        public bool IsAiming => isAiming;
        public AttackSettings MeleeAttackSettings => meleeAttackSettings;
        public AttackSettings RangedAttackSettings => rangedAttackSettings;
        #endregion

        #region System Functions
        private void Awake()
        {
            InitializeComponents();
        }

        private void FixedUpdate()
        {
            UpdateRotation();
        }

        private void OnDestroy()
        {
            CleanupCoroutines();
        }

        private void InitializeComponents()
        {
            health = GetComponent<EssenceHealth>();
            perception = GetComponent<AgentPerception>();
            agentMovement = GetComponent<AgentMovement>();
            navMeshAgent = GetComponent<NavMeshAgent>();
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
                StartCombatState();
            }
        }

        private void StartCombatState()
        {
            combatCoroutine = StartCoroutine(CombatRoutine());
        }

        private void StopCombatState()
        {
            if (combatCoroutine != null)
            {
                StopCoroutine(combatCoroutine);
                combatCoroutine = null;
            }
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
                if (debug) Debug.Log($"AgentCombat - CombatRoutine() HasTarget = {perception.HasTarget}");
                if (perception.HasTarget)
                {
                    ProcessCombat();
                }
                else
                {
                    canRotation = false;
                }
                yield return new WaitForSeconds(0.2f);
            }
        }

        private void ProcessCombat()
        {
            GameObject target = perception.CurrentTarget;
            float distance = perception.CurrentTargetDistance;

            if (target == null) return;

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
                return;
            }

            MoveToAttackPosition();
        }

        private void MoveToAttackPosition()
        {
            float distanceToAttackPosition = Vector3.Distance(transform.position, attackPosition);
            float distanceToSwitchAttackPosition = 1f;
            if (debug) Debug.Log($"AgentCombat - distanceToAttackPosition = {distanceToAttackPosition}");

            if (currentAttackType == AttackType.Ranged)
            {
                distanceToSwitchAttackPosition = rangedAttackSettings.optimalDistance / 2;
            }

            if (distanceToAttackPosition > distanceToSwitchAttackPosition)
            {
                agentMovement.MoveToPosition(attackPosition);                
                if (debug) Debug.Log($"Moving to attack position for {currentAttackType} attack, Distance: {distanceToAttackPosition:F1}");

                if (perception.CurrentTargetDistance < distanceToAttackPosition && !isFlee)
                {
                    // Агент отступает
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

        private void HandleAimingPhase(GameObject target)
        {
            AimAtTarget(target);

            if (IsReadyToAttack(target) && canAttack)
            {
                // Реализация атаки
                PerformAttack(target, currentAttackType);
            }
        }

        private void UpdateRotation()
        {
            if (canRotation)
            {
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );
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

            if (target != null && currentTargetAttack != target)
            {
                currentTargetAttack = target;
                targetHealth = currentTargetAttack.GetComponent<FlagFaction>().EssenceHealth;
            }
            

            if (targetHealth == null || targetHealth.IsDead()) return; // Если у цели нет здоровья или цель мертва, то нужны действия. Эта проверка должна быть раньше!!!

            if(debug) LogAttack(attackType, target, settings.attackDamage);

            if (attackType == AttackType.Melee)
            {
                agentEventsCombat.OnAttackMelle.Invoke();
                PerformMeleeAttack(target, targetHealth, settings);
            }
            else
            {
                agentEventsCombat.OnAttackRange.Invoke();
                PerformRangedAttack(target, targetHealth, settings);
            }

            StartCoroutine(AttackCooldown(settings));
        }

        private void PerformMeleeAttack(GameObject target, EssenceHealth targetHealth, AttackSettings settings)
        {
            // Визуальные эффекты для ближней атаки
            if (settings.projectilePrefab != null && settings.attackStartPosition != null)
            {
                GameObject meleeEffect = Instantiate(
                    settings.projectilePrefab,
                    settings.attackStartPosition.position,
                    settings.attackStartPosition.rotation,
                    settings.attackStartPosition
                );
                Destroy(meleeEffect, 1f);
            }
        }

        private void PerformRangedAttack(GameObject target, EssenceHealth targetHealth, AttackSettings settings)
        {
            // Дальняя атака - создание снаряда
            if (settings.projectilePrefab != null && settings.attackStartPosition != null)
            {
                Vector3 spawnPosition = settings.attackStartPosition.position;
                Quaternion spawnRotation = Quaternion.LookRotation(
                    (target.transform.position - spawnPosition).normalized
                );

                GameObject projectile = Instantiate(settings.projectilePrefab, spawnPosition, spawnRotation);

                // Настройка снаряда (требует реализации ProjectileController)
                /*
                ProjectileController projectileController = projectile.GetComponent<ProjectileController>();
                if (projectileController != null)
                {
                    projectileController.Initialize(target, settings.attackDamage);
                }
                else
                {
                    targetHealth.TakeDamage(settings.attackDamage, ProjectileType.Green, 1, BodyPart.Body);
                    Destroy(projectile, 2f);
                }
                */
            }
            else
            {
                // Fallback - мгновенное нанесение урона
                // targetHealth.TakeDamage(settings.attackDamage, ProjectileType.Green, 1, BodyPart.Body);
            }
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

            // Для смешанных весов (0.02-0.98) используем сложную логику выбора
            float meleePreference = CalculateMeleePreference(distance);
            float rangedPreference = CalculateRangedPreference(distance);

            meleePreference *= (1f - attackTypeWeight);
            rangedPreference *= attackTypeWeight;

            if (debug) Debug.Log($"Melee preference: {meleePreference}, Ranged preference: {rangedPreference}");

            return meleePreference >= rangedPreference ? AttackType.Melee : AttackType.Ranged;
        }

        private bool IsReadyToAttack(GameObject target)
        {
            if (target == null) return false;

            Vector3 directionToTarget = (target.transform.position - transform.position).normalized;
            float angleToTarget = Vector3.Angle(transform.forward, directionToTarget);

            return angleToTarget < 15f;
        }

        private float CalculateMeleePreference(float distance)
        {
            AttackSettings settings = meleeAttackSettings;

            return Mathf.Clamp01(1f - (distance - settings.optimalDistance) / (settings.optimalDistance * 3f));
        }

        private float CalculateRangedPreference(float distance)
        {
            AttackSettings settings = rangedAttackSettings;
            float optimalDistance = settings.optimalDistance;
            float maxDistance = settings.attackRange;

            if (distance >= optimalDistance * 0.8f && distance <= optimalDistance * 1.2f)
                return 1f;
            else if (distance <= maxDistance)
                return 0.7f;
            else
                return 0f;
        }

        private Vector3 CalculateAttackPosition(GameObject target, AttackType attackType)
        {
            AttackSettings settings = GetAttackSettings(attackType);
            Vector3 targetPosition = target.transform.position;
            float optimalDistance = settings.optimalDistance;

            Vector3 directionToTarget = (targetPosition - transform.position).normalized;
            Vector3 desiredPosition = targetPosition - directionToTarget * optimalDistance;

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

            return Vector3.zero;
        }

        private AttackSettings GetAttackSettings(AttackType attackType)
        {
            return attackType == AttackType.Melee ? meleeAttackSettings : rangedAttackSettings;
        }
        #endregion

        #region Utility Methods

        private void LogAttack(AttackType attackType, GameObject target, float damage)
        {
            string attackTypeString = attackType == AttackType.Melee ? "MELEE" : "RANGED";
            string debugMessage = $"<color=red><size=16><b>{attackTypeString} ATTACK!</b></size></color>\n" +
                                $"<color=yellow>Attacker: {gameObject}</color>\n" +
                                $"<color=cyan>Target: {target}</color>\n" +
                                $"<color=white>Damage: {damage}</color>";

            Debug.Log(debugMessage);
        }

        private void CleanupCoroutines()
        {
            if (combatCoroutine != null) StopCoroutine(combatCoroutine);
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

        public bool IsInAttackRange(Vector3 targetPosition, AttackType attackType = AttackType.Melee)
        {
            AttackSettings settings = GetAttackSettings(attackType);
            return Vector3.Distance(transform.position, targetPosition) <= settings.attackRange;
        }
        #endregion
    }
}