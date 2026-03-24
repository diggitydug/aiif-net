namespace Aiif.Net.Options;

public sealed class AiifOptions
{
    public string AiifVersion { get; set; } = "1.0";
    public string BaseDocsPath { get; internal set; } = "/ai-docs";
    public bool AutoMapEndpoints { get; set; } = true;
    public AiifEndpointAuthOptions EndpointAuth { get; set; } = new();
    public AiifEndpointDescriptionOptions EndpointDescriptions { get; set; } = new();
    public string? ApiName { get; set; }
    public string? ApiDescription { get; set; }
    public string BaseUrl { get; set; } = "http://localhost";
    public string? ApiVersion { get; set; }
    public string? AuthInstructionsFilePath { get; set; }
    public string? AgentRulesFilePath { get; set; }
    public AiifAuthOptions Auth { get; set; } = new();
    
    /// <summary>
    /// Internal: Auto-populated from SwaggerDoc if ApiName/ApiDescription are not explicitly set.
    /// </summary>
    internal string? SwaggerDocumentName { get; set; } = "v1";
}

public sealed class AiifEndpointAuthOptions
{
    // Public AIIF docs is the default to keep setup simple.
    public bool RequireAuthForDocument { get; set; }
    public bool RequireAuthForSummary { get; set; }
    public bool RequireAuthForEndpointDetail { get; set; }
    public bool RequireAuthForAuth { get; set; }
}

public sealed class AiifEndpointDescriptionOptions
{
    // If not set, Aiif.Net will use built-in defaults.
    public string? Document { get; set; }
    public string? Summary { get; set; }
    public string? EndpointDetail { get; set; }
    public string? Auth { get; set; }
}

public sealed class AiifAuthOptions
{
    public string Type { get; set; } = "none";
    public string Description { get; set; } = "No authentication required.";
    public string? Header { get; set; }
    public string? Scheme { get; set; }
    public List<string> Instructions { get; set; } = [];
    public AiifAuthAcquireOptions? Acquire { get; set; }
    public AiifAuthApplyOptions? Apply { get; set; }
    public AiifAuthRefreshOptions? Refresh { get; set; }
}

public sealed class AiifAuthAcquireOptions
{
    public string EndpointPath { get; set; } = "/auth/token";
    public string Method { get; set; } = "POST";
    public string? ResponseTokenField { get; set; }
    public string? ResponseExpiresInField { get; set; }
    public string? ResponseRefreshTokenField { get; set; }
}

public sealed class AiifAuthApplyOptions
{
    public string Location { get; set; } = "header";
    public string Name { get; set; } = "Authorization";
    public string? Prefix { get; set; }
}

public sealed class AiifAuthRefreshOptions
{
    public string Strategy { get; set; } = "reauthenticate";
    public string? EndpointPath { get; set; }
    public string? Method { get; set; }
    public int? BeforeExpirySeconds { get; set; }
}
