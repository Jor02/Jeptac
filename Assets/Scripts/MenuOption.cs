using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MenuOption : MonoBehaviour
{
    public SettingsOptions menuSetting;

    void Start()
    {
        switch (menuSetting)
        {
            case SettingsOptions.FullscreenMode:
                TMP_Dropdown screen = GetComponent<TMP_Dropdown>();
                screen.value = MenuScript.Instance.curSettings.FullscreenMode;
                screen.RefreshShownValue();
                break;
            case SettingsOptions.Resolution:
                TMP_Dropdown res = GetComponent<TMP_Dropdown>();
                res.ClearOptions();
                res.AddOptions(MenuScript.Instance.ResolutionSettings);
                res.value = MenuScript.Instance.curSettings.Resolution;
                res.RefreshShownValue();
                break;
            case SettingsOptions.Graphics:
                TMP_Dropdown graphics = GetComponent<TMP_Dropdown>();
                graphics.value = MenuScript.Instance.curSettings.Graphics;
                graphics.RefreshShownValue();
                break;
            case SettingsOptions.Master:
                GetComponent<Slider>().value = MenuScript.Instance.curSettings.Master;
                break;
            case SettingsOptions.Player:
                GetComponent<Slider>().value = MenuScript.Instance.curSettings.Player;
                break;
            case SettingsOptions.Ambience:
                GetComponent<Slider>().value = MenuScript.Instance.curSettings.Ambience;
                break;
            case SettingsOptions.Continue:
                gameObject.SetActive(MenuScript.Instance.curSettings.hasSave);
                break;
        }

        Destroy(this);
    }

    public enum SettingsOptions
    {
        FullscreenMode = 0,
        Resolution = 1,
        Graphics = 2,
        Master = 3,
        Player = 4,
        Ambience = 5,
        Continue = 6
    }
}
