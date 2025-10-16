using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace MozillaAI.Mcpd.Plugins.V1;

/// <summary>
/// BasePlugin provides sensible default implementations for all plugin methods.
/// Plugin developers can inherit from this class and override only the methods they need.
/// </summary>
/// <remarks>
/// Default behaviors:
/// <list type="bullet">
/// <item><description>Configure: no-op</description></item>
/// <item><description>Stop: no-op</description></item>
/// <item><description>GetMetadata: returns empty metadata (should be overridden)</description></item>
/// <item><description>GetCapabilities: returns no flows (should be overridden)</description></item>
/// <item><description>CheckHealth: returns OK</description></item>
/// <item><description>CheckReady: returns OK</description></item>
/// <item><description>HandleRequest: passes through unchanged (continue=true)</description></item>
/// <item><description>HandleResponse: passes through unchanged (continue=true)</description></item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// public class MyPlugin : BasePlugin
/// {
///     public override Task&lt;Metadata&gt; GetMetadata(Empty request, ServerCallContext context)
///     {
///         return Task.FromResult(new Metadata
///         {
///             Name = "my-plugin",
///             Version = "1.0.0",
///             Description = "Example plugin"
///         });
///     }
///
///     public override Task&lt;HTTPResponse&gt; HandleRequest(HTTPRequest request, ServerCallContext context)
///     {
///         // Custom logic here.
///         return Task.FromResult(new HTTPResponse { Continue = true });
///     }
/// }
/// </code>
/// </example>
public class BasePlugin : Plugin.PluginBase
{
    /// <summary>
    /// Logger instance for the plugin.
    /// </summary>
    protected ILogger Logger { get; private set; } = NullLogger.Instance;

    /// <summary>
    /// SetLogger is called by the SDK to provide a logger to the plugin.
    /// Plugins can override this to perform additional setup with the logger.
    /// </summary>
    /// <param name="logger">The logger instance to use.</param>
    public virtual void SetLogger(ILogger logger)
    {
        Logger = logger;
    }

    /// <summary>
    /// Configure is a no-op by default.
    /// </summary>
    public override Task<Empty> Configure(PluginConfig request, ServerCallContext context)
    {
        return Task.FromResult(new Empty());
    }

    /// <summary>
    /// Stop is a no-op by default.
    /// </summary>
    public override Task<Empty> Stop(Empty request, ServerCallContext context)
    {
        return Task.FromResult(new Empty());
    }

    /// <summary>
    /// GetMetadata returns empty metadata by default. Plugins should override this.
    /// </summary>
    public override Task<Metadata> GetMetadata(Empty request, ServerCallContext context)
    {
        return Task.FromResult(new Metadata());
    }

    /// <summary>
    /// GetCapabilities returns no flows by default. Plugins should override this.
    /// </summary>
    public override Task<Capabilities> GetCapabilities(Empty request, ServerCallContext context)
    {
        return Task.FromResult(new Capabilities());
    }

    /// <summary>
    /// CheckHealth returns OK by default.
    /// </summary>
    public override Task<Empty> CheckHealth(Empty request, ServerCallContext context)
    {
        return Task.FromResult(new Empty());
    }

    /// <summary>
    /// CheckReady returns OK by default.
    /// </summary>
    public override Task<Empty> CheckReady(Empty request, ServerCallContext context)
    {
        return Task.FromResult(new Empty());
    }

    /// <summary>
    /// HandleRequest passes through the request unchanged with continue=true.
    /// </summary>
    public override Task<HTTPResponse> HandleRequest(HTTPRequest request, ServerCallContext context)
    {
        return Task.FromResult(new HTTPResponse
        {
            Continue = true,
            StatusCode = 0,
            Headers = { request.Headers },
            Body = request.Body
        });
    }

    /// <summary>
    /// HandleResponse passes through the response unchanged with continue=true.
    /// </summary>
    public override Task<HTTPResponse> HandleResponse(HTTPResponse request, ServerCallContext context)
    {
        return Task.FromResult(new HTTPResponse
        {
            Continue = true,
            StatusCode = request.StatusCode,
            Headers = { request.Headers },
            Body = request.Body
        });
    }
}
