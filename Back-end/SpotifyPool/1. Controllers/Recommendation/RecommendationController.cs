using BusinessLogicLayer.Interface.Services_Interface.Recommendation;
using Microsoft.AspNetCore.Mvc;

namespace SpotifyPool._1._Controllers.Recommendation
{
    [Route("api/v1/recommendations")]
    [ApiController]
    public class RecommendationController(IRecommendation recommendationService) : ControllerBase
    {
        private readonly IRecommendation _recommendationService = recommendationService;

        [HttpGet("{trackId}")]
        public async Task<IActionResult> GetRecommendedSong(string trackId)
        {
            var result = await _recommendationService.GetRecommendations(trackId);
            return Ok(result);
        }
    }
}
