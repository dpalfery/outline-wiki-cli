# Outlinectl.Cli

Command-line interface application for interacting with Outline Wiki.

## Overview

This is the main executable project that provides the CLI user experience. It uses System.CommandLine for argument parsing, Microsoft.Extensions.Hosting for dependency injection, and Serilog for logging.

## Structure

### Entry Point
- **Program.cs**: Application entry point with DI setup and command registration

### Commands (`Commands/`)
- **AuthCommand**: Authentication and profile management
  - `auth login`: Interactive login
  - `auth whoami`: Show current profile
  - `auth logout`: Remove credentials
  - `auth list`: List all profiles
  
- **CollectionsCommand**: Collection operations
  - `collections list`: List all collections
  
- **DocsCommand**: Document operations
  - `docs search`: Search documents
  - `docs get`: Get document by ID
  - `docs create`: Create new document
  - `docs update`: Update existing document
  - `docs delete`: Delete document

### Services (`Services/`)
- **OutputFormatter**: Formats output for human-readable or JSON modes
- **IOutputFormatter**: Output formatting interface

## Features

### Command-Line Parsing
Uses System.CommandLine for:
- Command hierarchies (auth, collections, docs)
- Options and arguments with validation
- Help text generation
- Tab completion support

### Dependency Injection
Uses Microsoft.Extensions.Hosting:
- Service registration in `Program.cs`
- Constructor injection in commands
- Scoped service lifetimes
- Configuration integration

### Output Modes
Supports multiple output formats:
- **Human**: Formatted, colorful output for terminals
- **JSON**: Structured output for scripting
- **Quiet**: Minimal output (errors only)

### Interactive Shell
Built-in REPL mode:
```bash
outlinectl shell
> help
> auth whoami
> docs search "query"
> exit
```

## Global Options

All commands support these global options:
- `--json`: Output strictly valid JSON
- `--quiet`: Suppress all non-error output
- `--verbose`: Enable verbose logging

## Dependencies

- **Outlinectl.Core**: Service interfaces and models
- **Outlinectl.Api**: API client implementation
- **Outlinectl.Storage**: Configuration and credential storage
- **System.CommandLine**: Command-line parsing
- **Microsoft.Extensions.Hosting**: Dependency injection and hosting
- **Serilog**: Structured logging

## Configuration

The CLI configures services in `Program.cs`:

```csharp
services.AddSingleton<IOutputFormatter, OutputFormatter>();
services.AddSingleton<IStore, FileStore>();
services.AddSingleton<ISecureStore, KeyStore>();
services.AddSingleton<IAuthService, AuthService>();
services.AddHttpClient<IOutlineApiClient, OutlineApiClient>()
    .AddHttpMessageHandler<AuthHeaderHandler>()
    .AddStandardResilienceHandler();
services.AddSingleton<IDocumentService, DocumentService>();
```

## Usage Examples

### Authentication
```bash
# Login to Outline
outlinectl auth login

# Check current profile
outlinectl auth whoami

# List all profiles
outlinectl auth list --json

# Logout
outlinectl auth logout
```

### Collections
```bash
# List collections
outlinectl collections list

# JSON output
outlinectl collections list --json
```

### Documents
```bash
# Search documents
outlinectl docs search "meeting notes"

# Get specific document
outlinectl docs get <document-id>

# Create document
outlinectl docs create --title "My Doc" --collection <id>

# Update document
outlinectl docs update <id> --title "New Title"

# Delete document
outlinectl docs delete <id>
```

## Error Handling

Centralized error handling in `Program.cs`:
- Maps exceptions to exit codes
- Formats errors based on output mode
- Provides helpful error messages
- Logs errors with Serilog

### Exit Codes
- `0`: Success
- `1`: General error
- `10`: Unknown error
- `130`: Cancelled (Ctrl+C)

## Logging

Configured with Serilog:
- Console output with timestamps
- Respects `--quiet` and `--verbose` flags
- Structured logging for machine parsing
- Adjusts level based on output mode

## Command Implementation Pattern

Commands follow this pattern:

```csharp
public class MyCommand : Command
{
    public MyCommand() : base("mycommand", "Description")
    {
        var option = new Option<string>("--opt", "Option description");
        AddOption(option);
        
        this.SetHandler(async (InvocationContext context) =>
        {
            var host = context.GetHost();
            var service = host.Services.GetRequiredService<IMyService>();
            var optValue = context.ParseResult.GetValueForOption(option);
            
            // Do work
            await service.DoSomethingAsync(optValue);
        });
    }
}
```

## Interactive Shell

The shell command provides a REPL:
- Parses input with custom tokenizer
- Handles quoted arguments
- Supports command history
- Exits with `exit` or `quit`

## Testing

CLI commands are tested via:
- Integration tests that invoke commands
- Mocked services for unit tests
- Output capture for validation
- Exit code verification

## Extension

### Adding a New Command
1. Create command class in `Commands/`
2. Inherit from `Command`
3. Define options/arguments in constructor
4. Implement handler with DI resolution
5. Register in `Program.cs`
6. Add tests
7. Update documentation

### Adding a New Option
1. Create `Option<T>` with description
2. Add to command with `AddOption()`
3. Retrieve in handler with `GetValueForOption()`
4. Add validation if needed
5. Update help text
