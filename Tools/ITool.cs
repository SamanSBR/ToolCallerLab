using ToolCallerLab.Models;

namespace ToolCallerLab.Tools;

public interface ITool
{
    string Name { get; }
    string Description { get; }
    BinaryData ParametersSchema { get; }
    ToolResult Execute(BinaryData arguments);
}
