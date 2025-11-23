using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SimpleCharController
{
    public class AreaCombatEffect : MonoBehaviour
    {
        #region ========== VARIABLES ==================
        [Header("DeBug Settings")]
        public bool DeBug = false;
        public bool showGizmos = false;

        [Header("Area Damage Type Settings")]
        public bool destroyAfterLifeTime = true;

        [Space(5)]
#if UNITY_EDITOR_RUS
        [Tooltip("Single / Burst / Spread - разовый урон \n" + "Auto - постоянный урон пока цель в области\n")]
#else   
        [Tooltip("Single / Burst / Spread - single damage \n" + "Auto - Permanent damage while the target is in the area\n")]
#endif
        public TypeShooting shootingType = TypeShooting.Single;
        public float autoFireRate = 1f;

        [Header("Area Effect Settings")]
        public ProjectileType projectileType;
        public StatusEffectType effectType = StatusEffectType.None;
        public bool hitRaection;
        public int numberEffectTargets = 99;
        public float effectDamage;
        public float effectDuration;
        public float effectPower;
        public float effectRadius;

        [Header("VFX Settings")]
        public VFXCombatEffect VFXCombatEffect;
        #endregion

        #region ========== PRIVATE VARIABLES ==================
        private GameObject _owner;
        private List<GameObject> _targets;
        private EssenceHealth _ownerHealth;
        private HandlerCombatEffects _ownerHandlerCombatEffects;
        private int _currentNumberTarget;
        private float _lifeTime;

        // Для повторяющегося урона
        private Dictionary<GameObject, float> _targetDamageAccumulator;
        private List<GameObject> _targetsInArea;
        #endregion

        #region ========== SYSTEM FUNCTIONS ==================
        private void Awake()
        {
            _targets = new List<GameObject>();
            _targetsInArea = new List<GameObject>();
            _targetDamageAccumulator = new Dictionary<GameObject, float>();
        }

        private void FixedUpdate()
        {
            // Для повторяющегося урона проверяем все цели в области
            if (shootingType == TypeShooting.Auto && _targetsInArea.Count > 0)
            {
                ProcessAutoDamage();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (DeBug) Debug.Log("OnTriggerEnter");

            GameObject target = other.gameObject;

            // Для всех типов стрельбы добавляем цель в список находящихся в области
            if (IsValidTarget(target) && !_targetsInArea.Contains(target))
            {
                _targetsInArea.Add(target);

                // Для Auto - инициализируем аккумулятор и наносим первый урон
                if (shootingType == TypeShooting.Auto)
                {
                    _targetDamageAccumulator[target] = 0f;
                }

                ApplyEffectToTarget(target);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (DeBug) Debug.Log("OnTriggerExit");

            GameObject target = other.gameObject;

            // Удаляем цель из списка находящихся в области
            if (_targetsInArea.Contains(target))
            {
                _targetsInArea.Remove(target);

                // Для Auto - удаляем из словаря аккумулятора
                if (shootingType == TypeShooting.Auto && _targetDamageAccumulator.ContainsKey(target))
                {
                    _targetDamageAccumulator.Remove(target);
                }
            }
        }

        private void OnDrawGizmos()
        {
            if (!showGizmos) return;

            Gizmos.color = GetEffectColor();
            Gizmos.DrawWireSphere(transform.position, effectRadius);
        }
        #endregion

        #region ========== PRIVATE METHODS ==================
        private void ApplyEffectToExistingTargets()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, effectRadius); // Нужна маска слоев!!!

            foreach (Collider collider in colliders)
            {
                ApplyEffectToTarget(collider.gameObject);
                if (_currentNumberTarget >= numberEffectTargets) break;
            }

            // Запускаем VFX
            if (effectType == StatusEffectType.ChainLightning) VFXCombatEffect.PlayAreaEffectVFX(effectType, _targets);

            _currentNumberTarget = 0;
            _targets.Clear();
        }

        private void ProcessAutoDamage()
        {
            float damageInterval = 1f / autoFireRate;

            foreach (GameObject target in _targetsInArea)
            {
                if (target == null) continue;

                if (!_targetDamageAccumulator.ContainsKey(target))
                {
                    _targetDamageAccumulator[target] = 0f;
                }

                // Накопление времени с учетом Time.timeScale
                _targetDamageAccumulator[target] += Time.fixedDeltaTime;

                if (_targetDamageAccumulator[target] >= damageInterval)
                {
                    ApplyEffectToTarget(target);
                    _targetDamageAccumulator[target] = 0f; // Сбрасываем аккумулятор
                }
            }
        }

        private void ApplyEffectToTarget(GameObject target)
        {
            if (DeBug) Debug.Log("ApplyEffectToTarget");

            if (!IsValidTarget(target)) return;

            HitBox hitBox = target.GetComponent<HitBox>();

            if (DeBug) Debug.Log($"ApplyEffectToTarget - HITBOX = {target.gameObject.name}");
            if (hitBox == null || !hitBox.isAffectedByAreaEffects) return;

            if (DeBug) Debug.Log($"ApplyEffectToTarget - isAffectedByAreaEffects = {hitBox.isAffectedByAreaEffects}");

            _currentNumberTarget += 1;
            _targets.Add(target);
            EssenceHealth targetEssenceHealth = hitBox.EssencelHealth;

            if (targetEssenceHealth != null && targetEssenceHealth != _ownerHealth)
            {
                hitBox.TakeDamage(effectDamage, projectileType, -2, _owner, true, hitRaection);
            }

            HandlerCombatEffects targetHandler = hitBox.GetCombatEffects();

            if (targetHandler == null || targetHandler == _ownerHandlerCombatEffects) return;

            // Применяем соответствующий эффект
            ApplyStatusEffect(targetHandler);
        }

        private void ApplyStatusEffect(HandlerCombatEffects targetHandler)
        {
            switch (effectType)
            {
                case StatusEffectType.Freeze:
                    targetHandler.ApplyFreezeEffect(effectPower, effectDuration);
                    break;

                case StatusEffectType.Frostbite:
                    targetHandler.ApplyFrostbiteEffect(effectPower, effectDuration);
                    break;

                case StatusEffectType.ElectroShort:
                    targetHandler.ApplyElectroShortEffect(effectDuration);
                    break;

                case StatusEffectType.ChainLightning:
                    targetHandler.ApplyElectroShortEffect(effectDuration);
                    break;
            }
        }
        #endregion

        #region ========== ENUMERATORS ==================
        private IEnumerator LifeTimeCoroutine()
        {
            _lifeTime = 0f;

            while (_lifeTime < VFXCombatEffect.durationVFX)
            {
                _lifeTime += Time.deltaTime;
                yield return null;
            }

            // Запускаем эффект исчезновения VFX
            VFXCombatEffect.StopAreaEffectVFX();

            if (destroyAfterLifeTime) Destroy(gameObject);
        }
        #endregion

        #region ========== DETERMINISTIC FUNCTIONS ==================
        private bool IsValidTarget(GameObject target)
        {
            if (target == null) return false;

            HitBox hitBox = target.GetComponent<HitBox>();
            if (hitBox == null || !hitBox.isAffectedByAreaEffects) return false;

            EssenceHealth targetEssenceHealth = hitBox.EssencelHealth;
            if (targetEssenceHealth == null || targetEssenceHealth == _ownerHealth) return false;

            HandlerCombatEffects targetHandler = hitBox.GetCombatEffects();
            if (targetHandler == null || targetHandler == _ownerHandlerCombatEffects) return false;

            return true;
        }

        private Color GetEffectColor()
        {
            return effectType switch
            {
                StatusEffectType.Freeze => Color.blue,
                StatusEffectType.Frostbite => Color.cyan,
                StatusEffectType.ElectroShort => Color.yellow,
                _ => Color.white
            };
        }
        #endregion

        #region ========== PUBLIC API ==================
        public void Initialize(GameObject owner, EssenceHealth ownerHealth, HandlerCombatEffects ownerHandlerCombatEffects)
        {
            _owner = owner;
            _ownerHealth = ownerHealth;
            _ownerHandlerCombatEffects = ownerHandlerCombatEffects;

            InitializeAreaDamage();

            // Запускаем VFX
            //VFXCombatEffect.PlayAreaEffectVFX(effectType, effectRadius);
        }

        public void InitializeAreaDamage()
        {
            StartCoroutine(LifeTimeCoroutine());
            ApplyEffectToExistingTargets();
        }
        #endregion
    }
}