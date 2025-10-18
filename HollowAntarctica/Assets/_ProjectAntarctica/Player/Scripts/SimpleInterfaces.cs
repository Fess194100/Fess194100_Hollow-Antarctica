using UnityEngine;

namespace SimpleCharController
{
    public interface IDamageable
    {
        void TakeDamage(float damage, ProjectileType damageType, int chargeLevel, GameObject sender, bool isPlayer, bool hitReaction);
        bool IsDead();
        HandlerCombatEffects GetCombatEffects();
    }
}