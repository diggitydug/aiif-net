using System.Text.Json.Serialization;

namespace Aiif.Net.Models;

public sealed class AiifDocument
{
    [JsonPropertyName("aiif_version")]
    public string AiifVersion { get; init; } = "1.0";

    [JsonPropertyName("info")]
    public required AiifInfo Info { get; init; }

    [JsonPropertyName("auth")]
    public AiifAuth? Auth { get; init; }

    [JsonPropertyName("endpoints")]
    public required List<AiifEndpoint> Endpoints { get; init; }

    [JsonPropertyName("schemas")]
    public Dictionary<string, object> Schemas { get; init; } = [];

    [JsonPropertyName("errors")]
    public Dictionary<string, AiifError> Errors { get; init; } = [];

    [JsonPropertyName("agent_rules")]
    public List<string> AgentRules { get; init; } = [];
}

public sealed class AiifInfo
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("description")]
    public required string Description { get; init; }

    [JsonPropertyName("base_url")]
    public required string BaseUrl { get; init; }

    [JsonPropertyName("version")]
    public string? Version { get; init; }
}

public sealed class AiifAuth
{
    [JsonPropertyName("type")]
    public required string Type { get; init; }

    [JsonPropertyName("description")]
    public required string Description { get; init; }

    [JsonPropertyName("header")]
    public string? Header { get; init; }

    [JsonPropertyName("scheme")]
    public string? Scheme { get; init; }

    [JsonPropertyName("instructions")]
    public List<string> Instructions { get; init; } = [];

    [JsonPropertyName("acquire")]
    public AiifAuthAcquire? Acquire { get; init; }

    [JsonPropertyName("apply")]
    public AiifAuthApply? Apply { get; init; }

    [JsonPropertyName("refresh")]
    public AiifAuthRefresh? Refresh { get; init; }
}

public sealed class AiifAuthAcquire
{
    [JsonPropertyName("endpoint_path")]
    public required string EndpointPath { get; init; }

    [JsonPropertyName("method")]
    public required string Method { get; init; }

    [JsonPropertyName("response_token_field")]
    public string? ResponseTokenField { get; init; }

    [JsonPropertyName("response_expires_in_field")]
    public string? ResponseExpiresInField { get; init; }

    [JsonPropertyName("response_refresh_token_field")]
    public string? ResponseRefreshTokenField { get; init; }
}

public sealed class AiifAuthApply
{
    [JsonPropertyName("location")]
    public required string Location { get; init; }

    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("prefix")]
    public string? Prefix { get; init; }
}

public sealed class AiifAuthRefresh
{
    [JsonPropertyName("strategy")]
    public required string Strategy { get; init; }

    [JsonPropertyName("endpoint_path")]
    public string? EndpointPath { get; init; }

    [JsonPropertyName("method")]
    public string? Method { get; init; }

    [JsonPropertyName("before_expiry_seconds")]
    public int? BeforeExpirySeconds { get; init; }
}

public sealed class AiifEndpoint
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("method")]
    public required string Method { get; init; }

    [JsonPropertyName("path")]
    public required string Path { get; init; }

    [JsonPropertyName("description")]
    public required string Description { get; init; }

    [JsonPropertyName("auth_required")]
    public bool? AuthRequired { get; init; }

    [JsonPropertyName("params")]
    public List<AiifParameter> Params { get; init; } = [];

    [JsonPropertyName("request")]
    public object? Request { get; init; }

    [JsonPropertyName("request_content_type")]
    public string? RequestContentType { get; init; }

    [JsonPropertyName("response_content_type")]
    public string? ResponseContentType { get; init; }

    [JsonPropertyName("response")]
    public required object Response { get; init; }

    [JsonPropertyName("errors")]
    public List<string> Errors { get; init; } = [];

    [JsonPropertyName("x_auth_instructions")]
    public List<string> AuthInstructions { get; init; } = [];

    [JsonPropertyName("x_agent_rules")]
    public List<string> AgentRules { get; init; } = [];
}

public sealed class AiifParameter
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("location")]
    public required string Location { get; init; }

    [JsonPropertyName("type")]
    public required string Type { get; init; }

    [JsonPropertyName("required")]
    public required bool Required { get; init; }

    [JsonPropertyName("description")]
    public required string Description { get; init; }
}

public sealed class AiifError
{
    [JsonPropertyName("code")]
    public required string Code { get; init; }

    [JsonPropertyName("http_status")]
    public int HttpStatus { get; init; }

    [JsonPropertyName("message")]
    public required string Message { get; init; }

    [JsonPropertyName("description")]
    public required string Description { get; init; }
}

public sealed class AiifSummaryDocument
{
    [JsonPropertyName("api")]
    public required string Api { get; init; }

    [JsonPropertyName("base_url")]
    public required string BaseUrl { get; init; }

    [JsonPropertyName("auth_docs_path")]
    public string? AuthDocsPath { get; init; }

    [JsonPropertyName("agent_rules")]
    public List<string> AgentRules { get; init; } = [];

    [JsonPropertyName("endpoints")]
    public required List<AiifSummaryEndpoint> Endpoints { get; init; }
}

public sealed class AiifSummaryEndpoint
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("method")]
    public required string Method { get; init; }

    [JsonPropertyName("path")]
    public required string Path { get; init; }

    [JsonPropertyName("description")]
    public required string Description { get; init; }

    [JsonPropertyName("auth_required")]
    public required bool AuthRequired { get; init; }
}

public sealed class AiifEndpointDocument
{
    [JsonPropertyName("endpoint")]
    public required AiifEndpoint Endpoint { get; init; }

    [JsonPropertyName("schemas")]
    public Dictionary<string, object> Schemas { get; init; } = [];

    [JsonPropertyName("errors")]
    public Dictionary<string, AiifError> Errors { get; init; } = [];

    [JsonPropertyName("agent_rules")]
    public List<string> AgentRules { get; init; } = [];
}
