# @outline-cli/outlinectl

This is an npm wrapper that installs a prebuilt `outlinectl` native binary from GitHub Releases and exposes it as a global command.

## Install

```bash
npm i -g @dpalfery/outlinectl
```

After install, run:

```bash
outlinectl --help
```

## How it works

- On install, a `postinstall` script downloads the correct binary for your OS/CPU from GitHub Releases.
- The `outlinectl` command is a small Node launcher that executes that native binary.

## Environment variables

- `OUTLINECTL_DOWNLOAD_BASE`: Override the download base URL.
  - Example: `https://github.com/dpalfery/outline-wiki-cli/releases/download/v0.1.0`
- `OUTLINECTL_TAG`: Override the tag name (defaults to `v<package version>`).

## Troubleshooting

If install fails behind a firewall/proxy, download the binary manually and place it at:

- `vendor/<rid>/outlinectl` (macOS/Linux)
- `vendor/<rid>/outlinectl.exe` (Windows)

Where `<rid>` is one of: `win-x64`, `linux-x64`, `osx-arm64`, `osx-x64`.
