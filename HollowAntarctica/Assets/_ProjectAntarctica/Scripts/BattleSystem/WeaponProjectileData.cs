using SimpleCharController;
using UnityEngine;

[System.Serializable]
public class SpreadWeaponSettings
{
    public int projectilesCount = 5;
    public float spreadAngle = 15f;
    public float delayBetweenShots = 0.1f;
}

[CreateAssetMenu(fileName = "Data_Projectile", menuName = "Weapon/Projectile Data")]
public class WeaponProjectileData : ScriptableObject
{
    public ProjectileType Type;

    [Header("Standard Shot")]
    public GameObject StandardProjectilePrefab;
    public AudioClip soundShot_Standart;
    public int StandardAmmoCost = 1;
    public float StandardProjectileSpeed;
    public float StandardFireRate = 5.0f;
    public float baseDamageStandard;
    public Vector2 pitchSound_Standart = Vector2.one;

    [Space(10)] public TypeShooting typeShootingStandard = TypeShooting.Single;
    public SpreadWeaponSettings standardSpreadSettings;

    [Header("Charged Shot - Level 0")]
    public GameObject ChargedLvl0ProjectilePrefab;
    public AudioClip soundShot_0;
    public int ChargedLvl0AmmoCost = 2;
    public float ChargedLvl0ProjectileSpeed;
    public float Lvl0FireRate = 2.5f;
    public float baseDamageLvl0;
    public Vector2 pitchSound_0 = Vector2.one;
    [Space(10)] public TypeShooting typeShootingLvl0 = TypeShooting.Burst;
    public SpreadWeaponSettings lvl0SpreadSettings;

    [Header("Charged Shot - Level 1")]
    public GameObject ChargedLvl1ProjectilePrefab;
    public AudioClip soundShot_1;
    public TypeMovement typeMovementLvl1 = TypeMovement.Linear;
    public int ChargedLvl1AmmoCost = 2;
    public float ChargedLvl1ProjectileSpeed;
    public float baseDamageLvl1;
    public Vector2 pitchSound_1 = Vector2.one;

    [Header("Charged Shot - Level 2")]
    public GameObject ChargedLvl2ProjectilePrefab;
    public AudioClip soundShot_2;
    public TypeMovement typeMovementLvl2 = TypeMovement.Parabular;
    public int ChargedLvl2AmmoCost = 4;
    public float ChargedLvl2ProjectileSpeed;
    public float baseDamageLvl2;
    public Vector2 pitchSound_2 = Vector2.one;
    // «десь можно добавить поле дл€ эффекта (например, enum EffectType)

    [Header("Charged Shot - Level 3")]
    public GameObject ChargedLvl3ProjectilePrefab;
    public AudioClip soundShot_3;
    public TypeMovement typeMovementLvl3 = TypeMovement.Parabular;
    public int ChargedLvl3AmmoCost = 8;
    public float ChargedLvl3ProjectileSpeed;
    public float baseDamageLvl3;
    public Vector2 pitchSound_3 = Vector2.one;
    // Ёффект дл€ уровн€ 3

    [Header("Overload")]
    public float OverloadDamageToPlayer = 10f;
    public float OverloadDuration = 0.5f;
}