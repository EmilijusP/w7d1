using AgentFrameworkDemo;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using OpenAI;
using OpenAI.Chat;
using System;
using System.ClientModel;

var config = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();

var openAiClient = new OpenAIClient(new ApiKeyCredential(config["OpenAI:ApiKey"]!));

var chatClient = openAiClient.GetChatClient(config["OpenAI:Model"]!);

var anagramTools = new AnagramTools();

var agent = chatClient.AsAIAgent(
    name: "AnagramSolverAgent",
    instructions: "Tu esi protingas AI asistentas. Tavo specializacija: spręsti anagramas ir teikti informaciją apie laiką. Naudok turimus įrankius.",
    tools:
    [
        AIFunctionFactory.Create(anagramTools.FindAnagrams),
        AIFunctionFactory.Create(anagramTools.GetWordCount),
        AIFunctionFactory.Create(anagramTools.FilterByLength)
    ]
);

Console.WriteLine("Užduokite klausimą arba rašykite 'exit' norėdami baigti.");

while (true)
{
    Console.Write("Vartotojas: ");
    string? input = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(input) || input.ToLower() == "exit")
    {
        break;
    }

    var response = await agent.RunAsync(input);

    Console.WriteLine($"\nAsistentas: {response}\n");
}