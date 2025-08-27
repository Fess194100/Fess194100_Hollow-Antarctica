using System;
using UnityEngine;

namespace SimpleCharController
{
    public enum ClimbingType
    {
        climbLadder,
        ropeLadder,
    }

    public enum TypePortableObject
    {
        None,
        Curcle_0, Curcle_1, Curcle_2, Curcle_3, Curcle_4, Curcle_5, Curcle_6, Curcle_7, Curcle_8, Curcle_9,
        Box_0, Box_1, Box_2, Box_3, Box_4, Box_5, Box_6, Box_7, Box_8, Box_9,
    }

    public enum WeaponState
    {
        Ready,       // Оружие готово к стрельбе
        Firing,      // Идет обычная стрельба (которая может иметь свою задержку)
        Charging,    // Идет зарядка поражающего выстрела
        Overheating, // Оружие перегревается, но еще можно выстрелить
        Overloaded   // Оружие перегружено, стрельбы невозможна
    }

    public enum TypeShooting
    {
        Single,
        Auto,
        Burst, // одновременный выстрел несколькими снарядами
        Spread // Стрельба очередями
    }

    public enum ProjectileType
    {
        Green = 0,
        Blue = 1,
        Orange = 2
    }

    public enum TypeMovement
    {
        Linear,
        Parabular,
    }
}
