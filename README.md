# Bare-bones MCP demo server

A standalone ASP.NET Core MCP server on **.NET 10**. It uses the official C# SDK **2.2.0** and speaks the latest Model Context Protocol revision, **`2026-07-28`**.

`2.2.0` is the SDK version (C# and Python both ship 2.2.0). The protocol itself is dated, not semver'd: the current spec is [`2026-07-28`](https://modelcontextprotocol.io/specification/2026-07-28).

The server listens on Streamable HTTP at `http://127.0.0.1:3001/mcp` and exposes one of each MCP primitive:

| Primitive | Name / URI | What it does |
| --- | --- | --- |
| Tool | `echo` | Returns `echo: {message}` |
| Resource | `demo://server/info` | Static JSON describing this server |
| Prompt | `meeting_intro` | Builds a two-sentence meeting-intro template from `name` and `topic` |

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)

## Run the server

```bash
dotnet run
```

The process binds only to loopback: `http://127.0.0.1:3001`. Leave it running for VS Code and for the curl examples below.

## Set up this MCP server in VS Code

VS Code talks to MCP servers from a `mcp.json` file. This repo already includes [`.vscode/mcp.json`](.vscode/mcp.json).

1. Install a current [VS Code](https://code.visualstudio.com/) (1.102 or later) with **GitHub Copilot**.
2. Start this server in a terminal: `dotnet run`.
3. Open this folder in VS Code.
4. Open the Command Palette (`Ctrl+Shift+P` / `Cmd+Shift+P`) and run **MCP: List Servers**. You should see `mcp-demo`.
5. Start the server from that list if it is not already connected, and trust it when VS Code asks.
6. Open **Chat** (`Ctrl+Alt+I` / `Ctrl+Cmd+I`), switch to **Agent** mode, and try the three primitives:
   - Tool: “Call the echo tool with message hello from VS Code.”
   - Resource: in the chat input, **Add Context → MCP Resources**, then pick `demo://server/info` (or run **MCP: Browse Resources**).
   - Prompt: type `/.` in the chat input and choose `meeting_intro`, then fill in `name` and `topic`.

### What `.vscode/mcp.json` contains

```json
{
  "servers": {
    "mcp-demo": {
      "type": "http",
      "url": "http://127.0.0.1:3001/mcp"
    }
  }
}
```

`type: "http"` is Streamable HTTP. The URL must be the MCP endpoint (`/mcp`), not the site root.

### Add the same server by hand

If you prefer the UI:

1. Command Palette → **MCP: Add Server**.
2. Choose **HTTP (Server-Sent Events or Streaming HTTP)**.
3. URL: `http://127.0.0.1:3001/mcp`.
4. Name: `mcp-demo`.
5. Save it to the **workspace** so it lands in `.vscode/mcp.json`.

Workspace servers live in `.vscode/mcp.json`. User-wide servers live in the file opened by **MCP: Open User Configuration**.

The HTTP server must already be running. VS Code does not start `dotnet run` for you with this config.

### Cursor

Type `/` in Agent chat and pick `/mcp-demo/meeting_intro`. Cursor often skips the argument form and calls `prompts/get` with missing `name`/`topic`. The prompt still renders: leftover text like `martin on whats for dinner` is treated as speaker plus topic.

## Example curl calls

Every `2026-07-28` request is a standalone POST. There is no `initialize` handshake and no `Mcp-Session-Id`.

Required on every request:

- `Accept: application/json, text/event-stream`
- `MCP-Protocol-Version: 2026-07-28` — must match `params._meta["io.modelcontextprotocol/protocolVersion"]`
- `Mcp-Method` — must match the JSON-RPC `method`
- `params._meta.io.modelcontextprotocol/clientCapabilities`

Also required for `tools/call`, `resources/read`, and `prompts/get`:

- `Mcp-Name` — the tool/prompt `name`, or the resource `uri`

The SDK answers with an SSE stream (`Content-Type: text/event-stream`). The JSON-RPC result is the `data:` payload of the `message` event.

On Windows PowerShell, call `curl.exe` (plain `curl` is `Invoke-WebRequest`). From Git Bash, WSL, macOS, or Linux, `curl` is fine. The commands below use `\` line continuations (bash). In PowerShell, either paste each command as one line or replace `\` with a backtick.

### Discover the server

```bash
curl.exe -s -N -X POST http://127.0.0.1:3001/mcp -H "Content-Type: application/json" -H "Accept: application/json, text/event-stream" -H "MCP-Protocol-Version: 2026-07-28" -H "Mcp-Method: server/discover"  --data-binary @examples/discover.json
```

Equivalent body:

```json
{
  "jsonrpc": "2.0",
  "id": 1,
  "method": "server/discover",
  "params": {
    "_meta": {
      "io.modelcontextprotocol/protocolVersion": "2026-07-28",
      "io.modelcontextprotocol/clientInfo": { "name": "curl", "version": "1.0.0" },
      "io.modelcontextprotocol/clientCapabilities": {}
    }
  }
}
```

### Tool: list and call `echo`

```bash
curl.exe -s -N -X POST http://127.0.0.1:3001/mcp -H "Content-Type: application/json" -H "Accept: application/json, text/event-stream" -H "MCP-Protocol-Version: 2026-07-28" -H "Mcp-Method: tools/list" --data-binary @examples/tools-list.json
```

```bash
curl.exe -s -N -X POST http://127.0.0.1:3001/mcp -H "Content-Type: application/json" -H "Accept: application/json, text/event-stream" -H "MCP-Protocol-Version: 2026-07-28" -H "Mcp-Method: tools/call" -H "Mcp-Name: echo" --data-binary @examples/tools-call.json
```

`tools/call` body:

```json
{
  "jsonrpc": "2.0",
  "id": 3,
  "method": "tools/call",
  "params": {
    "name": "echo",
    "arguments": { "message": "hello demo" },
    "_meta": {
      "io.modelcontextprotocol/protocolVersion": "2026-07-28",
      "io.modelcontextprotocol/clientInfo": { "name": "curl", "version": "1.0.0" },
      "io.modelcontextprotocol/clientCapabilities": {}
    }
  }
}
```

Expected tool result text: `echo: hello demo`.

### Resource: list and read `demo://server/info`

```bash
curl.exe -s -N -X POST http://127.0.0.1:3001/mcp -H "Content-Type: application/json" -H "Accept: application/json, text/event-stream" -H "MCP-Protocol-Version: 2026-07-28" -H "Mcp-Method: resources/list" --data-binary @examples/resources-list.json
```

```bash
curl.exe -s -N -X POST http://127.0.0.1:3001/mcp -H "Content-Type: application/json" -H "Accept: application/json, text/event-stream" -H "MCP-Protocol-Version: 2026-07-28" -H "Mcp-Method: resources/read" -H "Mcp-Name: demo://server/info" --data-binary @examples/resources-read.json
```

`resources/read` body:

```json
{
  "jsonrpc": "2.0",
  "id": 5,
  "method": "resources/read",
  "params": {
    "uri": "demo://server/info",
    "_meta": {
      "io.modelcontextprotocol/protocolVersion": "2026-07-28",
      "io.modelcontextprotocol/clientInfo": { "name": "curl", "version": "1.0.0" },
      "io.modelcontextprotocol/clientCapabilities": {}
    }
  }
}
```

### Prompt: list and get `meeting_intro`

```bash
curl.exe -s -N -X POST http://127.0.0.1:3001/mcp -H "Content-Type: application/json" -H "Accept: application/json, text/event-stream" -H "MCP-Protocol-Version: 2026-07-28" -H "Mcp-Method: prompts/list" --data-binary @examples/prompts-list.json
```

```bash
curl.exe -s -N -X POST http://127.0.0.1:3001/mcp -H "Content-Type: application/json" -H "Accept: application/json, text/event-stream" -H "MCP-Protocol-Version: 2026-07-28" -H "Mcp-Method: prompts/get" -H "Mcp-Name: meeting_intro" --data-binary @examples/prompts-get.json
```

`prompts/get` body:

```json
{
  "jsonrpc": "2.0",
  "id": 7,
  "method": "prompts/get",
  "params": {
    "name": "meeting_intro",
    "arguments": {
      "name": "Alex",
      "topic": "MCP 2026-07-28"
    },
    "_meta": {
      "io.modelcontextprotocol/protocolVersion": "2026-07-28",
      "io.modelcontextprotocol/clientInfo": { "name": "curl", "version": "1.0.0" },
      "io.modelcontextprotocol/clientCapabilities": {}
    }
  }
}
```

Expected prompt text: `Write a two-sentence intro for Alex to open a meeting about MCP 2026-07-28.`

## Project layout

| File | Role |
| --- | --- |
| `Program.cs` | Kestrel host, stateless Streamable HTTP, `/mcp` |
| `DemoTools.cs` | `echo` tool |
| `DemoResources.cs` | `demo://server/info` resource |
| `DemoPrompts.cs` | `meeting_intro` prompt template |
| `.vscode/mcp.json` | VS Code MCP connection |
| `examples/*.json` | Ready-to-post JSON-RPC bodies |

## Further reading

- [MCP specification `2026-07-28`](https://modelcontextprotocol.io/specification/2026-07-28)
- [MCP C# SDK](https://csharp.sdk.modelcontextprotocol.io/v2/concepts/getting-started.html)
- [Add and manage MCP servers in VS Code](https://code.visualstudio.com/docs/copilot/customization/mcp-servers)
