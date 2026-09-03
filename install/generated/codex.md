# Codex CLI (OpenAI)

```bash
codex mcp add groupdocs-parser -- docker run --rm -i -v /path/to/documents:/data ghcr.io/groupdocs-parser/parser-net-mcp:latest
```

Or add to `~/.codex/config.toml`:

```toml
[mcp_servers.groupdocs-parser]
command = "docker"
args = ["run", "--rm", "-i", "-v", "/path/to/documents:/data", "ghcr.io/groupdocs-parser/parser-net-mcp:latest"]
```

Pin a version by replacing `:latest` with `:26.9.0` in the image tag.
