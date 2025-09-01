using UnityEngine;
using System.Collections;

namespace SimpleCharController
{
    public class HandlerCombatEffects : MonoBehaviour
    {
        public bool DeBug = true;

        [Header("Movement Controllers References")]
        [SerializeField] private MonoBehaviour movementController; // Базовый контроллер движения
        [SerializeField] private MonoBehaviour characterController; // Контроллер персонажа

        [Space(10)]
        [Header("CombatEvents")]
        public CombatEffectEvents combatEffectEvents;

        private StatusEffectType currentEffect = StatusEffectType.None;
        private Coroutine _effectCoroutine;
        private float _originalSpeed = 1f;

        void Start()
        {
            // Сохраняем оригинальную скорость если есть контроллер движения
            if (movementController != null)
            {
                // Здесь нужно получить ссылку на скорость из вашего контроллера движения
                // Например: _originalSpeed = movementController.moveSpeed;
            }
        }

        public void ApplyFrostbiteEffect(float freezePower, float freezeDuration)
        {
            if (DeBug)
            {
                Debug.Log($"<color=#a4e0fe>[FROSTBITE EFFECT]</color>\n" +
                         $"Power: {freezePower:F2}" + $"║ Duration: {freezeDuration:F2}s");
            }

            ApplyFrostbite(freezePower);
            ApplyStatusEffect(StatusEffectType.Frostbite, freezeDuration);
            combatEffectEvents.OnFrostbite.Invoke();
        }

        public void ApplyFreezeEffect(float freezePower, float freezeDuration)
        {
            if (DeBug)
            {
                Debug.Log($"<color=#00a8ff>[FREEZE EFFECT]</color>\n" +
                         $"Power: {freezePower:F2}" + $"║ Duration: {freezeDuration:F2}s");
            }

            ApplyFreeze(freezePower);
            ApplyStatusEffect(StatusEffectType.Freeze, freezeDuration);
            combatEffectEvents.OnFreeze.Invoke();
        }

        public void ApplyStunEffect(float duration)
        {
            ApplyStun(duration);
            ApplyStatusEffect(StatusEffectType.Stun, duration);
        }

        private void ApplyStatusEffect(StatusEffectType effectType, float duration)
        {
            // Останавливаем предыдущий эффект
            if (_effectCoroutine != null)
            {
                StopCoroutine(_effectCoroutine);
                RemoveEffect(currentEffect);
            }

            currentEffect = effectType;
            _effectCoroutine = StartCoroutine(EffectCoroutine(duration));
        }

        private IEnumerator EffectCoroutine(float duration)
        {
            if (DeBug) Debug.Log($"Effect started. Duration: {duration}");

            yield return new WaitForSeconds(duration);

            if (DeBug) Debug.Log($"Effect ended: {currentEffect}");
            RemoveEffect(currentEffect);
            currentEffect = StatusEffectType.None;
        }

        private void RemoveEffect(StatusEffectType effectType)
        {
            switch (effectType)
            {
                case StatusEffectType.Freeze:
                    RemoveFreeze();
                    break;

                case StatusEffectType.Frostbite:
                    RemoveFrostbite();
                    break;

                case StatusEffectType.Stun:
                    RemoveStun();
                    break;
            }
        }

        #region ========= МЕХАНИКИ ЭФФЕКТОВ ==================

        private void ApplyFreeze(float power)
        {
            // Полная остановка движения
            if (movementController != null)
            {
                // movementController.SetMovementEnabled(false);
                // movementController.SetRotationEnabled(false);
            }

            // Визуальные эффекты ледяной блок
            //VFXCombatEffect.Instance.ApplyFreezeVFX(transform, power);
        }

        private void RemoveFreeze()
        {
            combatEffectEvents.OnFreezeComplete.Invoke();
            // Восстановление движения
            if (movementController != null)
            {
                // movementController.SetMovementEnabled(true);
                // movementController.SetRotationEnabled(true);
            }

            // Удаление визуальных эффектов
            //VFXCombatEffect.Instance.RemoveFreezeVFX(transform);
        }

        private void ApplyFrostbite(float power)
        {
            // Замедление движения
            if (movementController != null)
            {
                float slowFactor = 1f - power; // power от 0 до 1
                // movementController.SetSpeedMultiplier(slowFactor);
            }

            // Визуальные эффекты обморожения
            //VFXCombatEffect.Instance.ApplyFrostbiteVFX(transform, power);
        }

        private void RemoveFrostbite()
        {
            combatEffectEvents.OnFrostbiteComplete.Invoke();
            // Восстановление скорости
            if (movementController != null)
            {
                // movementController.SetSpeedMultiplier(1f);
            }

            // Удаление визуальных эффектов
            //VFXCombatEffect.Instance.RemoveFrostbiteVFX(transform);
        }

        private void ApplyStun(float power)
        {
            // Оглушение - временная потеря контроля
            if (movementController != null)
            {
                // movementController.SetMovementEnabled(false);
            }

            // Визуальные эффекты оглушения
            //VFXCombatEffect.Instance.ApplyStunVFX(transform, power);
        }

        private void RemoveStun()
        {
            // Восстановление контроля
            if (movementController != null)
            {
                // movementController.SetMovementEnabled(true);
            }

            // Удаление визуальных эффектов
            //VFXCombatEffect.Instance.RemoveStunVFX(transform);
        }

        // Методы для Area of Effect (AoE) эффектов
        public void CreateFreezeArea(Vector3 position, float radius, float power, float duration)
        {
            if (DeBug) Debug.Log($"Creating freeze area at {position}, radius: {radius}");

            // Здесь будет логика создания области заморозки VFX
            //VFXCombatEffect.Instance.CreateFreezeAreaVFX(position, radius, duration);
        }

        public void CreateFreezeExplosion(Vector3 position, float radius, float power)
        {
            if (DeBug) Debug.Log($"Freeze explosion at {position}, radius: {radius}");

            // Здесь будет логика создания области КриоВзрыва VFX
            //VFXCombatEffect.Instance.CreateFreezeExplosionVFX(position, radius, power);

            // Поиск целей в радиусе и применение эффекта
            /*Collider[] colliders = Physics.OverlapSphere(position, radius);
            foreach (Collider collider in colliders)
            {
                IStatusEffectTarget target = collider.GetComponent<IStatusEffectTarget>();
                if (target != null)
                {
                    target.ApplyFreezeEffect(power * 0.5f, 3f); // Ослабленный эффект
                }
            }*/
        }

        #endregion

        #region =========== Публичные методы ================
        public bool IsFreeze()
        {
            return currentEffect == StatusEffectType.Freeze;
        }

        public bool IsFrostbite()
        {
            return currentEffect == StatusEffectType.Frostbite;
        }

        public bool IsStun()
        {
            return currentEffect == StatusEffectType.Stun;
        }
        public void ClearAllEffects()
        {
            if (_effectCoroutine != null)
            {
                StopCoroutine(_effectCoroutine);
                RemoveEffect(currentEffect);
                currentEffect = StatusEffectType.None;
            }
        }

        #endregion
    }
}