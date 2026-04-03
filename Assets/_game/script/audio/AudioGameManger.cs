using R3;

public class AudioGameManger : SingletonMonoBehaviour<AudioGameManger>
{
    public void InitAudioGameManager()
    {
        var settingModel = PlayerModelManager.instance.GetPlayerModel<SettingModel>();

        settingModel.bgmVolumeRx
            .Subscribe(musicVolume =>
            {
                AudioController.instance.SetVolumeMusic(musicVolume);
            })
            .AddTo(this);

        settingModel.sfxVolumeRx
            .Subscribe(soundVolume =>
            {
                AudioController.instance.SetVolumeSound(soundVolume);
            })
            .AddTo(this);
    }
}
