/*
  Downloads a prebuilt outlinectl binary from GitHub Releases.

  Defaults:
    - Repo: dpalfery/outline-wiki-cli
    - Tag: v<package.json version>

  Overrides:
    - OUTLINECTL_DOWNLOAD_BASE: base URL to download from
      e.g. https://github.com/dpalfery/outline-wiki-cli/releases/download/v0.1.0
    - OUTLINECTL_TAG: e.g. v0.1.0
*/

const fs = require("node:fs");
const path = require("node:path");
const https = require("node:https");

function resolveRid() {
  const platform = process.platform;
  const arch = process.arch;

  if (platform === "win32" && arch === "x64") return "win-x64";
  if (platform === "linux" && arch === "x64") return "linux-x64";
  if (platform === "darwin" && arch === "arm64") return "osx-arm64";
  if (platform === "darwin" && arch === "x64") return "osx-x64";

  return null;
}

function exeNameForRid(rid) {
  return rid.startsWith("win-") ? "outlinectl.exe" : "outlinectl";
}

function assetNameForRid(rid) {
  // We download single-file assets from GitHub releases.
  return rid.startsWith("win-") ? `outlinectl-${rid}.exe` : `outlinectl-${rid}`;
}

function download(url, destPath) {
  return new Promise((resolve, reject) => {
    const file = fs.createWriteStream(destPath);

    const request = https.get(url, {
      headers: {
        "User-Agent": "@outline-cli/outlinectl postinstall"
      }
    }, (res) => {
      // Follow redirects (GitHub sometimes redirects to S3)
      if (res.statusCode && res.statusCode >= 300 && res.statusCode < 400 && res.headers.location) {
        file.close();
        fs.unlinkSync(destPath);
        return resolve(download(res.headers.location, destPath));
      }

      if (res.statusCode !== 200) {
        file.close();
        fs.unlinkSync(destPath);
        return reject(new Error(`Download failed: ${res.statusCode} ${res.statusMessage}`));
      }

      res.pipe(file);
      file.on("finish", () => file.close(resolve));
    });

    request.on("error", (err) => {
      try {
        file.close();
        if (fs.existsSync(destPath)) fs.unlinkSync(destPath);
      } catch {
        // ignore cleanup errors
      }
      reject(err);
    });
  });
}

async function main() {
  const rid = resolveRid();
  if (!rid) {
    console.warn(`@outline-cli/outlinectl: unsupported platform/arch: ${process.platform}/${process.arch}`);
    console.warn("Skipping native binary download.");
    return;
  }

  const pkgPath = path.join(__dirname, "..", "package.json");
  const pkg = JSON.parse(fs.readFileSync(pkgPath, "utf8"));

  const tag = process.env.OUTLINECTL_TAG || `v${pkg.version}`;
  const base = process.env.OUTLINECTL_DOWNLOAD_BASE || `https://github.com/dpalfery/outline-wiki-cli/releases/download/${tag}`;

  const assetName = assetNameForRid(rid);
  const url = `${base}/${assetName}`;

  const vendorDir = path.join(__dirname, "..", "vendor", rid);
  fs.mkdirSync(vendorDir, { recursive: true });

  const exePath = path.join(vendorDir, exeNameForRid(rid));

  // If already present, don’t re-download.
  if (fs.existsSync(exePath)) return;

  const tmpPath = `${exePath}.tmp`;

  console.log(`@outline-cli/outlinectl: downloading ${assetName}...`);

  await download(url, tmpPath);

  fs.renameSync(tmpPath, exePath);

  if (!rid.startsWith("win-")) {
    fs.chmodSync(exePath, 0o755);
  }

  console.log("@outline-cli/outlinectl: installed native binary.");
}

main().catch((err) => {
  console.error("@outline-cli/outlinectl: postinstall failed");
  console.error(err && err.message ? err.message : String(err));
  process.exit(1);
});
