using UnityEngine;
using System.Collections;

namespace SimpleCharController
{
    public class AreaCombatEffect : MonoBehaviour
    {
        [Header("DeBug Settings")]
        public bool DeBug = false;
        public bool showGizmos = false;

        //References
        /*[SerializeField] private SphereCollider sphereCollider;*/

        [Header("Area Effect Settings")]
        public StatusEffectType effectType = StatusEffectType.None;
        public float _effectDuration = 0;
        public float _effectPower = 0;
        public float _effectRadius = 0;

        [Header("VFX Settings")]
        public VFXCombatEffect VFXCombatEffect;

        #region ========== PRIVATE VALUE ==================
        private GameObject _owner;
        private EssenceHealth _ownerHealth;
        private HandlerCombatEffects _ownerHandlerCombatEffects;
        private float _lifeTime;
        #endregion

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
            VFXCombatEffect.PlayAreaEffectVFX(effectType, _effectRadius);
        }

        private void ApplyEffectToExistingTargets()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, _effectRadius);
            foreach (Collider collider in colliders)
            {
                ApplyEffectToTarget(collider.gameObject);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (DeBug) Debug.Log("OnTrigger");
            ApplyEffectToTarget(other.gameObject);
        }

        private void ApplyEffectToTarget(GameObject target)
        {
            // Не применяем эффект к владельцу
            //if (target == _owner) return;

            // Ищем обработчик эффектов у цели
            HitBox hitBox = target.GetComponent<HitBox>();

            if (hitBox == null || !hitBox.isAffectedByAreaEffects) return;

            HandlerCombatEffects targetHandler = hitBox.GetCombatEffects();

            if (targetHandler == null) return;

            // Не применяем эффект к самому себе (на случай если владелец тоже имеет HandlerCombatEffects)
            if (targetHandler == _ownerHandlerCombatEffects) return;

            // Применяем соответствующий эффект
            switch (effectType)
            {
                case StatusEffectType.Freeze:
                    targetHandler.ApplyFreezeEffect(_effectPower, _effectDuration);
                    break;

                case StatusEffectType.Frostbite:
                    targetHandler.ApplyFrostbiteEffect(_effectPower, _effectDuration);
                    break;

                case StatusEffectType.Stun:
                    targetHandler.ApplyStunEffect(_effectDuration);
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
            Gizmos.DrawWireSphere(transform.position, _effectRadius);
        }

        private Color GetEffectColor()
        {
            return effectType switch
            {
                StatusEffectType.Freeze => Color.blue,
                StatusEffectType.Frostbite => Color.cyan,
                StatusEffectType.Stun => Color.yellow,
                _ => Color.white
            };
        }
        #endregion
    }
}