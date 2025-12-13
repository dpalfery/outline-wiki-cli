# Agent Guidelines for Outlinectl.Tests

## Project Purpose

Outlinectl.Tests provides comprehensive test coverage for all projects in the solution. It ensures code quality, catches regressions, and documents expected behavior.

## Architecture Role

- **Quality Assurance**: Verify functionality works as expected
- **Regression Prevention**: Catch breaking changes
- **Documentation**: Tests serve as usage examples
- **Design Feedback**: Testing reveals design issues

## Key Principles

### Test-Driven Development
- Write tests before or alongside code
- Tests define expected behavior
- Red-Green-Refactor cycle
- Tests guide design decisions

### Comprehensive Coverage
- Test happy paths
- Test error conditions
- Test edge cases
- Test boundary conditions

### Test Independence
- Each test runs in isolation
- No shared state between tests
- Order-independent execution
- Fast, repeatable tests

## Coding Guidelines

### Test Naming Convention
```
MethodName_Scenario_ExpectedBehavior
```

Examples:
```csharp
GetDocument_WithValidId_ReturnsDocument
GetDocument_WithInvalidId_ThrowsNotFoundException
GetDocument_WhenUnauthorized_ThrowsAuthException
```

### Test Structure (AAA Pattern)
```csharp
[Fact]
public async Task Method_Scenario_Result()
{
    // Arrange
    var mock = new Mock<IDependency>();
    mock.Setup(x => x.Method()).ReturnsAsync(value);
    var sut = new SystemUnderTest(mock.Object);
    
    // Act
    var result = await sut.ExecuteAsync();
    
    // Assert
    Assert.NotNull(result);
    Assert.Equal(expected, result.Property);
    
    // Verify (if needed)
    mock.Verify(x => x.Method(), Times.Once);
}
```

### Mocking Pattern
```csharp
// Setup return value
mockService
    .Setup(x => x.GetAsync(It.IsAny<string>()))
    .ReturnsAsync(testData);

// Setup exception
mockService
    .Setup(x => x.GetAsync("invalid"))
    .ThrowsAsync(new NotFoundException());

// Verify call was made
mockService.Verify(
    x => x.GetAsync("id"),
    Times.Once
);

// Verify call was never made
mockService.Verify(
    x => x.DeleteAsync(It.IsAny<string>()),
    Times.Never
);
```

## Common Modifications

### Adding Tests for New Feature
1. Create test file if needed
2. Add test class for component
3. Write failing test
4. Implement feature
5. Verify test passes
6. Add edge case tests

### Testing New Service
```csharp
public class NewServiceTests
{
    private readonly Mock<IDependency> _mockDependency;
    private readonly NewService _service;
    
    public NewServiceTests()
    {
        _mockDependency = new Mock<IDependency>();
        _service = new NewService(_mockDependency.Object);
    }
    
    [Fact]
    public async Task NewMethod_HappyPath_ReturnsExpected()
    {
        // Arrange
        _mockDependency
            .Setup(x => x.GetDataAsync())
            .ReturnsAsync(testData);
        
        // Act
        var result = await _service.NewMethodAsync();
        
        // Assert
        Assert.NotNull(result);
    }
}
```

### Testing API Client
```csharp
[Fact]
public async Task ApiCall_Success_ReturnsData()
{
    // Arrange
    var mockHandler = CreateMockHttpHandler(
        HttpStatusCode.OK,
        jsonResponse
    );
    var httpClient = new HttpClient(mockHandler.Object);
    var client = new OutlineApiClient(httpClient, mockAuthService.Object);
    
    // Act
    var result = await client.GetDataAsync();
    
    // Assert
    Assert.NotNull(result);
    Assert.NotEmpty(result.Data);
}

private Mock<HttpMessageHandler> CreateMockHttpHandler(
    HttpStatusCode statusCode,
    string content)
{
    var mockHandler = new Mock<HttpMessageHandler>();
    mockHandler
        .Protected()
        .Setup<Task<HttpResponseMessage>>(
            "SendAsync",
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>()
        )
        .ReturnsAsync(new HttpResponseMessage
        {
            StatusCode = statusCode,
            Content = new StringContent(content)
        });
    return mockHandler;
}
```

### Testing Exception Handling
```csharp
[Fact]
public async Task Method_InvalidInput_ThrowsArgumentException()
{
    // Arrange
    var service = new MyService();
    
    // Act & Assert
    var exception = await Assert.ThrowsAsync<ArgumentException>(
        () => service.MethodAsync(null)
    );
    
    Assert.Contains("parameter cannot be null", exception.Message);
}
```

### Theory Tests for Multiple Cases
```csharp
[Theory]
[InlineData("", false)]
[InlineData("short", false)]
[InlineData("valid-token-12345", true)]
public void ValidateToken_VariousInputs_ReturnsExpected(
    string token,
    bool expected)
{
    // Arrange
    var validator = new TokenValidator();
    
    // Act
    var result = validator.IsValid(token);
    
    // Assert
    Assert.Equal(expected, result);
}
```

