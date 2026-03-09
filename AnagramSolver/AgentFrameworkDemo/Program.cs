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

// === SEQUENTIAL === \\
//var finder = chatClient.AsAIAgent(
//    name: "Finder",
//    instructions: "Tu esi paieškos robotas. Tavo darbas - surasti anagramas vartotojo nurodytam žodžiui. " +
//                  "Naudok FindAnagrams įrankį. Grąžink TIK rastų žodžių sąrašą, atskirtą kableliais. " +
//                  "Jei nieko nerandi, grąžink žodį 'NERASTA'.",
//    tools: [AIFunctionFactory.Create(anagramTools.FindAnagrams)]
//);

//var analyzer = chatClient.AsAIAgent(
//    name: "Analyzer",
//    instructions: "Tu esi analizatorius. Tu gauni žodžių sąrašą. " +
//                  "Tavo užduotis: surikiuoti žodžius pagal įdomumą, " +
//                  "keisčiausi žodžiai turi būti apačioje."
//);

//var presenter = chatClient.AsAIAgent(
//    name: "Presenter",
//    instructions: "Tu esi pranešėjas. Tu gauni sausą analizės tekstą. " +
//                  "Tavo darbas - pateikti jį vartotojui labai patraukliai, naudojant Markdown formatavimą."
//);

//var agents = new List<AIAgent> { finder, analyzer, presenter };

//var workflow = AgentWorkflowBuilder.BuildSequential(agents);

//var workflowAgent = workflow.AsAIAgent(name: "AnagramPipeline");

// === HANDOFF === \\
var triageAgent = chatClient.AsAIAgent(
    name: "Triage",
    instructions: "Tu esi registratūros agentas (Triage). Tavo vienintelis tikslas - nukreipti vartotoją pas tinkamą specialistą. " +
                  "Vadovaukis šiomis taisyklėmis: " +
                  "1. Jei vartotojas prašo surasti ANAGRAMĄ, VISADA nukreipk jį pas 'Anagrams' agentą. " +
                  "2. Jei vartotojas klausia, KIEK IŠ VISO YRA ŽODŽIŲ, arba prašo SURASTI ŽODŽIUS IŠ TAM TIKRO RAIDŽIŲ SKAIČIAUS (pvz., 6 raidžių), VISADA nukreipk jį pas 'Analysis' agentą. " +
                  "Niekada nebandyk pats atsakyti į klausimus, neieškok žodžių ir neklausk patikslinančių klausimų. Tiesiog iškviesk nukreipimo įrankį."
);

var anagramsAgent = chatClient.AsAIAgent(
    name: "Anagrams",
    instructions: "Tu esi Anagramų specialistas. Naudok FindAnagrams įrankį, kad surastum vartotojui anagramas.",
    tools: [
        AIFunctionFactory.Create(anagramTools.FindAnagrams),
    ]
);

var analysisAgent = chatClient.AsAIAgent(
    name: "Analysis",
    instructions: "Tu esi Žodžių analizės specialistas. Naudok GetWordCount ir FilterByLength įrankius atsakinėti į klausimus. ",
    tools: [
        AIFunctionFactory.Create(anagramTools.GetWordCount),
        AIFunctionFactory.Create(anagramTools.FilterByLength),
    ]
);

var workflow = AgentWorkflowBuilder.CreateHandoffBuilderWith(triageAgent)
    .WithHandoffs(triageAgent, [analysisAgent, anagramsAgent])
    .WithHandoff(analysisAgent, triageAgent)
    .WithHandoff(anagramsAgent, triageAgent)
    .Build();

var workflowAgent = workflow.AsAIAgent(name: "HandoffPipeline");


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

}