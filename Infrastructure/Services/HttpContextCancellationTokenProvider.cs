using be_pomo_do.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace be_pomo_do.Infrastructure.Services;

/// <summary>Resolves CancellationToken from active HttpContext RequestAborted.</summary>
public class HttpContextCancellationTokenProvider : ICancellationTokenProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>Initializes a new instance of <see cref="HttpContextCancellationTokenProvider"/>.</summary>
    public HttpContextCancellationTokenProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    /// <inheritdoc/>
    public CancellationToken Token => _httpContextAccessor.HttpContext?.RequestAborted ?? CancellationToken.None;
}
