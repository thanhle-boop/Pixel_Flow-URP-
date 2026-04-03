using CandyCoded.HapticFeedback;

public static class HapticController
{
    public static void PlayLight()
    {
        if (!SettingController.hapticOnRx.Value)
        {
            return;
        }

        HapticFeedback.LightFeedback();
    }

    public static void PlayMedium()
    {
        if (!SettingController.hapticOnRx.Value)
        {
            return;
        }

        HapticFeedback.MediumFeedback();
    }

    public static void PlayHeavy()
    {
        if (!SettingController.hapticOnRx.Value)
        {
            return;
        }

        HapticFeedback.HeavyFeedback();
    }
}
