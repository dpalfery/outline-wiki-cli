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

## Installing

### npm (global install)

If you want an install experience like `npm i -g ...`, this repo includes an npm wrapper package that downloads a prebuilt `outlinectl` binary from GitHub Releases.

```bash
npm i -g @dpalfery/outlinectl
```

This requires Node.js on the target machine and a GitHub Release for the matching version that includes these assets:

- `outlinectl-win-x64.exe`
- `outlinectl-linux-x64`
- `outlinectl-osx-x64`
- `outlinectl-osx-arm64`

Tip: This repo includes a GitHub Actions workflow that builds and uploads these assets automatically when you push a tag like `v0.1.0`.

Recommended: use npm Trusted Publishing (OIDC) so you don't need long-lived publish tokens or 2FA-bypass tokens in CI. Configure a trusted publisher for this repo/workflow on npmjs.com, then use `.github/workflows/publish-npm.yml`.

The npm package looks for assets at:

`https://github.com/dpalfery/outline-wiki-cli/releases/download/v<version>/<asset>`

You can override the download location for private mirrors with `OUTLINECTL_DOWNLOAD_BASE` or `OUTLINECTL_TAG` (see [npm/outlinectl/README.md](npm/outlinectl/README.md)).

### Direct binary (no Node required)

To deploy to machines without Node.js, publish a self-contained single-file binary and copy it to the target machine.

Example (Windows x64):

```bash
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true
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

### Environment variables

- `OUTLINE_API_TOKEN`: Outline API token.
	- Used as a global override (if set, it is used instead of any stored token).
	- Useful for CI/CD: set the env var and run commands without writing secrets to disk.
- `OUTLINE_BASE_URL`: Outline instance base URL (e.g. `https://docs.example.com`).
	- Currently used as a fallback for `outlinectl auth login` when `--base-url` is omitted.

### Auth options

`outlinectl auth login` supports these options:

- `--base-url <url>`: Outline base URL (falls back to `OUTLINE_BASE_URL`).
- `--token <token>`: API token (overrides `OUTLINE_API_TOKEN`).
- `--token-stdin`: read the token from stdin (recommended to avoid shell history).
- `--profile <name>`: profile name (default: `default`). Login sets the current profile.

Tokens are stored separately from config:
- Windows: `%APPDATA%\outlinectl\secrets.json`
- Linux/macOS: `~/.config/outlinectl/secrets.json`

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
