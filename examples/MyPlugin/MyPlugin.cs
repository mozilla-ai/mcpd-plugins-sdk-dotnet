using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;
using MozillaAI.Mcpd.Plugins.V1;

/// <summary>
/// Example plugin implementation that demonstrates basic plugin functionality.
/// </summary>
public class MyPlugin : BasePlugin
{
    public override Task<Metadata> GetMetadata(Empty request, Grpc.Core.ServerCallContext context)
    {
        return Task.FromResult(new Metadata
        {
            Name = "my-plugin",
            Version = "1.0.0",
            Description = "Example plugin that demonstrates basic functionality"
        });
    }

    public override Task<Capabilities> GetCapabilities(Empty request, Grpc.Core.ServerCallContext context)
    {
        return Task.FromResult(new Capabilities
        {
            Flows = { FlowConstants.FlowRequest }
        });
    }

    public override Task<HTTPResponse> HandleRequest(HTTPRequest request, Grpc.Core.ServerCallContext context)
    {
        Logger.LogInformation("Processing request: {Method} {Path}", request.Method, request.Path);

        // Example: Add a custom header to all requests.
        var response = new HTTPResponse
        {
            Continue = true,
            StatusCode = 0,
            Body = request.Body
        };

        // Copy existing headers.
        foreach (var header in request.Headers)
        {
            response.Headers.Add(header.Key, header.Value);
        }

        // Add custom header.
        response.Headers["X-My-Plugin"] = "processed";
        Logger.LogDebug("Added custom header X-My-Plugin");

        return Task.FromResult(response);
    }
}
