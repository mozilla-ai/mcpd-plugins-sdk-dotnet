using MozillaAI.Mcpd.Plugins.V1;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        return await PluginServer.Serve<MyPlugin>(args);
    }
}
