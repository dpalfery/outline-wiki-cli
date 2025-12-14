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

# Clean Architecture + DDD Folder Structure (C#)

## **0-Base Layer**

**Purpose:** Cross-cutting or shared concerns used across all layers.

**Project:** `HotshotLogistics.Core`

**Contents:**

* **Dependency Injection / Config Extensions**
* **Logging, Email, Caching Adapters**
* **External API Integrations**
* **Constants / Enums / Utilities**
* **Factories / Contracts Shared Across Layers**
* **Base Exceptions:** Custom exception types used across the solution
  *Folder:* `Exceptions`
* **Base Repositories:** Shared repository interfaces and base implementations
  *Folder:* `Repositories`

> This is the foundational layer that other layers may reference for shared utilities and abstractions.

---

## **1-Presentation Layer**

**Purpose:** Entry point for all user interactions (HTTP, gRPC, SignalR, etc.)

**Projects:** `HotshotLogistics.Api`, `HotshotLogistics.UI`, or `HotshotLogistics.Web`

**Contents:**

* **Controllers / Endpoints:** ASP.NET Core API controllers or minimal APIs
  *Folder:* `Controllers`
* **Hubs:** SignalR hubs for real-time updates
  *Folder:* `Hubs`
* **Filters / Middleware:** Exception handling, logging, request validation
  *Folder:* `Middleware`
* **ViewModels / DTOs:** Request/response payloads specific to presentation
  *Folder:* `DTOs`
* **Static Content / Pages:** Razor pages or SPA static assets
  *Folder:* `wwwroot` or `Pages`
* **Program.cs / Startup.cs:** Composition root, DI setup, and pipeline config

> This layer calls into `2-Application` only. It does not directly reference persistence or domain implementations.

---

## **2-Application Layer**

**Purpose:** Orchestrates use cases and enforces application logic, coordinating between domain and infrastructure.

**Project:** `HotshotLogistics.Application`

**Contents:**

* **Services / Use Cases:** Application service classes implementing workflows
  *Folder:* `Services`
* **Commands / Queries / Handlers:** CQRS pattern logic, mediator handlers
  *Folder:* `Features` or `Handlers`
* **Validators:** Input validation (FluentValidation, custom logic)
  *Folder:* `Validators`
* **Authorization:** Policy providers, role/claim checks
  *Folder:* `Authorization`
* **DTOs:** Input/output models for use cases
  *Folder:* `DTOs`
* **Events / Notifications:** Application-level events or mediators
  *Folder:* `Events`
* **Dependency Injection Extensions:**
  *File:* `ServiceCollectionExtensions.cs`

> This layer depends only on `3-Domain` and `4-Contracts`.
> Contains no UI or infrastructure logic.

---

## **3-Domain Layer**

**Purpose:** Pure business logic, rules, and core models.

**Projects:**

* `HotshotLogistics.Domain` → concrete domain models and logic
* `HotshotLogistics.Contracts` → shared interfaces and abstractions

**Contents:**

* **Entities:** Aggregate roots and domain entities
  *Folder:* `Entities`
  *Example:* `Order.cs`, `Job.cs`
* **ValueObjects:** Immutable types without identity
  *Folder:* `ValueObjects`
* **Domain Services:** Business rules not tied to entities
  *Folder:* `Services`
* **Factories:** Construction logic enforcing invariants
  *Folder:* `Factories` (in `Contracts` if shared)
* **Repositories / Interfaces:** Domain contracts for persistence and messaging
  *Folder:* `Repositories`, `Hubs`, etc. (in `Contracts`)
* **Domain Events:** Core events representing state changes
  *Folder:* `Events`
* **DTOs (Domain-Scoped):** Internal payloads used inside domain boundaries
  *Folder:* `DTOs`
* **Dependencies:** Shared abstractions for dependency registration
* **README.md:** Explain model boundaries and design rules

> The domain is completely persistence-agnostic and unaware of infrastructure.

---

## **4-Persistence Layer**

**Purpose:** Implements data storage and retrieval using EF Core, Dapper, or external stores.

**Project:** `HotshotLogistics.Persistence`

**Contents:**

* **DbContext:** EF Core database context
  *Folder:* `Contexts`
* **Entity Configurations:** Mapping, relationships, and constraints
  *Folder:* `Configurations`
* **Repositories:** Implementation of domain repository interfaces
  *Folder:* `Repositories`
* **Migrations / Seed Data:** Database migrations and seeders
  *Folder:* `Migrations`, `Seed`
* **ReadModels / Projections:** Optimized models for queries
  *Folder:* `ReadModels`
* **README.md:** Connection strings, migration usage, conventions

> References `3-Domain` and `4-Contracts`, but never `1-Presentation`.

---

## **5-Test Layer**

**Purpose:** Centralized location for all test projects covering unit, integration, and UI tests.

**Projects:**

* `HotshotLogistics.Tests` → Unit tests for domain, application, and services
* `HotshotLogistics.IntegrationTests` → Integration tests for API and persistence
* `HotshotLogistics.Playwright-UI.Tests` → End-to-end UI tests using Playwright

**Contents:**

* **Unit Tests:** Test individual classes, methods, and business logic in isolation
  *Folder:* `HotshotLogistics.Tests`
* **Integration Tests:** Test API endpoints, database operations, and cross-layer interactions
  *Folder:* `HotshotLogistics.IntegrationTests`
* **UI Tests:** End-to-end browser-based tests for the admin dashboard
  *Folder:* `HotshotLogistics.Playwright-UI.Tests`
* **Test Data:** Shared fixtures, mocks, and sample data for tests
  *Folder:* `data`

> All test projects should reference their target projects but remain isolated from production code paths.
> Follow naming convention: `{ClassName}Tests.cs` for unit tests, `{Feature}IntegrationTests.cs` for integration tests.

---

## **6-Docs**

**Purpose:** Internal documentation, architectural decisions, and tasks.

* `architecture-general.md`
* `interface-cleanup-tasks.md`
* Diagrams, design notes, checklists

---

## **7-Deployment**

**Purpose:** CI-CD scripts, Docker Files, anything that helps with the deployment of the solution either locally or in production.

* `Docker files`
* `scripts`
* **important rule**: when deciding on which sku to use in Azure you must pick from teh free service skus provided as part of the 1 year azure free account

## **8-Agent-Instructions**
**Purpose:** Instructions for the AI agent to follow when generating code.

* `architecture-general.md`
* `interface-cleanup-tasks.md`
* `agent-instructions.md`



## Important Considerations

- **Environment**: This project runs on Windows with PowerShell 5.1. DO NOT use the `&&` operator in CLI commands. Use `;` or separate `Execute` tool calls instead.
- **Security**: Never log or expose API tokens in plain text
- **Cross-platform**: Ensure file paths work on Windows, Linux, and macOS
- **Backwards Compatibility**: Configuration format changes need migration support
- **User Experience**: Support both interactive and JSON output modes
