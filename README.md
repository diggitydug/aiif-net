# Aiif.Net

`Aiif.Net` is a .NET library for exposing AIIF v1 documentation endpoints from ASP.NET Core APIs.

It integrates with existing Swashbuckle `AddSwaggerGen(...)` configuration and only adds AIIF-specific OpenAPI entries.

It generates and serves the required AIIF endpoints:

- `GET /ai-docs` (full AIIF document)
- `GET /ai-docs/summary` (lightweight endpoint catalog)
- `GET /ai-docs/{endpoint}` (single endpoint document by exact endpoint `name`)
- `GET /ai-docs/auth` (served when `auth.type` is not `none`)

## Install

```xml
<ItemGroup>
  <PackageReference Include="Aiif.Net" Version="0.1.0" />
</ItemGroup>
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

app.Run();
```

`Aiif.Net` auto-maps AIIF endpoints by default (`AutoMapEndpoints = true`).

If you prefer explicit mapping, set `AutoMapEndpoints = false` and call `app.MapAiifEndpoints()` manually.

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

For `GET /ai-docs/{endpoint}`, `{endpoint}` must match endpoint `name` exactly (case-sensitive), as required by AIIF v1.

If no explicit endpoint name is provided, `Aiif.Net` derives a snake_case name from method + route.

## OpenAPI Visibility

`Aiif.Net` adds AI docs routes to OpenAPI through the Swagger document filter so humans can see that AIIF endpoints are exposed.

## NuGet Publishing

This repository includes a GitHub Actions workflow at [.github/workflows/publish-nuget.yml](.github/workflows/publish-nuget.yml).

- Trigger: push a git tag in the format `vX.Y.Z` (example: `v0.2.0`).
- Package version: derived from the tag (the leading `v` is removed).
- Secret required: `NUGET_API_KEY` in repository settings.
- A GitHub organization is not required; publishing to public NuGet works with a personal NuGet.org account/API key.

Example release commands:

```bash
git tag v0.2.0
git push origin v0.2.0
```
