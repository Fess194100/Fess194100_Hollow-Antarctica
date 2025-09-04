using UnityEngine;
using System.Collections;
using Unity.VisualScripting;
using System.Collections.Generic;

namespace SimpleCharController
{
    public class AreaCombatEffect : MonoBehaviour
    {
        [Header("DeBug Settings")]
        public bool DeBug = false;
        public bool showGizmos = false;

        [Header("Area Effect Settings")]
        public ProjectileType projectileType;
        public StatusEffectType effectType = StatusEffectType.None;
        public int numberEffectTargets = 99;
        public float effectDamage;
        public float effectDuration;
        public float effectPower;
        public float effectRadius;

        [Header("VFX Settings")]
        public VFXCombatEffect VFXCombatEffect;

        #region ========== PRIVATE VALUE ==================
        private GameObject _owner;
        private List<GameObject> _targets;
        private EssenceHealth _ownerHealth;
        private HandlerCombatEffects _ownerHandlerCombatEffects;
        private int _currentNumberTarget;
        private float _lifeTime;
        #endregion

        private void Awake()
        {
            _targets = new List<GameObject>();
        }

        public void Initialize(GameObject owner, EssenceHealth ownerHealth, HandlerCombatEffects ownerHandlerCombatEffects)
        {
            _owner = owner;
            _ownerHealth = ownerHealth;
            _ownerHandlerCombatEffects = ownerHandlerCombatEffects;

            /*if (sphereCollider != null)
            {
                sphereCollider.radius = _effectRadius;
                sphereCollider.isTrigger = true;
            }*/

            // Запускаем таймер жизни эффекта
            StartCoroutine(LifeTimeCoroutine());

            // Применяем эффект сразу ко всем кто уже в области
            ApplyEffectToExistingTargets();

            // Запускаем VFX
            //VFXCombatEffect.PlayAreaEffectVFX(effectType, effectRadius);
        }

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

        private void OnTriggerEnter(Collider other)
        {
            if (DeBug) Debug.Log("OnTrigger");
            ApplyEffectToTarget(other.gameObject);
        }

        private void ApplyEffectToTarget(GameObject target)
        {
            HitBox hitBox = target.GetComponent<HitBox>();

            if (hitBox == null || !hitBox.isAffectedByAreaEffects) return;

            _currentNumberTarget += 1;
            _targets.Add(target);
            EssenceHealth targetEssenceHealth = hitBox.GetEssenceHealth();

            if (targetEssenceHealth != null && targetEssenceHealth != _ownerHealth)
            {
                hitBox.TakeDamage(effectDamage, projectileType, -2);
            }

            HandlerCombatEffects targetHandler = hitBox.GetCombatEffects();

            if (targetHandler == null || targetHandler == _ownerHandlerCombatEffects) return;

            // Применяем соответствующий эффект
            switch (effectType)
            {
                case StatusEffectType.Freeze:
                    targetHandler.ApplyFreezeEffect(effectPower, effectDuration);
                    //VFXCombatEffect.PlayAreaEffectVFX(effectType, effectRadius);
                    break;

                case StatusEffectType.Frostbite:
                    targetHandler.ApplyFrostbiteEffect(effectPower, effectDuration);
                    //VFXCombatEffect.PlayAreaEffectVFX(effectType, effectRadius);
                    break;

                case StatusEffectType.ElectroShort:
                    targetHandler.ApplyElectroShortEffect(effectDuration);
                    //VFXCombatEffect.PlayAreaEffectVFX(effectType, effectRadius);
                    break;
                case StatusEffectType.ChainLightning:
                    targetHandler.ApplyElectroShortEffect(effectDuration);
                    break;
            }
        }

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

            Destroy(gameObject);
        }

        #region ======== DeBug =================
        // Визуализация радиуса в редакторе
        private void OnDrawGizmos()
        {
            if (!showGizmos) return;

            Gizmos.color = GetEffectColor();
            Gizmos.DrawWireSphere(transform.position, effectRadius);
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
    }
}