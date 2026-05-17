---
id: 001
date: 2026-05-15
version: 26.5.0
type: feature
---

# Initial public release of GroupDocs.Parser MCP Server

## What changed
- NuGet package `GroupDocs.Parser.Mcp` published with `McpServer` package type.
- Six MCP tools exposed:
  - `ExtractText` — extract plain text from a document (whole document or specific page).
  - `ExtractImages` — extract all embedded images and save them to storage as `<basename>_image<N>.<ext>` files.
  - `ExtractMetadata` — extract metadata (author, title, dates, EXIF, XMP, IPTC, custom properties) as JSON.
  - `ExtractTables` — extract tables as Markdown (default, renders inline in chat) or structured JSON.
  - `ExtractBarcodes` — detect and decode all barcodes / QR codes (Code128, QR, PDF417, DataMatrix, EAN, UPC, Aztec, more); returns decoded values + types + position + confidence as JSON.
  - `GetDocumentInfo` — return the file type, page count, and size of a document as JSON.
- Installable via `dnx GroupDocs.Parser.Mcp@26.5.0 --yes` (.NET 10 SDK required) or `dotnet tool install -g`.
- Docker image published to `ghcr.io/groupdocs-parser/parser-net-mcp` and `docker.io/groupdocs/parser-net-mcp`.
- Environment variables: `GROUPDOCS_MCP_STORAGE_PATH`, optional `GROUPDOCS_MCP_OUTPUT_PATH`, `GROUPDOCS_LICENSE_PATH`.
- Linux native graphics deps wired up: `SkiaSharp.NativeAssets.Linux.NoDependencies` (3.119.1) is referenced because `GroupDocs.Parser` 26.4.0 transitively requires SkiaSharp ≥ 3.119.1. `libgdiplus` + `libfontconfig1` are installed in the Docker image because Parser's image-extraction paths P/Invoke `System.Drawing`. The `System.Drawing.EnableUnixSupport` runtime flag is intentionally NOT set in the csproj — Parser's public API doesn't surface `System.Drawing.Common` types directly, and a no-op flag isn't needed.

## Pre-shipped pitfall remediations
- **Pitfall #16 (JSON tools never pipe through `OutputHelper.TruncateText`)** — fixed in `ExtractTablesTool` at clone time. The framework subproject's original implementation routed the `format='json'` path through `TruncateText`, which would have appended a non-JSON marker on responses > 5 KB and broken strict-JSON consumers. The Markdown branch keeps `TruncateText` because its output is prose, not strict JSON. `ExtractMetadataTool`, `ExtractBarcodesTool`, and `GetDocumentInfoTool` already return raw JSON correctly.
- **Pitfall #18 (engine exceptions surface diagnostically)** — all six tools wrap their engine calls in `try/catch (Exception ex)` and return per-tool descriptive failure strings (`Text extraction failed for '<file>': <ExceptionType>: <message> | inner(0): ...`, etc.) instead of letting them bubble up to MCP's canned `"An error occurred invoking '<tool>'"` wrapper. Pattern lifted from Conversion 26.5.2.
- **License class is exposed publicly** — `public sealed class License` in `net/src/GroupDocs.Parser/License.cs` is exposed with `public void SetLicense(string filePath)`. `ParserLicenseManager` uses the Metadata pattern (`new GroupDocs.Parser.License().SetLicense(licensePath)`).

## Why
Sixth product MCP server in the GroupDocs MCP framework family (after Metadata, Conversion, Comparison, Viewer, Watermark). Exposes GroupDocs.Parser for .NET as AI-callable tools for Claude, Cursor, VS Code / GitHub Copilot, and other MCP-compatible agents.

## Migration / impact
First release — no migration required.

## Distribution caveat — Docker only for now
- **NuGet.org publish is currently blocked**: the underlying `GroupDocs.Parser.dll` is 234.9 MB (essentially incompressible — embedded ONNX models for OCR / barcode detection). With cross-platform native asset bundles (SkiaSharp + ONNX Runtime for win-x64/x86/arm64, linux-x64/arm64, osx-x64/arm64, plus Alpine musl variants), the packed nupkg lands at ~320 MB — well over NuGet.org's 250 MB hard limit (Pitfall #12).
- **Mobile + exotic platforms are stripped at pack time** (iOS, Android, maccatalyst, tvOS, browser-wasm, linux-loongarch64/riscv64/mips64/ppc64le/s390x/armel, legacy osx fat dylib). These are not MCP-server targets and removing them is always safe — see the `StripMobileAndExoticRuntimes` MSBuild target in the csproj.
- **Distribution channel: Docker only**: ship via `ghcr.io/groupdocs-parser/parser-net-mcp` + `docker.io/groupdocs/parser-net-mcp`. The Docker image bundles the full multi-platform runtime set and is unaffected by NuGet's size limit. Users install via `docker run --rm -i ghcr.io/groupdocs-parser/parser-net-mcp:26.5.0`.
- **`dnx GroupDocs.Parser.Mcp@26.5.0 --yes` will not work yet** — the package isn't on NuGet.org. README + how-to guides note the Docker-first deployment.
- **Future path**: if upstream `GroupDocs.Parser` ships a smaller engine variant (or splits embedded ONNX models into a separate optional package), revisit the NuGet path then.

## TODO before MCP registry publish
- [ ] Polish `[Description("...")]` strings on the 6 tools after first dogfooding with AI clients — current strings are functional but may benefit from sharper format coverage statements.
- [ ] Capture a real screenshot of the Claude Desktop tools panel for the README, once published.
- [ ] Resolve the NuGet 250 MB limit issue before tagging a release that should appear on the MCP registry (NuGet path is the discovery route).
