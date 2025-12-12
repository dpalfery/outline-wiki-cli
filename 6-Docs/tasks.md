# Implementation Plan: Outlinectl

## 1. Project Initialization and Scaffolding
- [x] 1.1 Initialize Solution and Projects
    - Create a new .NET 10 Solution.
    - Create `Outlinectl.Core` (Class Library).
    - Create `Outlinectl.Api` (Class Library).
    - Create `Outlinectl.Storage` (Class Library).
    - Create `Outlinectl.Cli` (Console App).
    - Create `Outlinectl.Tests` (xUnit Test Project).
    - Reference dependencies properly.
- [x] 1.2 Setup Global Exception Handling and Logger in CLI
    - Implement `Program.cs` entry point (Reference: Design - Outlinectl.Cli).
    - Setup Serilog or similar for structured logging (hidden by default).
    - Implement a global exception handler that catches unhandled exceptions and outputs the standardized JSON error envelope if `--json` is active.
- [x] 1.3 Implement Domain Models
    - Create `Document`, `Collection`, `Profile` records in `Outlinectl.Core` (Reference: Design - Data Models).
    - Create `JsonEnvelope<T>` and `ApiError` models.
- [x] 1.4 Setup Output Formatter
    - Create `OutputFormatter` service in `Outlinectl.Cli` to handle strictly typed JSON vs Text output (Reference: Requirements - Global CLI Behavior).

## 2. Authentication & Configuration
- [x] 2.1 Implement Storage Layer
    - Create `IStore` interface in `Outlinectl.Storage`.
    - Implement `FileStore` for managing `config.json` (Profiles).
    - Implement `KeyStore` for secure token storage (Mock/Simple for MVP first, then credential store).
- [x] 2.2 Implement Auth Service
    - Create `IAuthService` in `Outlinectl.Core`.
    - Implement `Login`, `Logout`, `GetStatus`.
    - Integrate with `IStore`.
- [x] 2.3 Implement Auth Commands
    - Create `AuthCommand` in `Outlinectl.Cli`:
        - `auth login` (Interactive/Stdin).
        - `auth status`.
        - `auth logout`.
    - Wiring in `Program.cs`.
- [x] 2.4 Verify Auth and Config
    - Add unit tests for `AuthService`.
    - Manual verification command.

## 3. API Client & Core Logic
- [x] 3.1 Implement API Client Infrastructure
    - Create `IOutlineApiClient` in `Outlinectl.Api`.
    - Configure `HttpClient` with `Polly` for retries (Reference: Requirements - Diagnostics points to retries).
    - Implement `AuthHeaderHandler` to inject tokens.
- [x] 3.2 Implement Collections Logic
    - Add `ListCollections` method to `IOutlineApiClient`.
    - Create `CollectionsCommand` in `Outlinectl.Cli` (`list`).
    - Connect `CollectionsCommand` -> `DocumentService` (or direct/dedicated service) -> `ApiClient`.
    - Verify with mock or real API.
- [x] 3.3 Implement Documents Read Logic
    - Add `GetDocument` and `SearchDocuments` to `IOutlineApiClient`.
    - Create `DocumentService` in `Outlinectl.Core`.
    - Create `DocsCommand` in `Outlinectl.Cli`:
        - `docs list` / `search`.
        - `docs get`.
    - Test JSON output correctness (Requirements - Retrieve Document Content).
- [x] 3.4 Implement Documents Write Logic
    - Add `CreateDocument`, `UpdateDocument` to `IOutlineApiClient`.
    - Add methods to `DocumentService`.
    - Update `DocsCommand`:
        - `docs create`.
        - `docs update`.
    - Implement idempotency check (`dedupe-key`).
- [x] 3.5 Implement Export Logic
    - Add `ExportDocument` logic in `DocumentService` (fetch + write to disk).
    - Handle `subtree` option (recursive fetch).
    - Update `DocsCommand`: `docs export`.

## 4. Polishing and Verification
- [ ] 4.1 Implement Diagnostics
    - Create `DoctorCommand`.
    - Check connectivity, config validity.
- [ ] 4.2 End-to-End Verification
    - Create a walkthrough script `walkthrough.md`.
    - Run through a full lifecycle (Auth -> Create -> Get -> Search -> Update -> Export).
## 6. Comprehensive Unit Tests
- [x] 6.1 DocumentService Tests
    - Test `SearchDocumentsAsync`, `CreateDocumentAsync`, `ExportDocumentAsync`.
- [ ] 6.2 ApiClient Tests
    - Mock HTTP responses.
    - Test `ListCollections`, `Search`, `Documents` endpoints.
