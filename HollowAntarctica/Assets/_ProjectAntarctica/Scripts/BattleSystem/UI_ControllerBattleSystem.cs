using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class UIColorBattleSystem
{
    public string Name;
    [ColorUsage(true, true)]public UnityEngine.Color colorBG;
    [ColorUsage(true, true)] public UnityEngine.Color colorFilling;

    [Space(5)]
    public Image background;
    public Image filling;

    [Space(5)]
    public float maxFilling; // For normalization

    public void SwitchColorSlider()
    {
        background.color = colorBG;
        filling.color = colorFilling;
    }
}

namespace SimpleCharController
{
    public class UI_ControllerBattleSystem : MonoBehaviour
    {
        public List<UIColorBattleSystem> uiColorBattles;


        public void SetValueChargeLevel(float levelCharge)
        {
            uiColorBattles[0].filling.fillAmount = levelCharge * uiColorBattles[0].maxFilling;
        }

        public void SetValueOverheadLevel(float levelCharge)
        {
            uiColorBattles[3].filling.fillAmount = levelCharge * uiColorBattles[3].maxFilling;
        }

        private void Start()
        {
            SetColorSlider(ProjectileType.Green);
            SetColorSlider(WeaponState.Ready);
            SetValueChargeLevel(0);
            SetValueOverheadLevel(0);
        }

        public void SetColorSlider(ProjectileType projectileType)
        {
            switch (projectileType)
            {
                case ProjectileType.Green:
                    uiColorBattles[0].SwitchColorSlider();
                    break;
                case ProjectileType.Blue:
                    uiColorBattles[1].SwitchColorSlider();
                    break;
                case ProjectileType.Orange:
                    uiColorBattles[2].SwitchColorSlider();
                    break;
            }
        }

        public void SetColorSlider(WeaponState weaponState)
        {
            switch (weaponState)
            {
                case WeaponState.Ready:
                    uiColorBattles[3].SwitchColorSlider();
                    break;
                case WeaponState.Overloaded:
                    uiColorBattles[4].SwitchColorSlider();
                    break;
            }
        }

        private void SwitchColorSlider(int id)
        {
            uiColorBattles[id].background.color = uiColorBattles[id].colorBG;
            uiColorBattles[id].filling.color = uiColorBattles[id].colorFilling;
        }
    }
}
