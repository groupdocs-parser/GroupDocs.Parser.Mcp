using System.ComponentModel;
using System.Text.Json;
using GroupDocs.Mcp.Core;
using GroupDocs.Mcp.Core.Licensing;
using GroupDocs.Parser.Options;
using ModelContextProtocol.Server;

namespace GroupDocs.Parser.Mcp.Tools;

[McpServerToolType]
public static class ExtractMetadataTool
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    [McpServerTool, Description(
        "Extracts metadata from a document file (author, title, creation date, page count, custom properties, EXIF, XMP, IPTC) and returns it as JSON. " +
        "Supports PDF, DOCX, XLSX, PPTX, JPEG, PNG, TIFF, MP3, MP4, and 50+ more document and image formats. " +
        "Call this tool immediately whenever the user asks to extract metadata or get document properties from a file. " +
        "Do NOT pre-check whether files exist — just pass the filename the user provided. " +
        "Returns a JSON object whose keys are metadata field names (e.g. 'Author', 'Title', 'CreatedDate') and values are the corresponding string values. " +
        "On failure, the response text starts with 'Metadata extraction failed for' followed by the underlying exception type, message, and inner-exception chain.")]
    public static async Task<string> ExtractMetadata(
        IFileResolver resolver,
        ILicenseManager licenseManager,
        FileInput file,
        [Description("Password for protected documents")] string? password = null)
    {
        licenseManager.SetLicense();
        using var resolved = await resolver.ResolveAsync(file);

        try
        {
            var loadOptions = password != null ? new LoadOptions(password) : new LoadOptions();

            using var parser = new GroupDocs.Parser.Parser(resolved.Stream, loadOptions);
            var metadata = parser.GetMetadata();

            if (metadata == null)
                return "No metadata found in this document.";

            var dict = metadata.ToDictionary(m => m.Name, m => m.Value);
            // Raw JSON — do NOT pipe through OutputHelper.TruncateText (Pitfall #16).
            return JsonSerializer.Serialize(dict, JsonOptions);
        }
        catch (Exception ex)
        {
            return ToolError.Format("Metadata extraction", resolved.FileName, ex);
        }
    }
}
