using R3;
using System;

public class SettingModel : BasePlayerModel
{
    public ReactiveProperty<float> bgmVolumeRx = new(1);
    public ReactiveProperty<float> sfxVolumeRx = new(1);
    public ReactiveProperty<bool> hapticOnRx = new(true);

    public override int ModelVersion => 1;

    public override void ReadOrWrite(IFileStream stream, int version)
    {
        switch (version)
        {
            case 1: ReadOrWrite_v1(stream); break;
            default: throw new Exception($"model {nameof(SettingModel)} has invalid version {version}");
        }
    }

    private void ReadOrWrite_v1(IFileStream stream)
    {
        stream.ReadOrWriteRxFloat(ref bgmVolumeRx, nameof(bgmVolumeRx));
        stream.ReadOrWriteRxFloat(ref sfxVolumeRx, nameof(sfxVolumeRx));
        stream.ReadOrWriteRxBool(ref hapticOnRx, nameof(hapticOnRx));
    }
}
