using Outlinectl.Core.Domain;

namespace Outlinectl.Core.Services;

public class AuthService : IAuthService
{
    private readonly IStore _store;
    private readonly ISecureStore _secureStore;

    public AuthService(IStore store, ISecureStore secureStore)
    {
        _store = store;
        _secureStore = secureStore;
    }

    public async Task LoginAsync(string baseUrl, string token, string profileName = "default")
    {
        // 1. Save Profile (BaseUrl)
        var config = await _store.LoadConfigAsync();
        if (!config.Profiles.ContainsKey(profileName))
        {
            config.Profiles[profileName] = new Profile();
        }
        config.Profiles[profileName].BaseUrl = baseUrl.TrimEnd('/');
        config.CurrentProfile = profileName;
        await _store.SaveConfigAsync(config);

        // 2. Save Token
        await _secureStore.SetTokenAsync(profileName, token);
    }

    public async Task LogoutAsync(string profileName = "default")
    {
        // Remove token
        await _secureStore.DeleteTokenAsync(profileName);
        
        // Optional: Remove profile config or keep it? 
        // Typically logout just clears credentials.
        // We will keep the profile config (BaseUrl) as it might be useful for re-login.
    }

    public async Task<Profile?> GetProfileAsync(string profileName = "default")
    {
        var config = await _store.LoadConfigAsync();
        return config.Profiles.TryGetValue(profileName, out var profile) ? profile : null;
    }

    public async Task<string?> GetTokenAsync(string profileName = "default")
    {
        // Check Env var first? 
        // Requirements: "IF the OUTLINE_API_TOKEN environment variable is set THEN the system SHALL prioritize it..."
        // Logic should be here.
        
        var envToken = Environment.GetEnvironmentVariable("OUTLINE_API_TOKEN");
        if (!string.IsNullOrEmpty(envToken))
        {
            // If using env token, which profile? 
            // Env token usually overrides the "current" profile's token.
            // But if specific profile requested, maybe not.
            // Assuming env var is global override.
            return envToken;
        }

        return await _secureStore.GetTokenAsync(profileName);
    }

    public async Task SetCurrentProfileAsync(string profileName)
    {
        var config = await _store.LoadConfigAsync();
        if (config.Profiles.ContainsKey(profileName))
        {
            config.CurrentProfile = profileName;
            await _store.SaveConfigAsync(config);
        }
    }

    public async Task<string> GetCurrentProfileNameAsync()
    {
        var config = await _store.LoadConfigAsync();
        return config.CurrentProfile;
    }
}
