using System.CommandLine;
using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace MozillaAI.Mcpd.Plugins.V1;

/// <summary>
/// PluginServer provides a convenience method for running a plugin server with minimal boilerplate.
/// It handles command-line argument parsing, network setup, gRPC server creation, and graceful shutdown.
/// </summary>
/// <example>
/// <code>
/// public class Program
/// {
///     public static async Task&lt;int&gt; Main(string[] args)
///     {
///         return await PluginServer.Serve&lt;MyPlugin&gt;(args);
///     }
/// }
/// </code>
/// </example>
public static class PluginServer
{
    /// <summary>
    /// Starts a gRPC server for the specified plugin implementation.
    /// </summary>
    /// <typeparam name="T">The plugin implementation type that inherits from Plugin.PluginBase.</typeparam>
    /// <param name="args">Command-line arguments.</param>
    /// <param name="logger">Optional logger instance. If null, a default console logger is created.</param>
    /// <returns>Exit code (0 for success, 1 for error).</returns>
    public static async Task<int> Serve<T>(string[] args, ILogger? logger = null) where T : Plugin.PluginBase, new()
    {
        return await Serve(new T(), args, logger);
    }

    /// <summary>
    /// Starts a gRPC server for the specified plugin instance.
    /// </summary>
    /// <param name="implementation">The plugin implementation instance.</param>
    /// <param name="args">Command-line arguments.</param>
    /// <param name="logger">Optional logger instance. If null, a default console logger is created.</param>
    /// <returns>Exit code (0 for success, 1 for error).</returns>
    public static async Task<int> Serve(Plugin.PluginBase implementation, string[] args, ILogger? logger = null)
    {
        // Create default console logger if none provided.
        logger ??= LoggerFactory.Create(builder =>
        {
            builder.AddConsole(options => options.FormatterName = "mcpd");
            builder.AddConsoleFormatter<McpdLogFormatter, ConsoleFormatterOptions>();
            builder.AddFilter("Microsoft.AspNetCore", LogLevel.Warning);
            builder.AddFilter("Microsoft.Extensions", LogLevel.Warning);
            builder.AddFilter("Microsoft.Hosting", LogLevel.Warning);
        }).CreateLogger(implementation.GetType());

        // Provide logger to BasePlugin implementations.
        if (implementation is BasePlugin basePlugin)
        {
            basePlugin.SetLogger(logger);
        }

        var addressOption = new Option<string>(
            name: "--address",
            description: "gRPC address (socket path for unix, host:port for tcp)")
        {
            IsRequired = true
        };

        var networkOption = new Option<string>(
            name: "--network",
            description: "Network type (unix or tcp)",
            getDefaultValue: () => "unix");

        var rootCommand = new RootCommand("Plugin server for mcpd")
        {
            addressOption,
            networkOption
        };

        rootCommand.SetHandler(async (address, network) => await RunServer(implementation, address, network, logger), addressOption, networkOption);

        return await rootCommand.InvokeAsync(args);
    }

    private static async Task RunServer(Plugin.PluginBase implementation, string address, string network, ILogger logger)
    {
        var builder = WebApplication.CreateSlimBuilder();

        // Suppress ASP.NET Core diagnostic logs.
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole(options => options.FormatterName = "mcpd");
        builder.Logging.AddConsoleFormatter<McpdLogFormatter, ConsoleFormatterOptions>();
        builder.Logging.AddFilter("Microsoft.AspNetCore", LogLevel.Warning);
        builder.Logging.AddFilter("Microsoft.Extensions", LogLevel.Warning);
        builder.Logging.AddFilter("Microsoft.Hosting", LogLevel.Warning);

        // Configure Kestrel.
        builder.WebHost.ConfigureKestrel((_, serverOptions) =>
        {
            if (network.Equals("unix", StringComparison.OrdinalIgnoreCase))
            {
                // Clean up existing socket file.
                if (File.Exists(address))
                {
                    File.Delete(address);
                }

                serverOptions.ListenUnixSocket(address, listenOptions =>
                {
                    listenOptions.Protocols = HttpProtocols.Http2;
                });
            }
            else if (network.Equals("tcp", StringComparison.OrdinalIgnoreCase))
            {
                var parts = address.Split(':');
                if (parts.Length != 2 || !int.TryParse(parts[1], out var port))
                {
                    throw new ArgumentException($"Invalid TCP address format: {address}. Expected host:port");
                }

                var host = parts[0];
                var ipAddress = host == "localhost" ? IPAddress.Loopback : IPAddress.Parse(host);

                serverOptions.Listen(ipAddress, port, listenOptions =>
                {
                    listenOptions.Protocols = HttpProtocols.Http2;
                });
            }
            else
            {
                throw new ArgumentException($"Unknown network type: {network}. Expected 'unix' or 'tcp'");
            }
        });

        // Add gRPC services.
        builder.Services.AddGrpc();
        builder.Services.AddSingleton(implementation);

        var app = builder.Build();

        // Map gRPC service.
        app.MapGrpcService<PluginServiceAdapter>();

        logger.LogInformation("Plugin server listening on {Network} {Address}", network, address);


        try
        {
            await app.RunAsync();
        }
        finally
        {
            // Clean up Unix socket file if it was created.
            if (network.Equals("unix", StringComparison.OrdinalIgnoreCase) && File.Exists(address))
            {
                File.Delete(address);
            }
        }
    }

    /// <summary>
    /// Adapter class to bridge between ASP.NET Core DI and the plugin instance.
    /// </summary>
    private class PluginServiceAdapter : Plugin.PluginBase
    {
        private readonly Plugin.PluginBase _implementation;

        public PluginServiceAdapter(Plugin.PluginBase implementation)
        {
            _implementation = implementation;
        }

        public override Task<Google.Protobuf.WellKnownTypes.Empty> Configure(PluginConfig request, Grpc.Core.ServerCallContext context)
            => _implementation.Configure(request, context);

        public override Task<Google.Protobuf.WellKnownTypes.Empty> Stop(Google.Protobuf.WellKnownTypes.Empty request, Grpc.Core.ServerCallContext context)
            => _implementation.Stop(request, context);

        public override Task<Metadata> GetMetadata(Google.Protobuf.WellKnownTypes.Empty request, Grpc.Core.ServerCallContext context)
            => _implementation.GetMetadata(request, context);

        public override Task<Capabilities> GetCapabilities(Google.Protobuf.WellKnownTypes.Empty request, Grpc.Core.ServerCallContext context)
            => _implementation.GetCapabilities(request, context);

        public override Task<Google.Protobuf.WellKnownTypes.Empty> CheckHealth(Google.Protobuf.WellKnownTypes.Empty request, Grpc.Core.ServerCallContext context)
            => _implementation.CheckHealth(request, context);

        public override Task<Google.Protobuf.WellKnownTypes.Empty> CheckReady(Google.Protobuf.WellKnownTypes.Empty request, Grpc.Core.ServerCallContext context)
            => _implementation.CheckReady(request, context);

        public override Task<HTTPResponse> HandleRequest(HTTPRequest request, Grpc.Core.ServerCallContext context)
            => _implementation.HandleRequest(request, context);

        public override Task<HTTPResponse> HandleResponse(HTTPResponse request, Grpc.Core.ServerCallContext context)
            => _implementation.HandleResponse(request, context);
    }
}
