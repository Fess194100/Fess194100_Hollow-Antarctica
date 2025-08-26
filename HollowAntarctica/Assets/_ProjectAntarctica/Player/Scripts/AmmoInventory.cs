using UnityEngine;

namespace SimpleCharController
{
    public class AmmoInventory : MonoBehaviour
    {
        [Header("Initial Ammo")]
        [SerializeField] private int greenAmmo = 100;
        [SerializeField] private int blueAmmo = 100;
        [SerializeField] private int orangeAmmo = 100;

        // ѕровер€ет, достаточно ли патронов указанного типа
        public bool HasEnoughAmmo(ProjectileType type, int amount)
        {
            return GetCurrentAmmo(type) >= amount;
        }

        // ѕотребл€ет патроны и возвращает true если успешно
        public bool ConsumeAmmo(ProjectileType type, int amount)
        {
            if (!HasEnoughAmmo(type, amount))
                return false;

            switch (type)
            {
                case ProjectileType.Green:
                    greenAmmo -= amount;
                    break;
                case ProjectileType.Blue:
                    blueAmmo -= amount;
                    break;
                case ProjectileType.Orange:
                    orangeAmmo -= amount;
                    break;
            }

            return true;
        }

        // ƒобавл€ет патроны указанного типа
        public void AddAmmo(ProjectileType type, int amount)
        {
            switch (type)
            {
                case ProjectileType.Green:
                    greenAmmo += amount;
                    break;
                case ProjectileType.Blue:
                    blueAmmo += amount;
                    break;
                case ProjectileType.Orange:
                    orangeAmmo += amount;
                    break;
            }
        }

        // ¬озвращает текущее количество патронов указанного типа
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

        // ”станавливает количество патронов указанного типа
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

        // ¬озвращает максимальное количество патронов (дл€ UI)
        public int GetMaxAmmo(ProjectileType type)
        {
            return type switch
            {
                ProjectileType.Green => greenAmmo,
                ProjectileType.Blue => blueAmmo,
                ProjectileType.Orange => orangeAmmo,
                _ => 0
            };
        }
    }
}