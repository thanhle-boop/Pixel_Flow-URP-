using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HapticConfig", menuName = "Configs/HapticConfig")]
public class HapticConfig : ScriptableObject
{
    [System.Serializable]
    public class HapticConfigItem
    {
        public HapticType hapticType;
        public int delay;
        public int duration;
        public int amplitude;
    }

    [SerializeField] List<HapticConfigItem> hapticConfigItems;
    s
    public HapticConfigItem GetHapticConfigItem(HapticType hapticType)
    {
        var configItem = hapticConfigItems.Find(cfg => cfg.hapticType == hapticType);
        if (configItem == null)
        {
            Debug.LogError($"[AUDIOGAMECONFIG] Haptic index '{hapticType}' not found. Returning 0 cooldown.");
            return new HapticConfigItem() {hapticType = hapticType};
        }
        return configItem;
    }

    public static HapticConfig instance => Resources.Load<HapticConfig>("HapticConfig");
}