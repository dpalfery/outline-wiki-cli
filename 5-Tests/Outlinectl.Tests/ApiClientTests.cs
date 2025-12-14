using Moq;
using Moq.Protected;
using System.Net;
using System.Text.Json;
using Outlinectl.Api;
using Outlinectl.Core.DTOs;
using Outlinectl.Core.Services;
using Xunit;

namespace Outlinectl.Tests;

public class ApiClientTests
{
    private readonly Mock<HttpMessageHandler> _handlerMock;
    private readonly Mock<IAuthService> _authMock;
    private readonly OutlineApiClient _client;

    public ApiClientTests()
    {
        _handlerMock = new Mock<HttpMessageHandler>();
        _authMock = new Mock<IAuthService>();
        
        var httpClient = new HttpClient(_handlerMock.Object)
        {
            BaseAddress = new Uri("https://api.example.com/")
        };

        // Setup Auth Service to return profile (Client calls EnsureBaseUrlAsync)
        _authMock.Setup(x => x.GetCurrentProfileNameAsync()).ReturnsAsync("default");
        _authMock.Setup(x => x.GetProfileAsync("default")).ReturnsAsync(new Core.Domain.Profile { BaseUrl = "https://api.example.com" });

        _client = new OutlineApiClient(httpClient, _authMock.Object);
    }

    [Fact]
    public async Task ListCollectionsAsync_ShouldReturnCollections()
    {
        // Arrange
        var responseData = new StandardListResponse<CollectionDto>
        {
            Data = new List<CollectionDto>
            {
                new CollectionDto { Id = "c1", Name = "Collection 1" }
            }
        };

        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r => r.Method == HttpMethod.Post && r.RequestUri.ToString().EndsWith("api/collections.list")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(responseData))
            });

        // Act
        var result = await _client.ListCollectionsAsync();

        // Assert
        Assert.Single(result.Data);
        Assert.Equal("c1", result.Data[0].Id);
    }

    [Fact]
    public async Task CreateDocumentAsync_ShouldPostCorrectData()
    {
        // Arrange
        var responseDoc = new StandardResponse<DocumentDto>
        {
            Data = new DocumentDto { Id = "d1", Title = "New Doc" }
        };

        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r => 
                    r.Method == HttpMethod.Post && 
                    r.RequestUri.ToString().EndsWith("api/documents.create")
                    // Could inspect body content here too if desired
                ),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(responseDoc))
            });

        // Act
        var request = new CreateDocumentRequest { Title = "New Doc", CollectionId = "c1" };
        var result = await _client.CreateDocumentAsync(request);

        // Assert
        Assert.Equal("d1", result.Id);
    }

    [Fact]
    public async Task ListCollectionsAsync_WhenForbidden_ShouldThrowWithStatusAndBody()
    {
        // Arrange
        const string errorBody = "{\"message\":\"Forbidden: token scope insufficient\"}";

        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r => r.Method == HttpMethod.Post && r.RequestUri!.ToString().EndsWith("api/collections.list")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Forbidden,
                ReasonPhrase = "Forbidden",
                Content = new StringContent(errorBody)
            });

        // Act
        var ex = await Assert.ThrowsAsync<HttpRequestException>(() => _client.ListCollectionsAsync());

        // Assert
        Assert.Contains("403", ex.Message);
        Assert.Contains("Forbidden", ex.Message);
        Assert.Contains("token scope insufficient", ex.Message);
    }

    [Fact]
    public async Task EnsureBaseUrlAsync_ShouldThrowWhenProfileMissing()
    {
        // Arrange
        _authMock.Reset();
        _authMock.Setup(x => x.GetCurrentProfileNameAsync()).ReturnsAsync("default");
        _authMock.Setup(x => x.GetProfileAsync("default")).ReturnsAsync((Core.Domain.Profile?)null);

        var httpClient = new HttpClient(_handlerMock.Object);
        var client = new OutlineApiClient(httpClient, _authMock.Object);

        // Act / Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => client.ListCollectionsAsync());
    }
}
