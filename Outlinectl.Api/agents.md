# Agent Guidelines for Outlinectl.Api

## Project Purpose

Outlinectl.Api implements HTTP communication with the Outline Wiki API. It provides the concrete implementation of `IOutlineApiClient` with resilience, authentication, and error handling.

## Architecture Role

- **API Integration**: Implements HTTP calls to Outline Wiki
- **Resilience**: Applies retry and circuit breaker patterns
- **Authentication**: Injects bearer tokens via message handler
- **Response Mapping**: Converts HTTP responses to DTOs

## Key Principles

### HttpClient Best Practices
- Use `IHttpClientFactory` for client management
- Avoid creating `HttpClient` directly
- Configure via dependency injection
- Leverage message handlers for cross-cutting concerns

### Resilience Patterns
- Use Polly for retries and circuit breakers
- Handle transient failures gracefully
- Configure appropriate timeouts
- Log retry attempts for diagnostics

### Authentication Flow
- Use `AuthHeaderHandler` for token injection
- Retrieve credentials from `IAuthService`
- Support profile-based authentication
- Never cache tokens in the handler

## Coding Guidelines

### API Client Methods
```csharp
public async Task<TResponse> MethodNameAsync(/* params */)
{
    await EnsureBaseUrlAsync(); // Set base URL from profile
    var response = await _httpClient.PostAsJsonAsync("api/endpoint", request);
    response.EnsureSuccessStatusCode(); // Throw on error
    return await response.Content.ReadFromJsonAsync<TResponse>();
}
```

### Message Handler Pattern
```csharp
protected override async Task<HttpResponseMessage> SendAsync(
    HttpRequestMessage request, 
    CancellationToken cancellationToken)
{
    // Modify request (add headers, etc.)
    return await base.SendAsync(request, cancellationToken);
}
```

### Error Handling
- Use `EnsureSuccessStatusCode()` for standard errors
- Catch and map specific HTTP status codes if needed
- Let Polly handle transient failures
- Propagate non-recoverable errors

## Common Modifications

### Adding a New API Endpoint
1. Add method signature to `IOutlineApiClient` (in Core)
2. Implement in `OutlineApiClient`:
   ```csharp
   public async Task<ResponseDto> NewEndpointAsync(params)
   {
       await EnsureBaseUrlAsync();
       var response = await _httpClient.PostAsJsonAsync("api/endpoint", body);
       response.EnsureSuccessStatusCode();
       return await response.Content.ReadFromJsonAsync<ResponseDto>();
   }
   ```
3. Create DTO classes in Core if needed
4. Add unit tests with mocked HttpClient

### Modifying Authentication
- Update `AuthHeaderHandler.SendAsync()`
- Ensure profile changes are reflected
- Test with multiple profiles
- Verify token refresh behavior

### Changing Resilience Policies
- Modify `AddStandardResilienceHandler()` configuration
- Consider custom Polly policies
- Test failure scenarios
- Document new behavior

### Adding Request/Response Logging
- Create a logging message handler
- Add to handler pipeline
- Respect `--quiet` and `--verbose` flags
- Sanitize sensitive data

## Testing Considerations

### Unit Tests
- Mock `HttpMessageHandler` or `HttpClient`
- Verify request body and headers
- Test error responses
- Check retry behavior

### Integration Tests
- Use test Outline instance
- Verify end-to-end flow
- Test authentication
- Validate response parsing

### Example Test Structure
```csharp
[Fact]
public async Task ListCollections_ReturnsData()
{
    // Arrange
    var mockHandler = new Mock<HttpMessageHandler>();
    // Setup mock response
    
    // Act
    var result = await _client.ListCollectionsAsync();
    
    // Assert
    Assert.NotNull(result);
}
```

## Common Pitfalls

- ❌ Creating HttpClient instances directly
- ❌ Caching tokens in message handlers (not thread-safe)
- ❌ Ignoring transient failures
- ❌ Hardcoding base URLs
- ✅ Use HttpClientFactory
- ✅ Retrieve tokens per-request from IAuthService
- ✅ Apply resilience patterns
- ✅ Use profile-based configuration

## Security Notes

- Never log bearer tokens
- Use HTTPS in production
- Validate SSL certificates
- Sanitize error messages (no token leakage)
- Use secure credential storage via `ISecureStore`

## Performance Considerations

- Reuse HttpClient via factory
- Use streaming for large responses
- Consider pagination for list operations
- Enable HTTP/2 when possible
- Pool connections appropriately

## Outline API Specifics

### API Patterns
- Most endpoints use POST (even for reads)
- Request body contains parameters
- Responses follow `{ data: T, policies: [] }` pattern
- Authentication uses Bearer tokens

### Common Headers
```
Authorization: Bearer <token>
Content-Type: application/json
```

### Error Responses
- 401: Unauthorized (invalid/expired token)
- 403: Forbidden (insufficient permissions)
- 404: Not Found (resource doesn't exist)
- 429: Rate Limited (too many requests)
