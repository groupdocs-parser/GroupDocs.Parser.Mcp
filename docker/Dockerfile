FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish src/GroupDocs.Parser.Mcp/GroupDocs.Parser.Mcp.csproj \
    -c Release -o /app --no-self-contained

FROM base AS final

# Native graphics deps GroupDocs.Parser's image-extraction + barcode-scanning
# paths need on Linux. libgdiplus is the GDI+ implementation that the engine's
# image-decoding paths P/Invoke; libfontconfig1 is its hidden font-discovery
# dependency. Parser reads / decodes images — it does NOT rasterize new text
# glyphs, so ttf-mscorefonts-installer is not required (per Pitfall #17 tier 1).
USER root
RUN apt-get update \
 && apt-get install -y --no-install-recommends libgdiplus libfontconfig1 \
 && rm -rf /var/lib/apt/lists/*

# OCI image labels. VERSION is supplied by the publish workflow via
# --build-arg VERSION=YY.MM.N. Default 'dev' makes local `docker build` runs
# self-identify instead of inheriting the base image's misleading version.
ARG VERSION=dev
# DOCKER-FIRST: the io.modelcontextprotocol.server.name label lets the MCP Registry
# verify ownership of this GHCR image, which is how Parser is published while its nupkg
# is too big for NuGet (>250 MB). Revert plan: .github/workflows/publish_prod.yml.
LABEL io.modelcontextprotocol.server.name="io.github.groupdocs-parser/groupdocs-parser-mcp" \
      org.opencontainers.image.title="GroupDocs.Parser.Mcp" \
      org.opencontainers.image.description="MCP server exposing GroupDocs.Parser for .NET as AI-callable tools (ExtractText, ExtractImages, ExtractMetadata, ExtractTables, ExtractBarcodes, GetDocumentInfo) for Claude, Cursor, GitHub Copilot, and other MCP agents." \
      org.opencontainers.image.version="${VERSION}" \
      org.opencontainers.image.source="https://github.com/groupdocs-parser/GroupDocs.Parser.Mcp" \
      org.opencontainers.image.url="https://github.com/groupdocs-parser/GroupDocs.Parser.Mcp/pkgs/container/parser-net-mcp" \
      org.opencontainers.image.licenses="MIT" \
      org.opencontainers.image.vendor="Aspose Pty Ltd"

COPY --from=build /app .

ENV GROUPDOCS_MCP_STORAGE_PATH=/data
VOLUME /data

USER $APP_UID

ENTRYPOINT ["dotnet", "GroupDocs.Parser.Mcp.dll"]
