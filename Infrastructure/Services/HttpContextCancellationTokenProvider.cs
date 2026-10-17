using be_pomo_do.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace be_pomo_do.Infrastructure.Services;

// Resolves CancellationToken from active HttpContext RequestAborted.
public class HttpContextCancellationTokenProvider : ICancellationTokenProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    // Initializes a new instance of HttpContextCancellationTokenProvider.
    public HttpContextCancellationTokenProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    public CancellationToken Token => _httpContextAccessor.HttpContext?.RequestAborted ?? CancellationToken.None;
}
