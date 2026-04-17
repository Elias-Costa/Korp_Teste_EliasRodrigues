namespace InventoryService.Services;

public sealed class FailureSimulationState(bool enabled)
{
    private int _enabled = enabled ? 1 : 0;

    public bool IsEnabled => Interlocked.CompareExchange(ref _enabled, 0, 0) == 1;

    public bool Set(bool enabledValue)
    {
        Interlocked.Exchange(ref _enabled, enabledValue ? 1 : 0);
        return IsEnabled;
    }
}
