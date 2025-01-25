using BusinessLogicLayer.Interface.Microservices_Interface.OpenAI;
using Microsoft.AspNetCore.Mvc;

namespace SpotifyPool._1._Controllers.OpenAI
{
    [Route("api/[controller]")]
    [ApiController]
    public class OpenAIController(IOpenAIService openAIService) : ControllerBase
    {
        private readonly IOpenAIService _openAIService = openAIService;

        [HttpPost("chat")]
        public async Task<IActionResult> ChatWithAIAsync()
        {
            await _openAIService.TestOpenApi();
            return Ok();
        }
    }
}
