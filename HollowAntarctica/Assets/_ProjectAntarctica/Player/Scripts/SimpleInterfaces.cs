namespace SimpleCharController
{
    public interface IDamageable
    {
        void TakeDamage(float damage, ProjectileType damageType, int chargeLevel);
        bool IsDead();
    }

    public interface IStatusEffectTarget
    {
        void ApplyFreezeEffect(float freezePower, float freezeDuration);
        void ApplyStunEffect(float duration);
        // ... другие эффекты
    }
}