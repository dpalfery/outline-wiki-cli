# Agent Guidelines for Outlinectl

This document provides guidelines for AI agents working on the Outlinectl project.

## Project Context

Outlinectl is a .NET 10-based CLI tool for interacting with Outline Wiki. The solution follows a clean architecture pattern with separated concerns across multiple projects.

## Architecture Overview

- **Outlinectl.Core**: Domain models, DTOs, and service abstractions
- **Outlinectl.Api**: Outline API client with HTTP resilience
- **Outlinectl.Storage**: File-based configuration and secure credential storage
- **Outlinectl.Cli**: CLI application using System.CommandLine
- **Outlinectl.Tests**: Unit tests using xUnit and Moq

## Technology Stack

- **.NET 10**: Latest .NET version
- **System.CommandLine**: Command-line parsing and hosting
- **Serilog**: Structured logging
- **Polly**: HTTP resilience and retry policies
- **xUnit**: Testing framework
- **Moq**: Mocking library

## Coding Standards

### General Principles
- Enable nullable reference types (`<Nullable>enable</Nullable>`)
- Use implicit usings where appropriate
- Follow C# naming conventions (PascalCase for public members, camelCase for private)
- Prefer dependency injection over static dependencies
- Keep interfaces in the Core project

### Architecture Patterns
- **Dependency Flow**: Cli → Api/Storage → Core (Core has no dependencies)
- **Service Layer**: Business logic in Core.Services
- **DTO Pattern**: Use DTOs for API communication, Domain models for business logic
- **Command Pattern**: CLI commands are organized under Commands/ directory

### Error Handling
- Use exception handling middleware in CLI
- Map exceptions to appropriate exit codes
- Support JSON output mode for programmatic consumption
- Use structured logging with Serilog

### Testing
- Write unit tests for service layer
- Mock external dependencies (HTTP, file system)
- Use xUnit assertions
- Aim for meaningful test coverage

## Common Tasks

### Adding a New Command
1. Create command class in `Outlinectl.Cli/Commands/`
2. Inherit from `Command` base class
3. Define options and arguments
4. Implement handler with dependency injection via `SetHandler`
5. Register command in `Program.cs`

### Adding a New API Endpoint
1. Add method signature to `IOutlineApiClient` in Core
2. Implement in `OutlineApiClient` in Api project
3. Create DTOs if needed in Core/DTOs
4. Add unit tests in Tests project

### Adding Configuration
1. Update `Config` domain model in Core
2. Modify serialization in `FileStore` if needed
3. Update relevant services to use new configuration

## Project-Specific Guidelines

See individual project agent guidelines:
- [Outlinectl.Core/agents.md](./Outlinectl.Core/agents.md)
- [Outlinectl.Api/agents.md](./Outlinectl.Api/agents.md)
- [Outlinectl.Storage/agents.md](./Outlinectl.Storage/agents.md)
- [Outlinectl.Cli/agents.md](./Outlinectl.Cli/agents.md)
- [Outlinectl.Tests/agents.md](./Outlinectl.Tests/agents.md)

## Build and Test

```bash
# Restore dependencies
dotnet restore

# Build solution
dotnet build

# Run tests
dotnet test

# Run CLI
cd Outlinectl.Cli
dotnet run
```

## Important Considerations

- **Security**: Never log or expose API tokens in plain text
- **Cross-platform**: Ensure file paths work on Windows, Linux, and macOS
- **Backwards Compatibility**: Configuration format changes need migration support
- **User Experience**: Support both interactive and JSON output modes
