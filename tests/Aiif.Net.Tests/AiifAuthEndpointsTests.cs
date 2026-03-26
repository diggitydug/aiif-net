using System.Text.Json;
using Aiif.Net.DependencyInjection;
using Aiif.Net.Endpoints;
using Aiif.Net.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Aiif.Net.Tests;

public sealed class AiifAuthEndpointsTests
{
    [Fact]
    public async Task AiDocs_Summary_Uses_Default_Descriptions_For_Aiif_Endpoints()
    {
        await using var app = await CreateAppAsync();
        var client = app.GetTestClient();

        var response = await client.GetAsync("/ai-docs/summary");
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(stream);

        var endpoints = document.RootElement.GetProperty("endpoints").EnumerateArray().ToList();

        var auth = endpoints.First(e => e.GetProperty("name").GetString() == "get_aiif_auth");
        Assert.Equal(
            "Returns AIIF authentication instructions, token acquisition details, and auth application rules.",
            auth.GetProperty("description").GetString());

        var summary = endpoints.First(e => e.GetProperty("name").GetString() == "get_aiif_summary");
        Assert.Equal(
            "Returns a lightweight AIIF endpoint catalog for discovery (name, method, path, description, and auth requirement).",
            summary.GetProperty("description").GetString());

        var endpoint = endpoints.First(e => e.GetProperty("name").GetString() == "get_aiif_endpoint");
        Assert.Equal(
            "Returns the AIIF document for a single endpoint by endpoint name or route path.",
            endpoint.GetProperty("description").GetString());
    }

    [Fact]
    public async Task AiDocs_Summary_Allows_Developer_Override_For_Aiif_Descriptions()
    {
        await using var app = await CreateAppAsync(options =>
        {
            options.EndpointDescriptions.Summary = "Custom summary description.";
            options.EndpointDescriptions.EndpointDetail = "Custom endpoint detail description.";
            options.EndpointDescriptions.Auth = "Custom auth description.";
        });

        var client = app.GetTestClient();
        var response = await client.GetAsync("/ai-docs/summary");
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(stream);

        var endpoints = document.RootElement.GetProperty("endpoints").EnumerateArray().ToList();

        var auth = endpoints.First(e => e.GetProperty("name").GetString() == "get_aiif_auth");
        Assert.Equal("Custom auth description.", auth.GetProperty("description").GetString());

        var summary = endpoints.First(e => e.GetProperty("name").GetString() == "get_aiif_summary");
        Assert.Equal("Custom summary description.", summary.GetProperty("description").GetString());

        var endpoint = endpoints.First(e => e.GetProperty("name").GetString() == "get_aiif_endpoint");
        Assert.Equal("Custom endpoint detail description.", endpoint.GetProperty("description").GetString());
    }

    [Fact]
    public async Task AiDocsAuth_Returns_Instruction_List_And_Auth_Flows()
    {
        await using var app = await CreateAppAsync();
        var client = app.GetTestClient();

        var response = await client.GetAsync("/ai-docs/auth");
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(stream);

        Assert.Equal("bearer", document.RootElement.GetProperty("type").GetString());

        var instructions = document.RootElement.GetProperty("instructions");
        Assert.Equal(JsonValueKind.Array, instructions.ValueKind);
        Assert.All(instructions.EnumerateArray(), item => Assert.Equal(JsonValueKind.String, item.ValueKind));
        Assert.True(instructions.GetArrayLength() >= 2);

        var acquire = document.RootElement.GetProperty("acquire");
        Assert.Equal("/auth/token", acquire.GetProperty("endpoint_path").GetString());
        Assert.Equal("POST", acquire.GetProperty("method").GetString());

        var apply = document.RootElement.GetProperty("apply");
        Assert.Equal("header", apply.GetProperty("location").GetString());
        Assert.Equal("Authorization", apply.GetProperty("name").GetString());

        var refresh = document.RootElement.GetProperty("refresh");
        Assert.Equal("refresh_token", refresh.GetProperty("strategy").GetString());
    }

