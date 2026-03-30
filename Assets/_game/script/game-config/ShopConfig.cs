
public class ShopConfig : BaseConfig<ShopConfigItem>
{
    public ShopConfigItem GetShopItem(string uid)
    {
        return listConfigItems.Find(x => x.uid.Equals(uid));
    }
}