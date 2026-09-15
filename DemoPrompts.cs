using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

[McpServerPromptType]
public static class DemoPrompts
{
    [McpServerPrompt(Name = "meeting_intro"), Description("Builds a short meeting-intro prompt for a named topic.")]
    public static GetPromptResult MeetingIntro(
        RequestContext<GetPromptRequestParams> request,
        [Description("The person who will open the meeting")] string? name = null,
        [Description("The meeting topic")] string? topic = null)
    {
        name = string.IsNullOrWhiteSpace(name) ? GetArg(request, "name") : name;
        topic = string.IsNullOrWhiteSpace(topic) ? GetArg(request, "topic") : topic;
        var leftover = FirstArg(request, "input", "text", "query", "args", "argument");

        // having trouble getting the arguments. not solved :/
        Resolve(name, topic, leftover, out var who, out var what);

        return new GetPromptResult
        {
            Description = "Builds a short meeting-intro prompt for a named topic.",
            Messages =
            [
                new PromptMessage
                {
                    Role = Role.User,
                    Content = new TextContentBlock
                    {
                        Text = $"Write a two-sentence intro for {who} to open a meeting about {what}."
                    }
                }
            ]
        };
    }

    private static void Resolve(string? name, string? topic, string? leftover, out string who, out string what)
    {
        if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(topic))
        {
            who = name.Trim();
            what = topic.Trim();
            return;
        }

        var blob = string.Join(
            ' ',
            new[] { name, topic, leftover }.Where(static value => !string.IsNullOrWhiteSpace(value)));

        var onIndex = blob.IndexOf(" on ", StringComparison.OrdinalIgnoreCase);
        if (onIndex > 0)
        {
            who = blob[..onIndex].Trim();
            what = blob[(onIndex + 4)..].Trim();
            return;
        }

        who = string.IsNullOrWhiteSpace(name) ? "the host" : name.Trim();
        what = string.IsNullOrWhiteSpace(topic)
            ? string.IsNullOrWhiteSpace(leftover) ? "the agenda" : leftover.Trim()
            : topic.Trim();
    }

    private static string? FirstArg(RequestContext<GetPromptRequestParams> request, params string[] keys)
    {
        foreach (var key in keys)
        {
            var value = GetArg(request, key);
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return null;
    }

    private static string? GetArg(RequestContext<GetPromptRequestParams> request, string key)
    {
        if (request.Params?.Arguments is not { } arguments)
        {
            return null;
        }

        if (!arguments.TryGetValue(key, out var value))
        {
            return null;
        }

        return value.ValueKind switch
        {
            JsonValueKind.String => value.GetString(),
            JsonValueKind.Null or JsonValueKind.Undefined => null,
            _ => value.ToString()
        };
    }
}
