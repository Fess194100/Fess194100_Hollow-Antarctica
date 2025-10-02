using System.Collections;
using UnityEngine;

namespace SimpleCharController
{
    public class Projectile : MonoBehaviour
    {
        [Header("Links Components")]
        public new Rigidbody rigidbody;
        public CapsuleCollider capsuleCollider;

        [Header("Setting Projectile")]
        public AnimationCurve curveSpeed = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 0));
        public AnimationCurve curveGravity = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 0));
        [SerializeField] private float colliderUpdateDelay = 0.04f;


        [Header("Setting Effects")]
        public bool useEffect;
        public float effectDuration;
        public float effectPower;

        [Header("Visual Effects")]
        [SerializeField] private GameObject impactVFX;
        [SerializeField] private GameObject prefabCombatEffect;

        #region private Value
        
        private GameObject _owner;
        private EssenceHealth _ownerEssenceHealth;
        private HandlerCombatEffects _handlerCombatEffects;
        private TypeMovement _typeMovement;
        private ProjectileType _type;
        private bool _collisionProcessed = false;
        private int _chargeLevel;
        private float _speed;
        private float _damage;
        private float _lifeTime;
        private float _dieTime;
        private float _speedMultiplier = 1f;
        private Vector3 previsionPosition;
        private Vector3 _gravity;
        private Quaternion previsionRotation;
        #endregion
        public void Initialize(float speed, GameObject owner, EssenceHealth essenceHealth, ProjectileType projectileType, int chargeLevel, float damage, TypeMovement typeMovement)
        {
            _collisionProcessed = false;
            _speed = speed;
            _owner = owner;
            _ownerEssenceHealth = essenceHealth;
            _chargeLevel = chargeLevel;
            _damage = damage;
            _typeMovement = typeMovement;
            _type = projectileType;
            _dieTime = Mathf.Clamp(180f / _speed, 1f, 10f);
            _gravity = Vector3.down * rigidbody.mass;
            if (_typeMovement != TypeMovement.Parabular) StartCoroutine(UpdateCollider(1f));
            else StartCoroutine(UpdateCollider(2.75f));
        }

        private void FixedUpdate()
        {
            _lifeTime += Time.fixedDeltaTime;
            _speedMultiplier = curveSpeed.Evaluate(_lifeTime);
            float gravityMyltiplie = curveGravity.Evaluate(_lifeTime);

            if (_lifeTime > _dieTime)
            {
                DestroyProjectile();
            }
            switch (_typeMovement)
            {
                case TypeMovement.Linear:
                    rigidbody.velocity = transform.forward * (_speed * _speedMultiplier) + _gravity * gravityMyltiplie;
                    break;
                case TypeMovement.Parabular:
                    rigidbody.AddForce(transform.forward * (_speed * _speedMultiplier), ForceMode.Impulse);
                    break;
            }

            previsionPosition = transform.position;
            previsionRotation = transform.rotation;
        }
        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject == _owner || _collisionProcessed) return;

            IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();

            if (damageable != null)
            {
                damageable.TakeDamage(_damage, _type, _chargeLevel);
                bool wasKilled = damageable.IsDead();
                _handlerCombatEffects = damageable.GetCombatEffects();

                //Применяем эффекты
                if (_handlerCombatEffects != null && useEffect && _chargeLevel >= 0)
                {
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

            if (impactVFX != null) Instantiate(impactVFX, previsionPosition, previsionRotation);
            _collisionProcessed = true;
            DestroyProjectile();
        }

        private IEnumerator UpdateCollider(float miltiplier)
        {
            capsuleCollider.height = 0f;
            capsuleCollider.center = Vector3.zero;

            yield return new WaitForSeconds(colliderUpdateDelay);

            if (capsuleCollider != null)
            {
                float heightCollider = _speed * miltiplier * Time.fixedDeltaTime;
                float offsetZ = -((heightCollider / 2) - capsuleCollider.radius);
                capsuleCollider.height = heightCollider;
                capsuleCollider.center = new Vector3 (0, 0, offsetZ);
            }
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
            switch (_chargeLevel)
            {
                case 0:
                    _handlerCombatEffects.ApplyFrostbiteEffect(effectPower, effectDuration);
                    break;

                case 1:
                    _handlerCombatEffects.ApplyFrostbiteEffect(effectPower, effectDuration);
                    break;

                case 2:
                    _handlerCombatEffects.ApplyFreezeEffect(effectPower, effectDuration);
                    if (wasKilled) CreateAreaEffect();
                    break;

                case 3:
                    _handlerCombatEffects.ApplyFreezeEffect(effectPower, effectDuration);
                    if (wasKilled) CreateAreaEffect();
                    break;
            }
        }

        private void EffectOrange(bool wasKilled)
        {
            switch (_chargeLevel)
            {
                case 0:
                    _handlerCombatEffects.ApplyElectroShortEffect(effectDuration);
                    break;

                case 1:
                    _handlerCombatEffects.ApplyElectroShortEffect(effectDuration);
                    break;

                case 2:
                    if (!wasKilled) _handlerCombatEffects.ApplyElectroShortEffect(effectDuration);
                    else CreateAreaEffect();
                    break;

                case 3:
                    if (!wasKilled) _handlerCombatEffects.ApplyElectroShortEffect(effectDuration);
                    else CreateAreaEffect();
                    break;
            }
        }

        private void CreateAreaEffect()
        {
            if (prefabCombatEffect != null)
            {
                GameObject areaEffect = Instantiate(prefabCombatEffect, transform.position, Quaternion.identity);
                AreaCombatEffect areaScript = areaEffect.GetComponent<AreaCombatEffect>();

                if (areaScript != null)
                {
                    areaScript.Initialize(_owner, _ownerEssenceHealth, _handlerCombatEffects);
                }
            }
            else Debug.LogWarning("Missing prefabAreaCombatEffect");
        }
    }
}
