using R3;

public static class CurrencyController
{
    static CurrencyModel currencyModel => PlayerModelManager.instance.GetPlayerModel<CurrencyModel>();

    public static ReactiveProperty<long> GetGoldRx()
    {
        return currencyModel.gold;
    }

    public static long GetGold()
    {
        return currencyModel.gold.Value;
    }

    public static void AddGold(long amount)
    {
        currencyModel.gold.Value += amount;

        currencyModel.Save();
    }

    public static void SubtractGold(long amount)
    {
        currencyModel.gold.Value -= amount;

        currencyModel.Save();
    }
}
