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

        private void Start()
        {
            SetColorSlider(ProjectileType.Green);
        }

        public void SetColorSlider(ProjectileType projectileType)
        {
            switch (projectileType)
            {
                case ProjectileType.Green:
                    uiColorBattles[0].SwitchColorSlider();
                    //uiColorBattles[0].background.color = uiColorBattles[0].colorBG;
                    //uiColorBattles[0].filling.color = uiColorBattles[0].colorFilling;
                    break;
                case ProjectileType.Blue:
                    uiColorBattles[1].SwitchColorSlider();
                    //uiColorBattles[1].background.color = uiColorBattles[1].colorBG;
                    //uiColorBattles[1].filling.color = uiColorBattles[1].colorFilling;
                    break;
                case ProjectileType.Orange:
                    uiColorBattles[2].SwitchColorSlider();
                    //uiColorBattles[2].background.color = uiColorBattles[2].colorBG;
                    //uiColorBattles[2].filling.color = uiColorBattles[2].colorFilling;
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
