---
name: outline-cli
description: Allow droids to interact with Outline Wiki via the outlinectl CLI (auth, collections, docs) with automation-friendly JSON output.
---

This skill enables droids to operate an Outline Wiki instance through the included .NET CLI (`Outlinectl.Cli`).

Use this skill for:
- Authenticating against an Outline instance
- Listing collections
- Searching, getting, creating, updating, and exporting documents

For automation, prefer `--json` output and non-interactive commands.

## Configuration
This skill is **environment-first**. Droids should read configuration from environment variables and only pass explicit CLI options when overriding defaults.

- `OUTLINE_BASE_URL` (recommended): Outline instance URL (e.g. `https://docs.example.com`)
- `OUTLINE_API_TOKEN` (recommended): Outline API token
- `OUTLINE_COLLECTION_ID` (optional): Default collection ID for `docs create` and for filtering `docs search`
- `profile` (optional): Defaults to `default` (used for local config/credential store)

## Safety and Secrets
- Never print or log API tokens.
- Prefer passing tokens via stdin: `--token-stdin`.
- For runtime auth (API calls), `OUTLINE_API_TOKEN` overrides the stored token.

## How to Run

From the repo (recommended for droids working in this workspace):

```powershell
cd Outlinectl.Cli
dotnet run -- --help
```

Notes:
- Everything after `--` is passed to the CLI.
- Global options are supported on all commands: `--json`, `--quiet`, `--verbose`.

## Output Contract (JSON Mode)

When `--json` is set, stdout is a single JSON envelope:

```json
{
	"ok": true,
	"command": "docs.search",
	"data": { },
	"meta": { "durationMs": 0, "version": "1.0.0" }
}
```

On errors:

```json
{
	"ok": false,
	"command": "docs.get",
	"error": { "code": "", "message": "...", "hint": "..." },
	"meta": { "durationMs": 0, "version": "1.0.0" }
}
```

## Exit Codes (Practical Handling)
- `0`: Success
- `2`: Invalid input / missing required values (e.g., missing token)
- `3`: Not logged in (local status check)
- `4`: Document not found / get failed
- `10`: Unknown/unhandled error
- `130`: Cancelled (Ctrl+C)

## Core Workflows

### 1) Login (Non-Interactive)

Preferred (stdin):

```powershell
$env:OUTLINE_BASE_URL = "https://docs.example.com"
$env:OUTLINE_PROFILE = "default"
$env:OUTLINE_API_TOKEN = "<TOKEN>"

# If --base-url and --token are omitted, outlinectl will pull from OUTLINE_BASE_URL and OUTLINE_API_TOKEN.
dotnet run --project .\\Outlinectl.Cli -- auth login --profile $env:OUTLINE_PROFILE --json
```

Alternative (inline token; avoid if possible):

```powershell
dotnet run --project .\\Outlinectl.Cli -- auth login --base-url "https://docs.example.com" --token "<TOKEN>" --profile default --json
```

### 2) Verify Auth Status

```powershell
dotnet run --project .\Outlinectl.Cli -- auth status --json
```

### 3) List Collections

```powershell
dotnet run --project .\Outlinectl.Cli -- collections list --limit 50 --offset 0 --json
```

### 4) Search Documents

```powershell
dotnet run --project .\Outlinectl.Cli -- docs search --query "onboarding" --limit 10 --offset 0 --json
```

Optional filter:

```powershell
$env:OUTLINE_COLLECTION_ID = "<COLLECTION_ID>"

# If --collection-id is omitted, outlinectl will pull from OUTLINE_COLLECTION_ID.
dotnet run --project .\\Outlinectl.Cli -- docs search --query "policy" --json
```

### 5) Get Document

Automation / structured:

```powershell
dotnet run --project .\Outlinectl.Cli -- docs get --id "<DOC_ID>" --format json --json
```

Human-readable markdown (stdout is raw markdown text; do NOT use `--json`):

```powershell
dotnet run --project .\Outlinectl.Cli -- docs get --id "<DOC_ID>" --format markdown
```

### 6) Create Document

From stdin (preferred for large content):

```powershell
@"
# Title

Body goes here.
"@ | dotnet run --project .\\Outlinectl.Cli -- docs create --title "My Doc" --stdin --json
```

From file:

```powershell
dotnet run --project .\Outlinectl.Cli -- docs create --title "My Doc" --collection-id "<COLLECTION_ID>" --file ".\doc.md" --json
```

### 7) Update Document

```powershell
dotnet run --project .\Outlinectl.Cli -- docs update --id "<DOC_ID>" --title "New Title" --file ".\updated.md" --json
```

### 8) Export Document

Exports markdown files to a directory (optionally including descendants):

```powershell
dotnet run --project .\Outlinectl.Cli -- docs export "<DOC_ID>" --output-dir ".\export" --subtree --json
```

## Interactive Shell (Optional)

Use the shell only for manual exploration; avoid it for automation:

```powershell
dotnet run --project .\Outlinectl.Cli -- shell
```

Inside the shell, run commands as you would normally:

```text
docs search --query "onboarding" --json
auth status --json
exit
```

