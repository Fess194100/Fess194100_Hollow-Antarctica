using UnityEngine;
using System.Collections;
using SimpleCharController;
using UnityEngine.AI;

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
        public bool debug;

        [Header("Melee Attack Settings")]
        [SerializeField] private AttackSettings meleeAttackSettings = new AttackSettings();

        [Header("Ranged Attack Settings")]
        [SerializeField] private AttackSettings rangedAttackSettings = new AttackSettings();

        [Header("Attack Weights")]
        [Range(0f, 1f)]
        [SerializeField] private float attackTypeWeight = 0.5f; // 0 = melee, 1 = ranged

        private EssenceHealth health;
        private AgentPerception perception;
        private AgentMovement agentMovement;
        private NavMeshAgent navMeshAgent;
        private Coroutine combatCoroutine;

        // Combat state variables
        private bool canAttack = true;
        private bool isAiming = false;
        private bool canRotation = false;
        private Vector3 attackPosition;
        private Quaternion targetRotation;
        private AttackType currentAttackType;
        private float rotationSpeed = 120f;

        public enum AttackType
        {
            Melee,
            Ranged
        }

        private void Awake()
        {
            health = GetComponent<EssenceHealth>();
            perception = GetComponent<AgentPerception>();
            agentMovement = GetComponent<AgentMovement>();
            navMeshAgent = GetComponent<NavMeshAgent>();
        }

        private void FixedUpdate()
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
        public void OnStateChanged(AgentState newState)
        {
            // Остановка боевой корутины при выходе из состояния боя
            if (newState != AgentState.Combat && combatCoroutine != null)
            {
                StopCoroutine(combatCoroutine);
                combatCoroutine = null;
                isAiming = false;
                canRotation = false;
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
                if (perception.HasTarget)
                {
                    GameObject target = perception.GetCurrentTarget();
                    ProcessCombat(target);
                }
                else canRotation = false;
                yield return new WaitForSeconds(0.2f);
            }
        }

        private void ProcessCombat(GameObject target)
        {
            if (target == null) return;

            if (!isAiming)
            {
                canRotation = false;
                // Выбираем тип атаки и позицию
                currentAttackType = ChooseAttackType(target);
                attackPosition = CalculateAttackPosition(target, currentAttackType);

                // Если не можем найти позицию для атаки - отступаем
                if (attackPosition == Vector3.zero)
                {
                    if (debug) Debug.Log("No valid attack position found, fleeing!");
                    return;
                }

                // Двигаемся к позиции атаки
                float distanceToAttackPosition = Vector3.Distance(transform.position, attackPosition);
                if (distanceToAttackPosition > 1f)
                {
                    agentMovement.MoveToPosition(attackPosition);
                    if (debug) Debug.Log($"Moving to attack position for {currentAttackType} attack");
                }
                else
                {
                    // Достигли позиции - начинаем прицеливание
                    agentMovement.StopMovement();
                    isAiming = true;
                    if (debug) Debug.Log($"Starting aim for {currentAttackType} attack");
                }
            }
            else
            {
                // Прицеливание - поворачиваемся к цели
                AimAtTarget(target);

                // Проверяем готовность к атаке
                if (IsReadyToAttack(target, currentAttackType) && canAttack)
                {
                    PerformAttack(target, currentAttackType);
                }
            }
        }

        private AttackType ChooseAttackType(GameObject target)
        {
            float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);

            // Если вес дальних атак = 1 и цель слишком близко - отступаем
            if (attackTypeWeight >= 0.9f && distanceToTarget < meleeAttackSettings.optimalDistance)
            {
                return AttackType.Ranged; // Будем пытаться отступить для дальней атаки
            }

            // Определяем предпочтительный тип атаки на основе веса и дистанции
            float meleePreference = CalculateMeleePreference(distanceToTarget);
            float rangedPreference = CalculateRangedPreference(distanceToTarget);

            // Учитываем вес типа атаки
            meleePreference *= (1f - attackTypeWeight);
            rangedPreference *= attackTypeWeight;

            if (debug) Debug.Log($"Melee pref: {meleePreference}, Ranged pref: {rangedPreference}");

            return meleePreference >= rangedPreference ? AttackType.Melee : AttackType.Ranged;
        }

        private float CalculateMeleePreference(float distance)
        {
            AttackSettings settings = meleeAttackSettings;

            // Предпочтение ближней атаки уменьшается с расстоянием
            if (distance <= settings.optimalDistance)
                return 1f;
            else if (distance <= settings.attackRange)
                return 0.5f;
            else
                return 0f;
        }

        private float CalculateRangedPreference(float distance)
        {
            AttackSettings settings = rangedAttackSettings;

            // Предпочтение дальней атаки максимально на оптимальной дистанции
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

            // Проверяем доступность позиции через NavMesh
            NavMeshHit hit;
            if (NavMesh.SamplePosition(desiredPosition, out hit, 5f, NavMesh.AllAreas))
            {
                // Для дальней атаки проверяем, что позиция не слишком близко к цели
                if (attackType == AttackType.Ranged)
                {
                    float distanceToTargetFromPosition = Vector3.Distance(hit.position, targetPosition);
                    if (distanceToTargetFromPosition < meleeAttackSettings.optimalDistance * 1.5f)
                    {
                        // Ищем другую позицию подальше
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

            return Vector3.zero; // Нет валидной позиции
        }

        private void AimAtTarget(GameObject target)
        {
            if (target == null) return;

            Vector3 direction = (target.transform.position - transform.position).normalized;
            direction.y = 0; // Игнорируем разницу по высоте

            if (direction != Vector3.zero)
            {
                targetRotation = Quaternion.LookRotation(direction);
                canRotation = true;
            } 
            else canRotation = false;
        }

        private bool IsReadyToAttack(GameObject target, AttackType attackType)
        {
            if (target == null) return false;

            Vector3 directionToTarget = (target.transform.position - transform.position).normalized;
            float angleToTarget = Vector3.Angle(transform.forward, directionToTarget);

            // Для атаки нужно смотреть на цель с достаточной точностью
            return angleToTarget < 15f;
        }

        private void PerformAttack(GameObject target, AttackType attackType)
        {
            AttackSettings settings = GetAttackSettings(attackType);
            EssenceHealth targetHealth = target.GetComponent<EssenceHealth>();
            if (targetHealth == null || targetHealth.IsDead()) return;

            // Выводим дебаг информацию большими цветными буквами
            string attackTypeString = attackType == AttackType.Melee ? "MELEE" : "RANGED";
            string debugMessage = $"<color=red><size=16><b>{attackTypeString} ATTACK!</b></size></color>\n" +
                                $"<color=yellow>Attacker: {gameObject.name}</color>\n" +
                                $"<color=cyan>Target: {target.name}</color>\n" +
                                $"<color=white>Damage: {settings.attackDamage}</color>";

            Debug.Log(debugMessage);

            if (attackType == AttackType.Melee)
            {
                PerformMeleeAttack(target, targetHealth, settings);
            }
            else
            {
                PerformRangedAttack(target, targetHealth, settings);
            }

            StartCoroutine(AttackCooldown(settings));
        }

        private void PerformMeleeAttack(GameObject target, EssenceHealth targetHealth, AttackSettings settings)
        {
            // Ближняя атака - мгновенное нанесение урона
            //targetHealth.TakeDamage(settings.attackDamage, ProjectileType.Green, 1, BodyPart.Body);

            // Визуальные эффекты для ближней атаки (опционально)
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

                // Настройка снаряда (добавьте свой компонент управления снарядом)
                /*ProjectileController projectileController = projectile.GetComponent<ProjectileController>();
                if (projectileController != null)
                {
                    projectileController.Initialize(target, settings.attackDamage);
                }
                else
                {
                    // Fallback - мгновенное нанесение урона
                    targetHealth.TakeDamage(settings.attackDamage, ProjectileType.Green, 1, BodyPart.Body);
                    Destroy(projectile, 2f);
                }*/
            }
            else
            {
                // Fallback - мгновенное нанесение урона
                //targetHealth.TakeDamage(settings.attackDamage, ProjectileType.Green, 1, BodyPart.Body);
            }
        }

        private IEnumerator AttackCooldown(AttackSettings settings)
        {
            canAttack = false;
            isAiming = false; // Сбрасываем прицеливание после атаки

            if (debug) Debug.Log($"Attack cooldown: {1f / settings.attackRate}s");
            yield return new WaitForSeconds(1f / settings.attackRate);

            canAttack = true;
        }

        private AttackSettings GetAttackSettings(AttackType attackType)
        {
            return attackType == AttackType.Melee ? meleeAttackSettings : rangedAttackSettings;
        }

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

        public AttackType GetCurrentAttackType() => currentAttackType;
        public bool IsAiming() => isAiming;

        public AttackSettings GetMeleeAttackSettings() => meleeAttackSettings;
        public AttackSettings GetRangedAttackSettings() => rangedAttackSettings;
        #endregion

        private void OnDestroy()
        {
            if (combatCoroutine != null)
                StopCoroutine(combatCoroutine);
        }
    }
}