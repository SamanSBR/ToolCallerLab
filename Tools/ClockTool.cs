using System.Text.Json;
using ToolCallerLab.Models;

namespace ToolCallerLab.Tools;

public sealed class ClockTool : ITool
{
    public string Name => "ClockTool";
    public string Description => "Returns the actual current time. Optionally accepts an IANA time zone, such as Europe/Berlin or UTC.";
    public BinaryData ParametersSchema => BinaryData.FromString("""
    {
      "type": "object",
      "properties": {
        "timeZone": { "type": "string", "description": "Optional IANA time zone, for example Europe/Berlin or UTC." }
      },
      "additionalProperties": false
    }
    """);

    public ToolResult Execute(BinaryData arguments)
    {
        var request = JsonSerializer.Deserialize<Arguments>(arguments.ToString(), JsonOptions) ?? new Arguments();
        TimeZoneInfo zone;
        try
        {
            zone = string.IsNullOrWhiteSpace(request.TimeZone)
                ? TimeZoneInfo.Local
                : TimeZoneInfo.FindSystemTimeZoneById(request.TimeZone);
        }
        catch (TimeZoneNotFoundException)
        {
            return ToolResult.Failure($"Unknown time zone '{request.TimeZone}'.");
        }

        var now = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, zone);
        return ToolResult.Success($"{now:yyyy-MM-dd HH:mm:ss zzz} ({zone.Id})");
    }

    private sealed record Arguments(string? TimeZone = null);
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
}
