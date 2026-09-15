using System.ComponentModel;
using ModelContextProtocol.Server;

[McpServerToolType]
public static class DemoTools
{
    [McpServerTool(Name = "echo"), Description("Echoes the provided message back to the caller.")]
    public static string Echo([Description("The message to echo")] string message)
        => $"echo: {message}";
}
