# Outline Wiki CLI Instructions for Claude

This file provides Claude with instructions for interacting with Outline Wiki via the `outlinectl` CLI.

## Overview

Use the `outlinectl` CLI for:
- Authenticating against an Outline instance
- Listing collections
- Searching, getting, creating, updating, and exporting documents

**Always prefer `--json` output** for structured, parseable responses.

## Configuration

This CLI is **environment-first**. Read configuration from environment variables and only pass explicit CLI options when overriding defaults.

| Variable | Purpose | Example |
|----------|---------|---------|
| `OUTLINE_BASE_URL` | Outline instance URL | `https://docs.example.com` |
| `OUTLINE_API_TOKEN` | API authentication token | (secret) |

Optional:
- `profile`: Defaults to `default` (used for local config/credential store)

**Important notes:**
- The CLI does **not** read a default collection ID from environment variables.
- For `docs search`, provide `--collection-id` and/or `--parent-id` explicitly when you need scoped results.
- When `--parent-id` is provided, the user is looking for a sub-document—search all sub-documents of that parent for the relevant information.

## Security Requirements

- **Never print or log API tokens** in responses or tool outputs.
- Default login is environment-based. Use `--token-stdin` when you cannot set `OUTLINE_API_TOKEN` safely (e.g., piping from a secret manager).
- For runtime auth, `OUTLINE_API_TOKEN` overrides the stored token.

## Running the CLI

From the repository root:

```powershell
dotnet run --project .\Outlinectl.Cli -- <command> [options]
```

- Everything after `--` is passed to the CLI.
- Global options available on all commands: `--json`, `--quiet`, `--verbose`.

## Output Contract (JSON Mode)

When `--json` is set, stdout returns a single JSON envelope:

**Success:**
```json
{
  "ok": true,
  "command": "docs.search",
  "data": { },
  "meta": { "durationMs": 0, "version": "1.0.0" }
}
```

**Error:**
```json
{
  "ok": false,
  "command": "docs.get",
  "error": { "code": "", "message": "...", "hint": "..." },
  "meta": { "durationMs": 0, "version": "1.0.0" }
}
```

## Exit Codes

| Code | Meaning |
|------|---------|
| `0` | Success |
| `2` | Invalid input / missing required values |
| `3` | Not logged in (local status check) |
| `4` | Document not found / get failed |
| `10` | Unknown/unhandled error |
| `130` | Cancelled (Ctrl+C) |

## Required Workflow: Authenticate First

**Before running any commands, you MUST authenticate and verify success.**

### Step 1: Login

```powershell
dotnet run --project .\Outlinectl.Cli -- auth login --profile default --json
```

If `--base-url` and `--token` are omitted, the CLI pulls from `OUTLINE_BASE_URL` and `OUTLINE_API_TOKEN`.

**Success criteria:**
- Exit code is `0`
- JSON envelope has `"ok": true`

### Step 2: Verify Status

```powershell
dotnet run --project .\Outlinectl.Cli -- auth status --json
```

**If authentication fails, stop and inform the user before proceeding.**

## Command Reference

### List Collections

```powershell
dotnet run --project .\Outlinectl.Cli -- collections list --limit 50 --offset 0 --json
```

### Search Documents

```powershell
dotnet run --project .\Outlinectl.Cli -- docs search --query "<SEARCH_TERM>" --collection-id "<COLLECTION_ID>" --parent-id "<PARENT_DOC_ID>" --limit 10 --offset 0 --json
```

**Notes:**
- `--query` is required.
- Treat `--collection-id` and `--parent-id` as required inputs for scoped searches. If you don't have these values, ask the user.

### Get Document

**Structured JSON output:**
```powershell
dotnet run --project .\Outlinectl.Cli -- docs get --id "<DOC_ID>" --format json --json
```

**Human-readable markdown** (do NOT use `--json`):
```powershell
dotnet run --project .\Outlinectl.Cli -- docs get --id "<DOC_ID>" --format markdown
```

### Create Document

**From stdin (preferred for large content):**
```powershell
@"
# Title

Body goes here.
"@ | dotnet run --project .\Outlinectl.Cli -- docs create --title "My Doc" --stdin --json
```

**From file:**
```powershell
dotnet run --project .\Outlinectl.Cli -- docs create --title "My Doc" --collection-id "<COLLECTION_ID>" --file ".\doc.md" --json
```

### Update Document

```powershell
dotnet run --project .\Outlinectl.Cli -- docs update --id "<DOC_ID>" --title "New Title" --file ".\updated.md" --json
```

### Export Document

Export markdown files to a directory (optionally including descendants):

```powershell
dotnet run --project .\Outlinectl.Cli -- docs export "<DOC_ID>" --output-dir ".\export" --subtree --json
```

## Interactive Shell

Use only for manual exploration—avoid for automation:

```powershell
dotnet run --project .\Outlinectl.Cli -- shell
```

Inside the shell:
```text
docs search --query "onboarding" --json
auth status --json
exit
```

## Best Practices for Claude

1. **Always authenticate first** and verify with `auth status` before any other operation.
2. **Use `--json` output** for all automated operations to ensure parseable responses.
3. **Check `"ok": true`** in JSON responses before considering an operation successful.
4. **Ask the user** for `--collection-id` or `--parent-id` if not provided and required for the task.
5. **Never expose API tokens** in responses, logs, or displayed commands.
6. **Handle errors gracefully**—check exit codes and error messages in JSON responses.
