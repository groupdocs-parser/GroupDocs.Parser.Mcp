# GroupDocs.Parser MCP Server

MCP server that exposes [GroupDocs.Parser](https://products.groupdocs.com/parser) as AI-callable tools
for Claude, Cursor, GitHub Copilot, and other MCP agents.

## Installation

Requires [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0).

**Run directly with `dnx` (recommended — no install step):**

```bash
dnx GroupDocs.Parser.Mcp --yes
```

Pulls the latest stable release on every invocation. To pin to a specific
version (recommended for shared configs and CI), append `@<version>`:

```bash
dnx GroupDocs.Parser.Mcp@26.5.0 --yes
```

**Or install as a global dotnet tool:**

```bash
dotnet tool install -g GroupDocs.Parser.Mcp
groupdocs-parser-mcp
```

**Or run via Docker:**

```bash
docker run --rm -i \
  -v $(pwd)/documents:/data \
  ghcr.io/groupdocs-parser/parser-net-mcp:latest
```

## Available MCP Tools

| Tool | Description |
|---|---|
| `ExtractText` | Extracts plain text from a document (whole document or a single page). Truncates very large outputs. |
| `ExtractImages` | Extracts all embedded images and saves them to storage as `<basename>_image<N>.<ext>` files |
| `ExtractMetadata` | Extracts metadata (author, title, dates, custom properties, EXIF, XMP, IPTC) and returns it as JSON |
| `ExtractTables` | Extracts tables from a document as Markdown (default — renders in chat) or structured JSON |
| `ExtractBarcodes` | Detects all barcodes / QR codes and returns their decoded values, types, and positions as JSON |
| `GetDocumentInfo` | Returns the file type, page count, and size of a document as JSON (without modifying it) |

All tools support PDF, DOCX, XLSX, PPTX, HTML, EPUB, MSG, EML, JPG, PNG, TIFF, and 50+ more document and image formats.

## Example prompts for AI agents

Copy any of these into Claude Desktop, Cursor, or GitHub Copilot Chat after the
server is connected.

1. **Get a document's structure**: *"How many pages does invoice.pdf have, and what format is it?"*
2. **Pull a text snippet**: *"Extract the text from page 2 of contract.docx."*
3. **Mine the metadata**: *"What's the author and creation date of report.xlsx?"*
4. **Read a structured table**: *"Pull the line items table out of invoice.pdf as Markdown."*
5. **Scan for barcodes**: *"Are there any QR codes in shipping-label.png? If so, what do they decode to?"*

## Configuration

| Variable | Description | Default |
|---|---|---|
| `GROUPDOCS_MCP_STORAGE_PATH` | Base folder for input and output files | current directory |
| `GROUPDOCS_MCP_OUTPUT_PATH` | *(Optional)* separate folder for output files (used by `ExtractImages`) | `GROUPDOCS_MCP_STORAGE_PATH` |
| `GROUPDOCS_LICENSE_PATH` | Path to GroupDocs license file. In evaluation mode, text outputs may include a watermark and other outputs may be size-limited | (evaluation mode) |

## Usage with Claude Desktop

```json
{
  "mcpServers": {
    "groupdocs-parser": {
      "type": "stdio",
      "command": "dnx",
      "args": ["GroupDocs.Parser.Mcp", "--yes"],
      "env": {
        "GROUPDOCS_MCP_STORAGE_PATH": "/path/to/documents"
      }
    }
  }
}
```

> To pin: replace `"GroupDocs.Parser.Mcp"` with `"GroupDocs.Parser.Mcp@26.5.0"` in `args`.

## Usage with VS Code / GitHub Copilot

NuGet.org generates a ready-to-use `mcp.json` snippet on the [package page](https://www.nuget.org/packages/GroupDocs.Parser.Mcp).
Copy it directly into your `.vscode/mcp.json`.

Alternatively, add manually to `.vscode/mcp.json`:

```json
{
  "inputs": [
    {
      "type": "promptString",
      "id": "storage_path",
      "description": "Base folder for input and output files.",
      "password": false
    }
  ],
  "servers": {
    "groupdocs-parser": {
      "type": "stdio",
      "command": "dnx",
      "args": ["GroupDocs.Parser.Mcp", "--yes"],
      "env": {
        "GROUPDOCS_MCP_STORAGE_PATH": "${input:storage_path}"
      }
    }
  }
}
```

## Usage with Docker Compose

```bash
cd docker
docker compose up
```

Edit `docker/docker-compose.yml` to point volumes at your local documents folder.

## Documentation & guides

Step-by-step deployment guides and a published-package integration test suite
live in the companion repo
[**GroupDocs.Parser.Mcp.Tests**](https://github.com/groupdocs-parser/GroupDocs.Parser.Mcp.Tests):

- [Install from NuGet](https://github.com/groupdocs-parser/GroupDocs.Parser.Mcp.Tests/blob/master/how-to/01-install-from-nuget.md)
- [Run via Docker](https://github.com/groupdocs-parser/GroupDocs.Parser.Mcp.Tests/blob/master/how-to/02-run-via-docker.md)
- [Verify on the MCP registry](https://github.com/groupdocs-parser/GroupDocs.Parser.Mcp.Tests/blob/master/how-to/03-verify-mcp-registry.md)
- [Use with Claude Desktop](https://github.com/groupdocs-parser/GroupDocs.Parser.Mcp.Tests/blob/master/how-to/04-use-with-claude-desktop.md)
- [Use with VS Code / GitHub Copilot](https://github.com/groupdocs-parser/GroupDocs.Parser.Mcp.Tests/blob/master/how-to/05-use-with-vscode-copilot.md)
- [Run the integration tests](https://github.com/groupdocs-parser/GroupDocs.Parser.Mcp.Tests/blob/master/how-to/06-run-integration-tests.md)

## License

MIT — see [LICENSE](LICENSE)

<!-- mcp-name: io.github.groupdocs-parser/groupdocs-parser-mcp -->
