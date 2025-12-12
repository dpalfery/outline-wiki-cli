using Moq;
using Outlinectl.Core.DTOs;
using Outlinectl.Core.Services;
using Xunit;

namespace Outlinectl.Tests;

public class DocumentServiceTests
{
    private readonly Mock<IOutlineApiClient> _mockApiClient;
    private readonly Mock<IStore> _mockStore;
    private readonly DocumentService _service;

    public DocumentServiceTests()
    {
        _mockApiClient = new Mock<IOutlineApiClient>();
        _mockStore = new Mock<IStore>();
        _service = new DocumentService(_mockApiClient.Object, _mockStore.Object);
    }

    [Fact]
    public async Task SearchDocumentsAsync_ShouldPassCorrectParameters()
    {
        // Arrange
        var expectedResponse = new StandardListResponse<SearchResultDto> { Data = new List<SearchResultDto>() };
        _mockApiClient
            .Setup(x => x.SearchDocumentsAsync("query", "col1", 20, 5))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _service.SearchDocumentsAsync("query", "col1", 20, 5, false);

        // Assert
        Assert.Same(expectedResponse, result);
        _mockApiClient.Verify(x => x.SearchDocumentsAsync("query", "col1", 20, 5), Times.Once);
    }

    [Fact]
    public async Task CreateDocumentAsync_ShouldMapParametersCorrectly()
    {
        // Arrange
        var returnedDoc = new DocumentDto { Id = "doc1", Title = "New Doc" };
        _mockApiClient
            .Setup(x => x.CreateDocumentAsync(It.Is<CreateDocumentRequest>(
                r => r.Title == "New Doc" && r.CollectionId == "col1" && r.Text == "content" && r.ParentDocumentId == "p1"
            )))
            .ReturnsAsync(returnedDoc);

        // Act
        var result = await _service.CreateDocumentAsync("New Doc", "col1", "content", "p1", null);

        // Assert
        Assert.Equal("doc1", result.Id);
    }

    [Fact]
    public async Task ExportDocumentAsync_ShouldSaveFile()
    {
        // Verification of recursive export logic often requires mocking multiple calls.
        // And File IO. DocumentService uses File.WriteAllTextAsync which is hard to mock unless wrapped.
        // However, we can test that GetDocumentAsync and ListDocumentsAsync are called correctly.
        // We will skip File IO verification for this unit test or assume we trust the I/O if calls are correct.
        // Ideally we wrap IFileSystem, but for MVP we tested manually.
        // Let's at least verify it fetches the doc.

        // Arrange
        var doc = new DocumentDto { Id = "d1", Title = "My Doc", Text = "Hello", CollectionId = "c1" };
        _mockApiClient.Setup(x => x.GetDocumentAsync("d1")).ReturnsAsync(doc);

        var outputDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(outputDir);

        try
        {
            // Act
            await _service.ExportDocumentAsync("d1", outputDir, false);

            // Assert
            var filePath = Path.Combine(outputDir, "My Doc.md");
            Assert.True(File.Exists(filePath));
            Assert.Equal("Hello", await File.ReadAllTextAsync(filePath));
        }
        finally
        {
            if (Directory.Exists(outputDir)) Directory.Delete(outputDir, true);
        }
    }
}
