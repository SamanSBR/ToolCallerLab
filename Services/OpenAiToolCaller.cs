using OpenAI.Chat;
using ToolCallerLab.Models;
using ToolCallerLab.Tools;

namespace ToolCallerLab.Services;

public sealed class OpenAiToolCaller
{
    public async Task RunAsync(string userMessage, string apiKey, string model, IReadOnlyDictionary<string, ITool> tools)
    {
        var client = new ChatClient(model, apiKey);
        var options = new ChatCompletionOptions();
        foreach (var tool in tools.Values)
            options.Tools.Add(ChatTool.CreateFunctionTool(tool.Name, tool.Description, tool.ParametersSchema));

        var messages = new List<ChatMessage> { new UserChatMessage(userMessage) };
        while (true)
        {
            var completion = await client.CompleteChatAsync(messages, options);
            if (completion.Value.FinishReason != ChatFinishReason.ToolCalls)
            {
                messages.Add(new AssistantChatMessage(completion.Value));
                Console.WriteLine($"\nFINAL MODEL RESPONSE:\n{completion.Value.Content[0].Text}");
                return;
            }

            messages.Add(new AssistantChatMessage(completion.Value));
            foreach (var call in completion.Value.ToolCalls)
            {
                Console.WriteLine($"\nMODEL REQUESTED TOOL:\n{call.FunctionName}");
                Console.WriteLine($"\nARGUMENTS:\n{call.FunctionArguments.ToString()}");

                if (!tools.TryGetValue(call.FunctionName, out var tool))
                    throw new InvalidOperationException($"Model requested unsupported tool '{call.FunctionName}'.");

                var result = tool.Execute(call.FunctionArguments);
                Console.WriteLine($"\nTOOL RESULT:\n{result.Value}");
                messages.Add(new ToolChatMessage(call.Id, result.Value));
            }
        }
    }
}
