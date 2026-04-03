using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "AudioGameConfig", menuName = "Configs/AudioGameConfig")]
public class AudioGameConfig : ScriptableObject
{
    [System.Serializable]
    public class AudioGameConfigItem
    {
        public AudioIndex audioIndex;
        public float cooldown;
    }

    [SerializeField] List<AudioGameConfigItem> audioGameConfigs;

    public float GetCooldownByAudioIndex(AudioIndex audioIndex)
    {
        var configItem = audioGameConfigs.Find(cfg => cfg.audioIndex == audioIndex);
        if (configItem == null)
        {
            Debug.LogError($"[AUDIOGAMECONFIG] Audio index '{audioIndex}' not found. Returning 0 cooldown.");
            return 0f;
        }
        return configItem.cooldown;
    }

    public static AudioGameConfig instance => Resources.Load<AudioGameConfig>("AudioGameConfig");
}
