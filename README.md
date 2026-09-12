# ToolCallerLab

ToolCallerLab is a deliberately small C#/.NET 10 console application for learning structured LLM tool calling. The OpenAI model receives a normal message, chooses from two registered tools, and gets the local C# result before writing the final answer.

## Architecture

- `Program.cs` wires dependency injection, reads input, and starts the loop.
- `Tools/ITool.cs` defines the name, description, JSON schema, and execution contract.
- `Tools/ClockTool.cs` returns the current system time (or a requested time zone).
- `Tools/CalculatorTool.cs` evaluates arithmetic locally with a small recursive-descent parser.
- `Services/OpenAiToolCaller.cs` owns the model → tool call → tool result → model loop.
- `Models/` contains the small local result types.

## How tool calling works

The application registers both tools as OpenAI function tools with JSON schemas. It sends the user message to the model. If the response has tool calls, the app parses each structured argument payload, executes the matching C# tool, appends the tool result to the conversation, and asks the model again. It repeats until the model returns a normal final response. There is no keyword routing and the model does not calculate or invent the current time itself.

## Configure, build, and run

Set the API key in the environment; it is never stored in source code:

```bash
export OPENAI_API_KEY="your-api-key"
dotnet build
dotnet run
```

The default model is `gpt-4o-mini`. To override it, set `OPENAI_MODEL`. Type `exit` to quit.

To check the local calculator without making an API request:

```bash
dotnet run -- --self-test
```

## Example

```text
USER: Calculate 18 × 27
MODEL REQUESTED TOOL:
CalculatorTool
ARGUMENTS:
{"expression":"18 * 27"}
TOOL RESULT:
486
FINAL MODEL RESPONSE:
18 × 27 = 486.
```

The clock tool can be invoked by messages such as `What time is it in Europe/Berlin?`, while ordinary questions can be answered without a tool.
