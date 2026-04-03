using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PopupGameplaySettings : BasePopup
{
    [SerializeField] SliderSettingPresenter sliderBGM;
    [SerializeField] SliderSettingPresenter sliderSFX;
    [SerializeField] OnOffSettingPresenter onOffHaptic;

    [SerializeField] Button btnPolicy;
    [SerializeField] Button btnSupport;
    [SerializeField] Button btnRestartLevel;
    [SerializeField] Button btnHome;

    protected override void Start()
    {
        base.Start();

        sliderBGM.Init(SettingController.bgmVolumeRx.Value, SettingController.UpdateBGMVolume);
        sliderSFX.Init(SettingController.sfxVolumeRx.Value, SettingController.UpdateSFXVolume);
        onOffHaptic.Init(SettingController.hapticOnRx, SettingController.UpdateHapticOn);

        btnRestartLevel.OnClickAsObservable()
            .Subscribe(_ =>
            {
                GameManager.Instance.StartGame();
                ClosePopup();
            }).AddTo(this);


        btnHome.OnClickAsObservable()
            .Subscribe(_ =>
            {
                ClosePopup();
                SceneManager.LoadScene("3.menu");
            }).AddTo(this);
    }
}