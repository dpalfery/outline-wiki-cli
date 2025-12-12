# Agent Guidelines for Outlinectl.Core

## Project Purpose

Outlinectl.Core is the foundation layer containing domain models, DTOs, and service interfaces. It has no external dependencies and defines the contracts used by all other projects.

## Architecture Role

- **Zero Dependencies**: This project should have no external package dependencies (only .NET runtime)
- **Contract Definition**: All service interfaces are defined here
- **Domain Logic**: Business rules and domain models live here
- **Data Transfer**: DTOs for API communication

## Key Principles

### Dependency Direction
- Other projects depend on Core (Api, Storage, Cli, Tests → Core)
- Core depends on nothing (except .NET runtime)
- Core should never reference other projects in this solution

### Service Interfaces
- Define service interfaces in `Services/` directory
- Implementations belong in other projects (Api, Storage, etc.)
- Use interfaces for testability and loose coupling

### Domain vs DTO
- **Domain models** (`Domain/`) represent business concepts
- **DTOs** (`DTOs/`) represent API wire format
- Keep them separate for flexibility

## Coding Guidelines

### Namespace Organization
```
Outlinectl.Core.Domain      // Business entities
Outlinectl.Core.DTOs         // API data transfer objects
Outlinectl.Core.Services     // Service interfaces and implementations
Outlinectl.Core.Common       // Shared utilities and models
```

### Interface Design
- Prefix interfaces with `I` (e.g., `IAuthService`)
- Use async methods with `Async` suffix
- Return `Task<T>` for async operations
- Use nullable reference types appropriately

### Domain Models
- Use properties with init setters where immutability is desired
- Include validation logic if needed
- Keep models focused and cohesive

### DTOs
- Use simple POCOs for serialization
- Match Outline API response structure
- Use appropriate JSON attributes if needed

## Common Modifications

### Adding a New Service Interface
1. Create interface in `Services/IYourService.cs`
2. Define async method signatures
3. Document expected behavior with XML comments
4. Implement in appropriate project (Api/Storage/etc.)

### Adding a New Domain Model
1. Create class in `Domain/YourModel.cs`
2. Define properties and validation
3. Consider immutability needs
4. Add XML documentation

### Adding a New DTO
1. Create class in `DTOs/YourDto.cs`
2. Match API response structure
3. Use appropriate serialization attributes
4. Keep it simple (no business logic)

### Modifying Existing Services
- Update interface definition first
- Update all implementations in other projects
- Add tests for new behavior
- Consider backwards compatibility

## Testing Considerations

- Core interfaces are mocked in unit tests
- Service implementations are tested via their concrete classes
- Use test projects to verify interface contracts
- Keep Core models testable without external dependencies

## Common Pitfalls

- ❌ Adding external package dependencies
- ❌ Referencing other projects in the solution
- ❌ Mixing domain logic with DTO serialization
- ❌ Creating concrete implementations (use other projects)
- ✅ Define clear interface contracts
- ✅ Keep domain models simple and focused
- ✅ Maintain zero external dependencies

## Security Notes

- Never include credentials in domain models
- Use `ISecureStore` interface for sensitive data
- Domain models should be serialization-safe
- Validate input in service implementations

## Performance Considerations

- Keep models lightweight
- Avoid complex computations in properties
- Use async/await for I/O operations
- Consider memory allocation for large collections
