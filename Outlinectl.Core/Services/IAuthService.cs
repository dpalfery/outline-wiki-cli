using Outlinectl.Core.Domain;

namespace Outlinectl.Core.Services;

public interface IAuthService
{
    Task LoginAsync(string baseUrl, string token, string profileName = "default");
    Task LogoutAsync(string profileName = "default");
    Task<Profile?> GetProfileAsync(string profileName = "default");
    Task<string?> GetTokenAsync(string profileName = "default");
    Task SetCurrentProfileAsync(string profileName);
    Task<string> GetCurrentProfileNameAsync();
}
