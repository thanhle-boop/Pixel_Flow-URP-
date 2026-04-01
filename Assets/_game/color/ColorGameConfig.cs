using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ColorGameConfig", menuName = "Configs/ColorGameConfig")]
public class ColorGameConfig : ScriptableObject
{
    [System.Serializable]
    public class ColorConfigItem
    {
        public string name;
        public Color color;
    }

    [SerializeField] List<ColorConfigItem> colorConfigs;

    public Color GetColorByName(string colorName)
    {
        var nameLower = colorName.ToLower();

        var colorCfg = colorConfigs.Find(cfg => cfg.name.ToLower() == nameLower);

        if(colorCfg == null)
        {
            Debug.LogError($"[COLORGAMECONFIG] Color name '{colorName}' not found. Returning gray.");
            return Color.gray;
        }

        return colorCfg.color;
    }    

    public static ColorGameConfig instance => Resources.Load<ColorGameConfig>("ColorGameConfig");
}
