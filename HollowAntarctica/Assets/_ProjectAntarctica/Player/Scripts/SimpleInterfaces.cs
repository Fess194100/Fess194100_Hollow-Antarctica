namespace SimpleCharController
{
    public interface IDamageable
    {
        void TakeDamage(float damage, ProjectileType damageType, int chargeLevel);
        bool IsDead();
        HandlerCombatEffects GetCombatEffects();
    }

    /*public interface IStatusEffectTarget
    {
        void ApplyFreezeEffect(float freezePower, float freezeDuration, float freezeRadius, bool wasKilled, Vector3 positionArea);
        void ApplyStunEffect(float duration);
        // ... другие эффекты
    }*/
}