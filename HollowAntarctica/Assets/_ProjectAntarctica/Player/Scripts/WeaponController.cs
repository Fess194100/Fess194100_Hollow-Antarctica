using UnityEngine;
using UnityEngine.Events;
using System.Collections;

namespace SimpleCharController
{
    public class WeaponController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SimpleInputActions inputActions;
        [SerializeField] private Transform firePoint;
        [SerializeField] private AmmoInventory ammoInventory;
        [SerializeField] private PlayerHealth playerHealth;

        [Header("Weapon Settings")]
        [SerializeField] private LayerMask targetingMask = ~0;
        [SerializeField] private WeaponProjectileData[] projectileData;
        [SerializeField] private float timeToMaxCharge = 2f;
        [SerializeField] private float overheatThresholdTime = 0.5f;

        [Header("Weapon State")]
        [SerializeField] private ProjectileType currentProjectileType = ProjectileType.Green;
        [SerializeField] private WeaponState currentWeaponState = WeaponState.Ready;
        [SerializeField] private float currentChargeTime = 0f;
        [SerializeField] private float overheatTimer = 0f;
        [SerializeField] private float overloadTimer = 0f;

        [Header("Events")]
        public ProgressChargeWeaponEvents progressChargeWeapon;

        [Space(10)]
        public StateWeaponEvents stateWeapon;

        #region Private variables
        private Camera _mainCamera;
        private bool isAltFireHeld = false;        
        private int _lastChargeLevel = -1;
        private float _chargePercent;
        private float _overheatPercent;

        #endregion


        #region Inicialization
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

        #endregion


        #region Handlers & Update
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
                    //UpdateOverloaded();
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
                ReleaseStandardShot();
                StartCoroutine(ResetFiringState(data.StandardFireRate, -1));
            }
        }

        private void HandleWeaponSwitch()
        {
            // Конвертируем selectedWeaponSlot в ProjectileType
            ProjectileType newType = (ProjectileType)(inputActions.selectedWeaponSlot - 1);

            if (newType != currentProjectileType && IsValidProjectileType(newType))
            {
                currentProjectileType = newType;
                stateWeapon.OnWeaponTypeChanged?.Invoke(currentProjectileType);
                CancelCharging(); // Отменяем зарядку при смене оружия
            }
        }

        private void SetWeaponState(WeaponState newState)
        {
            if (currentWeaponState != newState)
            {
                currentWeaponState = newState;
                //OnWeaponStateChanged?.Invoke(newState);
            }
        }

        private bool IsValidProjectileType(ProjectileType type)
        {
            return type >= ProjectileType.Green && type <= ProjectileType.Orange;
        }

        #endregion


        #region ChargingWeapon
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
            StopCoroutine(ChargingRoutine());
            stateWeapon.OnWeaponStateChanged?.Invoke(currentWeaponState);

            // Нанести урон игроку
            playerHealth.TakeDamage(data.OverheatDamageToPlayer);

            // Потратить патроны
            ammoInventory.ConsumeAmmo(currentProjectileType, data.ChargedLvl3AmmoCost);

            // Запустить перегрузку
            StartCoroutine(OverloadRoutine(data.OverheatDuration));
        }

        private void CancelCharging()
        {
            if (currentWeaponState != WeaponState.Charging && currentWeaponState != WeaponState.Overheating) return;

            isAltFireHeld = false;
            StopCoroutine(ChargingRoutine());
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

                // Проверяем достижение уровней заряда
                CheckChargeLevelEvents();

                // Проверка на начало перегрева (используем currentChargeTime вместо chargePercent)
                if (_chargePercent >= 1 && overheatTimer <= overheatThresholdTime)
                {
                    // Если только начался перегрев
                    if (overheatTimer < Time.deltaTime)
                    {
                        StartOverheating();
                    }

                    overheatTimer += Time.deltaTime;
                    _overheatPercent = Mathf.Clamp01(overheatTimer / overheatThresholdTime);
                    progressChargeWeapon.OnOverheatProgressChanged?.Invoke(_overheatPercent);

                    // Если превысили порог перегрева - перегрузка
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

        #endregion


        #region ReleaseShot
        private void ReleaseStandardShot()
        {
            WeaponProjectileData data = GetCurrentProjectileData();
            if (data == null) return;

            if (ammoInventory.ConsumeAmmo(currentProjectileType, data.StandardAmmoCost))
            {
                SpawnProjectile(data.StandardProjectilePrefab, data.StandardProjectileSpeed, 0);
            }
        }

        private void ReleaseChargedShot()
        {
            if (currentWeaponState != WeaponState.Charging && currentWeaponState != WeaponState.Overheating) return;

            isAltFireHeld = false;
            StopCoroutine(ChargingRoutine());
            FireChargedShot(CalculateChargeLevel());
        }

        private void FireChargedShot(int chargeLevel)
        {
            WeaponProjectileData data = GetCurrentProjectileData();
            if (data == null) return;

            GameObject projectilePrefab = null;
            float speed = 0f;
            int ammoCost = 0;

            switch (chargeLevel)
            {
                case 0:
                    projectilePrefab = data.ChargedLvl0ProjectilePrefab;
                    speed = data.ChargedLvl0ProjectileSpeed;
                    ammoCost = data.ChargedLvl0AmmoCost;
                    break;
                case 1:
                    projectilePrefab = data.ChargedLvl1ProjectilePrefab;
                    speed = data.ChargedLvl1ProjectileSpeed;
                    ammoCost = data.ChargedLvl1AmmoCost;
                    break;
                case 2:
                    projectilePrefab = data.ChargedLvl2ProjectilePrefab;
                    speed = data.ChargedLvl2ProjectileSpeed;
                    ammoCost = data.ChargedLvl2AmmoCost;
                    break;
                case 3:
                    projectilePrefab = data.ChargedLvl3ProjectilePrefab;
                    speed = data.ChargedLvl3ProjectileSpeed;
                    ammoCost = data.ChargedLvl3AmmoCost;
                    break;
            }

            if (projectilePrefab != null && ammoInventory.ConsumeAmmo(currentProjectileType, ammoCost))
            {
                SpawnProjectile(projectilePrefab, speed, chargeLevel);
            }

            if (chargeLevel == 0)
            {
                currentWeaponState = WeaponState.Firing;
                StartCoroutine(ResetFiringState(data.Lvl0FireRate, chargeLevel));
            }
            else ResetCharged();
        }

        private void SpawnProjectile(GameObject projectilePrefab, float speed, int chargeLevel)
        {
            Vector3 targetPoint = GetTargetPoint();
            Vector3 shootDirection = (targetPoint - firePoint.position).normalized;
            Quaternion projectileRotation = Quaternion.LookRotation(shootDirection);

            GameObject projectile = Instantiate(projectilePrefab, firePoint.position, projectileRotation);
            Projectile projectileScript = projectile.GetComponent<Projectile>();

            if (projectileScript != null)
            {
                projectileScript.Initialize(speed, gameObject, currentProjectileType, chargeLevel);
            }
        }

        private Vector3 GetTargetPoint()
        {
            if (_mainCamera == null) return firePoint.position + firePoint.forward * 100f;

            Vector3 screenCenter = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f);
            Ray ray = _mainCamera.ScreenPointToRay(screenCenter);
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

        private WeaponProjectileData GetCurrentProjectileData()
        {
            int index = (int)currentProjectileType;
            if (index >= 0 && index < projectileData.Length)
            {
                return projectileData[index];
            }
            return null;
        }

        #endregion


        // Публичные методы для внешнего доступа
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