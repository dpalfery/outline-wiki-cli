# Agent Guidelines for Outlinectl.Storage

## Project Purpose

Outlinectl.Storage provides concrete implementations for configuration persistence and secure credential storage. It handles all file I/O and OS-specific credential management.

## Architecture Role

- **Configuration Persistence**: Implements `IStore` for config files
- **Secure Storage**: Implements `ISecureStore` for credentials
- **Cross-Platform**: Works on Windows, Linux, and macOS
- **Security**: Ensures tokens are never stored in plain text

## Key Principles

### Separation of Concerns
- Configuration (non-sensitive) → FileStore → JSON files
- Credentials (sensitive) → KeyStore → OS credential manager
- Never mix sensitive and non-sensitive storage

### Platform Independence
- Use .NET abstractions for file paths
- Leverage OS-native credential storage
- Test on all target platforms
- Handle platform-specific edge cases

### Security First
- Never store tokens in config files
- Use OS credential manager APIs
- Apply appropriate file permissions
- Encrypt sensitive data at rest

## Coding Guidelines

### File Store Pattern
```csharp
public async Task<Config> LoadConfigAsync()
{
    if (!File.Exists(_configPath))
        return new Config(); // Default
    
    try
    {
        var json = await File.ReadAllTextAsync(_configPath);
        return JsonSerializer.Deserialize<Config>(json) ?? new Config();
    }
    catch
    {
        return new Config(); // Graceful degradation
    }
}
```

### Secure Store Pattern
```csharp
public async Task SetAsync(string key, string value)
{
    // Use OS-specific credential APIs
    // Windows: CredWrite
    // macOS: Keychain
    // Linux: libsecret
}
```

### Path Handling
```csharp
// ✅ Correct
var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
var folder = Path.Combine(appData, "outlinectl");

// ❌ Incorrect
var folder = "C:\\Users\\...\\outlinectl"; // Windows-specific
```

## Common Modifications

### Changing Configuration Format
1. Update `Config` model in Core
2. Implement migration logic in `LoadConfigAsync`
3. Test with old and new formats
4. Document breaking changes

### Adding New Secure Values
1. Add methods to `ISecureStore` interface (in Core)
2. Implement in `KeyStore`
3. Use consistent key naming
4. Add tests

### Supporting New Platforms
1. Detect platform with `RuntimeInformation`
2. Implement platform-specific credential access
3. Add conditional compilation if needed
4. Test on target platform

### Migration Strategy
```csharp
public async Task<Config> LoadConfigAsync()
{
    var config = await LoadRawConfigAsync();
    
    // Check version and migrate if needed
    if (config.Version < 2)
    {
        config = MigrateToV2(config);
        await SaveConfigAsync(config);
    }
    
    return config;
}
```

## Testing Considerations

### Unit Tests
- Use temp directories for test files
- Clean up after tests
- Mock OS credential APIs
- Test error conditions

### Integration Tests
- Test actual file I/O
- Verify cross-platform paths
- Test concurrent access
- Validate permissions

### Test Structure
```csharp
[Fact]
public async Task SaveConfig_CreatesFile()
{
    // Arrange
    var tempPath = Path.GetTempFileName();
    var store = new FileStore(tempPath);
    
    // Act
    await store.SaveConfigAsync(new Config());
    
    // Assert
    Assert.True(File.Exists(tempPath));
    
    // Cleanup
    File.Delete(tempPath);
}
```

## Common Pitfalls

- ❌ Hardcoding file paths
- ❌ Storing tokens in JSON files
- ❌ Using platform-specific path separators
- ❌ Not handling missing files gracefully
- ✅ Use Environment.SpecialFolder
- ✅ Store tokens in OS credential manager
- ✅ Use Path.Combine for paths
- ✅ Return defaults for missing data

## Security Notes

### Token Storage
- NEVER store tokens in config.json
- ALWAYS use KeyStore for tokens
- Validate token before storing
- Clear tokens on profile deletion

### File Permissions
- Config files should be user-readable only
- On Unix, set permissions to 600
- Check write permissions before saving
- Handle permission errors gracefully

### Encryption
- OS credential managers handle encryption
- Don't implement custom crypto
- Trust platform security mechanisms
- Document security assumptions

## Platform-Specific Notes

### Windows
- Use `CredWrite`/`CredRead` APIs
- Store in Generic Credentials
- Target name: `outlinectl:{profileName}`

### macOS
- Use Security framework
- Store in Keychain
- Service name: `outlinectl`
- Account name: `{profileName}`

### Linux
- Use libsecret via D-Bus
- Requires Secret Service daemon
- Schema: `outlinectl.profile`
- Attribute: `profile={profileName}`

## Performance Considerations

- Cache config in memory when possible
- Avoid repeated file I/O
- Use async file operations
- Consider file locking for concurrent access

## Error Handling

### File Errors
- Missing file → Return default config
- Permission denied → Throw descriptive error
- Corrupt JSON → Return default, log warning
- Disk full → Throw, don't corrupt existing file

### Credential Errors
- Not found → Return null
- Access denied → Throw with helpful message
- OS error → Wrap and rethrow with context
