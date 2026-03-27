
using System;
using R3;

public class CurrencyModel : BasePlayerModel
{
    public ReactiveProperty<long> gold = new(0);//v1

    public override int ModelVersion => 1;

    public override void ReadOrWrite(IFileStream stream, int version)
    {
        switch (version)
        {
            case 1:
            ReadOrWrite_v1(stream);
            break;
            default:
            throw new Exception($"model {nameof(CurrencyModel)} has invalid version {version}");
        }
    }

    private void ReadOrWrite_v1(IFileStream stream)
    {
        stream.ReadOrWriteRxLong(ref gold, nameof(gold));
    }
}