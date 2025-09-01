using System;
using UnityEngine;
using UnityEngine.Events;

namespace SimpleCharController
{
    [Serializable]
    public class ImputBattleSystemEvents
    {
        public UnityEvent OnFire, OffFire, OnAltFire, OffAltFire, CancelAltFire, OnWeaponSwitch;
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

    [Serializable]
    public class CombatEffectEvents
    {
        public UnityEvent OnFrostbite;
        public UnityEvent OnFrostbiteComplete;

        [Space(10)]
        public UnityEvent OnFreeze;
        public UnityEvent OnFreezeComplete;
    }
}