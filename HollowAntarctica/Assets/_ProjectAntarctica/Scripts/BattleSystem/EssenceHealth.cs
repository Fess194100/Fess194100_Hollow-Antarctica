using UnityEngine;
using UnityEngine.Events;

namespace SimpleCharController
{
    public class EssenceHealth : MonoBehaviour, IDamageable
    {
        #region Variables
        public bool DeBug = false;

        [Header("References")]
        [SerializeField] private HandlerCombatEffects handlerCombatEffects;

        [Space(10)]
        [Header("Health Settings")]
        [SerializeField] private bool canTakeDamage = true;
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float currentHealth = 100f;

        [Space(5)]
        [SerializeField] private bool canTakeDamageFall = true;
        [SerializeField] private AnimationCurve damageAtTimeFalling;
        #endregion

        #region Private Variables
        private bool _isDead = false;
        #endregion

        #region Events
        [Header("Events")]
        public UnityEvent<float> OnHealthChanged; // Текущее здоровье
        public UnityEvent<float, BodyPart> OnDamageTaken;   // Количество полученного урона
        public UnityEvent<float> OnHealthRestored; // Количество восстановленного здоровья
        public UnityEvent OnDeath;
        public UnityEvent OnRespawn;
        #endregion

        #region Public Property
        public bool CanTakeDamage => canTakeDamage;
        public bool CanTakeDamageFall => canTakeDamageFall;
        #endregion

        #region System Functions
        private void Start()
        {
            currentHealth = maxHealth;
        }
        #endregion

        #region Private Methods
        private void Die()
        {
            if (_isDead) return;

            _isDead = true;
            currentHealth = 0f;
            OnHealthChanged?.Invoke(0f);
            OnDeath?.Invoke();

            // Здесь можно добавить логику смерти (анимация, отключение управления и т.д.)
            if (DeBug) Debug.Log("Player died!");
        }
        #endregion

        #region Deterministic function
        public bool IsDead() => _isDead;
        public float GetCurrentHealth() => currentHealth;
        public float GetMaxHealth() => maxHealth;
        public float GetHealthPercentage() => currentHealth / maxHealth;
        public HandlerCombatEffects GetCombatEffects() => handlerCombatEffects;
        #endregion

        #region Public API
        public void TakeDamage(float damage, ProjectileType projectileType, int chargeLevel, BodyPart bodyPart, GameObject sender, bool isPlayer, bool hitReaction)
        {
            if (!_isDead)
            {
                currentHealth -= damage;
                currentHealth = Mathf.Max(0, currentHealth);

                if (currentHealth <= 0.001f)
                {
                    Die();
                }
                else
                {
                    OnHealthChanged?.Invoke(currentHealth);
                    OnDamageTaken?.Invoke(damage, bodyPart);
                }

                if (DeBug) Debug.Log("Take damage = " + damage + " | Body part - " + bodyPart);
            }
        }

        public void TakeDamage(float damage, ProjectileType damageType, int chargeLevel, GameObject sender, bool isPlayer, bool hitReaction)
        {
            // Перегрузка для обратной совместимости
            TakeDamage(damage, damageType, chargeLevel, BodyPart.Body, sender, isPlayer, hitReaction);
        }

        public void TakeDamageFall(float timeFallin)
        {
            if (DeBug)Debug.Log($"Время падения = {timeFallin}");
            float damage = damageAtTimeFalling.Evaluate(timeFallin);
            if (damage > 0.1f && canTakeDamageFall) TakeDamage(damage, ProjectileType.Green, 0, BodyPart.Body, null, true, true);
        }

        public void RestoreHealth(float healAmount)
        {
            if (_isDead) return;

            currentHealth += healAmount;
            currentHealth = Mathf.Min(currentHealth, maxHealth);

            OnHealthChanged?.Invoke(currentHealth);
            OnHealthRestored?.Invoke(healAmount);
        }

        public void SetHealth(float health)
        {
            if (_isDead) return;

            currentHealth = Mathf.Clamp(health, 0f, maxHealth);
            OnHealthChanged?.Invoke(currentHealth);

            if (currentHealth <= 0f)
            {
                Die();
            }
        }

        public void SetMaxHealth(float maximusHealth)
        {
            maxHealth = Mathf.Clamp(maximusHealth, 0f, maximusHealth);
        }

        public void Respawn()
        {
            _isDead = false;
            currentHealth = maxHealth;
            OnHealthChanged?.Invoke(currentHealth);
            OnRespawn?.Invoke();
        }

        public void AddHealth(float countHealth)
        {
            SetHealth(countHealth + currentHealth);
        }
        #endregion
    }
}