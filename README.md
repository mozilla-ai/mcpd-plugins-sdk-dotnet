# `mcpd` plugins SDK for .NET

.NET SDK for developing `mcpd` middleware plugins.

## Overview

This SDK provides .NET types and gRPC interfaces for building middleware plugins that integrate with `mcpd`.
Plugins can process HTTP requests and responses, implementing capabilities like authentication, rate limiting,
content transformation, and observability.

## Installation

Add the package reference to your project:

```bash
dotnet add package MozillaAI.Mcpd.Plugins.Sdk
```

Or add it directly to your `.csproj` file:

```xml
<ItemGroup>
  <PackageReference Include="MozillaAI.Mcpd.Plugins.Sdk" Version="0.0.1" />
</ItemGroup>
```

## Usage

The SDK provides the `PluginServer` helper and `BasePlugin` class to minimize boilerplate.

### Example Plugin

```csharp
using Google.Protobuf.WellKnownTypes;
using MozillaAI.Mcpd.Plugins.V1;

public class MyPlugin : BasePlugin
{
    public override Task<Metadata> GetMetadata(Empty request, Grpc.Core.ServerCallContext context)
    {
        return Task.FromResult(new Metadata
        {
            Name = "my-plugin",
            Version = "1.0.0",
            Description = "Example plugin that does something useful"
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
        var response = new HTTPResponse
        {
            Continue = true,
            StatusCode = 0,
            Body = request.Body
        };

        foreach (var header in request.Headers)
        {
            response.Headers.Add(header.Key, header.Value);
        }

        return Task.FromResult(response);
    }
}

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        return await PluginServer.Serve<MyPlugin>(args);
    }
}
```

The `BasePlugin` class provides default implementations for all plugin methods. Override only the methods you need:

| Method | Default Behavior |
|--------|------------------|
| `CheckHealth()` | Returns OK |
| `CheckReady()` | Returns OK |
| `Configure()` | No-op |
| `Stop()` | No-op |
| `HandleRequest()` | Pass through unchanged |
| `HandleResponse()` | Pass through unchanged |
| `GetMetadata()` | Returns empty (should be overridden) |
| `GetCapabilities()` | Returns empty (should be overridden) |

### Modifying Requests

Plugins can modify incoming HTTP requests by setting the `ModifiedRequest` field in the returned `HTTPResponse`. This allows plugins to transform requests before they reach downstream handlers:

```csharp
public override Task<HTTPResponse> HandleRequest(HTTPRequest request, Grpc.Core.ServerCallContext context)
{
    // Create a modified version of the request.
    var modifiedRequest = new HTTPRequest
    {
        Method = request.Method,
        Url = request.Url,
        Path = request.Path,
        Body = request.Body,
        RemoteAddr = request.RemoteAddr,
        RequestUri = request.RequestUri
    };

    // Copy and modify headers.
    foreach (var header in request.Headers)
    {
        modifiedRequest.Headers.Add(header.Key, header.Value);
    }
    modifiedRequest.Headers["X-Custom-Header"] = "added-by-plugin";

    // Return response with modified request.
    return Task.FromResult(new HTTPResponse
    {
        Continue = true,
        StatusCode = 0,
        ModifiedRequest = modifiedRequest
    });
}
```

## Running Your Plugin

Build and run your plugin:

```bash
dotnet build
dotnet run -- --address /tmp/my-plugin.sock --network unix
```

Or for TCP:

```bash
dotnet run -- --address localhost:50051 --network tcp
```

## Proto Versioning

The SDK automatically downloads proto definitions from [mcpd-proto](https://github.com/mozilla-ai/mcpd-proto) at build time.

Current proto version: **v0.0.3**

| Version Type | Description |
|--------------|-------------|
| API Version | `plugins/v1/` (in proto repo) maps to `MozillaAI.Mcpd.Plugins.V1` namespace (in SDK) |
| Release Version | Proto repo tags like `v0.0.1`, `v0.0.2`, etc. |
| SDK Version | This repo's tags track SDK releases and may differ from proto versions |

To update the proto version, modify the `<ProtoVersion>` property in the SDK `.csproj` file.

## Repository Structure

```
mcpd-plugins-sdk-dotnet/
├── README.md                       # This file.
├── LICENSE                         # Apache 2.0 license.
├── mcpd-plugins-sdk-dotnet.sln     # Solution file.
├── .gitignore                      # Ignores proto/ and bin/obj directories.
├── src/
│   └── MozillaAI.Mcpd.Plugins.Sdk/
│       ├── MozillaAI.Mcpd.Plugins.Sdk.csproj  # SDK project file.
│       ├── BasePlugin.cs                       # BasePlugin helper class.
│       ├── PluginServer.cs                     # PluginServer helper class.
│       ├── FlowConstants.cs                    # Flow enum constants.
│       └── proto/                              # Downloaded protos (auto-generated, gitignored).
└── examples/
    └── MyPlugin/
        ├── MyPlugin.csproj         # Example plugin project.
        └── Program.cs              # Example plugin implementation.
```

## For SDK Maintainers

### Prerequisites

- .NET 9.0 SDK or later
- Protocol Buffer Compiler (protoc) - automatically installed via `Grpc.Tools` NuGet package

### Building the SDK

```bash
dotnet build
```

The proto files are automatically downloaded during the build process via the `FetchProto` target defined in the `.csproj` file.

### Running the Example

```bash
cd examples/MyPlugin
dotnet run -- --address /tmp/my-plugin.sock --network unix
```

### Updating Proto Version

1. Edit the `<ProtoVersion>` property in `src/MozillaAI.Mcpd.Plugins.Sdk/MozillaAI.Mcpd.Plugins.Sdk.csproj`
2. Run `dotnet clean` and `dotnet build`
3. Commit the updated project file

## Health Checking

The SDK follows [gRPC Health Checking Protocol](https://grpc.github.io/grpc/core/md_doc_health-checking.html) conventions:

```protobuf
rpc CheckHealth(google.protobuf.Empty) returns (google.protobuf.Empty);
rpc CheckReady(google.protobuf.Empty) returns (google.protobuf.Empty);
```

## License

Apache 2.0 - See LICENSE file for details.

## Contributing

This is an early PoC. Contribution guidelines coming soon.
