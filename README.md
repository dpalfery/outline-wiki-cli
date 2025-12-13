# Outlinectl - Outline Wiki CLI

A command-line interface (CLI) tool for interacting with Outline Wiki, built with .NET 10.

## Overview

Outlinectl provides a powerful CLI to manage and interact with your Outline Wiki instance. It supports authentication, document management, collection browsing, and more.

## Architecture

The solution is organized into five projects:

- **Outlinectl.Core**: Core domain models, DTOs, and service interfaces
- **Outlinectl.Api**: HTTP client implementation for the Outline API
- **Outlinectl.Storage**: File-based storage and secure keystore implementations
- **Outlinectl.Cli**: Main CLI application with command-line interface
- **Outlinectl.Tests**: Unit tests for the solution

## Prerequisites

- .NET 10.0 SDK or later
- An Outline Wiki instance with API access

## Building

```bash
dotnet restore
dotnet build
```

## Running

```bash
cd Outlinectl.Cli
dotnet run
```

## Usage

### Authentication

```bash
# Login to your Outline instance
outlinectl auth login

# View current profile
outlinectl auth whoami
```

### Collections

```bash
# List all collections
outlinectl collections list
```

### Documents

```bash
# Search for documents
outlinectl docs search "query"
```

### Interactive Shell

```bash
# Run in interactive mode
outlinectl shell
```

## Global Options

- `--json`: Output strictly valid JSON
- `--quiet`: Suppress all non-error output
- `--verbose`: Enable verbose logging

## Configuration

Configuration is stored in your application data folder:
- Windows: `%APPDATA%\outlinectl\config.json`
- Linux/macOS: `~/.config/outlinectl/config.json`

## Development

See individual project readme files for more details:
- [Outlinectl.Core](./Outlinectl.Core/readme.md)
- [Outlinectl.Api](./Outlinectl.Api/readme.md)
- [Outlinectl.Storage](./Outlinectl.Storage/readme.md)
- [Outlinectl.Cli](./Outlinectl.Cli/readme.md)
- [Outlinectl.Tests](./Outlinectl.Tests/readme.md)

## Testing

```bash
dotnet test
```

## License

See [LICENSE](./LICENSE) file for details.
