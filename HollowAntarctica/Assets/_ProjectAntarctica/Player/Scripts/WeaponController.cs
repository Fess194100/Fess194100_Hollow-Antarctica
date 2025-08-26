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
        public UnityEvent<WeaponState> OnWeaponStateChanged;
        public UnityEvent<float> OnChargeProgressChanged;
        public UnityEvent<float> OnOverheatProgressChanged;
        public UnityEvent<ProjectileType> OnWeaponTypeChanged;

        // Новые события для этапов зарядки
        public UnityEvent OnChargingStarted;
        public UnityEvent OnChargeLevel1Reached;
        public UnityEvent OnChargeLevel2Reached;
        public UnityEvent OnChargeLevel3Reached;
        public UnityEvent OnOverheatingStarted;
        public UnityEvent OnOverloadStarted;
        public UnityEvent OnOverloadFinished;

        // Приватные переменные
        private Coroutine chargingCoroutine;
        private Coroutine overheatCoroutine;
        private Coroutine overloadCoroutine;
        private bool isAltFireHeld = false;
        private Camera _mainCamera;
        private int _lastChargeLevel = -1;

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

        private void Update()
        {
            HandleWeaponState();
        }

        private void ValidateReferences()
        {
            if (inputActions == null)
                inputActions = FindObjectOfType<SimpleInputActions>();

            if (ammoInventory == null)
                ammoInventory = GetComponent<AmmoInventory>();

            if (playerHealth == null)
                playerHealth = GetComponent<PlayerHealth>();

            if (firePoint == null)
            {
                GameObject firePointObj = new GameObject("FirePoint");
                firePointObj.transform.SetParent(transform);
                firePointObj.transform.localPosition = Vector3.forward * 1.5f;
                firePoint = firePointObj.transform;
            }
        }

        private void SubscribeToEvents()
        {
            if (inputActions != null && inputActions.battleEvents != null)
            {
                inputActions.battleEvents.OnFire.AddListener(HandleFire);
                inputActions.battleEvents.OnAltFire.AddListener(StartCharging);
                inputActions.battleEvents.OffAltFire.AddListener(ReleaseChargedShot);
                inputActions.battleEvents.CancelAltFire.AddListener(CancelCharging);
                inputActions.battleEvents.OnWeaponSwitch.AddListener(HandleWeaponSwitch);
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (inputActions != null && inputActions.battleEvents != null)
            {
                inputActions.battleEvents.OnFire.RemoveListener(HandleFire);
                inputActions.battleEvents.OnAltFire.RemoveListener(StartCharging);
                inputActions.battleEvents.OffAltFire.RemoveListener(ReleaseChargedShot);
                inputActions.battleEvents.CancelAltFire.RemoveListener(CancelCharging);
                inputActions.battleEvents.OnWeaponSwitch.RemoveListener(HandleWeaponSwitch);
            }
        }

        private void HandleWeaponState()
        {
            switch (currentWeaponState)
            {
                case WeaponState.Charging:
                    UpdateCharging();
                    break;

                case WeaponState.Overheating:
                    UpdateOverheating();
                    break;

                case WeaponState.Overloaded:
                    UpdateOverloaded();
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
                FireStandardShot();
                StartCoroutine(ResetFiringState(data.StandardFireRate));
            }
        }

        private void StartCharging()
        {
            if (currentWeaponState != WeaponState.Ready) return;

            isAltFireHeld = true;
            SetWeaponState(WeaponState.Charging);
            currentChargeTime = 0f;
            _lastChargeLevel = -1;
            OnChargingStarted?.Invoke();
            chargingCoroutine = StartCoroutine(ChargingRoutine());
        }

        private IEnumerator ChargingRoutine()
        {
            while (isAltFireHeld && (currentWeaponState == WeaponState.Charging || currentWeaponState == WeaponState.Overheating))
            {
                currentChargeTime += Time.deltaTime;
                float chargePercent = Mathf.Clamp01(currentChargeTime / timeToMaxCharge);

                OnChargeProgressChanged?.Invoke(chargePercent);

                // Проверяем достижение уровней заряда
                CheckChargeLevelEvents(chargePercent);

                // Проверка на начало перегрева (используем currentChargeTime вместо chargePercent)
                if (chargePercent >= 1 && overheatTimer <= overheatThresholdTime)
                {
                    // Если только начался перегрев
                    if (overheatTimer < Time.deltaTime)
                    {
                        StartOverheating();
                    }

                    overheatTimer += Time.deltaTime;
                    float overheatPercent = Mathf.Clamp01(overheatTimer / overheatThresholdTime);
                    OnOverheatProgressChanged?.Invoke(overheatPercent);

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

        private void CheckChargeLevelEvents(float chargePercent)
        {
            int currentLevel = CalculateChargeLevel(currentChargeTime);

            if (currentLevel != _lastChargeLevel)
            {
                switch (currentLevel)
                {
                    case 1:
                        OnChargeLevel1Reached?.Invoke();
                        break;
                    case 2:
                        OnChargeLevel2Reached?.Invoke();
                        break;
                    case 3:
                        OnChargeLevel3Reached?.Invoke();
                        break;
                }
                _lastChargeLevel = currentLevel;
            }
        }

        private void StartOverheating()
        {
            if (currentWeaponState != WeaponState.Charging) return;

            SetWeaponState(WeaponState.Overheating);
            OnOverheatingStarted?.Invoke();
        }

        private void UpdateCharging()
        {
            if (!isAltFireHeld)
            {
                ReleaseChargedShot();
            }
        }

        private void UpdateOverheating()
        {
            if (!isAltFireHeld)
            {
                ReleaseChargedShot();
            }
        }

        private void ReleaseChargedShot()
        {
            if (currentWeaponState != WeaponState.Charging && currentWeaponState != WeaponState.Overheating)
                return;

            isAltFireHeld = false;
            StopChargingCoroutine();

            int chargeLevel = CalculateChargeLevel(currentChargeTime);
            FireChargedShot(chargeLevel);
        }

        private void StopChargingCoroutine()
        {
            if (chargingCoroutine != null)
            {
                StopCoroutine(chargingCoroutine);
                chargingCoroutine = null;
            }
        }

        private void CancelCharging()
        {
            if (currentWeaponState != WeaponState.Charging && currentWeaponState != WeaponState.Overheating)
                return;

            isAltFireHeld = false;
            StopChargingCoroutine();
            ResetChargedOverheat();
        }

        private void TriggerOverload()
        {
            WeaponProjectileData data = GetCurrentProjectileData();
            if (data == null) return;

            SetWeaponState(WeaponState.Overloaded);
            StopChargingCoroutine();
            OnOverloadStarted?.Invoke();

            // Нанести урон игроку
            playerHealth.TakeDamage(data.OverheatDamageToPlayer);

            // Потратить патроны
            ammoInventory.ConsumeAmmo(currentProjectileType, data.ChargedLvl3AmmoCost);

            // Запустить перегрузку
            overloadCoroutine = StartCoroutine(OverloadRoutine(data.OverheatDuration));
        }

        private IEnumerator OverloadRoutine(float duration)
        {
            overloadTimer = duration;

            while (overloadTimer > 0f)
            {
                overloadTimer -= Time.deltaTime;
                yield return null;
            }

            ResetChargedOverheat();
            OnOverloadFinished?.Invoke();
            overloadCoroutine = null;
        }

        private void ResetChargedOverheat()
        {
            currentChargeTime = overheatTimer = 0f;
            OnChargeProgressChanged?.Invoke(0f);
            OnOverheatProgressChanged?.Invoke(0f);
            SetWeaponState(WeaponState.Ready);
        }

        private void UpdateOverloaded()
        {
            // Логика для состояния перегрузки
        }

        private int CalculateChargeLevel(float chargeTime)
        {
            float percent = chargeTime / timeToMaxCharge;
            if (percent <= 0.25f) return 0;
            if (percent <= 0.5f) return 1;
            if (percent <= 0.75f) return 2;
            return 3;
        }

        private void FireStandardShot()
        {
            WeaponProjectileData data = GetCurrentProjectileData();
            if (data == null) return;

            if (ammoInventory.ConsumeAmmo(currentProjectileType, data.StandardAmmoCost))
            {
                SpawnProjectile(data.StandardProjectilePrefab, data.StandardProjectileSpeed, 0);
            }
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

            ResetChargedOverheat();
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

        private IEnumerator ResetFiringState(float fireRate)
        {
            yield return new WaitForSeconds(1f / fireRate);
            if (currentWeaponState == WeaponState.Firing)
            {
                SetWeaponState(WeaponState.Ready);
            }
        }

        private void HandleWeaponSwitch()
        {
            // Конвертируем selectedWeaponSlot в ProjectileType
            ProjectileType newType = (ProjectileType)(inputActions.selectedWeaponSlot - 1);

            if (newType != currentProjectileType && IsValidProjectileType(newType))
            {
                currentProjectileType = newType;
                OnWeaponTypeChanged?.Invoke(currentProjectileType);
                CancelCharging(); // Отменяем зарядку при смене оружия
            }
        }

        private bool IsValidProjectileType(ProjectileType type)
        {
            return type >= ProjectileType.Green && type <= ProjectileType.Orange;
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

        private void SetWeaponState(WeaponState newState)
        {
            if (currentWeaponState != newState)
            {
                currentWeaponState = newState;
                OnWeaponStateChanged?.Invoke(newState);
            }
        }

        // Публичные методы для внешнего доступа
        public bool CanShoot()
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
            return Mathf.Clamp01(currentChargeTime / timeToMaxCharge);
        }

        public float GetOverheatProgress()
        {
            if (currentChargeTime <= timeToMaxCharge) return 0f;
            return Mathf.Clamp01((currentChargeTime - timeToMaxCharge) / overheatThresholdTime);
        }

        public ProjectileType GetCurrentWeaponType()
        {
            return currentProjectileType;
        }
    }
}