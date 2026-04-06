using System.Threading;
using Cysharp.Threading.Tasks;

public interface IHapticService
{
    UniTask PlayHaptic(int delayMs, int durationMs, int amplitude, System.Threading.CancellationToken ct = default);
}