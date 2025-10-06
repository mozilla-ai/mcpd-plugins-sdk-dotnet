namespace MozillaAI.Mcpd.Plugins.V1;

/// <summary>
/// Provides constants for Flow enum values for convenience.
/// </summary>
public static class FlowConstants
{
    /// <summary>
    /// FlowRequest indicates the plugin handles incoming HTTP requests.
    /// </summary>
    public const Flow FlowRequest = Flow.Request;

    /// <summary>
    /// FlowResponse indicates the plugin handles outgoing HTTP responses.
    /// </summary>
    public const Flow FlowResponse = Flow.Response;
}
