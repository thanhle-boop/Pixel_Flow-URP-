
using System.IO;

public class MapLevelConfigIO : ExtraConfigReadWriteManager
{
    private MapLevelConfig mapLevelConfig;

    public static MapLevelConfig MapLevelConfig => ConfigManager.instance.GetExtraConfig<MapLevelConfigIO>().mapLevelConfig;

    public override string configFilename => "map_level_cfg";

    #region config text

    protected override void OnReadConfig_text(string text)
    {
        mapLevelConfig = new MapLevelConfig();
        mapLevelConfig.test = StaticUtils.StringToInt(text);
    }

    #endregion

    #region config binary

    protected override void OnReadConfig_binary(BinaryReader reader)
    {
        mapLevelConfig = new MapLevelConfig();
        mapLevelConfig.test = reader.ReadInt32();
    }

    protected override void OnWriteConfig_binary(BinaryWriter writer)
    {
        writer.Write(mapLevelConfig.test);
    }

    #endregion

    
}