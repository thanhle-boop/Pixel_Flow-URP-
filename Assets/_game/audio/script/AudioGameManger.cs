using R3;
using System.Collections.Generic;
using UnityEngine;

public class AudioGameManger : SingletonMonoBehaviour<AudioGameManger>
{
    Dictionary<AudioIndex, float> audioCooldowns = new Dictionary<AudioIndex, float>();

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

    //handle sound layering
    public void PlaySFX(AudioIndex audioIndex)
    {
        var cooldown = AudioGameConfig.instance.GetCooldownByAudioIndex(audioIndex);

        if (!audioCooldowns.ContainsKey(audioIndex))
        {
            audioCooldowns.Add(audioIndex, Time.time - cooldown);
        }

        if (Time.time - audioCooldowns[audioIndex] < cooldown)
        {
            return;
        }

        AudioController.instance.PlaySound(audioIndex.ToString());
        audioCooldowns[audioIndex] = Time.time;
    }
}
