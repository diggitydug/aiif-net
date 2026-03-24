using Aiif.Net.Options;
using Microsoft.AspNetCore.Http;

namespace Aiif.Net.Endpoints;

public static class AiifEndpointAccessPolicy
{
    public static bool IsAiifPath(PathString requestPath, AiifOptions options)
    {
        var normalizedPath = NormalizeRequestPath(requestPath);
        var basePath = NormalizeBasePath(options.BaseDocsPath);

        if (string.Equals(normalizedPath, basePath, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (string.Equals(normalizedPath, $"{basePath}/summary", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (string.Equals(normalizedPath, $"{basePath}/auth", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return normalizedPath.StartsWith($"{basePath}/", StringComparison.OrdinalIgnoreCase);
    }

    public static bool RequiresAuth(PathString requestPath, AiifOptions options)
    {
        var normalizedPath = NormalizeRequestPath(requestPath);
        var basePath = NormalizeBasePath(options.BaseDocsPath);

        if (string.Equals(normalizedPath, basePath, StringComparison.OrdinalIgnoreCase))
        {
            return options.EndpointAuth.RequireAuthForDocument;
        }

        if (string.Equals(normalizedPath, $"{basePath}/summary", StringComparison.OrdinalIgnoreCase))
        {
            return options.EndpointAuth.RequireAuthForSummary;
        }

        if (string.Equals(normalizedPath, $"{basePath}/auth", StringComparison.OrdinalIgnoreCase))
        {
            return options.EndpointAuth.RequireAuthForAuth;
        }

        if (normalizedPath.StartsWith($"{basePath}/", StringComparison.OrdinalIgnoreCase))
        {
            return options.EndpointAuth.RequireAuthForEndpointDetail;
        }

        return false;
    }

    private static string NormalizeRequestPath(PathString requestPath)
    {
        var value = requestPath.Value;
        if (string.IsNullOrWhiteSpace(value))
        {
            return "/";
        }

        var normalized = value.Trim();
        if (!normalized.StartsWith('/'))
        {
            normalized = "/" + normalized;
        }

        return normalized.Length > 1 ? normalized.TrimEnd('/') : normalized;
    }

    private static string NormalizeBasePath(string basePath)
    {
        if (string.IsNullOrWhiteSpace(basePath))
        {
            return "/ai-docs";
        }

        var normalized = basePath.Trim();
        if (!normalized.StartsWith('/'))
        {
            normalized = "/" + normalized;
        }

        return normalized.Length > 1 ? normalized.TrimEnd('/') : normalized;
    }
}