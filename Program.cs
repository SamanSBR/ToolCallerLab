++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++using Microsoft.Extensions.DependencyInjection;
using OpenAI.Chat;
using ToolCallerLab.Services;
using ToolCallerLab.Tools;

var services = new ServiceCollection();
services.AddSingleton<ITool, ClockTool>();
services.AddSingleton<ITool, CalculatorTool>();
services.AddSingleton<OpenAiToolCaller>();

using var provider = services.BuildServiceProvider();

if (args.Contains("--self-test", StringComparer.OrdinalIgnoreCase))
{
    var calculator = provider.GetServices<ITool>().OfType<CalculatorTool>().Single();
    var clock = provider.GetServices<ITool>().OfType<ClockTool>().Single();
    Console.WriteLine($"ClockTool self-test: {clock.Execute(BinaryData.FromString("""{}""")).Value}");
    Console.WriteLine($"CalculatorTool self-test: {calculator.Execute(BinaryData.FromString("""{"expression":"(10 + 5) * 3"}""")).Value}");
    Console.WriteLine($"CalculatorTool self-test: {calculator.Execute(BinaryData.FromString("""{"expression":"18 * 27"}""")).Value}");
    return;
}

var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
if (string.IsNullOrWhiteSpace(apiKey))
{
    Console.Error.WriteLine("OPENAI_API_KEY is not set. Configure it before running the app.");
    return;
}

var model = Environment.GetEnvironmentVariable("OPENAI_MODEL") ?? "gpt-4o-mini";
var caller = provider.GetRequiredService<OpenAiToolCaller>();
var tools = provider.GetServices<ITool>().ToDictionary(tool => tool.Name, StringComparer.Ordinal);

Console.WriteLine("ToolCallerLab - type a message, or 'exit' to quit.");
Console.WriteLine($"Model: {model}");

while (true)
{
    Console.Write("\nUSER: ");
    var input = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(input))
        continue;
    if (input.Equals("exit", StringComparison.OrdinalIgnoreCase))
        break;

    await caller.RunAsync(input, apiKey, model, tools);
}
