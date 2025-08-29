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
        public UnityEvent<float> OnHealthChanged; // ������� ��������
        public UnityEvent<float> OnDamageTaken;   // ���������� ����������� �����
        public UnityEvent<float> OnHealthRestored; // ���������� ���������������� ��������
        public UnityEvent OnDeath;

        private bool _isDead = false;

        private void Start()
        {
            currentHealth = maxHealth;
            OnHealthChanged?.Invoke(currentHealth);
        }

        // ��������� ����� ������
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

        // �������������� �������� ������
        public void RestoreHealth(float healAmount)
        {
            if (_isDead) return;

            currentHealth += healAmount;
            currentHealth = Mathf.Min(currentHealth, maxHealth);

            OnHealthChanged?.Invoke(currentHealth);
            OnHealthRestored?.Invoke(healAmount);
        }

        // ��������� �������� ��������
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

        // ������ ������
        private void Die()
        {
            if (_isDead) return;

            _isDead = true;
            currentHealth = 0f;
            OnHealthChanged?.Invoke(0f);
            OnDeath?.Invoke();

            // ����� ����� �������� ������ ������ (��������, ���������� ���������� � �.�.)
            Debug.Log("Player died!");
        }

        // ����������� ������
        public void Respawn()
        {
            _isDead = false;
            currentHealth = maxHealth;
            OnHealthChanged?.Invoke(currentHealth);
        }

        // ��������, ����� �� �����
        public bool IsDead()
        {
            return _isDead;
        }

        // ��������� �������� ��������
        public float GetCurrentHealth()
        {
            return currentHealth;
        }

        // ��������� ������������� ��������
        public float GetMaxHealth()
        {
            return maxHealth;
        }

        // ��������� �������� �������� (0-1)
        public float GetHealthPercentage()
        {
            return currentHealth / maxHealth;
        }
    }
}