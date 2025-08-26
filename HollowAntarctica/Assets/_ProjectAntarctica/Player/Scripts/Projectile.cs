using UnityEngine;

namespace SimpleCharController
{
    public class Projectile : MonoBehaviour
    {
        [Header("Links Components")]
        public new Rigidbody rigidbody;


        [Header("Setting Projectile")]
        public int Damage;
        public ProjectileType Type;
        
        public float freezeDuration;
        public float freezePower;
        public float healAmount;

        [Header("Visual Effects")]
        [SerializeField] private GameObject impactVFX; // Префаб с эффектом попадания
        [SerializeField] private GameObject trailVFX; // Префаб с эффектом следа

        
        private GameObject _owner; // Кто выпустил снаряд (игрок)
        private float _speed;
        private int _chargeLevel;

        public void Initialize(float speed, GameObject owner, ProjectileType projectileType, int chargeLevel)
        {
            _speed = speed;
            _owner = owner;
            _chargeLevel = chargeLevel;
            rigidbody.velocity = transform.forward * _speed;

            /*// Спавним VFX следа, если он есть
            if (trailVFX != null)
            {
                Instantiate(trailVFX, transform.position, transform.rotation, transform);
            }*/
        }

        private void FixedUpdate()
        {
            rigidbody.velocity = transform.forward * _speed;
        }
        private void OnCollisionEnter(Collision collision)
        {
            // 1. Не взаимодействуем с владельцем
            if (collision.gameObject == _owner) return;

            // 2. Наносим урон, если объект может его получить
            IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(Damage, Type, _chargeLevel);

                // 3. Применяем эффекты (например, вампиризм проверяем здесь)
                if (Type == ProjectileType.Green && _chargeLevel >= 2)
                {
                    bool wasKilled = damageable.IsDead();
                    if (wasKilled)
                    {
                        // Восстанавливаем здоровье владельцу
                        _owner.GetComponent<PlayerHealth>().RestoreHealth(healAmount);
                    }
                }
            }
            else //Debug.LogWarning("IDamageable damageable = null" + collision.gameObject);

            // 4. Для синего снаряда - заморозка (применим эффект к цели)
            if (Type == ProjectileType.Blue)
            {
                IStatusEffectTarget effectTarget = collision.gameObject.GetComponent<IStatusEffectTarget>();
                if (effectTarget != null)
                {
                    effectTarget.ApplyFreezeEffect(freezePower, freezeDuration);
                }
            }

            // 5. Спавним VFX взрыва/попадания
            Instantiate(impactVFX, transform.position, transform.rotation);

            // 6. Уничтожаем снаряд
            Destroy(gameObject);
        }
    }
}
