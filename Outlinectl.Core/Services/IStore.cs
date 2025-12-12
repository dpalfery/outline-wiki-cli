using Outlinectl.Core.Domain;

namespace Outlinectl.Core.Services;

public interface IStore
{
    Task<Config> LoadConfigAsync();
    Task SaveConfigAsync(Config config);
}

public interface ISecureStore
{
    Task<string?> GetTokenAsync(string profileName);
    Task SetTokenAsync(string profileName, string token);
    Task DeleteTokenAsync(string profileName);
}
