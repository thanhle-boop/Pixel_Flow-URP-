using UnityEngine;
using UnityEngine.UI;

public class PopupSettings : BasePopup
{
    [SerializeField] SliderSettingPresenter sliderBGM;
    [SerializeField] SliderSettingPresenter sliderSFX;
    [SerializeField] OnOffSettingPresenter onOffHaptic;

    [SerializeField] Button btnPolicy;
    [SerializeField] Button btnSave;
    [SerializeField] Button btnSupport;
    [SerializeField] Button btnRestore;

    protected override void Start()
    {
        base.Start();

        sliderBGM.Init(SettingController.bgmVolumeRx.Value, SettingController.UpdateBGMVolume);
        sliderSFX.Init(SettingController.sfxVolumeRx.Value, SettingController.UpdateSFXVolume);
        onOffHaptic.Init(SettingController.hapticOnRx, SettingController.UpdateHapticOn);
    }
}