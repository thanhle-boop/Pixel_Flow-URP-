using ObservableCollections;
using System;

public class LevelModelItem : IFileStreamObject
{
    public int level;

    public int ModelVersion => 1;

    public void ReadOrWrite(IFileStream stream, int version)
    {
        switch (version)
        {
            case 1: ReadOrWrite_v1(stream); break;
            default: throw new Exception($"model {nameof(LevelModelItem)} has invalid version {version}");
        }
    }

    private void ReadOrWrite_v1(IFileStream stream)
    {
        stream.ReadOrWriteInt(ref level, nameof(level));
    }
}

public class LevelModel : BasePlayerModel
{
    public ObservableList<LevelModelItem> lLevel = new ObservableList<LevelModelItem>();

    public override int ModelVersion => 1;

    public override void ReadOrWrite(IFileStream stream, int version)
    {
        switch (version)
        {
            case 1: ReadOrWrite_v1(stream); break;
            default: throw new Exception($"model {nameof(LevelModel)} has invalid version {version}");
        }
    }

    private void ReadOrWrite_v1(IFileStream stream)
    {
        stream.ReadOrWriteRxListObj(ref lLevel, nameof(lLevel));
    }

    public override void OnModelInitializing()
    {
        base.OnModelInitializing();

        lLevel.Add(new LevelModelItem { level = 1 });
    }
}
