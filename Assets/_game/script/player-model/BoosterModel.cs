using R3;
using System;
using System.Collections.Generic;

public class BoosterModelItem : IFileStreamObject
{
    public BoosterIndex boosterIndex;
    public ReactiveProperty<int> countRx = new(0);

    public int ModelVersion => 1;

    public void ReadOrWrite(IFileStream stream, int version)
    {
        switch (version)
        {
            case 1: ReadOrWrite_v1(stream); break;
            default: throw new Exception($"model {nameof(BoosterModelItem)} has invalid version {version}");
        }
    }

    private void ReadOrWrite_v1(IFileStream stream)
    {
        stream.ReadOrWriteEnum(ref boosterIndex, nameof(boosterIndex));
        stream.ReadOrWriteRxInt(ref countRx, nameof(countRx));
    }
}

public class BoosterModel : BasePlayerModel
{
    public List<BoosterModelItem> boosterItems = new List<BoosterModelItem>();

    public override int ModelVersion => 1;

    public override void ReadOrWrite(IFileStream stream, int version)
    {
        switch (version)
        {
            case 1: ReadOrWrite_v1(stream); break;
            default: throw new Exception($"model {nameof(BoosterModel)} has invalid version {version}");
        }
    }

    private void ReadOrWrite_v1(IFileStream stream)
    {
        stream.ReadOrWriteListObj(ref boosterItems, nameof(boosterItems));
    }

    public override void OnModelInitializing()
    {
        base.OnModelInitializing();

        for (int i = 0; i < Enum.GetValues(typeof(BoosterIndex)).Length; i++)
        {
            var boosterIndex = (BoosterIndex)i;
            if (!boosterItems.Exists(item => item.boosterIndex == boosterIndex))
            {
                var newItem = new BoosterModelItem()
                {
                    boosterIndex = boosterIndex,
                    countRx = new ReactiveProperty<int>(HardCodeInGame.INIT_BOOSTER_COUNT)
                };
                boosterItems.Add(newItem);
            }
        }
    }
}
