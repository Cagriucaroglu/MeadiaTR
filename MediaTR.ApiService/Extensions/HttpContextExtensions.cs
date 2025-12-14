namespace MediaTR.ApiService.Extensions;

/// <summary>
/// Extension methods for HttpContext to support platform detection and IP extraction.
/// </summary>
public static class HttpContextExtensions
{
    private const string ClientTypeHeader = "X-Client-Type";

    /// <summary>
    /// Determines if the request is from a mobile client based on X-Client-Type header.
    /// </summary>
    /// <param name="context">HTTP context</param>
    /// <returns>True if client type is "mobile", false otherwise (defaults to web)</returns>
    public static bool IsMobileClient(this HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(ClientTypeHeader, out var clientType))
        {
            return string.Equals(clientType.ToString(), "mobile", StringComparison.OrdinalIgnoreCase);
        }

        // Default to web if header is missing (backward compatibility)
        return false;
    }

    /// <summary>
    /// Extracts the client's IP address from the HTTP context.
    /// </summary>
    /// <param name="context">HTTP context</param>
    /// <returns>Client IP address or "unknown" if not available</returns>
    public static string GetClientIpAddress(this HttpContext context)
    {
        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}
