# Claude Code

```bash
claude mcp add groupdocs-parser -- docker run --rm -i -v /path/to/documents:/data ghcr.io/groupdocs-parser/parser-net-mcp:latest
```

With a GroupDocs license:

```bash
claude mcp add groupdocs-parser -- docker run --rm -i -v /path/to/documents:/data -v /path/to/license-folder:/license -e GROUPDOCS_LICENSE_PATH=/license/GroupDocs.Total.lic ghcr.io/groupdocs-parser/parser-net-mcp:latest
```

Pin a version by replacing `:latest` with `:26.7.3` in the image tag.
