using System.ComponentModel;
using ModelContextProtocol.Server;

[McpServerResourceType]
public static class DemoResources
{
    [McpServerResource(UriTemplate = "demo://server/info", Name = "server_info", MimeType = "application/json")]
    [Description("Static demo resource describing this MCP server.")]
    public static string GetServerInfo() =>
        """{"name":"mcp-demo","version":"1.0.0","protocol":"2026-07-28","sdk":"ModelContextProtocol.AspNetCore 2.2.0","primitives":["tools/call","resources/read","prompts/get"]}""";

    [McpServerResource(UriTemplate = "demo://welcome", Name = "welcome", MimeType = "text/plain")]
    [Description("Short welcome text for anyone attaching this MCP resource.")]
    public static string GetWelcome() =>
        "Welcome to mcp-demo. Try the echo tool, the meeting_intro prompt, or the server_info resource.";
}
