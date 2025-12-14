using System.Text.Json;
using Outlinectl.Core.Domain;
using Outlinectl.Core.Services;

namespace Outlinectl.Storage;

public class FileStore : IStore
{
    private readonly string _configPath;
    private readonly JsonSerializerOptions _options = new() { WriteIndented = true };

    public FileStore()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var folder = Path.Combine(appData, "outlinectl");
        Directory.CreateDirectory(folder);
        _configPath = Path.Combine(folder, "config.json");
    }

    public async Task<Config> LoadConfigAsync()
    {
        if (!File.Exists(_configPath))
        {
            return new Config();
        }

        try
        {
            var json = await File.ReadAllTextAsync(_configPath);
            return JsonSerializer.Deserialize<Config>(json, _options) ?? new Config();
        }
        catch
        {
            return new Config();
        }
    }

    public async Task SaveConfigAsync(Config config)
    {
        var json = JsonSerializer.Serialize(config, _options);
        await File.WriteAllTextAsync(_configPath, json);
    }
}
