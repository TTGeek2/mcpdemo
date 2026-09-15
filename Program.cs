using ModelContextProtocol.AspNetCore;
using ModelContextProtocol.Protocol;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://127.0.0.1:3001");

builder.Services
    .AddMcpServer(options =>
    {
        options.ServerInfo = new Implementation
        {
            Name = "mcp-demo",
            Description = "Use if user wants a demonstration of how a mcp server works",
           
            Version = "1.0.0",
        };
    })
    .WithHttpTransport(options =>
    {
        // in latest mcp version communication is stateless.
        options.SessionMode = HttpServerSessionMode.Stateless;
    })
    // scan the code and find classes marked with tools attribute, resource attribute and prompts attribute.
    .WithToolsFromAssembly()
    .WithResourcesFromAssembly()
    .WithPromptsFromAssembly();

var app = builder.Build();

// sets up endpoint routing and streamable http transport
app.MapMcp("/mcp");

app.Run();
