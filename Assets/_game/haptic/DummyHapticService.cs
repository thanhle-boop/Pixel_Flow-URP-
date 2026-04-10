using Cysharp.Threading.Tasks;
using UnityEngine;

public class DummyHapticService : IHapticService
{
    public async UniTask PlayHaptic(int delayMs, int durationMs, int amplitude, System.Threading.CancellationToken ct = default)
    {
        if (delayMs > 0) await UniTask.Delay(delayMs, cancellationToken: ct);
        Debug.Log($"<color=cyan>[Haptic Editor]</color> Play: Delay {delayMs}ms, Duration {durationMs}ms, Amp {amplitude}");
    }
}
