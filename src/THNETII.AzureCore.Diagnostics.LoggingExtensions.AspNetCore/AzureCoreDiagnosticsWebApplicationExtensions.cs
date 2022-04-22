using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace THNETII.AzureCore.Diagnostics.LoggingExtensions;

public static class AzureCoreDiagnosticsWebApplicationExtensions
{
    public static IApplicationBuilder UseAzureCoreDiagnosticsLogger(
        this IApplicationBuilder app
        )
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(app);
#else
        _ = app ?? throw new ArgumentNullException(nameof(app));
#endif
        _ = app.ApplicationServices.GetRequiredService
            <AzureCoreDiagnosticsEventSourceLogForwarder>();
        return app;
    }
}
