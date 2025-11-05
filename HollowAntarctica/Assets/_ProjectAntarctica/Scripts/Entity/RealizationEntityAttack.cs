using SimpleCharController;
using AdaptivEntityAgent;
using UnityEngine;

public class RealizationEntityAttack : MonoBehaviour
{
    #region Variables
    [Header("References")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private EssenceHealth ownerEssenceHealth;
    [SerializeField] private AgentPerception perception;
    [SerializeField] private MeleeHit meleeHit;

    [Space(15)]
    [Header("Weapon Settings")]
    [SerializeField] private WeaponProjectileData projectileData;
    [SerializeField] private WeaponProjectileData melleData;

    #endregion

    #region Private Variables
    private GameObject targetObject;
    #endregion

    #region Private Function

    private void SpawnProjectile(ProjectileType projectileType ,GameObject projectilePrefab, float speed, float damage, int chargeLevel, Vector3 directionOffset, TypeMovement typeMovement)
    {
        Vector3 shootDirection = Vector3.forward;
        if (targetObject != null) shootDirection = (targetObject.transform.position + Vector3.up - firePoint.position).normalized;

        Quaternion projectileRotation = Quaternion.LookRotation(shootDirection);

        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, projectileRotation);
        Projectile projectileScript = projectile.GetComponent<Projectile>();

        if (projectileScript != null)
        {
            projectileScript.Initialize(speed, gameObject, ownerEssenceHealth, projectileType, chargeLevel, damage, typeMovement, false, false);
        }
    }

    private void MelleAttack(bool enable)
    {
        if (meleeHit == null || melleData == null) return;

        if (enable)
        {
            meleeHit.StartAttack(melleData.baseDamageStandard, 0, melleData.Type);
        }
        else meleeHit.EndAttack();
    }
    #endregion

    #region Public API
    public void StartMelleAttack() => MelleAttack(true);
    public void EndMelleAttack() => MelleAttack(false);

    public void RangedAttack()
    {
        if (perception != null) targetObject = perception.CurrentTarget;

        if (projectileData != null && firePoint != null)
        {
            SpawnProjectile(projectileData.Type ,projectileData.StandardProjectilePrefab, projectileData.StandardProjectileSpeed, 
                            projectileData.baseDamageStandard, 0, Vector3.zero, TypeMovement.Linear);
        }
    }
    #endregion
}
