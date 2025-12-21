using UnityEngine;
using UnityEngine.Events;

namespace SimpleCharController
{
    public class HitBox : MonoBehaviour, IDamageable
    {
        [Header("Hit Box Settings")]
        [SerializeField] private EssenceHealth parentHealth;
        [SerializeField] private HandlerCombatEffects parentHandlerEffects;
        [SerializeField] private BodyPart bodyPart = BodyPart.Body;
        [SerializeField] private float damageMultiplier = 1f;
        [Tooltip("A hitbox for interacting with the effects area. One per character!")]
        [SerializeField] public bool isAffectedByAreaEffects;

        [Space(10)]
        [Header("Switch Layer")]
        [SerializeField] private bool canSwitchLayer = true;
        [SerializeField] private LayerMask defaultLayer;
        [SerializeField] private LayerMask deathLayer;

        [Space(10)]
        [Header("Events")]
        public UnityEvent OnHitImpact;

        #region Public Property
        public BodyPart BodyPart => bodyPart;
        public EssenceHealth EssencelHealth => parentHealth;
        #endregion

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

            if (parentHealth != null)
            {
                parentHealth.OnDeath.AddListener(() => SwitchLayer(deathLayer));
                parentHealth.OnRespawn.AddListener(() => SwitchLayer(defaultLayer));
            }
        }

        private void OnDestroy()
        {
            if (parentHealth != null)
            {
                parentHealth.OnDeath.RemoveListener(() => SwitchLayer(deathLayer));
                parentHealth.OnRespawn.RemoveListener(() => SwitchLayer(defaultLayer));
            }
        }
        private void SwitchLayer(LayerMask layerMask)
        {
            if (!canSwitchLayer) return;
            gameObject.layer = LayerMaskToInt(layerMask);
        }

        private int LayerMaskToInt(LayerMask layerMask)
        {
            for (int i = 0; i < 32; i++)
                if ((layerMask.value & (1 << i)) != 0)
                    return i;
            return 0;
        }

        public void TakeDamage(float damage, ProjectileType damageType, int chargeLevel, GameObject sender, bool isPlayer, bool hitReaction)
        {
            if (parentHealth != null)
            {
                float finalDamage = damage * damageMultiplier;
                parentHealth.TakeDamage(finalDamage, damageType, chargeLevel, bodyPart, sender, isPlayer, hitReaction);

            }

            OnHitImpact?.Invoke();
        }

        public bool IsDead()
        {
            return parentHealth != null && parentHealth.IsDead();
        }

        public HandlerCombatEffects GetCombatEffects() { return parentHandlerEffects; }
    }
}