using System.ComponentModel;
using GroupDocs.Mcp.Core;
using GroupDocs.Mcp.Core.Licensing;
using GroupDocs.Parser.Options;
using ModelContextProtocol.Server;

namespace GroupDocs.Parser.Mcp.Tools;

[McpServerToolType]
public static class ExtractTextTool
{
    [McpServerTool, Description(
        "Extracts plain text from a document file. " +
        "Supports PDF, DOCX, XLSX, PPTX, TXT, HTML, CSV, EML, MSG, RTF, ODT, EPUB, and 50+ more formats. " +
        "Call this tool immediately whenever the user asks to extract text, read text, or get the content of a document. " +
        "Do NOT pre-check whether files exist — just pass the filename the user provided. " +
        "Returns the document's plain text (truncated with a marker if it exceeds the configured budget). " +
        "On failure, the response text starts with 'Text extraction failed for' followed by the underlying exception type, message, and inner-exception chain.")]
    public static async Task<string> ExtractText(
        IFileResolver resolver,
        ILicenseManager licenseManager,
        OutputHelper output,
        FileInput file,
        [Description("Password for protected documents")] string? password = null,
        [Description("Page number to extract from (1-based). Omit for all pages.")] int? page = null)
    {
        licenseManager.SetLicense();
        using var resolved = await resolver.ResolveAsync(file);

        try
        {
            var loadOptions = password != null ? new LoadOptions(password) : new LoadOptions();

            using var parser = new GroupDocs.Parser.Parser(resolved.Stream, loadOptions);

            using var textReader = page.HasValue
                ? parser.GetText(page.Value - 1)
                : parser.GetText();

            if (textReader == null)
                return "No text could be extracted from this document.";

            var text = textReader.ReadToEnd();

            var prefix = licenseManager.IsLicensed
                ? string.Empty
                : "[Evaluation mode] Output may be limited and include watermarks.\n\n";

            return prefix + output.TruncateText(text,
                "Use 'page' parameter to extract specific pages (e.g. page: 1).");
        }
        catch (Exception ex)
        {
            return ToolError.Format("Text extraction", resolved.FileName, ex, page.HasValue ? $" (page={page})" : null);
        }
    }
}
