#if UNITY_IOS
using CandyCoded.HapticFeedback;
using Cysharp.Threading.Tasks;

public class IOSHapticService : IHapticService
{
    public async UniTask PlayHaptic(int delayMs, int durationMs, int amplitude, System.Threading.CancellationToken ct = default)
    {
        if (delayMs > 0) await UniTask.Delay(delayMs, cancellationToken: ct);

        if (amplitude <= 130) HapticFeedback.LightFeedback();
        else if (amplitude <= 200) HapticFeedback.MediumFeedback();
        else HapticFeedback.HeavyFeedback();
    }
}
#endif