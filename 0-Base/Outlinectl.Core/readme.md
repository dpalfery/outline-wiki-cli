# Outlinectl.Core

Core library containing domain models, DTOs, and service interfaces for the Outlinectl solution.

## Overview

This project contains the foundational elements of the Outlinectl application with no external dependencies (except .NET runtime). It defines the contracts and domain models used by all other projects.

## Structure

### Domain Models (`Domain/`)
- **Config**: Application configuration including profiles and API settings
- **Collection**: Represents an Outline collection
- **Document**: Represents an Outline document
- **Profile**: Authentication profile for an Outline instance

### Data Transfer Objects (`DTOs/`)
- **CollectionDto**: API response for collections
- **DocumentDto**: API response for documents
- **SearchResultDto**: API response for search results
- **StandardListResponse<T>**: Generic paginated list response

### Services (`Services/`)

#### Interfaces
- **IStore**: Configuration persistence interface
- **ISecureStore**: Secure credential storage interface
- **IAuthService**: Authentication and profile management
- **IOutlineApiClient**: Outline API client contract
- **IDocumentService**: Document operations service

#### Implementations
- **AuthService**: Manages authentication profiles and credentials
- **DocumentService**: Handles document operations and formatting

### Common (`Common/`)
- **ApiError**: Error response model
- **JsonEnvelope**: Generic wrapper for JSON responses
- **OutputFormat**: Enum for output formatting (Human/Json)

## Key Design Principles

### No External Dependencies
This project has no external dependencies beyond the .NET runtime, making it portable and reusable.

### Interface Segregation
Service interfaces are defined here but implemented in other projects:
- `IStore` and `ISecureStore` → Implemented in Outlinectl.Storage
- `IOutlineApiClient` → Implemented in Outlinectl.Api

### Domain-Driven Design
Domain models represent business concepts independently of data transfer concerns. DTOs handle API serialization/deserialization.

## Usage Example

```csharp
// Service usage (typically via dependency injection)
public class MyService
{
    private readonly IAuthService _authService;
    private readonly IDocumentService _documentService;

    public MyService(IAuthService authService, IDocumentService documentService)
    {
        _authService = authService;
        _documentService = documentService;
    }

    public async Task DoWorkAsync()
    {
        var profile = await _authService.GetCurrentProfileAsync();
        // Use profile...
    }
}
```

## Features

### Authentication Service
- Profile management (create, update, delete, list)
- Current profile tracking
- Token validation support

### Document Service
- Document search and retrieval
- Content formatting
- Collection filtering

## Extension Points

To extend the Core functionality:
1. Add new domain models in `Domain/`
2. Add corresponding DTOs in `DTOs/`
3. Define service interfaces in `Services/`
4. Implement services in appropriate projects

## Testing

This project is tested via Outlinectl.Tests. Mock implementations of interfaces are used for testing dependent services.
