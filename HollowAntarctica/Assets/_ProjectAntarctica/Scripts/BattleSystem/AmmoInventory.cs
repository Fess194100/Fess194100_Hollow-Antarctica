using UnityEngine;
using UnityEngine.Events;

namespace SimpleCharController
{
    public class AmmoInventory : MonoBehaviour
    {
        #region Variables
        [Header("Initial Ammo")]
        [SerializeField] private int greenAmmo = 100;
        [SerializeField] private int blueAmmo = 100;
        [SerializeField] private int orangeAmmo = 100;

        [Header("Events")]
        public UnityEvent OnChengedAmmoGreen;
        public UnityEvent OnChengedAmmoBlue;
        public UnityEvent OnChengedAmmoOrange;
        #endregion

        #region API
        public bool HasEnoughAmmo(ProjectileType type, int amount)
        {
            return GetCurrentAmmo(type) >= amount;
        }

        public bool ConsumeAmmo(ProjectileType type, int amount)
        {
            if (!HasEnoughAmmo(type, amount))
                return false;

            switch (type)
            {
                case ProjectileType.Green:
                    greenAmmo -= amount;
                    OnChengedAmmoGreen.Invoke();
                    break;
                case ProjectileType.Blue:
                    blueAmmo -= amount;
                    OnChengedAmmoBlue.Invoke();
                    break;
                case ProjectileType.Orange:
                    orangeAmmo -= amount;
                    OnChengedAmmoOrange.Invoke();
                    break;
            }

            return true;
        }

        public void AddAmmo(ProjectileType type, int amount)
        {
            switch (type)
            {
                case ProjectileType.Green:
                    greenAmmo += amount;
                    OnChengedAmmoGreen.Invoke();
                    break;
                case ProjectileType.Blue:
                    blueAmmo += amount;
                    OnChengedAmmoBlue.Invoke();
                    break;
                case ProjectileType.Orange:
                    orangeAmmo += amount;
                    OnChengedAmmoOrange.Invoke();
                    break;
            }
        }

        public int GetCurrentAmmo(ProjectileType type)
        {
            return type switch
            {
                ProjectileType.Green => greenAmmo,
                ProjectileType.Blue => blueAmmo,
                ProjectileType.Orange => orangeAmmo,
                _ => 0
            };
        }

        public void SetAmmo(ProjectileType type, int amount)
        {
            switch (type)
            {
                case ProjectileType.Green:
                    greenAmmo = amount;
                    break;
                case ProjectileType.Blue:
                    blueAmmo = amount;
                    break;
                case ProjectileType.Orange:
                    orangeAmmo = amount;
                    break;
            }
        }
        #endregion
    }
}