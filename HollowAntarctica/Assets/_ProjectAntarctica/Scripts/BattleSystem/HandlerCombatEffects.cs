using UnityEngine;
using System.Collections;
using Breeze.Core;

namespace SimpleCharController
{
    public class HandlerCombatEffects : MonoBehaviour
    {
        public bool DeBug = true;

        [Header("Movement Controllers References")]
        [SerializeField] private BreezeSystem agentController; // AgentController
        [SerializeField] private MonoBehaviour characterController; // Контроллер персонажа

        [Space(10)]
        [Header("CombatEvents")]
        public CombatEffectEvents combatEffectEvents;

        private StatusEffectType currentEffect = StatusEffectType.None;
        private Coroutine _effectCoroutine;
        private float _originalSpeedMultiplier = 1f;

        public void ApplyFrostbiteEffect(float freezePower, float freezeDuration)
        {
            if (DeBug)
            {
                Debug.Log($"<color=#a4e0fe>[FROSTBITE EFFECT]</color>\n" +
                         $"Power: {freezePower:F2}" + $"║ Duration: {freezeDuration:F2}s");
            }

            //ApplyFrostbite(freezePower);
            ApplyStatusEffect(StatusEffectType.Frostbite, freezeDuration);
            combatEffectEvents.OnFrostbite.Invoke();
            ApplyFrostbite(freezePower);
        }

        public void ApplyFreezeEffect(float freezePower, float freezeDuration)
        {
            if (DeBug)
            {
                Debug.Log($"<color=#00a8ff>[FREEZE EFFECT]</color>\n" +
                         $"Power: {freezePower:F2}" + $"║ Duration: {freezeDuration:F2}s");
            }

            //ApplyFreeze(freezePower);
            ApplyStatusEffect(StatusEffectType.Freeze, freezeDuration);
            combatEffectEvents.OnFreeze.Invoke();
            ApplyFreeze(freezePower);
        }

        public void ApplyElectroShortEffect(float duration)
        {
            if (DeBug)
            {
                Debug.Log($"<color=#feac20>[ELECTROSHORT EFFECT]</color>\n" +
                         $"║ Duration: {duration:F2}s");
            }

            //ApplyElectroShort();
            ApplyStatusEffect(StatusEffectType.ElectroShort, duration);
            ApplyElectroShort();
        }

        public void ApplyChainLightningEffect(float power)
        {
            if (DeBug)
            {
                Debug.Log($"<color=#feac20>[ChainLightning EFFECT]</color>\n" +
                         $"║ Power: {power:F2}s");
            }

            //ApplyChainLightning(power);
            ApplyStatusEffect(StatusEffectType.ChainLightning, 0.04f);
            ApplyChainLightning(power);
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

                case StatusEffectType.ElectroShort:
                    RemoveElectroShort();
                    break;
            }
        }

        #region ========= МЕХАНИКИ ЭФФЕКТОВ ==================

        private void ApplyFreeze(float power)
        {
            // Полная остановка движения
            if (agentController != null)
            {
                agentController.SetSpeedMultiplier(0);
            }

            // Визуальные эффекты ледяной блок
            //VFXCombatEffect.Instance.ApplyFreezeVFX(transform, power);
        }

        private void RemoveFreeze()
        {
            combatEffectEvents.OnFreezeComplete.Invoke();
            // Восстановление движения
            if (agentController != null)
            {
                agentController.SetSpeedMultiplier(1);
            }

            // Удаление визуальных эффектов
            //VFXCombatEffect.Instance.RemoveFreezeVFX(transform);
        }

        private void ApplyFrostbite(float power)
        {
            // Замедление движения
            if (agentController != null)
            {
                agentController.SetSpeedMultiplier(power);
            }

            // Визуальные эффекты обморожения
            //VFXCombatEffect.Instance.ApplyFrostbiteVFX(transform, power);
        }

        private void RemoveFrostbite()
        {
            combatEffectEvents.OnFrostbiteComplete.Invoke();
            // Восстановление скорости
            if (agentController != null)
            {
                agentController.SetSpeedMultiplier(1f);
            }

            // Удаление визуальных эффектов
            //VFXCombatEffect.Instance.RemoveFrostbiteVFX(transform);
        }

        private void ApplyElectroShort()
        {
            combatEffectEvents.OnElectroShort.Invoke();
            // Временная потеря контроля
            if (agentController != null)
            {
                // movementController.SetMovementEnabled(false);
            }

            // Визуальные эффекты оглушения
            //VFXCombatEffect.Instance.ApplyStunVFX(transform, power);
        }

        private void RemoveElectroShort()
        {
            combatEffectEvents.OnElectroShortComplete.Invoke();
            // Восстановление контроля
            if (agentController != null)
            {
                // movementController.SetMovementEnabled(true);
            }

            // Удаление визуальных эффектов
            //VFXCombatEffect.Instance.RemoveStunVFX(transform);
        }

        private void ApplyChainLightning(float power)
        {
            combatEffectEvents.OnChainLightning.Invoke();
            // Временная потеря контроля
            if (agentController != null)
            {
                // movementController.SetMovementEnabled(false);
            }

            // Визуальные эффекты оглушения
            //VFXCombatEffect.Instance.ApplyStunVFX(transform, power);
        }

        private void RemoveChainLightning()
        {
            combatEffectEvents.OnChainLightningComplete.Invoke();
            // Восстановление контроля
            if (agentController != null)
            {
                // movementController.SetMovementEnabled(true);
            }

            // Удаление визуальных эффектов
            //VFXCombatEffect.Instance.RemoveStunVFX(transform);
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
            return currentEffect == StatusEffectType.ElectroShort;
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

        public GameObject GetGameObject()
        {
            return gameObject;
        }

        #endregion
    }
}