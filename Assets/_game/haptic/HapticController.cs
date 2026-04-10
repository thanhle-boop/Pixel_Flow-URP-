using Cysharp.Threading.Tasks;

public static class HapticController
{
    private static readonly IHapticService _service;

    static HapticController()
    {
#if UNITY_EDITOR
        _service = new DummyHapticService();
#elif UNITY_IOS
        _service = new IOSHapticService();
#elif UNITY_ANDROID
        _service = new AndroidHapticService();
#else
        _service = new DummyHapticService(); 
#endif
    }

    public static void PlayHaptic(HapticType type)
    {
        var config = HapticConfig.instance.GetHapticConfigItem(type);
        _service.PlayHaptic(config.delay, config.duration, config.amplitude).Forget();
    }
}
