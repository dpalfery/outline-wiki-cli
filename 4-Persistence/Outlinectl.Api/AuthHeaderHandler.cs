using System.Net.Http.Headers;
using Outlinectl.Core.Services;

namespace Outlinectl.Api;

public class AuthHeaderHandler : DelegatingHandler
{
    private readonly IAuthService _authService;

    public AuthHeaderHandler(IAuthService authService)
    {
        _authService = authService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var profileName = await _authService.GetCurrentProfileNameAsync();
        var token = await _authService.GetTokenAsync(profileName);

        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