## Testing Patterns

### Constructor-Based Setup
```csharp
public class MyServiceTests
{
    private readonly Mock<IDependency> _mockDep;
    private readonly MyService _sut;
    
    public MyServiceTests()
    {
        _mockDep = new Mock<IDependency>();
        _sut = new MyService(_mockDep.Object);
    }
    
    [Fact]
    public async Task Test1() { /* ... */ }
    
    [Fact]
    public async Task Test2() { /* ... */ }
}
```

### IDisposable for Cleanup
```csharp
public class FileStoreTests : IDisposable
{
    private readonly string _tempPath;
    private readonly FileStore _store;
    
    public FileStoreTests()
    {
        _tempPath = Path.GetTempFileName();
        _store = new FileStore(_tempPath);
    }
    
    public void Dispose()
    {
        if (File.Exists(_tempPath))
            File.Delete(_tempPath);
    }
}
```

### Fixture for Shared Setup
```csharp
public class ApiClientTestFixture : IDisposable
{
    public Mock<IAuthService> MockAuthService { get; }
    
    public ApiClientTestFixture()
    {
        MockAuthService = new Mock<IAuthService>();
        // Shared setup
    }
    
    public void Dispose()
    {
        // Cleanup
    }
}

public class ApiClientTests : IClassFixture<ApiClientTestFixture>
{
    private readonly ApiClientTestFixture _fixture;
    
    public ApiClientTests(ApiClientTestFixture fixture)
    {
        _fixture = fixture;
    }
}
```

## Common Pitfalls

- ❌ Tests with multiple assertions (should test one thing)
- ❌ Tests that depend on execution order
- ❌ Tests with complex logic or loops
- ❌ Slow tests (network, file I/O without mocks)
- ❌ Tests that share mutable state
- ✅ Fast, focused tests
- ✅ Clear arrange-act-assert structure
- ✅ Proper mocking of dependencies
- ✅ Independent, isolated tests

## Testing Best Practices

### Mock Verification
```csharp
// Verify exact call
mock.Verify(x => x.Method("arg"), Times.Once);

// Verify with any argument
mock.Verify(x => x.Method(It.IsAny<string>()), Times.Once);

// Verify never called
mock.Verify(x => x.Delete(It.IsAny<string>()), Times.Never);

// Verify at least once
mock.Verify(x => x.Log(It.IsAny<string>()), Times.AtLeastOnce);
```

### Assertion Best Practices
```csharp
// Specific assertions
Assert.Equal(expected, actual);
Assert.True(condition, "failure message");
Assert.Contains("substring", actualString);
Assert.Empty(collection);
Assert.NotNull(result);

// Collection assertions
Assert.Single(collection);
Assert.Equal(3, collection.Count);
Assert.All(collection, item => Assert.NotNull(item));

// Exception assertions
await Assert.ThrowsAsync<SpecificException>(() => code);
```

### Test Data Management
```csharp
// Use meaningful test data
var testProfile = new Profile
{
    Name = "test-profile",
    BaseUrl = "https://test.example.com"
};

// Use builder pattern for complex objects
var document = new DocumentBuilder()
    .WithTitle("Test Doc")
    .WithContent("Content")
    .Build();

// Use constants for magic values
private const string TestToken = "test-token-12345";
private const string TestProfileName = "test-profile";
```

## Performance Testing

### Timing Tests
```csharp
[Fact]
public async Task Operation_CompletesQuickly()
{
    // Arrange
    var sw = Stopwatch.StartNew();
    
    // Act
    await service.FastOperationAsync();
    
    // Assert
    sw.Stop();
    Assert.True(
        sw.ElapsedMilliseconds < 100,
        $"Operation took {sw.ElapsedMilliseconds}ms"
    );
}
```

### Memory Tests
```csharp
[Fact]
public void Operation_DoesNotLeakMemory()
{
    // Arrange
    var before = GC.GetTotalMemory(forceFullCollection: true);
    
    // Act
    for (int i = 0; i < 1000; i++)
    {
        service.Operation();
    }
    
    // Assert
    GC.Collect();
    var after = GC.GetTotalMemory(forceFullCollection: true);
    var delta = after - before;
    Assert.True(delta < 1_000_000, $"Memory grew by {delta} bytes");
}
```

## Coverage Analysis

### Running Coverage
```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

### Coverage Goals
- **Overall**: 80%+ coverage
- **Critical paths**: 100% coverage
- **Public APIs**: High coverage
- **Error handling**: Comprehensive coverage

### Coverage Exclusions
- DTOs (simple property bags)
- Program.cs entry point
- UI/presentation code
- Platform-specific code that can't be tested

## CI/CD Integration

### Test Execution
- Run on every commit
- Run on pull requests
- Generate coverage reports
- Fail build on test failures

### Test Reports
- xUnit XML output
- Coverage reports (OpenCover/Cobertura)
- Test timing data
- Failure screenshots (if applicable)
