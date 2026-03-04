using AnagramSolver.Contracts.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.AI;
using AnagramSolver.Contracts.Models;

namespace AnagramSolver.Api.Controllers
{
    [Route("api/ai/chat")]
    [ApiController]
    public class AiChatController : ControllerBase
    {
        private readonly IAiChatService _aiChatService;

        public AiChatController(IAiChatService aiChatService)
        {
            _aiChatService = aiChatService;
        }

        [HttpPost]
        public async Task<ActionResult<Contracts.Models.ChatResponse>> PostMessage([FromBody] Contracts.Models.ChatRequest request, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest("Message cannot be empty.");
            }

            if (string.IsNullOrWhiteSpace(request.SessionId))
            {
                return BadRequest("SessionId cannot be empty.");
            }

            var aiResponse = await _aiChatService.GetResponseAsync(request.SessionId, request.Message, ct);

            var response = new Contracts.Models.ChatResponse
            {
                Response = aiResponse,
                SessionId = request.SessionId
            };

            return Ok(response);
         
        }
    }
}
