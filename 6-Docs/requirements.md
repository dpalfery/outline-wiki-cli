# Requirements Document: Outlinectl

## Introduction
Outlinectl is a cross-platform (.NET 8) command-line tool that wraps the Outline knowledge base API into stable, scriptable commands and machine-readable JSON outputs. Its primary purpose is to enable coding agents (like Cursor, Kilo Code, Factory.ai Droid) and developers to reliably search, read, create, and update Outline content as a "skill". It prioritizes deterministic outputs, secure token handling, and ease of use in automated workflows.

## Requirements

### Requirement 1: Authentication and Configuration
**User Story:** As a developer or agent, I want to authentic functionality so that I can securely access the Outline instance.

#### Acceptance Criteria
1. WHEN the user runs `auth login` with a valid base URL and token THEN the system SHALL store the credentials securely and return a success profile.
2. WHEN the user provides a token via stdin using `--token-stdin` THEN the system SHALL read the token without exposing it in command history.
3. IF the `OUTLINE_API_TOKEN` environment variable is set THEN the system SHALL prioritize it over stored credentials if applicable or use it for `auth login`.
4. WHEN the user runs `auth status` THEN the system SHALL verify connectivity using the stored credentials and return an "ok" status with server metadata.
5. WHEN the user runs `auth logout` THEN the system SHALL remove the stored token from the secure store.
6. WHEN the user runs `config list` THEN the system SHALL display the current configuration profiles.

### Requirement 2: List Collections
**User Story:** As a user/agent, I want to list available document collections so that I can discover where documents are stored.

#### Acceptance Criteria
1. WHEN `collections list` is executed THEN the system SHALL return a list of collections including `id`, `name`, and `url`.
2. IF `--limit` or `--offset` parameters are provided THEN the system SHALL paginate the results accordingly.
3. IF `--json` is specified THEN the system SHALL return the list in the standard JSON envelope.

### Requirement 3: Search Documents
**User Story:** As an agent, I want to search for documents using keywords so that I can retrieve relevant context for a task.

#### Acceptance Criteria
1. WHEN `docs search` is executed with a `--query` string THEN the system SHALL return a list of matching documents with snippets.
2. IF `--collection-id` is specified THEN the system SHALL filter results to that specific collection.
3. IF `--include-archived` is set to true THEN the system SHALL include archived documents in the results.
4. WHEN `--json` is used THEN the output SHALL be a structured JSON list meant for machine consumption.

### Requirement 4: Retrieve Document Content
**User Story:** As a user/agent, I want to get the full content of a specific document so that I can read it.

#### Acceptance Criteria
1. WHEN `docs get` is executed with a valid `--id` THEN the system SHALL return the document content.
2. IF `--format markdown` is specified (or default) THEN the system SHALL output raw Markdown content to stdout.
3. IF `--format json` is specified THEN the system SHALL return the full document object as JSON.
4. IF the document is not found THEN the system SHALL return exit code 4 and an error message.

### Requirement 5: Create Documents
**User Story:** As an agent, I want to create new documents so that I can publish ADRs, runbooks, or notes.

#### Acceptance Criteria
1. WHEN `docs create` is executed with a `--title` and `--collection-id` THEN the system SHALL create a new document.
2. IF content is provided via `--text`, `--file`, or `--stdin` THEN the system SHALL use that content for the document body.
3. IF `--parent-id` is specified THEN the document SHALL be created as a child of that parent document.
4. IF `--dedupe-key` is provided AND a mapping exists locally THEN the system SHALL return the existing document info instead of creating a duplicate (idempotency).
5. WHEN the creation is successful THEN the system SHALL output the new document's ID and URL.

### Requirement 6: Update Documents
**User Story:** As an agent, I want to update an existing document so that I can modify content or append information.

#### Acceptance Criteria
1. WHEN `docs update` is executed with a valid `--id` THEN the system SHALL update the document's content.
2. IF content is provided via `--text`, `--file` or `--stdin` THEN the system SHALL replace the document body with the new content.
3. IF the update is successful THEN the system SHALL return the updated document metadata.
4. IF the user attempts a destructive update without `--force` (if interactive) THEN the system SHALL prompt for confirmation (or fail in non-interactive mode if required).

### Requirement 7: Export Documents
**User Story:** As a user, I want to export documents to the local filesystem so that I can back them up or use them offline.

#### Acceptance Criteria
1. WHEN `docs export` is executed with a `--id` and `--dir` THEN the system SHALL write the document as a Markdown file in the specified directory.
2. IF `--subtree` is specified THEN the system SHALL recursively export all child documents.
3. IF `--rewrite-links` is set to `relative` (default) THEN the system SHALL convert internal Outline links to relative file paths.

### Requirement 8: Global CLI Behavior
**User Story:** As an agent developer, I want consistent CLI behavior so that my integrations are reliable.

#### Acceptance Criteria
1. WHEN any command is run with `--json` THEN the system SHALL output a standard JSON envelope with `ok`, `command`, `data`, and `meta` fields.
2. IF a command fails THEN the system SHALL return a non-zero exit code (e.g., 2 for validation, 3 for auth, 4 for not found).
3. IF `--dry-run` is specified THEN the system SHALL print the intended action without mutating any state.
4. IF `--quiet` is specified THEN the system SHALL suppress all human-readable output on stdout, only printing errors to stderr.

### Requirement 9: Diagnostics
**User Story:** As a user, I want to verify the tool's health so that I can troubleshoot issues.

#### Acceptance Criteria
1. WHEN `doctor` is executed THEN the system SHALL check config, auth, permissions, and network connectivity.
2. THE system SHALL report the status of each check to the user.
