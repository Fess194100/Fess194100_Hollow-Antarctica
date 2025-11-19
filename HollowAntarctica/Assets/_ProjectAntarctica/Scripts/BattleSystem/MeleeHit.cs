using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

namespace SimpleCharController
{
    public class MeleeHit : MonoBehaviour
    {
        #region Variables
        [Header("Links Components")]
        [SerializeField] private EssenceHealth ownerEssenceHealth;
        [SerializeField] private Collider weaponCollider;

        [Header("Settings")]
        public bool debugMode = false;

        [Space(10)]
        public LayerMask excludeLayersForPlayer;

        [Space(10)]
        public UnityEvent OnMeleeHit;
        #endregion

        #region Private Variables
        private bool _isAttackActive = false;
        private bool _isPlayer;
        private int _chargeLevel;
        private float _damage;
        private ProjectileType _projectileType;
        private HashSet<EssenceHealth> _alreadyHitTargets = new HashSet<EssenceHealth>();
        #endregion

        #region System Functions
        private void Awake()
        {
            SetWeaponCollider(false);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!_isAttackActive) return;
            
            ProcessHit(collision);
        }
        #endregion

        #region Private Methods

        private void ProcessHit(Collision collision)
        {
            if (!_isAttackActive) return;
            if (debugMode) Debug.Log($"Target - {collision.gameObject} //// name - {collision.gameObject.name}");

            HitBox targetHitBox = collision.gameObject.GetComponent<HitBox>();
            if (targetHitBox == null) return;

            EssenceHealth targetHealth = targetHitBox.EssencelHealth;
            if (targetHealth == null) return;

            if (targetHealth.IsDead())
            {
                if (debugMode) Debug.Log($"Target {targetHitBox.name} is already dead");
                return;
            }

            if (_alreadyHitTargets.Contains(targetHealth))
            {
                if (debugMode) Debug.Log($"Target {targetHealth.name} already hit in this attack");
                return;
            }

            if (ownerEssenceHealth != null)
            {
                if (targetHealth == ownerEssenceHealth)
                {
                    if (debugMode) Debug.Log("Cannot hit yourself");
                    return;
                }
            }            

            _alreadyHitTargets.Add(targetHealth);
            targetHitBox.TakeDamage(_damage, _projectileType, _chargeLevel, ownerEssenceHealth?.gameObject, _isPlayer, true);

            if (debugMode) Debug.Log($"Successfully hit {targetHealth.name} for {_damage} damage. Body part: {targetHitBox.BodyPart}");
        }
        #endregion

        #region Public API

        public void StartAttack(float damage, int chargeLevel = 0, ProjectileType projectileType = ProjectileType.Green)
        {
            _damage = damage;
            _chargeLevel = chargeLevel;
            _projectileType = projectileType;
            _isAttackActive = true;
            SetWeaponCollider(true);
            OnMeleeHit.Invoke();
            if (debugMode) Debug.Log($"Melee attack started: {damage} damage, charge level {chargeLevel}");
        }

        public void EndAttack()
        {
            _isAttackActive = false;
            SetWeaponCollider(false);
            ClearHitRecords();

            if (debugMode) Debug.Log("Melee attack ended");
        }

        public void SetWeaponCollider(bool enabled)
        {
            if (weaponCollider != null)
                weaponCollider.enabled = enabled;
        }

        public void ClearHitRecords()
        {
            _alreadyHitTargets.Clear();
        }
        #endregion
    }
}