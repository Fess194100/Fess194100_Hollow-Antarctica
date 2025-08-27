using SimpleCharController;
using UnityEngine;

[System.Serializable]
public class BurstSettings
{
    public int projectilesCount = 3;
    public float spreadAngle = 5f; // Угол разброса в градусах
}

[System.Serializable]
public class SpreadSettings
{
    public int projectilesCount = 5;
    public float spreadAngle = 15f; // Угол разброса в градусах
    public float delayBetweenShots = 0.1f; // Задержка между выстрелами
}

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
    public BurstSettings standardBurstSettings; // Настройки для режима Burst
    public SpreadSettings standardSpreadSettings; // Настройки для режима Spread

    [Header("Charged Shot - Level 0")]
    public GameObject ChargedLvl0ProjectilePrefab;
    public TypeShooting typeShootingLvl0 = TypeShooting.Burst;
    public int ChargedLvl0AmmoCost = 2;
    public float ChargedLvl0ProjectileSpeed;
    public float Lvl0FireRate = 2.5f;
    public float baseDamageLvl0;
    public BurstSettings lvl0BurstSettings; // Настройки для режима Burst
    public SpreadSettings lvl0SpreadSettings; // Настройки для режима Spread

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
    // Здесь можно добавить поле для эффекта (например, enum EffectType)

    [Header("Charged Shot - Level 3")]
    public GameObject ChargedLvl3ProjectilePrefab;
    public TypeMovement typeMovementLvl3 = TypeMovement.Parabular;
    public int ChargedLvl3AmmoCost = 8;
    public float ChargedLvl3ProjectileSpeed;
    public float baseDamageLvl3;
    // Эффект для уровня 3

    [Header("Overheat")]
    public float OverheatDamageToPlayer = 10f;
    public float OverheatDuration = 0.5f;
}