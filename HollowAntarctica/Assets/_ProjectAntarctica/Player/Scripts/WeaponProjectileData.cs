using SimpleCharController;
using UnityEngine;

[CreateAssetMenu(fileName = "Data_Projectile", menuName = "Weapon/Projectile Data")]
public class WeaponProjectileData : ScriptableObject
{
    public ProjectileType Type;

    [Header("Standard Shot")]
    public GameObject StandardProjectilePrefab;
    public TypeShooting typeShootingStandard = TypeShooting.Single;
    public int StandardAmmoCost = 1;
    public float StandardProjectileSpeed;
    public float StandardFireRate = 5.0f;
    public float baseDamageStandard;

    [Header("Charged Shot - Level 0")]
    public GameObject ChargedLvl0ProjectilePrefab;
    public TypeShooting typeShootingLvl0 = TypeShooting.Burst;
    public int ChargedLvl0AmmoCost = 2;
    public float ChargedLvl0ProjectileSpeed;
    public float baseDamageLvl0;

    [Header("Charged Shot - Level 1")]
    public GameObject ChargedLvl1ProjectilePrefab;
    public TypeMovement typeMovementLvl1 = TypeMovement.Linear;
    public int ChargedLvl1AmmoCost = 2;
    public float ChargedLvl1ProjectileSpeed;
    public float baseDamageLvl1;

    [Header("Charged Shot - Level 2")]
    public GameObject ChargedLvl2ProjectilePrefab;
    public TypeMovement typeMovementLvl2 = TypeMovement.Parabular;
    public int ChargedLvl2AmmoCost = 4;
    public float ChargedLvl2ProjectileSpeed;
    public float baseDamageLvl2;
    // «десь можно добавить поле дл€ эффекта (например, enum EffectType)

    [Header("Charged Shot - Level 3")]
    public GameObject ChargedLvl3ProjectilePrefab;
    public TypeMovement typeMovementLvl3 = TypeMovement.Parabular;
    public int ChargedLvl3AmmoCost = 8;
    public float ChargedLvl3ProjectileSpeed;
    public float baseDamageLvl3;
    // Ёффект дл€ уровн€ 3

    [Header("Overheat")]
    public float OverheatDamageToPlayer = 10f;
    public float OverheatDuration = 0.5f;
}