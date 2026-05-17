# AGENTS.md — Guide for AI coding agents

Brief orientation for AI coding agents (Claude Code, Copilot, Cursor, Aider, Amp, Codex) working in this repository.

## What this repo is

A standalone **MCP server** for [GroupDocs.Parser for .NET](https://products.groupdocs.com/parser) — exposes document-parsing operations (text / images / metadata / tables / barcodes / info) as AI-callable tools via the Model Context Protocol.

Published to NuGet as `GroupDocs.Parser.Mcp` with the `McpServer` package type, and to `ghcr.io/groupdocs-parser/parser-net-mcp` + `docker.io/groupdocs/parser-net-mcp` as a container image.

## MCP tools exposed

| Tool | Description |
|---|---|
| `ExtractText` | Plain text from a document (whole or per page). Truncates large outputs. |
| `ExtractImages` | Saves embedded images to storage as `<basename>_image<N>.<ext>`. Returns saved-path list. |
| `ExtractMetadata` | Author / title / dates / EXIF / XMP / IPTC / custom properties as JSON. |
| `ExtractTables` | Tables as Markdown (default) or structured JSON (`format='json'`). |
| `ExtractBarcodes` | Decoded values + types + page + confidence + angle for every barcode found. |
| `GetDocumentInfo` | File type + page count + size as JSON (no modification). |

All tools accept `FileInput` (resolved via `IFileResolver`) and an optional `password` for protected documents. Text-extracting tools wrap engine-level exceptions in a descriptive error string (per-tool prefixes: `Text extraction failed for`, `Image extraction failed for`, `Metadata extraction failed for`, `Table extraction failed for`, `Barcode extraction failed for`, `Document-info lookup failed for`) instead of letting them bubble up to MCP's canned generic wrapper — critical for diagnosing native-deps issues on Linux.

## Folder layout

```
src/                                           ← all projects + sln + Directory.Build.props
  GroupDocs.Parser.Mcp/
    Program.cs                                 ← host bootstrap + stdio transport
    ParserLicenseManager.cs                    ← applies GroupDocs.Total license
    Tools/
      ExtractTextTool.cs                       ← [McpServerTool] — ExtractText
      ExtractImagesTool.cs                     ← [McpServerTool] — ExtractImages
      ExtractMetadataTool.cs                   ← [McpServerTool] — ExtractMetadata
      ExtractTablesTool.cs                     ← [McpServerTool] — ExtractTables
      ExtractBarcodesTool.cs                   ← [McpServerTool] — ExtractBarcodes
      GetDocumentInfoTool.cs                   ← [McpServerTool] — GetDocumentInfo
    .mcp/
      server.json                              ← NuGet.org reads this to generate mcp.json snippet
    GroupDocs.Parser.Mcp.csproj                ← PackageType=McpServer + ToolCommandName
  GroupDocs.Parser.Mcp.Tests/
  GroupDocs.Parser.Mcp.sln
  Directory.Build.props
build/
  dependencies.props                           ← single source of truth for all versions
changelog/                                     ← one MD file per change (see changelog/README.md)
docker/
  Dockerfile                                   ← multi-stage, runtime on aspnet:10.0
  docker-compose.yml
.github/workflows/                             ← build_packages.yml, run_tests.yml, publish_prod.yml, publish_docker.yml
```

## Dependencies

- `GroupDocs.Mcp.Core` + `GroupDocs.Mcp.Local.Storage` — infrastructure NuGet packages
- `GroupDocs.Parser` — the actual parsing engine
- `ModelContextProtocol` — MCP SDK for .NET
- `Microsoft.Extensions.Hosting` — host builder for the stdio server
- `SkiaSharp.NativeAssets.Linux.NoDependencies` (3.119.1) — pinned because the upstream `GroupDocs.Parser` nuspec transitively requires it

## Commands you can run

```bash
# Restore + build
dotnet restore
dotnet build src/GroupDocs.Parser.Mcp.sln -c Release

# Run tests
dotnet test src/GroupDocs.Parser.Mcp.sln -c Release

# Run the server locally (stdio)
dotnet run --project src/GroupDocs.Parser.Mcp

# Local pack (writes to ./build_out) — validates server.json version matches dependencies.props
pwsh ./build.ps1

# Build + run the Docker image
docker build -f docker/Dockerfile -t parser-net-mcp:local .
docker run --rm -i -v $(pwd)/documents:/data parser-net-mcp:local
```

## Version scheme

CalVer `YY.M.N` (M not zero-padded). The version lives in **two** places that MUST stay in lockstep:
1. `build/dependencies.props` → `<GroupDocsParserMcp>`
2. `src/GroupDocs.Parser.Mcp/.mcp/server.json` → both top-level `"version"` and `packages[0].version`

`build.ps1` enforces this at pack time (`Assert-ServerJsonVersionMatchesDependencies`) — if they drift, the build fails.

## House rules

1. **Tools must have rich `[Description("...")]` strings** — these are what AI agents read via the MCP protocol. Enumerate supported formats and describe the response format (JSON shape / saved-path / Markdown).
2. **Never add new env vars beyond** `GROUPDOCS_MCP_STORAGE_PATH`, `GROUPDOCS_MCP_OUTPUT_PATH`, `GROUPDOCS_LICENSE_PATH` without updating `server.json`, `docker-compose.yml`, and `README.md` together.
3. **JSON-emitting tools return raw JSON** — do not pass through `OutputHelper.TruncateText`. The truncation marker breaks `JsonDocument.Parse`. The JSON-branch of `ExtractTablesTool` and the JSON paths in `ExtractMetadataTool`/`ExtractBarcodesTool`/`GetDocumentInfoTool` all return `JsonSerializer.Serialize(...)` directly. The Markdown branch of `ExtractTablesTool` and plain-text `ExtractTextTool` may use `TruncateText` because their output isn't strict JSON.
4. **Engine calls live inside a `try/catch`** that returns a descriptive `<Operation> failed for '<file>': <ExceptionType>: <message>` string. Keep `resolver.ResolveAsync` OUTSIDE the catch so file-not-found errors propagate cleanly.
5. **Tests use xUnit + Moq** — mock `IFileResolver`, `IFileStorage`, `ILicenseManager`, `OutputHelper`.
6. **Changelog entries required** — any PR that changes behaviour adds `changelog/NNN-slug.md`.
7. **Target framework is `net10.0` only**.

## Release flow

See [RELEASE.md](RELEASE.md) for the per-release checklist.

## What NOT to change

- Do not hardcode the version in `.csproj` — it flows from `$(GroupDocsParserMcp)` in `dependencies.props`.
- Do not remove the `<PackageType>McpServer</PackageType>` or `<ToolCommandName>groupdocs-parser-mcp</ToolCommandName>` from the csproj.
- Do not remove the `StripNativeRuntimePdbs` MSBuild target — defense against NuGet.org's 250 MB hard limit.
- Do not change the `.mcp/server.json` schema URL without cross-checking with the NuGet MCP docs.
