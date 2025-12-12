# PRD / Spec-Kit: **Outlinectl** — C# CLI wrapper for Outline Wiki APIs (agent/IDE “skills” friendly)

## 1) Product Summary
**Outlinectl** is a cross-platform (.NET 8) command-line tool that wraps the **Outline knowledge base/wiki** HTTP API into stable, scriptable commands and **machine-readable JSON outputs**, enabling coding agents/tools (Cursor, Kilo Code, Factory.ai Droid, Claude Code) to reliably **search, read, create, and update** Outline content as “skills”.

**One-liner value:** “Give developer agents a safe, deterministic interface to Outline wiki content.”

---

## 2) Problem Statement
Coding assistants frequently need a canonical source of internal knowledge (architecture decisions, runbooks, API contracts). Outline provides this via UI + API, but:
- Tooling is **not standardized** as a CLI for agent invocation.
- API usage requires **manual auth, pagination, formatting**, and dealing with inconsistent error handling.
- Agents need **deterministic outputs** (JSON), idempotency controls, and guardrails.

---

## 3) Goals / Non-Goals
### Goals
1. Provide a **stable CLI** with **predictable JSON** for agent workflows.
2. Support common knowledge workflows:
   - search docs
   - fetch doc by id/url/slugs
   - create/update documents
   - list collections
   - export markdown (single doc / subtree)
3. Support both **human-friendly** and **agent-friendly** modes:
   - `--text` for human-friendly output (default)
   - `--json` for structured output
   - `--quiet` and strict exit codes
4. Secure token handling suitable for CI and local dev.
5. Minimal friction install: `dotnet tool install -g outlinectl`.
6. Support a read-only mode, confirm before update and confirm before destructive operations.

### Non-Goals (initial)
- Full parity with every Outline API endpoint.
- Admin/SSO provisioning features (users/groups) in MVP.
- Two-way “folder sync” with conflict resolution in MVP (planned for later).

---

## 4) Primary Users & Use Cases
### Personas
- **Developer Agent / IDE tool**: runs commands non-interactively; requires JSON and stable schemas. uses wiki as a knowledge base for added context.
- **Developer / Tech Lead**: uses CLI to publish ADRs, update runbooks, search quickly.
- **CI/CD pipeline**: publishes release notes, migration notes, incident summaries.

### Top Use Cases (MVP)
1. “Search Outline for ‘rate limiter’ and return top 5 results with snippet. Search in an identified collection and subtree to pull relavant context for a programing task” 
2. “Get the doc content for ID X and output Markdown.”
3. “Create a new doc in Collection Y titled ‘ADR-012…’ and return its URL.”
4. “Update doc X replacing a section / appending a changelog entry.”

---

## 5) Assumptions & Dependencies
- Outline instance is reachable via a base URL, e.g. `https://outline.example.com`.
- An API token can be generated in Outline for programmatic access. API token is available in environment variables.
- Outline API offers endpoints for documents/collections/search (commonly exposed under `/api/...`).
- Tools will invoke this as a “skill” by executing commands and reading stdout/stderr + exit code.

---

## 6) UX & CLI Design Principles
1. **Deterministic**: same input → same output format.
2. **Non-interactive by default**: no prompts unless `--interactive`.
3. **Machine-readable**: `--json` returns strictly valid JSON; no extra text on stdout.
4. **Human-friendly**: default output is concise tables / summaries; `--verbose` for debugging.
5. **Safe**: supports `--dry-run`, confirmation for destructive actions, and redaction of secrets.

---

## 7) Command Surface (Spec)
**Binary name:** `outlinectl`

### Global flags
- `--base-url <url>` (optional if configured)
- `--token <token>` (optional; prefer env/config/credential store)
- `--json` (machine output)
- `--output <format>` = `json|text|markdown` (default `text`, `--json` implies `json`)
- `--quiet` (no human text; errors still on stderr)
- `--verbose` (debug diagnostics; redact secrets)
- `--timeout <seconds>` (default 30)
- `--dry-run` (prints intended action; no mutation)
- `--api-version <v>` (future-proofing; default `v1`)

### Config / Auth
#### `outlinectl auth login`
- Stores base URL + token securely.
- Options:
  - `--base-url`
  - `--token` (or read from stdin `--token-stdin`), first looks for env var `OUTLINE_API_TOKEN`.
  - `--profile <name>` (default `default`)
- Output (JSON): profile summary (token omitted).

#### `outlinectl auth status`
- Verifies connectivity/auth via a lightweight endpoint.
- Output: ok + server metadata if available.

#### `outlinectl auth logout`
- Removes token from secure store/config.

#### `outlinectl config set|get|list`
- Manages profiles; supports CI-friendly flat config.

### Collections
#### `outlinectl collections list`
- Params: `--limit`, `--offset`, `--all`
- Output fields: `id`, `name`, `url`, `createdAt`, `updatedAt`

### Documents
#### `outlinectl docs search --query "<text>"`
- Params:
  - `--collection-id <id>` optional
  - `--limit <n>` default 10
  - `--include-archived` default false
- Output fields: `id`, `title`, `url`, `snippet`, `updatedAt`, `collectionId`

#### `outlinectl docs get --id <docId>`
- Params:
  - `--format markdown|json` (default markdown for human; JSON for agents)
- Output:
  - Markdown: raw markdown on stdout (no extra text unless `--verbose`)
  - JSON: document object (id/title/text/… depending on API)

