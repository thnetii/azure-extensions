using Microsoft.Extensions.DependencyInjection;

namespace THNETII.AzureCore.Diagnostics.LoggingExtensions;

public static class AzureCoreDiagnosticsServiceCollectionExtensions
{
    public static IServiceCollection AddAzureCoreDiagnosticsLogging(
        this IServiceCollection services
        )
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(services);
#else
        _ = services ?? throw new ArgumentNullException(nameof(services));
#endif
        services.AddSingleton<AzureCoreDiagnosticsEventSourceLogForwarder>();
        return services;
    }
}
