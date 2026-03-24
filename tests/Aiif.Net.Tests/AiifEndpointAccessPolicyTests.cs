using Aiif.Net.Endpoints;
using Aiif.Net.Options;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace Aiif.Net.Tests;

public sealed class AiifEndpointAccessPolicyTests
{
    [Theory]
    [InlineData("/ai-docs")]
    [InlineData("/ai-docs/")]
    [InlineData("/ai-docs/summary")]
    [InlineData("/ai-docs/auth")]
    [InlineData("/ai-docs/get_order")]
    public void IsAiifPath_ReturnsTrue_ForKnownAiifRoutes(string path)
    {
        var options = new AiifOptions();

        Assert.True(AiifEndpointAccessPolicy.IsAiifPath(new PathString(path), options));
    }

    [Fact]
    public void RequiresAuth_DefaultsToFalse_ForAiifRoutes()
    {
        var options = new AiifOptions();

        Assert.False(AiifEndpointAccessPolicy.RequiresAuth(new PathString("/ai-docs"), options));
        Assert.False(AiifEndpointAccessPolicy.RequiresAuth(new PathString("/ai-docs/summary"), options));
        Assert.False(AiifEndpointAccessPolicy.RequiresAuth(new PathString("/ai-docs/auth"), options));
        Assert.False(AiifEndpointAccessPolicy.RequiresAuth(new PathString("/ai-docs/get_order"), options));
    }

    [Fact]
    public void RequiresAuth_RespectsPerEndpointFlags()
    {
        var options = new AiifOptions
        {
            EndpointAuth = new AiifEndpointAuthOptions
            {
                RequireAuthForDocument = true,
                RequireAuthForSummary = false,
                RequireAuthForEndpointDetail = true,
                RequireAuthForAuth = true
            }
        };

        Assert.True(AiifEndpointAccessPolicy.RequiresAuth(new PathString("/ai-docs"), options));
        Assert.False(AiifEndpointAccessPolicy.RequiresAuth(new PathString("/ai-docs/summary"), options));
        Assert.True(AiifEndpointAccessPolicy.RequiresAuth(new PathString("/ai-docs/get_order"), options));
        Assert.True(AiifEndpointAccessPolicy.RequiresAuth(new PathString("/ai-docs/auth"), options));
    }
}
