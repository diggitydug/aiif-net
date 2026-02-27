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

    private static async Task<WebApplication> CreateAppAsync()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();

        builder.Services.AddAiif(options =>
        {
            options.AutoMapEndpoints = false;
            options.AiifVersion = "1.0";
            options.BaseDocsPath = "/ai-docs";
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
        });

        var app = builder.Build();

        app.MapGet("/weather/current", () => Results.Ok(new { temperature = 21 }));
        app.MapAiifEndpoints();

        await app.StartAsync();
        return app;
    }
}
