using SimpleCharController;
using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class VFXCombatEffect
{
    public bool DeBug = false;

    public List<GameObject> targets = new List<GameObject>();
    public float durationVFX;

    public VFXChainLightningController vFXChainLightningController;
    public void PlayAreaEffectVFX(StatusEffectType effectType, float radius)
    {
        if (DeBug) Debug.Log($"PlayAreaEffectVFX. Type - {effectType}");
    }

    public void StopAreaEffectVFX()
    {
        if (DeBug) Debug.Log($"StopAreaEffectVFX");
    }

    public void PlayAreaEffectVFX(StatusEffectType effectType, List<GameObject> targetsEffect)
    {
        if (DeBug) Debug.Log($"PlayAreaEffectVFX_2. Type - {effectType}, collires - {targets}");

        targets = targetsEffect;

        if (vFXChainLightningController != null) vFXChainLightningController.InitializeChainLightning(targetsEffect);
    }
}
