# MyPlugin Example

Example `mcpd` plugin that demonstrates basic plugin functionality using the .NET SDK.

## What This Example Does

This plugin intercepts HTTP requests and adds a custom header `X-My-Plugin: processed` to demonstrate request processing capabilities.

## Running the Example

```bash
dotnet run -- --address /tmp/my-plugin.sock --network unix
```

Or using TCP:

```bash
dotnet run -- --address localhost:50051 --network tcp
```

## Key Concepts Demonstrated

- Extending `BasePlugin` to inherit default implementations
- Implementing `GetMetadata()` to provide plugin information
- Implementing `GetCapabilities()` to declare request flow support
- Implementing `HandleRequest()` to process HTTP requests
- Adding custom headers while preserving existing ones

## Plugin Metadata

| Property | Value |
|----------|-------|
| Name | `my-plugin` |
| Version | `1.0.0` |
| Description | Example plugin that demonstrates basic functionality |
| Capabilities | Request flow (`FlowRequest`) |

## Code Structure

The example is organized into two files:

- `MyPlugin.cs`: Contains the `MyPlugin` class that extends `BasePlugin` and implements the plugin logic
- `Program.cs`: Entry point that starts the plugin server using `PluginServer.Serve<MyPlugin>()`