    [Fact]
    public async Task AiDocs_Includes_TopLevel_Auth_Instructions_Array()
    {
        await using var app = await CreateAppAsync();
        var client = app.GetTestClient();

        var response = await client.GetAsync("/ai-docs");
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(stream);

        var auth = document.RootElement.GetProperty("auth");
        var instructions = auth.GetProperty("instructions");

        Assert.Equal(JsonValueKind.Array, instructions.ValueKind);
        Assert.True(instructions.GetArrayLength() > 0);
        Assert.All(instructions.EnumerateArray(), item => Assert.Equal(JsonValueKind.String, item.ValueKind));
    }

    [Fact]
    public async Task AiDocs_EndpointDocument_Can_Be_Resolved_By_Path()
    {
        await using var app = await CreateAppAsync();
        var client = app.GetTestClient();

        var response = await client.GetAsync("/ai-docs/weather/current");
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(stream);

        var endpoint = document.RootElement.GetProperty("endpoint");
        Assert.Equal("/weather/current", endpoint.GetProperty("path").GetString());
    }

    [Fact]
    public async Task AiDocs_EndpointDocument_Can_Be_Resolved_By_Name()
    {
        await using var app = await CreateAppAsync();
        var client = app.GetTestClient();

        var summaryResponse = await client.GetAsync("/ai-docs/summary");
        summaryResponse.EnsureSuccessStatusCode();

        await using var summaryStream = await summaryResponse.Content.ReadAsStreamAsync();
        using var summaryDocument = await JsonDocument.ParseAsync(summaryStream);

        var endpointName = summaryDocument.RootElement
            .GetProperty("endpoints")
            .EnumerateArray()
            .First(endpoint => endpoint.GetProperty("path").GetString() == "/weather/current")
            .GetProperty("name")
            .GetString();

        Assert.False(string.IsNullOrWhiteSpace(endpointName));

        var response = await client.GetAsync($"/ai-docs/{endpointName}");
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(stream);

        var endpoint = document.RootElement.GetProperty("endpoint");
        Assert.Equal(endpointName, endpoint.GetProperty("name").GetString());
    }

    [Fact]
    public async Task ExportAiifDocumentToFileAsync_Writes_Aiif_Json_File()
    {
        await using var app = await CreateAppAsync();
        var outputPath = Path.Combine(Path.GetTempPath(), $"aiif-net-{Guid.NewGuid():N}.json");

        await app.ExportAiifDocumentToFileAsync(outputPath);

        Assert.True(File.Exists(outputPath));

        await using var stream = File.OpenRead(outputPath);
        using var document = await JsonDocument.ParseAsync(stream);

        Assert.Equal("1.0", document.RootElement.GetProperty("aiif_version").GetString());
        Assert.Equal("Test API", document.RootElement.GetProperty("info").GetProperty("name").GetString());

        File.Delete(outputPath);
    }

    private static async Task<WebApplication> CreateAppAsync(Action<AiifOptions>? configure = null)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();

        builder.Services.AddAiif(options =>
        {
            options.AutoMapEndpoints = false;
            options.AiifVersion = "1.0";
            options.ApiName = "Test API";
            options.ApiDescription = "Test API Description";
            options.BaseUrl = "https://api.example.com/v1";
            options.ApiVersion = "1.0.0";
            options.Auth = new AiifAuthOptions
            {
                Type = "bearer",
                Description = "Include a valid access token in the Authorization header.",
                Header = "Authorization",
                Scheme = "Bearer",
                Instructions =
                [
                    "Acquire an access token using POST /auth/token before calling protected endpoints.",
                    "Send Authorization: Bearer <token> on protected requests."
                ],
                Acquire = new AiifAuthAcquireOptions
                {
                    EndpointPath = "/auth/token",
                    Method = "POST",
                    ResponseTokenField = "access_token",
                    ResponseExpiresInField = "expires_in",
                    ResponseRefreshTokenField = "refresh_token"
                },
                Apply = new AiifAuthApplyOptions
                {
                    Location = "header",
                    Name = "Authorization",
                    Prefix = "Bearer"
                },
                Refresh = new AiifAuthRefreshOptions
                {
                    Strategy = "refresh_token",
                    EndpointPath = "/auth/refresh",
                    Method = "POST",
                    BeforeExpirySeconds = 60
                }
            };

            configure?.Invoke(options);
        });

        var app = builder.Build();

        app.MapGet("/weather/current", () => Results.Ok(new { temperature = 21 }));
        app.MapAiifEndpoints();

        await app.StartAsync();
        return app;
    }
}
