namespace ToolCallerLab.Models;

public sealed record ToolResult(string Value, bool IsError)
{
    public static ToolResult Success(string value) => new(value, false);
    public static ToolResult Failure(string value) => new(value, true);
}
