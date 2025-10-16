using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;

namespace MozillaAI.Mcpd.Plugins.V1;

/// <summary>
/// Console formatter that outputs log messages in a format compatible with mcpd's log inference.
/// </summary>
internal sealed class McpdLogFormatter : ConsoleFormatter
{
    public McpdLogFormatter() : base("mcpd")
    {
    }

    public override void Write<TState>(in LogEntry<TState> logEntry, IExternalScopeProvider? scopeProvider, TextWriter textWriter)
    {
        var message = logEntry.Formatter(logEntry.State, logEntry.Exception);
        if (string.IsNullOrEmpty(message))
        {
            return;
        }

        var levelString = GetLevelString(logEntry.LogLevel);
        textWriter.Write(levelString);
        textWriter.Write(' ');
        textWriter.WriteLine(message);

        if (logEntry.Exception != null)
        {
            textWriter.WriteLine(logEntry.Exception.ToString());
        }
    }

    private static string GetLevelString(LogLevel logLevel)
    {
        return logLevel switch
        {
            LogLevel.Trace => "[TRACE]",
            LogLevel.Debug => "[DEBUG]",
            LogLevel.Information => "[INFO]",
            LogLevel.Warning => "[WARN]",
            LogLevel.Error => "[ERROR]",
            LogLevel.Critical => "[ERROR]",
            _ => "[INFO]"
        };
    }
}
