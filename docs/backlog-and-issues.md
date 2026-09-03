# Backlog & Known Issues

Running list of ideas, planned work, and known limitations for the
GroupDocs.Parser MCP server. Grouped by topic. Terse on purpose — each line is
a ticket, not an essay. `[ ]` = open, `[x]` = shipped (kept for context).

**Current surface (26.9.0):** `extract_text`, `extract_images`, `extract_tables`,
`extract_metadata`, `extract_barcodes`, `get_document_info`.

**Channel: Docker/GHCR only (OCI).** The packed tool is ≈320 MiB, over NuGet.org's 250 MB limit,
so there is no `dnx` path. Registry entry is registered as `oci`.

---

## Confirmed defects — external audit, 2026-08-16

Source: black-box test round against `ghcr.io/groupdocs-parser/parser-net-mcp:latest`
(26.7.3, licensed), 46 family-wide defects reported and all 46 independently reproduced with
control calls. A later validation round found **zero false positives**.

`S#` = shared core (`GroupDocs.Mcp.Core`) · `M#` = this repo · `P#` = GroupDocs.Parser library

**Verdict: good.** Text extraction is genuinely high quality (full articles, Cyrillic, DOCX page
markers); 59 images extracted and verified; password flow correct. Two gaps.

### Shared core — fixed once in `GroupDocs.Mcp.Core`, lands here on the next bump

- [ ] **S1** Passing `fileName` crashes any tool — **High**. Unhandled `ArgumentException` in
      `FileResolver.ResolveAsync`; client sees only `An error occurred invoking '<tool>'`.
- [ ] **S2** Missing files return an opaque error — **High**. The `Available files:` listing the
      descriptions promise is built and then thrown away in stderr; also capped at 20 entries.
- [ ] **S3** `isError` is set on crashes but not on real failures — **Med**.

Nothing to do in this repo for S1–S3 beyond re-testing after the Core bump.

### MCP wrapper — this repo

None specific to the wrapper.

### Product library — upstream

- [ ] **P1** EXIF metadata is not found in JPEGs that have it — **Med**.
      *Proof:* `extract_metadata` returned "No metadata found" on two JPEGs with **verified EXIF
      APP1 segments**. The Metadata MCP server reads the same files correctly.
      *Fix:* wire EXIF/IPTC reading into `extract_metadata` — or, if Parser deliberately does not
      do image metadata, say so in the tool description and point callers at the Metadata product.
      **P1** — decide whether Parser owns image metadata; either implement or document the
      boundary. Empty results on files that clearly have data is what users report as "it's
      broken".
- [ ] **P2** `extract_tables` reports tables that are empty — **Low**.
      *Proof:* 5 × "1 rows × 1 cols", all blank, from a PDF with real content.
      *Fix:* either return the detected content or don't report the region as a table. **P2**

---

## Known issues & limitations

- **Docker-first**: no NuGet/`dnx` channel by design (size). Any documentation or install snippet
  implying `dnx` is wrong for this product.
- Password-protected documents are handled correctly, with a clear error when the password is
  missing.
- Barcode extraction was not exercised with positive fixtures during the audit — the synthetic
  fixture set had none.

---

## Tools & functionality

- [ ] **P1** EXIF/IPTC in `extract_metadata`, or an explicit documented boundary. **P1**
- [ ] **P2** suppress or populate phantom tables. **P2**
- [ ] `extract_text` — page-range parameter to keep agent context small on large documents. **P2**
- [ ] `extract_images` — filter by page or minimum size. **P2**

## Testing & CI

- [ ] **The companion test repo currently validates nothing.** Its harness launches
      `dnx GroupDocs.Parser.Mcp@<ver>` from nuget.org — a package that was **never published**
      (Docker-first). Every test dies at fixture init with a dnx 404, and the suite still reports
      green. **P1 — this is the single most important item in this file.**
- [ ] Replace it with container-based tests that exercise the published GHCR image. **P1**
- [ ] Add the two mandatory probes: the **`fileName`-only form**, and a **missing file** asserting
      the promised `Available files:` text. **P1**
- [ ] Per-tool Linux smoke test in image CI — call every tool once in the built container. **P1**
- [ ] Add a positive barcode fixture so `extract_barcodes` gets real coverage. **P2**
- [ ] Add a JPEG-with-EXIF fixture so P1 gets a regression test. **P1**

## Documentation & discoverability

- [ ] State the image-metadata boundary in `extract_metadata` (P1 above). **P1**
- [ ] Make the Docker-only channel unmistakable in README and the Registry description. **P1**
- [ ] Licensing section covering the metered option once it ships. **P1**

## Platform & infra (longer-term)

- [ ] Metered licensing (`GROUPDOCS_METERED_PUBLIC_KEY` / `_PRIVATE_KEY`) via
      `GroupDocs.Mcp.Core`, plus the `get_license_status` tool. **P1**
- [ ] Revisit the NuGet size gate if the embedded models ever shrink. **P2**
- [ ] HTTP/SSE transport for shared/team deploys (stdio stays default). **P2**

---

*Evidence: `TEMP_ThirdPartyAnalysis/parser.md` (per-product findings),
`ALL-PRODUCTS-REPORT.md` (10-product sweep), `VALIDATION-REPORT.md` (the dnx-404 finding is
"New findings #1" there). Conventions: any behaviour change ships with a `changelog/NNN-*.md`
entry and a CalVer bump.*
