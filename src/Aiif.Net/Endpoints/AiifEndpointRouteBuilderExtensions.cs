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

        endpoints.MapGet(basePath, (AiifDocumentBuilder builder) =>
            Results.Json(builder.BuildDocument()))
            .WithName("GetAiifDocument")
            .WithTags("AIIF");

        endpoints.MapGet($"{basePath}/summary", (AiifDocumentBuilder builder) =>
            Results.Json(builder.BuildSummary()))
            .WithName("GetAiifSummary")
            .WithTags("AIIF");

        if (!string.Equals(options.Auth.Type, "none", StringComparison.OrdinalIgnoreCase))
        {
            endpoints.MapGet($"{basePath}/auth", (AiifDocumentBuilder builder) =>
                Results.Json(builder.BuildAuth()))
                .WithName("GetAiifAuth")
                .WithTags("AIIF");
        }

        endpoints.MapGet($"{basePath}/{{endpoint}}", (string endpoint, AiifDocumentBuilder builder) =>
        {
            var document = builder.BuildEndpointDocument(endpoint);
            return document is null ? Results.NotFound() : Results.Json(document);
        })
            .WithName("GetAiifEndpoint")
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
}
