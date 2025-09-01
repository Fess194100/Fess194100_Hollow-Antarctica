using SimpleCharController;
using UnityEngine;
using System;

[Serializable]
public class VFXCombatEffect
{
    public bool DeBug = true;

    public float durationVFX;

    public void PlayAreaEffectVFX(StatusEffectType effectType, float radius)
    {
        if (DeBug) Debug.Log($"PlayAreaEffectVFX. Type - {effectType}");
    }

    public void StopAreaEffectVFX()
    {
        if (DeBug) Debug.Log($"StopAreaEffectVFX");
    }
}
