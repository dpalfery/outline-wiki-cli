using Moq;
using Outlinectl.Core.Domain;
using Outlinectl.Core.Services;
using Xunit;

namespace Outlinectl.Tests;

public class AuthServiceTests
{
    private readonly Mock<IStore> _mockStore;
    private readonly Mock<ISecureStore> _mockSecureStore;
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        _mockStore = new Mock<IStore>();
        _mockSecureStore = new Mock<ISecureStore>();
        _service = new AuthService(_mockStore.Object, _mockSecureStore.Object);
    }

    [Fact]
    public async Task LoginAsync_ShouldSaveUrlAndToken()
    {
        // Arrange
        var config = new Config();
        _mockStore.Setup(s => s.LoadConfigAsync()).ReturnsAsync(config);

        // Act
        await _service.LoginAsync("https://example.com/", "my-token", "prod");

        // Assert
        Assert.Equal("prod", config.CurrentProfile);
        Assert.True(config.Profiles.ContainsKey("prod"));
        Assert.Equal("https://example.com", config.Profiles["prod"].BaseUrl); // Should verify trimming
        
        _mockStore.Verify(s => s.SaveConfigAsync(It.IsAny<Config>()), Times.Once);
        _mockSecureStore.Verify(s => s.SetTokenAsync("prod", "my-token"), Times.Once);
    }

    [Fact]
    public async Task LogoutAsync_ShouldRemoveToken()
    {
        // Act
        await _service.LogoutAsync("prod");

        // Assert
        _mockSecureStore.Verify(s => s.DeleteTokenAsync("prod"), Times.Once);
    }

    [Fact]
    public async Task GetTokenAsync_ShouldReturnTokenFromSecureStore()
    {
        // Arrange
        Environment.SetEnvironmentVariable("OUTLINE_API_TOKEN", null);
        _mockSecureStore.Setup(s => s.GetTokenAsync("default")).ReturnsAsync("secret-token");

        // Act
        var token = await _service.GetTokenAsync("default");

        // Assert
        Assert.Equal("secret-token", token);
    }

    [Fact]
    public async Task GetTokenAsync_ShouldPrioritizeEnvVar()
    {
        // This test involves Environment Variables which are process-global.
        // It might be flaky if running in parallel or affect other tests.
        // We will skip this or be careful.
        // Let's set env var, test, clear env var.
        
        Environment.SetEnvironmentVariable("OUTLINE_API_TOKEN", "env-token");
        try
        {
            _mockSecureStore.Setup(s => s.GetTokenAsync("default")).ReturnsAsync("store-token");
            
            var token = await _service.GetTokenAsync("default");
            
            Assert.Equal("env-token", token);
        }
        finally
        {
            Environment.SetEnvironmentVariable("OUTLINE_API_TOKEN", null);
        }
    }
}
