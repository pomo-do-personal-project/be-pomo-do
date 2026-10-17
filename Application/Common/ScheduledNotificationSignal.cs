namespace be_pomo_do.Application.Common;

public class ScheduledNotificationSignal
{
    private readonly SemaphoreSlim _signal = new(0);
    private readonly object _lock = new();
    private DateTime? _nextDueUtc;

    public DateTime? NextDueUtc
    {
        get { lock (_lock) return _nextDueUtc; }
    }

    public void SetNextDueUtc(DateTime? dueUtc)
    {
        lock (_lock)
        {
            _nextDueUtc = dueUtc;
        }
    }

    public void NotifyNewSchedule(DateTime dueUtc)
    {
        lock (_lock)
        {
            if (!_nextDueUtc.HasValue || dueUtc < _nextDueUtc.Value)
            {
                _nextDueUtc = dueUtc;
            }
        }
        Trigger();
    }

    public void Trigger()
    {
        try
        {
            if (_signal.CurrentCount == 0)
            {
                _signal.Release();
            }
        }
        catch (ObjectDisposedException) { }
    }

    public async Task<bool> WaitAsync(TimeSpan timeout, CancellationToken cancellationToken)
    {
        return await _signal.WaitAsync(timeout, cancellationToken);
    }
}
