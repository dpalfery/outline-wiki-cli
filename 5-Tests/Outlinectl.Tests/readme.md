# Outlinectl.Tests

Unit and integration test suite for the Outlinectl solution.

## Overview

This project contains tests for all components of the Outlinectl application using xUnit and Moq for mocking.

## Structure

Test files mirror the structure of the projects they test:
- **ApiClientTests.cs**: Tests for OutlineApiClient
- **AuthServiceTests.cs**: Tests for AuthService
- **DocumentServiceTests.cs**: Tests for DocumentService

## Testing Framework

### xUnit
- Modern, extensible testing framework
- Theory-based tests for parameterized scenarios
- Parallel test execution
- Built-in assertion library

### Moq
- Mocking framework for dependencies
- Verify method calls and interactions
- Setup return values and behaviors
- Test isolation

### Coverage Tools
- **coverlet.collector**: Code coverage analysis
- **Microsoft.NET.Test.Sdk**: Test execution infrastructure

## Test Categories

### Unit Tests
Focus on individual components in isolation:
- Mock external dependencies
- Test business logic
- Verify error handling
- Check edge cases

### Integration Tests
Test components working together:
- Use real implementations where possible
- Test end-to-end scenarios
- Verify API interactions
- Test configuration loading

## Running Tests

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test /p:CollectCoverage=true

# Run specific test
dotnet test --filter "FullyQualifiedName~ApiClientTests"

# Run with verbosity
dotnet test --logger "console;verbosity=detailed"
```

## Test Structure

### Standard Test Pattern
```csharp
public class MyServiceTests
{
    [Fact]
    public async Task MethodName_Scenario_ExpectedResult()
    {
        // Arrange
        var mockDependency = new Mock<IDependency>();
        mockDependency
            .Setup(x => x.MethodAsync(It.IsAny<string>()))
            .ReturnsAsync("result");
        
        var service = new MyService(mockDependency.Object);
        
        // Act
        var result = await service.DoSomethingAsync("input");
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal("expected", result.Value);
        
        // Verify
        mockDependency.Verify(
            x => x.MethodAsync("input"),
            Times.Once
        );
    }
}
```

### Theory Tests
For parameterized tests:
```csharp
[Theory]
[InlineData("input1", "expected1")]
[InlineData("input2", "expected2")]
public async Task Method_WithVariousInputs_ReturnsCorrectOutput(
    string input, 
    string expected)
{
    // Arrange
    var service = new MyService();
    
    // Act
    var result = await service.ProcessAsync(input);
    
    // Assert
    Assert.Equal(expected, result);
}
```

## Mock Examples

### Mocking IAuthService
```csharp
var mockAuthService = new Mock<IAuthService>();
mockAuthService
    .Setup(x => x.GetCurrentProfileAsync())
    .ReturnsAsync(new Profile 
    { 
        Name = "test",
        BaseUrl = "https://test.example.com"
    });
```

### Mocking HttpClient
```csharp
var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
mockHttpMessageHandler
    .Protected()
    .Setup<Task<HttpResponseMessage>>(
        "SendAsync",
        ItExpr.IsAny<HttpRequestMessage>(),
        ItExpr.IsAny<CancellationToken>()
    )
    .ReturnsAsync(new HttpResponseMessage
    {
        StatusCode = HttpStatusCode.OK,
        Content = new StringContent(jsonResponse)
    });

var httpClient = new HttpClient(mockHttpMessageHandler.Object);
```

### Mocking IStore
```csharp
var mockStore = new Mock<IStore>();
mockStore
    .Setup(x => x.LoadConfigAsync())
    .ReturnsAsync(new Config { CurrentProfile = "test" });
```

## Test Coverage

Aim for:
- **80%+ code coverage** overall
- **100% coverage** for critical paths
- **Edge case coverage** for error handling
- **Integration coverage** for key workflows

### Critical Areas
- Authentication flow
- API client requests
- Error handling and retries
- Configuration persistence
- Token management

## Testing Best Practices

### Test Naming
Use descriptive names that explain:
- What is being tested
- Under what conditions
- What the expected outcome is

Format: `MethodName_Scenario_ExpectedBehavior`

Examples:
- `Login_WithValidToken_StoresCredentials`
- `SearchDocuments_WithInvalidQuery_ThrowsException`
- `LoadConfig_FileNotExists_ReturnsDefaultConfig`

### Arrange-Act-Assert
Always structure tests clearly:
1. **Arrange**: Set up dependencies and test data
2. **Act**: Execute the code under test
3. **Assert**: Verify the results

### Test Isolation
- Each test should be independent
- Don't rely on test execution order
- Clean up resources after tests
- Use fresh mocks for each test

### Avoid Test Smells
- ❌ Tests that depend on each other
- ❌ Tests with logic/loops
- ❌ Tests that test multiple things
- ❌ Slow tests (should be fast)
- ✅ One assertion per test (ideally)
- ✅ Clear, focused tests
- ✅ Fast, repeatable execution

## Testing Async Code

```csharp
[Fact]
public async Task AsyncMethod_Scenario_Result()
{
    // Arrange
    var service = new MyService();
    
    // Act
    var result = await service.AsyncMethodAsync();
    
    // Assert
    Assert.NotNull(result);
}
```

## Testing Exceptions

```csharp
[Fact]
public async Task Method_InvalidInput_ThrowsException()
{
    // Arrange
    var service = new MyService();
    
    // Act & Assert
    await Assert.ThrowsAsync<ArgumentException>(
        () => service.MethodAsync(null)
    );
}
```

## Debugging Tests

### Running Single Test
```bash
dotnet test --filter "FullyQualifiedName=Outlinectl.Tests.ApiClientTests.Login_Success"
```

### Visual Studio
- Right-click test → Debug Test
- Use breakpoints in test code
- Inspect mock setup and invocations

### Test Output
```csharp
[Fact]
public void Test_WithOutput()
{
    // Use ITestOutputHelper for test output
    _output.WriteLine("Debug information");
}
```

## Continuous Integration

Tests run automatically on:
- Pull request creation
- Commits to main branch
- Manual workflow trigger

### CI Configuration
- Run on multiple platforms (Windows, Linux, macOS)
- Generate coverage reports
- Fail build on test failures
- Report test results

## Extension

### Adding Tests for New Features
1. Create test file mirroring source structure
2. Write failing tests first (TDD)
3. Implement feature
4. Ensure tests pass
5. Refactor if needed
6. Add edge case tests

### Adding Integration Tests
1. Use real dependencies where safe
2. Consider test environment setup
3. Use test data that doesn't affect production
4. Clean up after tests

### Performance Tests
```csharp
[Fact]
public async Task Operation_CompletesInReasonableTime()
{
    var sw = Stopwatch.StartNew();
    
    await service.OperationAsync();
    
    sw.Stop();
    Assert.True(sw.ElapsedMilliseconds < 1000);
}
```
