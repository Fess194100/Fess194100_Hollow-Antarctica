using UnityEngine;

namespace SimpleCharController
{
    public class AmmoInventory : MonoBehaviour
    {
        [Header("Initial Ammo")]
        [SerializeField] private int greenAmmo = 100;
        [SerializeField] private int blueAmmo = 100;
        [SerializeField] private int orangeAmmo = 100;

        // ���������, ���������� �� �������� ���������� ����
        public bool HasEnoughAmmo(ProjectileType type, int amount)
        {
            return GetCurrentAmmo(type) >= amount;
        }

        // ���������� ������� � ���������� true ���� �������
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

        // ��������� ������� ���������� ����
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

        // ���������� ������� ���������� �������� ���������� ����
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

        // ������������� ���������� �������� ���������� ����
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

        // ���������� ������������ ���������� �������� (��� UI)
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