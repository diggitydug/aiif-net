using System.Text.Json;
using Aiif.Net.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Aiif.Net.Endpoints;

public static class AiifDocumentExportExtensions
{
    public static Task ExportAiifDocumentToFileAsync(
        this WebApplication app,
        string outputPath,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(app);
        return app.Services.ExportAiifDocumentToFileAsync(outputPath, cancellationToken);
    }

    public static async Task ExportAiifDocumentToFileAsync(
        this IServiceProvider services,
        string outputPath,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);

        var builder = services.GetRequiredService<AiifDocumentBuilder>();
        var document = builder.BuildDocument();

        var serializerOptions = services.GetService<IOptions<JsonOptions>>()?.Value?.SerializerOptions;
        var effectiveSerializerOptions = serializerOptions is null
            ? new JsonSerializerOptions(JsonSerializerDefaults.Web)
            : new JsonSerializerOptions(serializerOptions);
        effectiveSerializerOptions.WriteIndented = true;

        var fullOutputPath = Path.GetFullPath(outputPath);
        var outputDirectory = Path.GetDirectoryName(fullOutputPath);
        if (!string.IsNullOrWhiteSpace(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }

        var json = JsonSerializer.Serialize(document, effectiveSerializerOptions);
        await File.WriteAllTextAsync(fullOutputPath, json, cancellationToken);
    }
}