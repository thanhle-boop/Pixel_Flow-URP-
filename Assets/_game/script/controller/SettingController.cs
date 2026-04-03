using R3;

public static class SettingController
{
    static SettingModel settingModel => PlayerModelManager.instance.GetPlayerModel<SettingModel>();

    public static ReactiveProperty<float> bgmVolumeRx => settingModel.bgmVolumeRx;
    public static ReactiveProperty<float> sfxVolumeRx => settingModel.sfxVolumeRx;
    public static ReactiveProperty<bool> hapticOnRx => settingModel.hapticOnRx;

    public static void UpdateBGMVolume(float volume)
    {
        bgmVolumeRx.Value = volume;
        settingModel.Save();
    }

    public static void UpdateSFXVolume(float volume)
    {
        sfxVolumeRx.Value = volume;
        settingModel.Save();
    }

    public static void UpdateHapticOn(bool on)
    {
        hapticOnRx.Value = on;
        settingModel.Save();
    }
}
