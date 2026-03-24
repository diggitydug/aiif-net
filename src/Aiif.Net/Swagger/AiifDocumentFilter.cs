using Aiif.Net.Options;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Aiif.Net.Swagger;

public sealed class AiifDocumentFilter : IDocumentFilter
{
    private readonly AiifOptions _options;

    public AiifDocumentFilter(IOptions<AiifOptions> options)
    {
        _options = options.Value;
    }

    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        swaggerDoc.Extensions["x-aiif-version"] = new OpenApiString(_options.AiifVersion);
        swaggerDoc.Extensions["x-aiif-generated-by"] = new OpenApiString("Aiif.Net");

        swaggerDoc.Info ??= new OpenApiInfo();
        swaggerDoc.Info.Extensions["x-aiif-compliant"] = new OpenApiBoolean(true);

        EnsureAiifPaths(swaggerDoc);
    }

    private void EnsureAiifPaths(OpenApiDocument swaggerDoc)
    {
        var basePath = NormalizeBasePath(_options.BaseDocsPath);
        var summaryPath = $"{basePath}/summary";
        var endpointPath = $"{basePath}/{{endpoint}}";
        var authPath = $"{basePath}/auth";

        if (!swaggerDoc.Paths.ContainsKey(basePath))
        {
            swaggerDoc.Paths[basePath] = new OpenApiPathItem
            {
                Operations =
                {
                    [OperationType.Get] = new OpenApiOperation
                    {
                        Summary = "Get full AIIF document",
                        Description = "Returns the full AIIF v1 document for this API.",
                        Tags = [new OpenApiTag { Name = "AIIF" }],
                        Responses =
                        {
                            ["200"] = new OpenApiResponse { Description = "AIIF document" }
                        }
                    }
                }
            };
        }

        if (!swaggerDoc.Paths.ContainsKey(summaryPath))
        {
            swaggerDoc.Paths[summaryPath] = new OpenApiPathItem
            {
                Operations =
                {
                    [OperationType.Get] = new OpenApiOperation
                    {
                        Summary = "Get AIIF endpoint summary",
                        Description = "Returns a lightweight catalog of available endpoints.",
                        Tags = [new OpenApiTag { Name = "AIIF" }],
                        Responses =
                        {
                            ["200"] = new OpenApiResponse { Description = "AIIF summary" }
                        }
                    }
                }
            };
        }

        if (!swaggerDoc.Paths.ContainsKey(endpointPath))
        {
            swaggerDoc.Paths[endpointPath] = new OpenApiPathItem
            {
                Operations =
                {
                    [OperationType.Get] = new OpenApiOperation
                    {
                        Summary = "Get AIIF endpoint document",
                        Description = "Returns AIIF documentation for a single endpoint name.",
                        Tags = [new OpenApiTag { Name = "AIIF" }],
                        Parameters =
                        {
                            new OpenApiParameter
                            {
                                Name = "endpoint",
                                In = ParameterLocation.Path,
                                Required = true,
                                Description = "AIIF endpoint name or API route path",
                                Schema = new OpenApiSchema { Type = "string" }
                            }
                        },
                        Responses =
                        {
                            ["200"] = new OpenApiResponse { Description = "AIIF endpoint document" },
                            ["404"] = new OpenApiResponse { Description = "Endpoint not found" }
                        }
                    }
                }
            };
        }

        if (!string.Equals(_options.Auth.Type, "none", StringComparison.OrdinalIgnoreCase) && !swaggerDoc.Paths.ContainsKey(authPath))
        {
            swaggerDoc.Paths[authPath] = new OpenApiPathItem
            {
                Operations =
                {
                    [OperationType.Get] = new OpenApiOperation
                    {
                        Summary = "Get AIIF authentication guidance",
                        Description = "Returns auth flow documentation for agent clients.",
                        Tags = [new OpenApiTag { Name = "AIIF" }],
                        Responses =
                        {
                            ["200"] = new OpenApiResponse { Description = "AIIF auth document" }
                        }
                    }
                }
            };
        }
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
