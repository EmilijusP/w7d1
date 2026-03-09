using AgentFrameworkDemo;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
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

var finder = chatClient.AsAIAgent(
    name: "Finder",
    instructions: "Tu esi paieškos robotas. Tavo darbas - surasti anagramas vartotojo nurodytam žodžiui. " +
                  "Naudok FindAnagrams įrankį. Grąžink TIK rastų žodžių sąrašą, atskirtą kableliais. " +
                  "Jei nieko nerandi, grąžink žodį 'NERASTA'.",
    tools: [AIFunctionFactory.Create(anagramTools.FindAnagrams)]
);

var analyzer = chatClient.AsAIAgent(
    name: "Analyzer",
    instructions: "Tu esi analizatorius. Tu gauni žodžių sąrašą. " +
                  "Tavo užduotis: surikiuoti žodžius pagal įdomumą, " +
                  "keisčiausi žodžiai turi būti apačioje."
);

var presenter = chatClient.AsAIAgent(
    name: "Presenter",
    instructions: "Tu esi pranešėjas. Tu gauni sausą analizės tekstą. " +
                  "Tavo darbas - pateikti jį vartotojui labai patraukliai, naudojant Markdown formatavimą."
);

var agents = new List<AIAgent> { finder, analyzer, presenter };

var workflow = AgentWorkflowBuilder.BuildSequential(agents);

var workflowAgent = workflow.AsAIAgent(name: "AnagramPipeline");

Console.WriteLine("Įveskite žodį anagramų paieškai arba rašykite 'exit' norėdami baigti.");

while (true)
{
    Console.Write("Vartotojas: ");
    string? input = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(input) || input.ToLower() == "exit")
    {
        break;
    }

    await foreach (var chunk in workflowAgent.RunStreamingAsync(input))
    {
        Console.Write(chunk.Text);
    }
    Console.WriteLine("\n");

    //var response = await workflowAgent.RunAsync(input);
    //Console.WriteLine($"\nAsistentas: {response}\n");
}