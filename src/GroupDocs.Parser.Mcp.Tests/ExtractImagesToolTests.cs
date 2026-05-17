using GroupDocs.Mcp.Core;
using GroupDocs.Mcp.Core.Licensing;
using GroupDocs.Parser.Mcp.Tools;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace GroupDocs.Parser.Mcp.Tests;

public class ExtractImagesToolTests
{
    private readonly Mock<IFileResolver> _resolver = new();
    private readonly Mock<ILicenseManager> _licenseManager = new();
    private readonly Mock<IFileStorage> _storage = new();
    private readonly OutputHelper _output;

    public ExtractImagesToolTests()
    {
        _output = new OutputHelper(_storage.Object, Microsoft.Extensions.Options.Options.Create(new McpConfig()));
    }

    [Fact]
    public async Task ExtractImages_WhenResolverThrows_PropagatesException()
    {
        _resolver
            .Setup(r => r.ResolveAsync(It.IsAny<FileInput>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FileNotFoundException("missing.pdf"));

        var ex = await Assert.ThrowsAsync<FileNotFoundException>(() =>
            ExtractImagesTool.ExtractImages(
                _resolver.Object,
                _storage.Object,
                _licenseManager.Object,
                _output,
                new FileInput { FilePath = "missing.pdf" }));

        Assert.Contains("missing.pdf", ex.Message);
    }

    [Fact]
    public async Task ExtractImages_SetsLicense_BeforeResolving()
    {
        var sequence = new List<string>();

        _licenseManager
            .Setup(l => l.SetLicense())
            .Callback(() => sequence.Add("license"));

        _resolver
            .Setup(r => r.ResolveAsync(It.IsAny<FileInput>(), It.IsAny<CancellationToken>()))
            .Callback(() => sequence.Add("resolve"))
            .ThrowsAsync(new InvalidOperationException("short-circuit"));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            ExtractImagesTool.ExtractImages(
                _resolver.Object,
                _storage.Object,
                _licenseManager.Object,
                _output,
                new FileInput { FilePath = "anything.pdf" }));

        Assert.Equal(new[] { "license", "resolve" }, sequence);
    }
}
