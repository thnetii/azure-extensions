using System.Diagnostics.Tracing;

using Azure.Core.Diagnostics;

using Microsoft.Extensions.Logging;

namespace THNETII.AzureCore.Diagnostics.LoggingExtensions;

public sealed class AzureCoreDiagnosticsEventSourceLogForwarder : IDisposable
{
    private readonly ILogger logger;
    private readonly AzureEventSourceListener listener;

    public AzureCoreDiagnosticsEventSourceLogForwarder(
        ILoggerFactory? loggerFactory
        ) : base()
    {
        loggerFactory ??= Microsoft.Extensions.Logging.Abstractions
            .NullLoggerFactory.Instance;

        const string loggerName = $"{nameof(Azure)}."
            + $"{nameof(Azure.Core)}."
            + $"{nameof(Azure.Core.Diagnostics)}."
            + AzureEventSourceListener.TraitName;
        logger = loggerFactory.CreateLogger(loggerName);

        var eventLevel = EventLevel.LogAlways;
        for (int i = 0; i <= (int)LogLevel.Critical; i++)
        {
            var logLevel = (LogLevel)i;
            eventLevel = LogLevelToEventLevel(logLevel);
            if (logger.IsEnabled(logLevel))
                break;
        }
        listener = new AzureEventSourceListener(OnEventWritten, eventLevel);
    }

    private void OnEventWritten(EventWrittenEventArgs args, string message)
    {
        LogLevel logLevel = EventLevelToLogLevel(args.Level);
        EventId eventId = new(args.EventId, args.EventName);
        var state = EventArgsToDictionary(args, out var exception);
        logger.Log(logLevel, eventId, state, exception, (_1, _2) => message);
    }

    private static Dictionary<string, object?> EventArgsToDictionary(EventWrittenEventArgs args, out Exception? exception)
    {
        var dict = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
        {
            { nameof(args.ActivityId), args.ActivityId },
            { nameof(args.Channel), args.Channel },
            { nameof(args.EventSource), args.EventSource.Name },
            { nameof(args.EventSource) + nameof(Guid), args.EventSource.Guid },
            { nameof(EventKeywords), args.Keywords },
            { nameof(EventLevel), args.Level },
            { nameof(EventOpcode), args.Opcode },
            { nameof(args.RelatedActivityId), args.RelatedActivityId },
            { nameof(EventTags), args.Tags },
            { nameof(EventTask), args.Task },
            { nameof(args.Version), args.Version },
            #if NETCOREAPP3_1_OR_GREATER
            { nameof(args.OSThreadId), args.OSThreadId },
            { nameof(args.TimeStamp), args.TimeStamp },
            #endif
        };

        List<Exception> exceptions = new();
        if (args.PayloadNames is { Count: int nameCount }
            && args.Payload is { Count: int valueCount }
            && nameCount == valueCount)
        {
#if NETCOREAPP3_1_OR_GREATER
            foreach (var (name, value) in args.PayloadNames.Zip(args.Payload))
            {
#else
            foreach (var (name, value) in args.PayloadNames.Zip(args.Payload, (name, value) => (name, value)))
            {
#endif
                if (!dict.ContainsKey(name))
                    dict.Add(name, value);
                if (value is AggregateException aggrExcept)
                    exceptions.AddRange(aggrExcept.InnerExceptions);
                else if (value is Exception except)
                    exceptions.Add(except);
            }
        }

        exception = exceptions.Count switch
        {
            1 => exceptions[0],
            > 1 => new AggregateException(exceptions),
            _ => null,
        };

        return dict;
    }

    private static EventLevel LogLevelToEventLevel(LogLevel level) => level switch
    {
        LogLevel.Trace => EventLevel.LogAlways,
        LogLevel.Debug => EventLevel.Verbose,
        LogLevel.Information => EventLevel.Informational,
        LogLevel.Warning => EventLevel.Warning,
        LogLevel.Error => EventLevel.Error,
        LogLevel.Critical => EventLevel.Critical,
        _ => EventLevel.LogAlways,
    };

    private static LogLevel EventLevelToLogLevel(EventLevel level) => level switch
    {
        EventLevel.LogAlways => LogLevel.Trace,
        EventLevel.Critical => LogLevel.Critical,
        EventLevel.Error => LogLevel.Error,
        EventLevel.Warning => LogLevel.Warning,
        EventLevel.Informational => LogLevel.Information,
        EventLevel.Verbose => LogLevel.Debug,
        _ => LogLevel.None,
    };

    public void Dispose()
    {
        ((IDisposable)listener).Dispose();
    }
}
