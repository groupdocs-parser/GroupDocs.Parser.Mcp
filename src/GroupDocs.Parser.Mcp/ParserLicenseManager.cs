using GroupDocs.Mcp.Core;
using GroupDocs.Mcp.Core.Licensing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GroupDocs.Parser.Mcp;

public class ParserLicenseManager : LicenseManager
{
    public ParserLicenseManager(IOptions<McpConfig> config, ILogger<LicenseManager> logger) : base(config, logger) { }
    protected override void SetLicenseFromPath(string licensePath)
    {
        new GroupDocs.Parser.License().SetLicense(licensePath);
    }
}
