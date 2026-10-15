using GroupDocs.Parser.Mcp.Tools;
using Xunit;

namespace GroupDocs.Parser.Mcp.Tests;

public class ToolErrorTests
{
    [Fact]
    public void Format_PermissionDenied_AppendsTheDockerFix()
    {
        // The exact shape seen on Linux CI: UnauthorizedAccessException wrapping IOException.
        var ex = new UnauthorizedAccessException("Access to the path '/data/images_image1.jpeg' is denied.",
            new IOException("Permission denied"));

        var text = ToolError.Format("Image extraction", "images.pdf", ex);

        Assert.StartsWith("Image extraction failed for 'images.pdf': System.UnauthorizedAccessException:", text);
        Assert.Contains("--user \"$(id -u):$(id -g)\"", text);
        Assert.Contains("GROUPDOCS_MCP_OUTPUT_PATH", text);
    }

    [Fact]
    public void Format_ReadOnlyFileSystem_AppendsTheFix()
    {
        var text = ToolError.Format("Image extraction", "deck.pptx", new IOException("Read-only file system : '/data/x.png'"));

        Assert.EndsWith(ToolError.PermissionHint, text);
    }

    [Fact]
    public void Format_EngineFailure_HasNoHint()
    {
        var text = ToolError.Format("Text extraction", "corrupted.pdf", new InvalidDataException("Unsupported format"));

        Assert.DoesNotContain("Fix:", text);
        Assert.Equal("Text extraction failed for 'corrupted.pdf': System.IO.InvalidDataException: Unsupported format", text);
    }
}
