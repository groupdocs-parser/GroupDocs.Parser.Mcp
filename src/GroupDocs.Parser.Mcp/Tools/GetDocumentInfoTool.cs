using System.ComponentModel;
using System.Text;
using System.Text.Json;
using GroupDocs.Mcp.Core;
using GroupDocs.Mcp.Core.Licensing;
using GroupDocs.Parser.Options;
using ModelContextProtocol.Server;

namespace GroupDocs.Parser.Mcp.Tools;

[McpServerToolType]
public static class GetDocumentInfoTool
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    [McpServerTool, Description(
        "Returns basic information about a document — file type, page count, size — as JSON, without modifying the file. " +
        "Supports PDF, DOCX, XLSX, PPTX, PNG, JPG, HTML, EPUB, MSG, EML, and 50+ more document formats. " +
        "Call this tool whenever the user asks to get document info, check a file's details, or inspect it before extracting text / metadata / images / tables. " +
        "Do NOT pre-check whether files exist — just pass the filename the user provided. " +
        "Returns a JSON object with fields `fileName`, `fileType` (extension), `fileTypeName` (engine-reported format name), `pageCount`, and `size`. " +
        "On failure, the response text starts with 'Document-info lookup failed for' followed by the underlying exception type, message, and inner-exception chain.")]
    public static async Task<string> GetDocumentInfo(
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
            var info = parser.GetDocumentInfo();

            if (info == null)
                return "Could not retrieve document information.";

            var result = new
            {
                fileName = resolved.FileName,
                fileType = info.FileType?.Extension,
                fileTypeName = info.FileType?.ToString(),
                pageCount = info.PageCount,
                size = info.Size
            };

            // Raw JSON — do NOT pipe through OutputHelper.TruncateText (Pitfall #16).
            return JsonSerializer.Serialize(result, JsonOptions);
        }
        catch (Exception ex)
        {
            return FormatException(ex, resolved.FileName);
        }
    }

    private static string FormatException(Exception ex, string fileName)
    {
        var sb = new StringBuilder();
        sb.Append($"Document-info lookup failed for '{fileName}': ");
        sb.Append($"{ex.GetType().FullName}: {ex.Message}");
        var inner = ex.InnerException;
        for (int depth = 0; inner != null && depth < 5; depth++, inner = inner.InnerException)
            sb.Append($" | inner({depth}): {inner.GetType().FullName}: {inner.Message}");
        return sb.ToString();
    }
}
