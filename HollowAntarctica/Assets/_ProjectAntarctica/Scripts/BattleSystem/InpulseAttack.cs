using UnityEngine;
using UnityEngine.Events;

public class InpulseAttack : MonoBehaviour
{
    [SerializeField] private AnimationCurve forseAtMeleeLevel;
    [SerializeField] private AnimationCurve forseAtRangedLevel;

    [Space(10)]
    public UnityEvent<float> impulseMelee;
    public UnityEvent<float> impulseRanged;

    public void ImpulseMeleeAttack(int chargedLevel)
    {
        float force = forseAtMeleeLevel.Evaluate((float)chargedLevel);
        impulseMelee.Invoke(force);
    }

    public void ImpulseRangedAttack(int chargedLevel)
    {
        float force = forseAtRangedLevel.Evaluate((float)chargedLevel);
        impulseRanged.Invoke(force);
    }
}
