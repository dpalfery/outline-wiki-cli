# Outlinectl.Storage

Storage implementation for configuration and secure credential management.

## Overview

This project provides file-based storage implementations for the `IStore` and `ISecureStore` interfaces defined in Outlinectl.Core. It handles persisting application configuration and securely storing API tokens.

## Structure

- **FileStore.cs**: JSON-based configuration storage
- **KeyStore.cs**: Platform-specific secure credential storage

## Features

### File Store (`FileStore`)
Implements `IStore` interface for configuration persistence:
- Stores configuration in JSON format
- Located in application data folder
- Handles serialization/deserialization
- Creates directories automatically

### Key Store (`KeyStore`)
Implements `ISecureStore` interface for secure credential storage:
- Uses OS-native credential storage
- Windows: Credential Manager
- macOS: Keychain
- Linux: Secret Service API (libsecret)
- Tokens never stored in plain text

## Configuration Location

Configuration is stored in platform-specific locations:

| Platform | Location |
|----------|----------|
| Windows  | `%APPDATA%\outlinectl\config.json` |
| Linux    | `~/.config/outlinectl/config.json` |
| macOS    | `~/Library/Application Support/outlinectl/config.json` |

## Usage

Both stores are registered in dependency injection:

```csharp
services.AddSingleton<IStore, FileStore>();
services.AddSingleton<ISecureStore, KeyStore>();
```

### File Store Example

```csharp
public class ConfigService
{
    private readonly IStore _store;

    public ConfigService(IStore store)
    {
        _store = store;
    }

    public async Task<Config> GetConfigAsync()
    {
        return await _store.LoadConfigAsync();
    }

    public async Task SaveConfigAsync(Config config)
    {
        await _store.SaveConfigAsync(config);
    }
}
```

### Key Store Example

```csharp
public class AuthService
{
    private readonly ISecureStore _secureStore;

    public AuthService(ISecureStore secureStore)
    {
        _secureStore = secureStore;
    }

    public async Task SaveTokenAsync(string profileName, string token)
    {
        await _secureStore.SetAsync(profileName, token);
    }

    public async Task<string?> GetTokenAsync(string profileName)
    {
        return await _secureStore.GetAsync(profileName);
    }
}
```

## Configuration Format

Configuration is stored as JSON:

```json
{
  "currentProfile": "default",
  "profiles": {
    "default": {
      "name": "default",
      "baseUrl": "https://your-outline.example.com",
      "apiToken": null
    }
  }
}
```

Note: `apiToken` in config is null; actual tokens are stored securely in KeyStore.

## Security Features

### Secure Token Storage
- Tokens stored in OS credential manager
- Encrypted at rest by OS
- Never written to config.json
- Isolated per profile

### File Permissions
- Config files have user-only read/write on Unix
- Respects OS file permissions
- No sensitive data in plain text files

## Error Handling

### File Store
- Returns empty config if file doesn't exist
- Returns empty config on deserialization errors
- Creates directory structure automatically
- Handles concurrent access gracefully

### Key Store
- Returns null if credential not found
- Throws on OS credential manager errors
- Handles profile deletion
- Supports credential updates

## Cross-Platform Considerations

### Path Handling
- Uses `Path.Combine()` for cross-platform paths
- Uses `Environment.SpecialFolder` for system directories
- Respects platform conventions

### Credential Storage
- Windows: Windows Credential Manager API
- macOS: Keychain via Security framework
- Linux: libsecret via D-Bus

## Testing

Storage implementations are tested with:
- Temporary file system locations
- Mocked credential storage
- Cross-platform path verification
- Concurrent access tests

## Extension

### Adding New Storage Types
1. Define interface in Core
2. Implement in Storage project
3. Register in DI container
4. Add unit tests

### Migrating Configuration
When changing config format:
1. Load old format
2. Convert to new format
3. Save and backup old version
4. Document migration in release notes
