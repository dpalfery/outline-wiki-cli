# Outlinectl.Api

HTTP client library for interacting with the Outline Wiki API.

## Overview

This project implements the `IOutlineApiClient` interface from Outlinectl.Core, providing HTTP communication with Outline Wiki instances. It includes resilience patterns, authentication handling, and API endpoint implementations.

## Structure

- **OutlineApiClient.cs**: Main API client implementation
- **AuthHeaderHandler.cs**: HTTP message handler for authentication headers

## Features

### API Client (`OutlineApiClient`)
Implements all Outline API endpoints:
- Collection listing
- Document search
- Document retrieval
- Document creation and updates
- Document deletion

### Authentication Handler (`AuthHeaderHandler`)
- Automatically injects authentication headers
- Retrieves tokens from `IAuthService`
- Supports profile-based authentication

### Resilience
Uses Polly via `AddStandardResilienceHandler()` for:
- Automatic retries with exponential backoff
- Circuit breaker pattern
- Timeout handling

## Dependencies

- **Outlinectl.Core**: Service interfaces and models
- **Microsoft.Extensions.Http**: HttpClient factory integration
- **Microsoft.Extensions.Http.Polly**: Resilience and retry policies

## Usage

The API client is registered in dependency injection:

```csharp
services.AddTransient<AuthHeaderHandler>();
services.AddHttpClient<IOutlineApiClient, OutlineApiClient>()
    .AddHttpMessageHandler<AuthHeaderHandler>()
    .AddStandardResilienceHandler(); // Polly defaults
```

### Example Usage

```csharp
public class MyService
{
    private readonly IOutlineApiClient _apiClient;

    public MyService(IOutlineApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task ListCollectionsAsync()
    {
        var response = await _apiClient.ListCollectionsAsync(limit: 25);
        foreach (var collection in response.Data)
        {
            Console.WriteLine(collection.Name);
        }
    }
}
```

## API Endpoints

### Collections
- `ListCollectionsAsync(limit, offset)`: List all collections

### Documents
- `SearchDocumentsAsync(query, collectionId, limit, offset)`: Search documents
- `GetDocumentAsync(documentId)`: Get document by ID
- `CreateDocumentAsync(title, text, collectionId)`: Create new document
- `UpdateDocumentAsync(documentId, title, text)`: Update existing document
- `DeleteDocumentAsync(documentId)`: Delete document

## Configuration

### Base URL
The base URL is dynamically set based on the current authentication profile:

```csharp
var profile = await _authService.GetProfileAsync(profileName);
_httpClient.BaseAddress = new Uri(profile.BaseUrl);
```

### Authentication
Authentication tokens are injected via `AuthHeaderHandler`:
- Retrieves token from `IAuthService`
- Adds `Authorization: Bearer <token>` header
- Handles profile switching

## Error Handling

- HTTP errors trigger `HttpRequestException`
- Network failures are handled by Polly resilience
- 401 Unauthorized indicates token issues
- 404 Not Found for missing resources

## Testing

The API client is tested using:
- Mocked `HttpMessageHandler`
- Mocked `IAuthService`
- Integration tests with test Outline instances

See `ApiClientTests.cs` in Outlinectl.Tests.

## Extension

To add a new API endpoint:

1. Add method to `IOutlineApiClient` interface (in Core)
2. Implement in `OutlineApiClient` class
3. Create DTO if needed (in Core)
4. Add unit tests
5. Update this documentation

## Security Considerations

- Tokens are never logged
- HTTPS is enforced for production
- Credentials flow through `ISecureStore`
- Authentication handler isolates token management
