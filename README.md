# Aiif.Net

`Aiif.Net` is a .NET library for exposing AIIF v1 documentation endpoints from ASP.NET Core APIs.

It integrates with existing Swashbuckle `AddSwaggerGen(...)` configuration and only adds AIIF-specific OpenAPI entries.

It generates and serves the required AIIF endpoints:

- `GET /ai-docs` (full AIIF document)
- `GET /ai-docs/summary` (lightweight endpoint catalog)
- `GET /ai-docs/{endpoint}` (single endpoint document by exact endpoint `name`)
- `GET /ai-docs/auth` (served when `auth.type` is not `none`)

## Install

```bash
dotnet add package Aiif.Net --version 0.2.0
```

## Local Package Build

Build a local NuGet package into a local feed directory:

```bash
./scripts/build-local-package.sh
```

Default output feed:

```text
./artifacts/local-packages
```

Optional custom output path:

```bash
./scripts/build-local-package.sh /absolute/path/to/local-packages
```

## Basic Setup

```csharp
using Aiif.Net.DependencyInjection;
using Aiif.Net.Endpoints;
using Aiif.Net.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAiif(
    aiif =>
    {
        aiif.AiifVersion = "1.0";
        aiif.BaseDocsPath = "/ai-docs";
        aiif.AutoMapEndpoints = false;

        aiif.ApiName = "Orders API";
        aiif.ApiDescription = "Manages orders and line items.";
        aiif.BaseUrl = "https://api.example.com/v1";
        aiif.ApiVersion = "1.2.0";

        aiif.AuthInstructionsFilePath = "aiif-auth.txt";
        aiif.AgentRulesFilePath = "aiif-rules.txt";

        aiif.Auth.Type = "bearer";
        aiif.Auth.Description = "Protected endpoints require bearer token.";
        aiif.Auth.Header = "Authorization";
        aiif.Auth.Scheme = "Bearer";
        aiif.Auth.Instructions =
        [
            "Acquire an access token using POST /auth/token before calling protected endpoints.",
            "Send Authorization: Bearer <token> on protected requests.",
            "Refresh credentials before expiry or when unauthorized is returned."
        ];
        aiif.Auth.Acquire = new AiifAuthAcquireOptions
        {
            EndpointPath = "/auth/token",
            Method = "POST",
            ResponseTokenField = "access_token",
            ResponseExpiresInField = "expires_in",
            ResponseRefreshTokenField = "refresh_token"
        };
        aiif.Auth.Apply = new AiifAuthApplyOptions
        {
            Location = "header",
            Name = "Authorization",
            Prefix = "Bearer"
        };
        aiif.Auth.Refresh = new AiifAuthRefreshOptions
        {
            Strategy = "refresh_token",
            EndpointPath = "/auth/refresh",
            Method = "POST",
            BeforeExpirySeconds = 60
        };
    },
    swagger =>
    {
        swagger.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title = "Orders API",
            Version = "v1",
            Description = "Swagger document for Orders API."
        });
    });

var app = builder.Build();
app.MapAiifEndpoints();

app.Run();
```

`Aiif.Net` supports auto-mapping AIIF endpoints (`AutoMapEndpoints = true`), but explicit mapping is recommended for predictable startup behavior.

Recommended setup:

```csharp
builder.Services.AddAiif(aiif =>
{
    aiif.AutoMapEndpoints = false;
});

var app = builder.Build();
app.MapAiifEndpoints();
```

This guarantees AIIF routes are available in all hosting setups.

## Export AIIF JSON Without Running The HTTP Server

`Aiif.Net` can generate the full AIIF document directly to a file from your app's DI container.

This is useful for CI pipelines and spec validation workflows where you want a static file without starting Kestrel and calling `/ai-docs` over HTTP.

### 1. Add a command path in your consuming app

