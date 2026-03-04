using AnagramSolver.Contracts.Interfaces;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnagramSolver.BusinessLogic.Services
{
    public class AiChatService : IAiChatService
    {
        private readonly Kernel _kernel;
        private readonly IChatCompletionService _chatCompletionService;
        private static readonly ConcurrentDictionary<string, ChatHistory> _sessionHistories = new();

        public AiChatService(Kernel kernel, IChatCompletionService chatCompletionService)
        {
            _kernel = kernel;
            _chatCompletionService = chatCompletionService;
        }

        public async Task<string> GetResponseAsync(string sessionId, string prompt)
        {
            var chatHistory = _sessionHistories.GetOrAdd(sessionId, _ =>
            {
                var history = new ChatHistory();
                history.AddSystemMessage(
                    "You are a smart AI assistant, that helps finding anagrams and answering questions."
                );
                return history;
            });

            chatHistory.AddUserMessage(prompt);

            var executionSettings = new OpenAIPromptExecutionSettings
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
            };

            var result = await _chatCompletionService.GetChatMessageContentAsync(
                chatHistory, 
                kernel: _kernel, 
                executionSettings: executionSettings);

            var responseMessage = result.Content ?? "Error. No answer from AI.";

            chatHistory.AddAssistantMessage(responseMessage);

            return responseMessage;
        }
    }
}
