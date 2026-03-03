using Microsoft.SemanticKernel;
using Microsoft.Extensions.Configuration;

var config = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();

var builder = Kernel.CreateBuilder();
builder.AddOpenAIChatCompletion(
    modelId: config["OpenAI:Model"]!,
    apiKey: config["OpenAI:ApiKey"]!);

var kernel = builder.Build();

// Paprastas LLM kvietimas
var result = await kernel.InvokePromptAsync(
    "Kas yra Semantic Kernel? Atsakyk 2 sakiniais.");
Console.WriteLine(result);
