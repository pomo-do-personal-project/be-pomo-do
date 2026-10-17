namespace be_pomo_do.Application.Common.Interfaces;

// Provides the active CancellationToken for the current request execution context.
public interface ICancellationTokenProvider
{
    // Gets the current cancellation token from HttpContext or CancellationToken.None if absent.
    CancellationToken Token { get; }
}
