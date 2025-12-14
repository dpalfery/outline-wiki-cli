// MVP Implementation: Stores tokens effectively in a separate file (potentially with simple encryption later)
// For real secure storage we'd use something like specific OS keychains, but for a portable MVP:
// We will follow the text: "Secure storage for API tokens (using OS-specific credential stores/keychains where possible, or falling back to secure file permissions if needed/configured)."
// Since I cannot implement cross-platform keychain interactively easily without libs, I will usage a user-local file.
// Or actually, simple environment variable checking + local file fallback.

using System.Text.Json;
using Outlinectl.Core.Services;

namespace Outlinectl.Storage;

public class KeyStore : ISecureStore
{
    private readonly string _secretsPath;

    public KeyStore()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var folder = Path.Combine(appData, "outlinectl");
        Directory.CreateDirectory(folder);
        // In a real app we'd restrict ACLs here on creation
        _secretsPath = Path.Combine(folder, "secrets.json");
    }

    private async Task<Dictionary<string, string>> LoadSecretsAsync()
    {
        if (!File.Exists(_secretsPath)) return new Dictionary<string, string>();
        try
        {
            var json = await File.ReadAllTextAsync(_secretsPath);
            return JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
        }
        catch { return new Dictionary<string, string>(); }
    }

    private async Task SaveSecretsAsync(Dictionary<string, string> secrets)
    {
        var json = JsonSerializer.Serialize(secrets); // Minified for secrets
        await File.WriteAllTextAsync(_secretsPath, json);
        HardenSecretsFile();
    }

    public async Task<string?> GetTokenAsync(string profileName)
    {
        // Env var override for default?
        // Logic for Env var usually belongs in AuthService or higher, but storage is just storage.
        // We just return what we have.
        var secrets = await LoadSecretsAsync();
        return secrets.TryGetValue(profileName, out var token) ? token : null;
    }

    public async Task SetTokenAsync(string profileName, string token)
    {
        var secrets = await LoadSecretsAsync();
        secrets[profileName] = token;
        await SaveSecretsAsync(secrets);
    }

    public async Task DeleteTokenAsync(string profileName)
    {
        var secrets = await LoadSecretsAsync();
        if (secrets.Remove(profileName))
        {
            await SaveSecretsAsync(secrets);
        }
    }

    private void HardenSecretsFile()
    {
        if (OperatingSystem.IsWindows()) return;

        try
        {
            File.SetUnixFileMode(_secretsPath, UnixFileMode.UserRead | UnixFileMode.UserWrite);
        }
        catch
        {
            // Best-effort hardening; ignore if not supported.
        }
    }
}
