namespace be_pomo_do.Application.Common.Interfaces;

/// <summary>Provides the active CancellationToken for the current request execution context.</summary>
public interface ICancellationTokenProvider
{
    /// <summary>Gets the current cancellation token from HttpContext or CancellationToken.None if absent.</summary>
    CancellationToken Token { get; }
}
