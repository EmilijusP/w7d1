using Microsoft.SemanticKernel;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;
using Microsoft.SemanticKernel.ChatCompletion;
using AgentDemo;
using Microsoft.SemanticKernel.Connectors.OpenAI;

var config = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();

var builder = Kernel.CreateBuilder();
builder.AddOpenAIChatCompletion(
    modelId: config["OpenAI:Model"]!,
    apiKey: config["OpenAI:ApiKey"]!);

builder.Plugins.AddFromType<TimePlugin>();
builder.Plugins.AddFromType<FindAnagramsPlugin>();

var kernel = builder.Build();

var chatService = kernel.GetRequiredService<IChatCompletionService>();
var history = new ChatHistory();
history.AddSystemMessage("Tu esi draugiškas .NET asistentas.");

Console.WriteLine("Užduokite klausimą arba rašykite 'exit' norėdami baigti.");

while (true)
{
    Console.Write("Vartotojas: ");
    string? input = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(input) || input.ToLower() == "exit")
    {
        break;
    }

    history.AddUserMessage(input);

    var settings = new OpenAIPromptExecutionSettings { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() };
    var result = await chatService.GetChatMessageContentAsync(history, kernel: kernel, executionSettings: settings);

    Console.WriteLine($"\nAsistentas: {result}\n");
    history.AddMessage(result.Role, result.Content ?? string.Empty);
}