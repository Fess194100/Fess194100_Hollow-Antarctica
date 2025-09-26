namespace SimpleCharController
{
    public interface IDamageable
    {
        void TakeDamage(float damage, ProjectileType damageType, int chargeLevel);
        bool IsDead();
        HandlerCombatEffects GetCombatEffects();
    }
}