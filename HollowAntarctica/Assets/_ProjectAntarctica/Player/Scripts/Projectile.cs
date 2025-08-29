using UnityEngine;

namespace SimpleCharController
{
    public class Projectile : MonoBehaviour
    {
        [Header("Links Components")]
        public new Rigidbody rigidbody;

        [Header("Setting Projectile")]
        public AnimationCurve curveSpeed = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 0));
        
        public float freezeDuration;
        public float freezePower;
        public float healAmount;

        [Header("Visual Effects")]
        [SerializeField] private GameObject impactVFX;

        
        private GameObject _owner;
        private TypeMovement _typeMovement;
        private ProjectileType _type;
        private float _speed;
        private int _chargeLevel;
        private float _damage;
        private float _lifeTime;
        private float _dieTime;
        private float _speedMultiplier = 1f;
        public void Initialize(float speed, GameObject owner, ProjectileType projectileType, int chargeLevel, float damage, TypeMovement typeMovement)
        {
            _speed = speed;
            _owner = owner;
            _chargeLevel = chargeLevel;
            _damage = damage;
            _typeMovement = typeMovement;
            _type = projectileType;
            _dieTime = Mathf.Clamp(180f / _speed, 1f, 10f);
        }

        private void FixedUpdate()
        {
            _lifeTime += Time.fixedDeltaTime;
            _speedMultiplier = curveSpeed.Evaluate(_lifeTime);

            if (_lifeTime > _dieTime)
            {
                DestroyProjectile();
            }
            switch (_typeMovement)
            {
                case TypeMovement.Linear:
                    rigidbody.velocity = transform.forward * (_speed * _speedMultiplier);
                    break;
                case TypeMovement.Parabular:
                    rigidbody.AddForce(transform.forward * (_speed * _speedMultiplier), ForceMode.Impulse);
                    break;
            }
        }
        private void OnCollisionEnter(Collision collision)
        {
            // 1. Не взаимодействуем с владельцем
            if (collision.gameObject == _owner) return;

            // 2. Наносим урон, если объект может его получить
            IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(_damage, _type, _chargeLevel);

                // 3. Применяем эффекты (например, вампиризм проверяем здесь)
                if (_type == ProjectileType.Green && _chargeLevel >= 2)
                {
                    bool wasKilled = damageable.IsDead();
                    if (wasKilled)
                    {
                        // Восстанавливаем здоровье владельцу
                        _owner.GetComponent<EssenceHealth>().RestoreHealth(healAmount);
                    }
                }
            }
            else //Debug.LogWarning("IDamageable damageable = null" + collision.gameObject);

            // 4. Для синего снаряда - заморозка (применим эффект к цели)
            if (_type == ProjectileType.Blue)
            {
                IStatusEffectTarget effectTarget = collision.gameObject.GetComponent<IStatusEffectTarget>();
                if (effectTarget != null)
                {
                    effectTarget.ApplyFreezeEffect(freezePower, freezeDuration);
                }
            }

            // 5. Спавним VFX взрыва/попадания
            Instantiate(impactVFX, transform.position, transform.rotation);
            DestroyProjectile();
        }

        private void DestroyProjectile()
        {
            Destroy(gameObject);
        }
    }
}
