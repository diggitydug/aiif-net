using Aiif.Net.Options;
using Aiif.Net.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Aiif.Net.Endpoints;

public static class AiifEndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapAiifEndpoints(this IEndpointRouteBuilder endpoints)
    {
        if (HasAiifEndpoints(endpoints))
        {
            return endpoints;
        }

        var options = endpoints.ServiceProvider.GetRequiredService<IOptions<AiifOptions>>().Value;
        var basePath = NormalizeBasePath(options.BaseDocsPath);

        var documentDescription = ResolveDescription(
            options.EndpointDescriptions.Document,
            "Returns the full AIIF document for this API, including auth guidance, endpoint catalog, and agent rules.");
        var summaryDescription = ResolveDescription(
            options.EndpointDescriptions.Summary,
            "Returns a lightweight AIIF endpoint catalog for discovery (name, method, path, description, and auth requirement).");
        var endpointDetailDescription = ResolveDescription(
            options.EndpointDescriptions.EndpointDetail,
            "Returns the AIIF document for a single endpoint by endpoint name or route path.");
        var authDescription = ResolveDescription(
            options.EndpointDescriptions.Auth,
            "Returns AIIF authentication instructions, token acquisition details, and auth application rules.");

        endpoints.MapGet(basePath, (AiifDocumentBuilder builder) =>
            Results.Json(builder.BuildDocument()))
            .WithName("GetAiifDocument")
            .WithDescription(documentDescription)
            .WithTags("AIIF");

        endpoints.MapGet($"{basePath}/summary", (AiifDocumentBuilder builder) =>
            Results.Json(builder.BuildSummary()))
            .WithName("GetAiifSummary")
            .WithDescription(summaryDescription)
            .WithTags("AIIF");

        if (!string.Equals(options.Auth.Type, "none", StringComparison.OrdinalIgnoreCase))
        {
            endpoints.MapGet($"{basePath}/auth", (AiifDocumentBuilder builder) =>
                Results.Json(builder.BuildAuth()))
                .WithName("GetAiifAuth")
                .WithDescription(authDescription)
                .WithTags("AIIF");
        }

        endpoints.MapGet($"{basePath}/{{**endpoint}}", (string endpoint, AiifDocumentBuilder builder) =>
        {
            var document = builder.BuildEndpointDocument(endpoint);
            return document is null ? Results.NotFound() : Results.Json(document);
        })
            .WithName("GetAiifEndpoint")
            .WithDescription(endpointDetailDescription)
            .WithTags("AIIF");

        return endpoints;
    }

    private static bool HasAiifEndpoints(IEndpointRouteBuilder endpoints)
    {
        return endpoints.DataSources
            .SelectMany(source => source.Endpoints)
            .Any(endpoint =>
            {
                var endpointName = endpoint.Metadata.GetMetadata<IEndpointNameMetadata>()?.EndpointName;
                return endpointName is "GetAiifDocument" or "GetAiifSummary" or "GetAiifEndpoint" or "GetAiifAuth";
            });
    }

    private static string NormalizeBasePath(string basePath)
    {
        if (string.IsNullOrWhiteSpace(basePath))
        {
            return "/ai-docs";
        }

        var path = basePath.Trim();
        if (!path.StartsWith('/'))
        {
            path = "/" + path;
        }

        return path.TrimEnd('/');
    }

    private static string ResolveDescription(string? configured, string fallback)
    {
        return string.IsNullOrWhiteSpace(configured) ? fallback : configured.Trim();
    }
}
