using UnityEngine;

namespace SimpleCharController
{
    public class HitBox : MonoBehaviour, IDamageable
    {
        [Header("Hit Box Settings")]
        [SerializeField] private EssenceHealth parentHealth;
        [SerializeField] private HandlerCombatEffects parentHandlerEffects;
        [SerializeField] private BodyPart bodyPart = BodyPart.Body;
        [SerializeField] private float damageMultiplier = 1f;
        [SerializeField] public bool isAffectedByAreaEffects;

        private void Awake()
        {
            if (parentHealth == null)
            {
                parentHealth = GetComponentInParent<EssenceHealth>();
            }

            if (parentHandlerEffects == null)
            {
                parentHandlerEffects = GetComponentInParent<HandlerCombatEffects>();
            }
        }

        public void TakeDamage(float damage, ProjectileType damageType, int chargeLevel)
        {
            if (parentHealth != null)
            {
                float finalDamage = damage * damageMultiplier;
                parentHealth.TakeDamage(finalDamage, damageType, chargeLevel, bodyPart);
            }
        }

        public bool IsDead()
        {
            return parentHealth != null && parentHealth.IsDead();
        }

        public HandlerCombatEffects GetCombatEffects() { return parentHandlerEffects; }

        public EssenceHealth GetEssenceHealth() { return parentHealth; }

        // Для быстрой настройки в инспекторе
        public void SetParentHealth(EssenceHealth health) => parentHealth = health;
        public void SetDamageMultiplier(float multiplier) => damageMultiplier = multiplier;
        public void SetBodyPart(BodyPart part) => bodyPart = part;
    }
}