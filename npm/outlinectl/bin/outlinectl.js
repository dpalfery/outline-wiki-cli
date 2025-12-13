#!/usr/bin/env node

const { spawnSync } = require("node:child_process");
const path = require("node:path");
const fs = require("node:fs");

function resolveRid() {
  const platform = process.platform;
  const arch = process.arch;

  // Node arch values: x64, arm64
  if (platform === "win32" && arch === "x64") return "win-x64";
  if (platform === "linux" && arch === "x64") return "linux-x64";
  if (platform === "darwin" && arch === "arm64") return "osx-arm64";
  if (platform === "darwin" && arch === "x64") return "osx-x64";

  return null;
}

function exeNameForRid(rid) {
  return rid.startsWith("win-") ? "outlinectl.exe" : "outlinectl";
}

const rid = resolveRid();
if (!rid) {
  console.error(`outlinectl: unsupported platform/arch: ${process.platform}/${process.arch}`);
  process.exit(1);
}

const exe = path.join(__dirname, "..", "vendor", rid, exeNameForRid(rid));
if (!fs.existsSync(exe)) {
  console.error("outlinectl: native binary not found.");
  console.error("Try reinstalling: npm i -g @outline-cli/outlinectl");
  console.error("If you are behind a proxy/firewall, set OUTLINECTL_DOWNLOAD_BASE and reinstall.");
  process.exit(1);
}

const result = spawnSync(exe, process.argv.slice(2), {
  stdio: "inherit",
  windowsHide: true
});

process.exit(result.status ?? 1);
