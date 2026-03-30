
public class ShopConfigItem : BaseConfigItem
{
    public string uid;
    public string rewardType;
    public long rewardAmount;

    public override void ReadOrWrite(IFileStream stream)
    {
        stream.ReadOrWriteString(ref uid);
        stream.ReadOrWriteString(ref rewardType);
        stream.ReadOrWriteLong(ref rewardAmount);
    }
}