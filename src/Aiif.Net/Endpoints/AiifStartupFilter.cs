using Aiif.Net.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Aiif.Net.Endpoints;

internal sealed class AiifStartupFilter : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        return app =>
        {
            next(app);

            var options = app.ApplicationServices.GetRequiredService<IOptions<AiifOptions>>().Value;
            if (!options.AutoMapEndpoints)
            {
                return;
            }

            if (app is IEndpointRouteBuilder endpoints)
            {
                endpoints.MapAiifEndpoints();
            }
        };
    }
}
