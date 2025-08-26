using System;
using UnityEngine;
using UnityEngine.Events;

namespace SimpleCharController
{
    [Serializable]
    public class ImputBattleSystemEvents
    {
        public UnityEvent OnFire, OnAltFire, OffAltFire, CancelAltFire, OnWeaponSwitch;
    }

    [Serializable]
    public class ProgressChargeWeaponEvents
    {
        public UnityEvent<float> OnChargeProgressChanged;
        public UnityEvent<float> OnOverheatProgressChanged;
    }

    [Serializable]
    public class StateWeaponEvents
    {
        public UnityEvent<ProjectileType> OnWeaponTypeChanged;
        public UnityEvent<WeaponState> OnWeaponStateChanged;

        [Space(10)]
        public UnityEvent OnChargeLevel1Reached;
        public UnityEvent OnChargeLevel2Reached;
        public UnityEvent OnOverloadFinished;
    }
}