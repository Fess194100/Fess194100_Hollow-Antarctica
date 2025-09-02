using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SimpleCharController
{
    public class WeaponController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SimpleInputActions inputActions;
        [SerializeField] private Transform firePoint;
        [SerializeField] private AmmoInventory ammoInventory;
        [SerializeField] private EssenceHealth playerHealth;
        [SerializeField] private HandlerCombatEffects playerCombatEffects;
        [SerializeField] private RectTransform crosshairRectTransform;

        [Header("Weapon Settings")]
        [SerializeField] private LayerMask targetingMask = ~0;
        [SerializeField] private WeaponProjectileData[] projectileData;
        [SerializeField] private float timeToMaxCharge = 2f;
        [SerializeField] private float overheatThresholdTime = 0.5f;
        [SerializeField] private float autoFireRateMultiplier = 1f;
        [SerializeField] private Vector2 offsetAim;
        [SerializeField] private AnimationCurve offsetAimYAtFOV;

        [Header("Weapon State")]
        [SerializeField] private ProjectileType currentProjectileType = ProjectileType.Green;
        [SerializeField] private WeaponState currentWeaponState = WeaponState.Ready;
        [SerializeField] private float currentChargeTime = 0f;
        [SerializeField] private float overheatTimer = 0f;
        [SerializeField] private float overloadTimer = 0f;

        [Header("Events")]
        public ProgressChargeWeaponEvents progressChargeWeapon;
        public StateWeaponEvents stateWeapon;

        private Camera _mainCamera;
        private bool isAltFireHeld = false;
        private int _lastChargeLevel = -1;
        private float _chargePercent;
        private float _overheatPercent;
        private Coroutine _autoFireCoroutine;
        private Coroutine _spreadFireCoroutine;

        private void Awake()
        {
            ValidateReferences();
            SubscribeToEvents();
            _mainCamera = Camera.main;
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void ValidateReferences()
        {
            if (inputActions == null) Debug.LogError("inputActions == null");
            if (ammoInventory == null) Debug.LogError("ammoInventory == null");
            if (playerHealth == null) Debug.LogError("playerHealth == null");
            if (firePoint == null) Debug.LogError("firePoint == null");
        }

        private void SubscribeToEvents()
        {
            if (inputActions != null && inputActions.imputBattleEvents != null)
            {
                inputActions.imputBattleEvents.OnFire.AddListener(HandleFire);
                inputActions.imputBattleEvents.OnAltFire.AddListener(StartCharging);
                inputActions.imputBattleEvents.OffAltFire.AddListener(ReleaseChargedShot);
                inputActions.imputBattleEvents.CancelAltFire.AddListener(CancelCharging);
                inputActions.imputBattleEvents.OnWeaponSwitch.AddListener(HandleWeaponSwitch);
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (inputActions != null && inputActions.imputBattleEvents != null)
            {
                inputActions.imputBattleEvents.OnFire.RemoveListener(HandleFire);
                inputActions.imputBattleEvents.OnAltFire.RemoveListener(StartCharging);
                inputActions.imputBattleEvents.OffAltFire.RemoveListener(ReleaseChargedShot);
                inputActions.imputBattleEvents.CancelAltFire.RemoveListener(CancelCharging);
                inputActions.imputBattleEvents.OnWeaponSwitch.RemoveListener(HandleWeaponSwitch);
            }
        }

        private void Update()
        {
            HandleWeaponState();
        }

        private void HandleWeaponState()
        {
            switch (currentWeaponState)
            {
                case WeaponState.Charging:
                    if (!isAltFireHeld) ReleaseChargedShot();
                    break;

                case WeaponState.Overheating:
                    if (!isAltFireHeld) ReleaseChargedShot();
                    break;

                case WeaponState.Overloaded:
                    break;
            }
        }

        private void HandleFire()
        {
            if (currentWeaponState != WeaponState.Ready) return;

            WeaponProjectileData data = GetCurrentProjectileData();
            if (data == null) return;

            if (ammoInventory.HasEnoughAmmo(currentProjectileType, data.StandardAmmoCost))
            {
                SetWeaponState(WeaponState.Firing);

                switch (data.typeShootingStandard)
                {
                    case TypeShooting.Single:
                        ReleaseStandardShot();
                        StartCoroutine(ResetFiringState(data.StandardFireRate, -1));
                        break;

                    case TypeShooting.Auto:
                        if (_autoFireCoroutine == null)
                        {
                            _autoFireCoroutine = StartCoroutine(AutoFireRoutine(data));
                        }
                        break;

                    case TypeShooting.Burst:
                        ReleaseBurstShot(data, data.standardSpreadSettings, false);
                        StartCoroutine(ResetFiringState(data.StandardFireRate, -1));
                        break;

                    case TypeShooting.Spread:
                        if (_spreadFireCoroutine == null)
                        {
                            _spreadFireCoroutine = StartCoroutine(SpreadFireRoutine(data, data.standardSpreadSettings, false));
                        }
                        break;
                }
            }
        }

        private IEnumerator AutoFireRoutine(WeaponProjectileData data)
        {
            while (inputActions.fire && currentWeaponState == WeaponState.Firing)
            {
                if (ammoInventory.HasEnoughAmmo(currentProjectileType, data.StandardAmmoCost))
                {
                    ReleaseStandardShot();
                    yield return new WaitForSeconds(1f / (data.StandardFireRate * autoFireRateMultiplier));
                }
                else
                {
                    break;
                }
            }

            _autoFireCoroutine = null;
            SetWeaponState(WeaponState.Ready);
        }

        private void ReleaseStandardShot()
        {
            WeaponProjectileData data = GetCurrentProjectileData();
            if (data == null) return;

            if (ammoInventory.ConsumeAmmo(currentProjectileType, data.StandardAmmoCost))
            {
                SpawnProjectile(data.StandardProjectilePrefab, data.StandardProjectileSpeed, data.baseDamageStandard, 0, Vector3.zero, TypeMovement.Linear);
            }
        }

        private void HandleWeaponSwitch()
        {
            ProjectileType newType = (ProjectileType)(inputActions.selectedWeaponSlot - 1);

            if (newType != currentProjectileType && IsValidProjectileType(newType))
            {
                currentProjectileType = newType;
                stateWeapon.OnWeaponTypeChanged?.Invoke(currentProjectileType);
                CancelCharging();
            }
        }

        private void SetWeaponState(WeaponState newState)
        {
            if (currentWeaponState != newState)
            {
                currentWeaponState = newState;
            }
        }

        private bool IsValidProjectileType(ProjectileType type)
        {
            return type >= ProjectileType.Green && type <= ProjectileType.Orange;
        }

        private void StartCharging()
        {
            if (currentWeaponState != WeaponState.Ready) return;

            isAltFireHeld = true;
            SetWeaponState(WeaponState.Charging);
            stateWeapon.OnWeaponStateChanged?.Invoke(currentWeaponState);
            StartCoroutine(ChargingRoutine());
        }

        private void StartOverheating()
        {
            if (currentWeaponState != WeaponState.Charging) return;

            SetWeaponState(WeaponState.Overheating);
            stateWeapon.OnWeaponStateChanged?.Invoke(currentWeaponState);
        }

        private void TriggerOverload()
        {
            WeaponProjectileData data = GetCurrentProjectileData();
            if (data == null) return;

            SetWeaponState(WeaponState.Overloaded);
            StopAllCoroutines();

            stateWeapon.OnWeaponStateChanged?.Invoke(currentWeaponState);

            if(playerHealth != null && playerCombatEffects != null) ApplyOwerloarEffect(data);

            ammoInventory.ConsumeAmmo(currentProjectileType, data.ChargedLvl3AmmoCost);
            StartCoroutine(OverloadRoutine(data.OverloadDuration));
        }

        private void ApplyOwerloarEffect(WeaponProjectileData data)
        {
            playerHealth.TakeDamage(data.OverloadDamageToPlayer, currentProjectileType, 3);

            switch (currentProjectileType)
            {
                case ProjectileType.Green:
                    break;
                case ProjectileType.Blue:
                    playerCombatEffects.ApplyFrostbiteEffect(0.5f, 1f);
                    break;
                case ProjectileType.Orange:
                    playerCombatEffects.ApplyStunEffect(2f);
                    break;
            }
        }

        private void CancelCharging()
        {
            if (currentWeaponState != WeaponState.Charging && currentWeaponState != WeaponState.Overheating) return;

            isAltFireHeld = false;
            StopAllCoroutines();
            ResetCharged();
        }

        private void ResetCharged()
        {
            currentChargeTime = overheatTimer = _chargePercent = _overheatPercent = 0f;
            _lastChargeLevel = -1;
            progressChargeWeapon.OnChargeProgressChanged?.Invoke(0f);
            progressChargeWeapon.OnOverheatProgressChanged?.Invoke(0f);
            SetWeaponState(WeaponState.Ready);
        }

        private IEnumerator ChargingRoutine()
        {
            while (isAltFireHeld && (currentWeaponState == WeaponState.Charging || currentWeaponState == WeaponState.Overheating))
            {
                currentChargeTime += Time.deltaTime;
                _chargePercent = currentChargeTime / timeToMaxCharge;

                progressChargeWeapon.OnChargeProgressChanged?.Invoke(Mathf.Clamp01(_chargePercent));
                CheckChargeLevelEvents();

                if (_chargePercent >= 1 && overheatTimer <= overheatThresholdTime)
                {
                    if (overheatTimer < Time.deltaTime)
                    {
                        StartOverheating();
                    }

                    overheatTimer += Time.deltaTime;
                    _overheatPercent = Mathf.Clamp01(overheatTimer / overheatThresholdTime);
                    progressChargeWeapon.OnOverheatProgressChanged?.Invoke(_overheatPercent);

                    if (overheatTimer >= overheatThresholdTime)
                    {
                        TriggerOverload();
                        yield break;
                    }
                }

                yield return null;
            }
        }

        private IEnumerator OverloadRoutine(float duration)
        {
            overloadTimer = duration;

            while (overloadTimer > 0f)
            {
                overloadTimer -= Time.deltaTime;
                yield return null;
            }

            ResetCharged();
            stateWeapon.OnOverloadFinished?.Invoke();
        }

        private IEnumerator ResetFiringState(float fireRate, int chargeLevel)
        {
            yield return new WaitForSeconds(1f / fireRate);
            if (currentWeaponState == WeaponState.Firing)
            {
                SetWeaponState(WeaponState.Ready);
                if (chargeLevel == 0) ResetCharged();
            }
        }

        private void CheckChargeLevelEvents()
        {
            int currentLevel = CalculateChargeLevel();

            if (currentLevel != _lastChargeLevel)
            {
                switch (currentLevel)
                {
                    case 1:
                        stateWeapon.OnChargeLevel1Reached?.Invoke();
                        break;
                    case 2:
                        stateWeapon.OnChargeLevel2Reached?.Invoke();
                        break;
                    case 3:
                        stateWeapon.OnWeaponStateChanged?.Invoke(currentWeaponState);
                        break;
                }
                _lastChargeLevel = currentLevel;
            }
        }

        private int CalculateChargeLevel()
        {
            if (_chargePercent <= 0.33f) return 0;
            if (_chargePercent <= 0.66f) return 1;
            if (_chargePercent <= 0.99f) return 2;
            return 3;
        }

        private void ReleaseChargedShot()
        {
            if (currentWeaponState != WeaponState.Charging && currentWeaponState != WeaponState.Overheating) return;

            isAltFireHeld = false;
            StopAllCoroutines();
            FireChargedShot(CalculateChargeLevel());
        }

        private void FireChargedShot(int chargeLevel)
        {
            WeaponProjectileData data = GetCurrentProjectileData();
            if (data == null) return;

            if (chargeLevel == 0)
            {
                HandleChargedLevel0Shot(data);
            }
            else
            {
                HandleHigherChargeLevels(data, chargeLevel);
            }
        }

        private void HandleChargedLevel0Shot(WeaponProjectileData data)
        {
            switch (data.typeShootingLvl0)
            {
                case TypeShooting.Single:
                    ReleaseChargedLevel0Single(data);
                    break;

                case TypeShooting.Auto:
                    if (_autoFireCoroutine == null)
                    {
                        _autoFireCoroutine = StartCoroutine(AutoChargedFireRoutine(data));
                    }
                    break;

                case TypeShooting.Burst:
                    ReleaseBurstShot(data, data.lvl0SpreadSettings, true);
                    break;

                case TypeShooting.Spread:
                    if (_spreadFireCoroutine == null)
                    {
                        _spreadFireCoroutine = StartCoroutine(SpreadFireRoutine(data, data.lvl0SpreadSettings, true));
                    }
                    break;
            }

            currentWeaponState = WeaponState.Firing;
            StartCoroutine(ResetFiringState(data.Lvl0FireRate, 0));
        }

        private IEnumerator AutoChargedFireRoutine(WeaponProjectileData data)
        {
            while (isAltFireHeld && currentWeaponState == WeaponState.Firing)
            {
                if (ammoInventory.HasEnoughAmmo(currentProjectileType, data.ChargedLvl0AmmoCost))
                {
                    ReleaseChargedLevel0Single(data);
                    yield return new WaitForSeconds(1f / (data.Lvl0FireRate * autoFireRateMultiplier));
                }
                else
                {
                    break;
                }
            }

            _autoFireCoroutine = null;
            ResetCharged();
        }

        private void ReleaseChargedLevel0Single(WeaponProjectileData data)
        {
            if (ammoInventory.ConsumeAmmo(currentProjectileType, data.ChargedLvl0AmmoCost))
            {
                SpawnProjectile(data.ChargedLvl0ProjectilePrefab, data.ChargedLvl0ProjectileSpeed, data.baseDamageLvl0, 0, Vector3.zero, TypeMovement.Linear);
            }
        }

        private void HandleHigherChargeLevels(WeaponProjectileData data, int chargeLevel)
        {
            GameObject projectilePrefab = null;
            float speed = 0f;
            float damage = 0f;
            int ammoCost = 0;
            TypeMovement typeMovement = TypeMovement.Linear;

            switch (chargeLevel)
            {
                case 1:
                    projectilePrefab = data.ChargedLvl1ProjectilePrefab;
                    speed = data.ChargedLvl1ProjectileSpeed;
                    damage = data.baseDamageLvl1;
                    ammoCost = data.ChargedLvl1AmmoCost;
                    typeMovement = data.typeMovementLvl1;
                    break;
                case 2:
                    projectilePrefab = data.ChargedLvl2ProjectilePrefab;
                    speed = data.ChargedLvl2ProjectileSpeed;
                    damage = data.baseDamageLvl2;
                    ammoCost = data.ChargedLvl2AmmoCost;
                    typeMovement = data.typeMovementLvl2;
                    break;
                case 3:
                    projectilePrefab = data.ChargedLvl3ProjectilePrefab;
                    speed = data.ChargedLvl3ProjectileSpeed;
                    damage = data.baseDamageLvl3;
                    ammoCost = data.ChargedLvl3AmmoCost;
                    typeMovement = data.typeMovementLvl3;
                    break;
            }

            if (projectilePrefab != null && ammoInventory.ConsumeAmmo(currentProjectileType, ammoCost))
            {
                SpawnProjectile(projectilePrefab, speed, damage, chargeLevel, Vector3.zero, typeMovement);
            }

            ResetCharged();
        }

        private void ReleaseBurstShot(WeaponProjectileData data, SpreadWeaponSettings burstSettings, bool isCharged)
        {
            GameObject projectilePrefab = isCharged ? data.ChargedLvl0ProjectilePrefab : data.StandardProjectilePrefab;
            float speed = isCharged ? data.ChargedLvl0ProjectileSpeed : data.StandardProjectileSpeed;
            int ammoCost = isCharged ? data.ChargedLvl0AmmoCost : data.StandardAmmoCost;
            float damage = isCharged ? data.baseDamageLvl0 : data.baseDamageStandard;

            if (!ammoInventory.HasEnoughAmmo(currentProjectileType, ammoCost * burstSettings.projectilesCount))
                return;

            List<Vector2> spreadAngles = GenerateSpreadAngles(burstSettings.spreadAngle, burstSettings.projectilesCount);
            Vector3 baseDirection = (GetTargetPoint() - firePoint.position).normalized;

            for (int i = 0; i < burstSettings.projectilesCount; i++)
            {
                Vector3 spreadDirection = CalculateSpreadDirection(baseDirection, spreadAngles[i]);
                Quaternion projectileRotation = Quaternion.LookRotation(spreadDirection);

                if (ammoInventory.ConsumeAmmo(currentProjectileType, ammoCost))
                {
                    GameObject projectile = Instantiate(projectilePrefab, firePoint.position, projectileRotation);
                    Projectile projectileScript = projectile.GetComponent<Projectile>();

                    if (projectileScript != null)
                    {
                        projectileScript.Initialize(speed, gameObject, playerHealth, currentProjectileType, isCharged ? 0 : -1, damage, TypeMovement.Linear);
                    }
                }
            }
        }

        //-----------------------------------------------------------------------------------

        private IEnumerator SpreadFireRoutine(WeaponProjectileData data, SpreadWeaponSettings spreadSettings, bool isCharged)
        {
            GameObject projectilePrefab = isCharged ? data.ChargedLvl0ProjectilePrefab : data.StandardProjectilePrefab;
            float speed = isCharged ? data.ChargedLvl0ProjectileSpeed : data.StandardProjectileSpeed;
            int ammoCost = isCharged ? data.ChargedLvl0AmmoCost : data.StandardAmmoCost;
            float damage = isCharged ? data.baseDamageLvl0 : data.baseDamageStandard;

            if (!ammoInventory.HasEnoughAmmo(currentProjectileType, ammoCost * spreadSettings.projectilesCount))
                yield break;

            List<Vector2> spreadAngles = GenerateSpreadAngles(spreadSettings.spreadAngle, spreadSettings.projectilesCount);

            for (int i = 0; i < spreadSettings.projectilesCount; i++)
            {
                if (!ammoInventory.HasEnoughAmmo(currentProjectileType, ammoCost))
                    yield break;

                Vector3 currentBaseDirection = (GetTargetPoint() - firePoint.position).normalized;
                Vector3 spreadDirection = CalculateSpreadDirection(currentBaseDirection, spreadAngles[i]);
                Quaternion projectileRotation = Quaternion.LookRotation(spreadDirection);

                if (ammoInventory.ConsumeAmmo(currentProjectileType, ammoCost))
                {
                    GameObject projectile = Instantiate(projectilePrefab, firePoint.position, projectileRotation);
                    Projectile projectileScript = projectile.GetComponent<Projectile>();

                    if (projectileScript != null)
                    {
                        projectileScript.Initialize(speed, gameObject, playerHealth, currentProjectileType, isCharged ? 0 : -1, damage, TypeMovement.Linear);
                    }
                }

                yield return new WaitForSeconds(spreadSettings.delayBetweenShots);
            }

            _spreadFireCoroutine = null;
            if (isCharged)
            {
                ResetCharged();
            }
            else
            {
                yield return new WaitForSeconds(1 / data.StandardFireRate);
                SetWeaponState(WeaponState.Ready);
            }
        }

        private List<Vector2> GenerateSpreadAngles(float maxSpreadAngle, int count)
        {
            List<Vector2> angles = new List<Vector2>();

            if (count == 1)
            {
                angles.Add(Vector2.zero);
                return angles;
            }

            for (int i = 0; i < count; i++)
            {
                float randomDirectionAngle = Random.Range(0f, 360f);
                float randomSpreadDistance = Random.Range(0f, maxSpreadAngle);

                float x = randomSpreadDistance * Mathf.Cos(randomDirectionAngle * Mathf.Deg2Rad);
                float y = randomSpreadDistance * Mathf.Sin(randomDirectionAngle * Mathf.Deg2Rad);

                angles.Add(new Vector2(x, y));
            }

            return angles;
        }

        private Vector3 CalculateSpreadDirection(Vector3 baseDirection, Vector2 spreadAngleDegrees)
        {
            Vector3 right, up;
            CreateBaseCoordinateSystem(baseDirection, out right, out up);
            Vector3 spreadOffset = baseDirection;

            if (spreadAngleDegrees.x != 0f)
            {
                spreadOffset = Quaternion.AngleAxis(spreadAngleDegrees.x, up) * spreadOffset;
            }

            if (spreadAngleDegrees.y != 0f)
            {
                spreadOffset = Quaternion.AngleAxis(spreadAngleDegrees.y, right) * spreadOffset;
            }

            return spreadOffset.normalized;
        }        

        private void CreateBaseCoordinateSystem(Vector3 direction, out Vector3 right, out Vector3 up)
        {
            Vector3 forward = direction.normalized;

            if (Mathf.Abs(Vector3.Dot(forward, Vector3.up)) > 0.999f)
            {
                right = Vector3.right;
                up = Vector3.Cross(right, forward).normalized;
            }
            else
            {
                right = Vector3.Cross(forward, Vector3.up).normalized;
                up = Vector3.Cross(right, forward).normalized;
            }
        }

        //-----------------------------------------------------------------------------------

        private void SpawnProjectile(GameObject projectilePrefab, float speed, float damage, int chargeLevel, Vector3 directionOffset, TypeMovement typeMovement)
        {
            Vector3 targetPoint = GetTargetPoint();
            Vector3 shootDirection = (targetPoint - firePoint.position).normalized;

            

            if (directionOffset != Vector3.zero)
            {
                shootDirection = Quaternion.LookRotation(shootDirection) * directionOffset;
            }

            Quaternion projectileRotation = Quaternion.LookRotation(shootDirection);

            GameObject projectile = Instantiate(projectilePrefab, firePoint.position, projectileRotation);
            Projectile projectileScript = projectile.GetComponent<Projectile>();

            if (projectileScript != null)
            {
                projectileScript.Initialize(speed, gameObject, playerHealth, currentProjectileType, chargeLevel, damage, typeMovement);
            }
        }

        private Vector3 GetTargetPoint()
        {
            if (_mainCamera == null) return firePoint.position + firePoint.forward * 100f;

            Vector3 uiWorldPosition = GetUIElementWorldPosition();
            Ray ray = _mainCamera.ScreenPointToRay(uiWorldPosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 1000f, targetingMask))
            {
                return hit.point;
            }
            else
            {
                return ray.origin + ray.direction * 1000f;
            }
        }

        private Vector3 GetUIElementWorldPosition()
        {
            Vector2 screenPoint;
            if (crosshairRectTransform != null) screenPoint = RectTransformUtility.WorldToScreenPoint(_mainCamera, crosshairRectTransform.position) + offsetAim;
            else screenPoint = new Vector2 (Screen.width * 0.5f, Screen.height * 0.5f) + offsetAim;

            screenPoint.y += offsetAimYAtFOV.Evaluate(_mainCamera.fieldOfView);
            return new Vector3(screenPoint.x, screenPoint.y, 0f);
        }

        private WeaponProjectileData GetCurrentProjectileData()
        {
            int index = (int)currentProjectileType;
            if (index >= 0 && index < projectileData.Length)
            {
                return projectileData[index];
            }
            return null;
        }

        // ��������� ������ ��� �������� �������
        /*public bool CanShoot()
        {
            return currentWeaponState == WeaponState.Ready;
        }

        public bool IsCharging()
        {
            return currentWeaponState == WeaponState.Charging;
        }

        public bool IsOverheating()
        {
            return currentWeaponState == WeaponState.Overheating;
        }

        public bool IsOverloaded()
        {
            return currentWeaponState == WeaponState.Overloaded;
        }

        public float GetChargeProgress()
        {
            return Mathf.Clamp01(_chargePercent);
        }

        public float GetOverheatProgress()
        {
            return Mathf.Clamp01(_overheatPercent);
        }

        public ProjectileType GetCurrentWeaponType()
        {
            return currentProjectileType;
        }*/
    }
}