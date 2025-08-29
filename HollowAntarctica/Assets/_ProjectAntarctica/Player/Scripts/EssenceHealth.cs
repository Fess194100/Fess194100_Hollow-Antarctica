using UnityEngine;
using UnityEngine.Events;

namespace SimpleCharController
{
    public class EssenceHealth : MonoBehaviour, IDamageable
    {
        [Header("Health Settings")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float currentHealth = 100f;

        [Header("Events")]
        public UnityEvent<float> OnHealthChanged; // Текущее здоровье
        public UnityEvent<float> OnDamageTaken;   // Количество полученного урона
        public UnityEvent<float> OnHealthRestored; // Количество восстановленного здоровья
        public UnityEvent OnDeath;

        private bool _isDead = false;

        private void Start()
        {
            currentHealth = maxHealth;
            OnHealthChanged?.Invoke(currentHealth);
        }

        // Нанесение урона игроку
        public void TakeDamage(float damage, ProjectileType projectileType, int chargeLevel)
        {
            if (_isDead) return;

            currentHealth -= damage;
            currentHealth = Mathf.Max(0, currentHealth);

            OnHealthChanged?.Invoke(currentHealth);
            OnDamageTaken?.Invoke(damage);

            if (currentHealth <= 0f)
            {
                Die();
            }
        }

        // Восстановление здоровья игроку
        public void RestoreHealth(float healAmount)
        {
            if (_isDead) return;

            currentHealth += healAmount;
            currentHealth = Mathf.Min(currentHealth, maxHealth);

            OnHealthChanged?.Invoke(currentHealth);
            OnHealthRestored?.Invoke(healAmount);
        }

        // Установка здоровья напрямую
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

        // Смерть игрока
        private void Die()
        {
            if (_isDead) return;

            _isDead = true;
            currentHealth = 0f;
            OnHealthChanged?.Invoke(0f);
            OnDeath?.Invoke();

            // Здесь можно добавить логику смерти (анимация, отключение управления и т.д.)
            Debug.Log("Player died!");
        }

        // Воскрешение игрока
        public void Respawn()
        {
            _isDead = false;
            currentHealth = maxHealth;
            OnHealthChanged?.Invoke(currentHealth);
        }

        // Проверка, мертв ли игрок
        public bool IsDead()
        {
            return _isDead;
        }

        // Получение текущего здоровья
        public float GetCurrentHealth()
        {
            return currentHealth;
        }

        // Получение максимального здоровья
        public float GetMaxHealth()
        {
            return maxHealth;
        }

        // Получение процента здоровья (0-1)
        public float GetHealthPercentage()
        {
            return currentHealth / maxHealth;
        }
    }
}