```csharp
using Aiif.Net.Endpoints;

var builder = WebApplication.CreateBuilder(args);
var aiifExportPath = GetArgumentValue(args, "--export-aiif");

// ... AddAiif + endpoint mappings ...

var app = builder.Build();
app.MapAiifEndpoints();

if (!string.IsNullOrWhiteSpace(aiifExportPath))
{
    await app.ExportAiifDocumentToFileAsync(aiifExportPath);
    return;
}

app.Run();

static string? GetArgumentValue(string[] args, string key)
{
    for (var i = 0; i < args.Length; i++)
    {
        if (string.Equals(args[i], key, StringComparison.OrdinalIgnoreCase))
        {
            return i + 1 < args.Length ? args[i + 1] : null;
        }
    }

    return null;
}
```

### 2. Run the export command

```bash
dotnet run --project src/YourApi/YourApi.csproj -- --export-aiif ./generated/your-api.aiif.json
```

`Aiif.Net` will create the target directory if it does not exist.

### 3. Validate with `aiif-spec`

```bash
python ../aiif-spec/tools/validate-aiif.py --aiif ./generated/your-api.aiif.json
```

If your project uses a different Python command, replace `python` with `python3` or `py`.

## Reducing Configuration Boilerplate with Swagger Integration

`ApiName` and `ApiDescription` are now optional. If not explicitly set, they default to `"API"` and `"API documentation generated by Aiif.Net"` respectively.

To avoid duplicating API title and description between Swagger and AIIF, define the `OpenApiInfo` once and share it:

```csharp
using Aiif.Net.DependencyInjection;
using Aiif.Net.Options;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Define OpenApiInfo once
var openApiInfo = new OpenApiInfo
{
    Title = "Orders API",
    Version = "1.2.0",
    Description = "Manages orders and line items."
};

builder.Services.AddAiif(
    aiif =>
    {
        aiif.AiifVersion = "1.0";
        aiif.AutoMapEndpoints = false;
        // Omit ApiName and ApiDescription — they'll be populated from openApiInfo below
        aiif.BaseUrl = "https://api.example.com/v1";
        aiif.ApiVersion = openApiInfo.Version;
        aiif.Auth.Type = "bearer";
        // ... auth config ...
    },
    swagger =>
    {
        swagger.SwaggerDoc("v1", openApiInfo); // Reuse the same info
        // ... other swagger config ...
    });

// Auto-populate AIIF ApiName and ApiDescription from OpenApiInfo
builder.Services.AutoMapAiifFromSwagger(openApiInfo);

var app = builder.Build();
app.MapAiifEndpoints();
app.Run();
```

The `AutoMapAiifFromSwagger(...)` extension method copies the `Title` and `Description` from the `OpenApiInfo` to AIIF's `ApiName` and `ApiDescription` fields, eliminating duplication.

Default behavior:
- If `ApiName` is not explicitly set, it defaults to the Swagger `Title` (or `"API"` if no Swagger info is provided)
- If `ApiDescription` is not explicitly set, it defaults to the Swagger `Description` (or `"API documentation generated by Aiif.Net"` if no Swagger info is provided)
- `BaseDocsPath` is automatically set to `"/ai-docs"` and cannot be overridden

This approach follows the **single source of truth** principle—define metadata once in Swagger and AIIF uses it automatically.

## AIIF Endpoint Access Control

By default, AIIF docs endpoints are public and do not require authentication.

Default (public) endpoints:

- `GET /ai-docs`
- `GET /ai-docs/summary`
- `GET /ai-docs/{endpoint}`
- `GET /ai-docs/auth` (when `auth.type` is not `none`)

To require auth on specific AIIF docs routes, set `EndpointAuth` options:

```csharp
builder.Services.AddAiif(aiif =>
{
    aiif.EndpointAuth.RequireAuthForDocument = true;       // /ai-docs
    aiif.EndpointAuth.RequireAuthForSummary = false;       // /ai-docs/summary
    aiif.EndpointAuth.RequireAuthForEndpointDetail = true; // /ai-docs/{endpoint}
    aiif.EndpointAuth.RequireAuthForAuth = true;           // /ai-docs/auth
});
```

Notes:

