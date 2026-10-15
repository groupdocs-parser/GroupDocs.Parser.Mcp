using System.Text;

namespace GroupDocs.Parser.Mcp.Tools;

// Shared descriptive-error formatter for the tool surface. Engine failures are
// surfaced as text (not MCP's opaque "An error occurred invoking '<tool>'") so
// AI agents and integration tests can read the cause. The text always starts
// with "<op> failed for '<file>'[<subjectSuffix>]: ..." — integration tests match
// that prefix. `subjectSuffix` carries per-tool context like " (page=3)".
internal static class ToolError
{
    // Appended when the failure is a write the process is not allowed to make. The usual cause is
    // Docker on Linux: the image runs as a non-root user (uid 1654), a bind mount keeps its host
    // owner, and a folder the user owns is read-only inside the container - so extract_images
    // cannot save into /data. Docker Desktop on Windows and macOS does not enforce this.
    internal const string PermissionHint =
        " | Fix: the server cannot write to the output folder. In Docker on Linux the container runs as a " +
        "non-root user, so a host folder you own is read-only to it: add --user \"$(id -u):$(id -g)\" to " +
        "docker run (in a JSON client config use the numbers, e.g. \"--user\", \"1000:1000\"), or point " +
        "GROUPDOCS_MCP_OUTPUT_PATH at a writable mounted folder.";

    public static string Format(string op, string file, Exception ex, string? subjectSuffix = null)
    {
        var sb = new StringBuilder();
        sb.Append($"{op} failed for '{file}'{subjectSuffix}: {ex.GetType().FullName}: {ex.Message}");
        var inner = ex.InnerException;
        for (int d = 0; inner != null && d < 5; d++, inner = inner.InnerException)
        {
            sb.Append($" | inner({d}): {inner.GetType().FullName}: {inner.Message}");
        }
        if (IsPermissionFailure(ex))
        {
            sb.Append(PermissionHint);
        }
        return sb.ToString();
    }

    // UnauthorizedAccessException anywhere in the chain, or the OS "Permission denied" /
    // "Read-only file system" IOException that .NET raises for a bind mount it cannot write.
    internal static bool IsPermissionFailure(Exception ex)
    {
        for (Exception? e = ex; e != null; e = e.InnerException)
        {
            if (e is UnauthorizedAccessException) return true;
            if (e is IOException io &&
                (io.Message.Contains("Permission denied", StringComparison.OrdinalIgnoreCase) ||
                 io.Message.Contains("Read-only file system", StringComparison.OrdinalIgnoreCase)))
                return true;
        }
        return false;
    }
}
