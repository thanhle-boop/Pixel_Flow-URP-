using Cysharp.Threading.Tasks;

public class AndroidHapticService : IHapticService
{
    public async UniTask PlayHaptic(int delayMs, int durationMs, int amplitude, System.Threading.CancellationToken ct = default)
    {
        if (delayMs > 0) await UniTask.Delay(delayMs, cancellationToken: ct);

#if UNITY_ANDROID && !UNITY_EDITOR
        using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        using (AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
        using (AndroidJavaObject vibrator = currentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator"))
        using (AndroidJavaClass vibrationEffectClass = new AndroidJavaClass("android.os.VibrationEffect"))
        {
            if (vibrator.Call<bool>("hasVibrator"))
            {
                AndroidJavaObject effect = vibrationEffectClass.CallStatic<AndroidJavaObject>("createOneShot", (long)durationMs, amplitude);
                vibrator.Call("vibrate", effect);
            }
        }
#endif
    }
}