- `Aiif.Net` only defines endpoint auth policy. Your app's auth middleware or auth attributes enforce it.
- For custom middleware, check `AiifEndpointAccessPolicy.IsAiifPath(...)` and `AiifEndpointAccessPolicy.RequiresAuth(...)`.
- Public-by-default keeps AI agent discovery simple and is the recommended starting point.

## AIIF Endpoint Descriptions

Aiif.Net now provides built-in default descriptions for AIIF routes:

- `GET /ai-docs`
- `GET /ai-docs/summary`
- `GET /ai-docs/{endpoint}`
- `GET /ai-docs/auth`

Default description texts:

- `/ai-docs`: `Returns the full AIIF document for this API, including auth guidance, endpoint catalog, and agent rules.`
- `/ai-docs/summary`: `Returns a lightweight AIIF endpoint catalog for discovery (name, method, path, description, and auth requirement).`
- `/ai-docs/{endpoint}`: `Returns the AIIF document for a single endpoint by endpoint name or route path.`
- `/ai-docs/auth`: `Returns AIIF authentication instructions, token acquisition details, and auth application rules.`

You can override these descriptions per endpoint if needed:

```csharp
builder.Services.AddAiif(aiif =>
{
    aiif.EndpointDescriptions.Document = "Custom description for /ai-docs";
    aiif.EndpointDescriptions.Summary = "Custom description for /ai-docs/summary";
    aiif.EndpointDescriptions.EndpointDetail = "Custom description for /ai-docs/{endpoint}";
    aiif.EndpointDescriptions.Auth = "Custom description for /ai-docs/auth";
});
```

If a value is empty or not set, Aiif.Net falls back to the built-in default description.

## Annotating Endpoints

### Minimal API annotation

```csharp
using Aiif.Net.Annotations;

app.MapGet("/orders/{order_id}", (string order_id) => Results.Ok(new { order_id }))
   .WithName("get_order")
   .WithMetadata(new AiifAuthInstructionAttribute("Requires bearer token with orders.read scope."))
   .WithMetadata(new AiifAgentRuleAttribute("If 429 is returned, wait at least 1 second before retry."));
```

### Controller annotation

```csharp
using Aiif.Net.Annotations;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("orders")]
public class OrdersController : ControllerBase
{
    [HttpGet("{order_id}")]
    [AiifAuthInstruction("Requires bearer token with orders.read scope.")]
    [AiifAgentRule("Do not retry 422 responses until input is corrected.")]
    public IActionResult Get(string order_id) => Ok(new { order_id });
}
```

## Auxiliary Files

`AddAiif(...)` supports local file paths for auth instructions and global agent rules.

- `AuthInstructionsFilePath`
- `AgentRulesFilePath`

Paths may be absolute or relative to the app content root.

Recommended format: one instruction/rule per line.

For auth guidance, include explicit token lifetime and refresh behavior (for example: token valid for 3600 seconds, refresh when 60 seconds remain, otherwise re-authenticate if refresh token is unavailable).

The generated `auth.instructions` field is emitted as a JSON array of plain-English strings per AIIF spec.

### aiif-auth.txt example

```text
Acquire access token via POST /auth/token.
Access tokens are valid for 3600 seconds (60 minutes).
Send Authorization: Bearer <token> for protected endpoints.
Use refresh token flow when your token is within 60 seconds of expiry.
If no refresh token is provided, re-authenticate via POST /auth/token.
```

### aiif-rules.txt example

```text
Do not call endpoints not explicitly listed in /ai-docs or /ai-docs/summary.
If a 429 response omits Retry-After, wait at least 1 second before retry.
Require explicit user confirmation before destructive operations.
```

## Endpoint Naming

For `GET /ai-docs/{endpoint}`, `{endpoint}` can be either:

- Endpoint `name` (recommended for stable AI agent integrations)
- Endpoint `path` (for example: `questions/unanswered` or `/questions/unanswered`)

Name matching is exact by value. Path matching is normalized to handle optional leading `/`.

If no explicit endpoint name is provided, `Aiif.Net` derives a snake_case name from method + route.

## OpenAPI Visibility

`Aiif.Net` adds AI docs routes to OpenAPI through the Swagger document filter so humans can see that AIIF endpoints are exposed.
