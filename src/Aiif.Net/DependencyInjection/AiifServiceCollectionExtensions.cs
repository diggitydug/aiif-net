using Aiif.Net.Options;
using Aiif.Net.Endpoints;
using Aiif.Net.Services;
using Aiif.Net.Swagger;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
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
}
