using UnityEngine;

namespace SimpleCharController
{
    public class Projectile : MonoBehaviour
    {
        [Header("Links Components")]
        public new Rigidbody rigidbody;

        [Header("Setting Projectile")]
        public AnimationCurve curveSpeed = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 0));

        [Header("Setting Effects")]
        public bool useEffect;
        public float effectDuration;
        public float effectPower;
        public float effectRadius;

        [Header("Visual Effects")]
        [SerializeField] private GameObject impactVFX;

        
        private GameObject _owner;
        private EssenceHealth _ownerEssenceHealth;
        private HandlerCombatEffects _handlerCombatEffects;
        private TypeMovement _typeMovement;
        private ProjectileType _type;
        private float _speed;
        private int _chargeLevel;
        private float _damage;
        private float _lifeTime;
        private float _dieTime;
        private float _speedMultiplier = 1f;

        public void Initialize(float speed, GameObject owner, EssenceHealth essenceHealth, ProjectileType projectileType, int chargeLevel, float damage, TypeMovement typeMovement)
        {
            _speed = speed;
            _owner = owner;
            _ownerEssenceHealth = essenceHealth;
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
            if (collision.gameObject == _owner) return;

            IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();

            if (damageable != null)
            {
                damageable.TakeDamage(_damage, _type, _chargeLevel);
                bool wasKilled = damageable.IsDead();

                //Применяем эффекты
                if (useEffect && _chargeLevel >= 0)
                {
                    _handlerCombatEffects = damageable.GetCombatEffects();
                    switch (_type)
                    {
                        case ProjectileType.Green: EffectGreen(wasKilled); break;
                        case ProjectileType.Blue: EffectBlue(wasKilled); break;
                        case ProjectileType.Orange: EffectOrange(wasKilled); break;
                    }
                }
            }
            else
            {
                //Столкновение снаряда не с сущностью (Стена, колонна, пол, потолок...)
            }

            Instantiate(impactVFX, transform.position, transform.rotation);
            DestroyProjectile();
        }

        private void DestroyProjectile()
        {
            Destroy(gameObject);
        }

        private void EffectGreen(bool wasKilled)
        {
            switch (_chargeLevel) // для добавления разных механик. Если не добавлять, то switch убрать .
            {
                case 0:
                    break;
                case 1:
                    break;
                case 2:
                    break;
                case 3:
                    break;
            }

            if (wasKilled) _ownerEssenceHealth.RestoreHealth(effectPower);
        }

        private void EffectBlue(bool wasKilled)
        {
            if (_handlerCombatEffects == null) return;

            switch (_chargeLevel)
            {
                case 0:
                    _handlerCombatEffects.ApplyFrostbiteEffect(effectPower, effectDuration);
                    break;

                case 1:
                    _handlerCombatEffects.ApplyFrostbiteEffect(effectPower, effectDuration);
                    break;

                case 2:
                    _handlerCombatEffects.ApplyFreezeEffect(effectPower, effectDuration, effectRadius, wasKilled, transform.position);
                    break;

                case 3: // Заряженный 3 уровень - полная заморозка + взрыв при убийстве
                    _handlerCombatEffects.ApplyFreezeEffect(effectPower, effectDuration, effectRadius, wasKilled, transform.position);
                    break;
            }
        }

        private void EffectOrange(bool wasKilled)
        {
            // Заглушка для оранжевых снарядов - будет реализовано позже
            if (_handlerCombatEffects != null)
            {
                // Базовый эффект оглушения для оранжевых снарядов
                if (_chargeLevel >= 2)
                {
                    _handlerCombatEffects.ApplyStunEffect(effectDuration);
                }
            }
        }
    }
}