#### `outlinectl docs create`
- Required:
  - `--collection-id <id>`
  - `--title "<title>"`
- Content source (exactly one):
  - `--text "<markdown>"`
  - `--file <path.md>`
  - `--stdin`
- Options:
  - `--publish` (if Outline distinguishes draft/published)
  - `--parent-id <docId>` (create as child)
  - `--template-id <docId>` (optional future)
  - `--dedupe-key <string>` (idempotency key stored locally; see §10)
- Output: created doc id + url.

#### `outlinectl docs update --id <docId>`
- Content source (one):
  - `--text`
  - `--file`
  - `--stdin`
- Patch helpers (optional, later if Outline API supports partial updates):
  - `--append-file <path.md>`
  - `--prepend-text "<...>"`
- Output: updated doc metadata.

#### `outlinectl docs export --id <docId>`
- Exports a document (and optionally subtree) to a folder.
- Options:
  - `--dir <path>`
  - `--subtree` (include children)
  - `--rewrite-links relative|absolute` (default relative)
- Output: file paths written.

### Diagnostics
#### `outlinectl doctor`
- Checks config, auth, TLS, permissions, rate limiting.

---

## 8) Output Contract (Agent-Safe)
### Standard JSON envelope (when `--json` or `--output json`)
All commands MUST return:
```json
{
  "ok": true,
  "command": "docs.search",
  "data": { },
  "meta": {
    "requestId": "optional",
    "durationMs": 123,
    "pagination": { "limit": 10, "offset": 0, "nextOffset": 10 }
  }
}
```

On error:
```json
{
  "ok": false,
  "command": "docs.update",
  "error": {
    "code": "AUTH_FAILED",
    "message": "Unauthorized",
    "hint": "Run outlinectl auth login or set OUTLINE_TOKEN"
  },
  "meta": { "durationMs": 45 }
}
```

### Exit codes
- `0` success
- `2` invalid arguments / validation
- `3` auth failed
- `4` not found
- `5` conflict (edit collision / precondition failed)
- `6` rate limited / retryable failure
- `10` unknown/unhandled

---

## 9) API Mapping (Wrapper Layer)
> Exact endpoints vary by Outline deployment/version; design supports mapping via a single typed client.

Logical API operations (typical Outline API patterns):
- Collections:
  - `collections.list`
- Documents:
  - `documents.info` / `documents.get`
  - `documents.list`
  - `documents.search`
  - `documents.create`
  - `documents.update`
  - `documents.export` (if not native, implemented via get + traversal)
- Auth:
  - token-based bearer header (preferred)

Implementation detail:
- All HTTP calls go through `IOutlineApiClient`.
- Use `HttpClientFactory`, resilient retries for 429/5xx with exponential backoff.
- Support pagination parameters consistently (`limit`, `offset`) where applicable.

---

## 10) Idempotency & Safe Writes
### Idempotency
For agent usage, `docs create` should support **idempotent create**:
- `--dedupe-key <string>` stores a local mapping:
  - key → created document id/url
- In CI, allow external state via:
  - `--dedupe-db <path>` (sqlite) or `--dedupe-file <json>` (later)

### Concurrency control (optional, recommended)
If Outline exposes a revision/version field:
- `docs update` supports `--if-revision <n>`
- If mismatch → exit code `5` with conflict details.

---

## 11) Security & Compliance
- Token sources (priority order):
  1. `--token` flag
  2. `OUTLINE_TOKEN` env var
  3. secure credential store (OS keychain via .NET)
- Always redact tokens in logs and error output.
- Support `--ca-cert <path>` (optional) for self-hosted TLS.
- Avoid writing document content to disk unless explicitly requested (`export`, `--file`, etc.).

---

## 12) Architecture (C#/.NET)
### Tech stack
- .NET 10
- `System.CommandLine` (or Spectre.Console.Cli) for commands
- `HttpClient` + `Polly` for retries/timeouts
- `System.Text.Json` for strict JSON schemas

### Modules
- `Outlinectl.Cli` — command parsing, output selection, exit codes
- `Outlinectl.Core` — domain models, validation, pagination
- `Outlinectl.Api` — HTTP client, DTOs, auth headers, rate limit handling
- `Outlinectl.Storage` — profiles, credential store, optional dedupe db

---

## 13) Integration Patterns (“Skills”)
To support Cursor/Kilo/Factory.ai/Claude Code:
- Commands must be **single-shot**, non-interactive, and JSON-stable.
- Provide “skill recipes” in docs, e.g.:
  - `outlinectl docs search --query "..." --json`
  - `outlinectl docs get --id ... --output markdown`
  - `outlinectl docs update --id ... --stdin --json` (agent pipes content)

Recommended defaults for agent mode:
- `--json --quiet` (stdout only JSON, stderr only on failure)
- Add `OUTLINE_BASE_URL` and `OUTLINE_TOKEN` env vars in tool settings.


## 15) Acceptance Criteria (Testable)
1. Running `outlinectl docs search --query "x" --json` outputs valid JSON and exits `0`.
2. Invalid args exit `2` and return JSON error envelope when `--json`.
3. Auth failure exits `3` with actionable hint.
4. `docs get --id` can output raw markdown with no extra text (unless `--verbose`).
5. `docs create` returns created doc id + url; supports stdin content.
6. Retries occur on 429 and 5xx with bounded backoff; exit `6` if exhausted.
7. No tokens appear in logs or JSON outputs.

