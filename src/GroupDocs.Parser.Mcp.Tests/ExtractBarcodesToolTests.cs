using GroupDocs.Mcp.Core;
using GroupDocs.Mcp.Core.Licensing;
using GroupDocs.Parser.Mcp.Tools;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace GroupDocs.Parser.Mcp.Tests;

public class ExtractBarcodesToolTests
{
    private readonly Mock<IFileResolver> _resolver = new();
    private readonly Mock<ILicenseManager> _licenseManager = new();

    [Fact]
    public async Task ExtractBarcodes_WhenResolverThrows_PropagatesException()
    {
        _resolver
            .Setup(r => r.ResolveAsync(It.IsAny<FileInput>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FileNotFoundException("missing.pdf"));

        var ex = await Assert.ThrowsAsync<FileNotFoundException>(() =>
            ExtractBarcodesTool.ExtractBarcodes(
                _resolver.Object,
                _licenseManager.Object,
                new FileInput { FilePath = "missing.pdf" }));

        Assert.Contains("missing.pdf", ex.Message);
    }

    [Fact]
    public async Task ExtractBarcodes_SetsLicense_BeforeResolving()
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
            ExtractBarcodesTool.ExtractBarcodes(
                _resolver.Object,
                _licenseManager.Object,
                new FileInput { FilePath = "anything.pdf" }));

        Assert.Equal(new[] { "license", "resolve" }, sequence);
    }
}
