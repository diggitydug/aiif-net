using Aiif.Net.Options;
using Aiif.Net.Endpoints;
using Aiif.Net.Services;
using Aiif.Net.Swagger;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Aiif.Net.DependencyInjection;

public static class AiifServiceCollectionExtensions
{
    public static IServiceCollection AddAiif(
        this IServiceCollection services)
    {
        return services.AddAiif(_ => { });
    }

    public static IServiceCollection AddAiif(
        this IServiceCollection services,
        Action<AiifOptions> configureAiif)
    {
        return services.AddAiif(configureAiif, _ => { });
    }

    public static IServiceCollection AddAiif(
        this IServiceCollection services,
        Action<AiifOptions> configureAiif,
        Action<SwaggerGenOptions> configureSwagger)
    {
        services.AddEndpointsApiExplorer();
        services.Configure(configureAiif);
        services.AddSingleton<AiifDocumentBuilder>();
        services.AddSingleton<IStartupFilter, AiifStartupFilter>();

        services.AddSwaggerGen(options =>
        {
            options.DocumentFilter<AiifDocumentFilter>();
            configureSwagger(options);
        });

        return services;
    }

    /// <summary>
    /// Convenience method to populate AIIF options from OpenApiInfo (from SwaggerDoc).
    /// Use this to avoid duplicating API title and description between SwaggerDoc and AIIF.
    /// 
    /// Example:
    /// <code>
    /// var openApiInfo = new OpenApiInfo { Title = "My API", Description = "My API Description" };
    /// services.AddAiif(aiif => { aiif.ApiVersion = "v1"; }, sw => sw.SwaggerDoc("v1", openApiInfo));
    /// services.AutoMapAiifFromSwagger(openApiInfo);
    /// </code>
    /// </summary>
    public static IServiceCollection AutoMapAiifFromSwagger(
        this IServiceCollection services,
        OpenApiInfo openApiInfo)
    {
        services.Configure<AiifOptions>(options =>
        {
            if (string.IsNullOrWhiteSpace(options.ApiName) && !string.IsNullOrWhiteSpace(openApiInfo?.Title))
            {
                options.ApiName = openApiInfo.Title;
            }

            if (string.IsNullOrWhiteSpace(options.ApiDescription) && !string.IsNullOrWhiteSpace(openApiInfo?.Description))
            {
                options.ApiDescription = openApiInfo.Description;
            }
        });

        return services;
    }
}
