using BusinessLogicLayer.Implement.Services.Recommendation;
using BusinessLogicLayer.Interface.Services_Interface.Recommendation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SpotifyPool._1._Controllers.Recommendation
{
    [Route("api/v1/recommendations")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] // "Bearer"
    public class RecommendationController(IRecommendation recommendationService) : ControllerBase
    {
        private readonly IRecommendation _recommendationService = recommendationService;

        [AllowAnonymous, HttpGet("{trackId}/EuclideanDistance")]
        public async Task<IActionResult> GetRecommendedSongByEuclideanDistance(string trackId)
        {
            var result = await _recommendationService.GetRecommendations(trackId, RecommendationBLL.CalculateEuclideanDistance);
            return Ok(result);
        }

        [AllowAnonymous, HttpGet("{trackId}/WeightedEuclideanDistance")]
        public async Task<IActionResult> GetRecommendedSongByWeightedEulideanDisctance(string trackId)
        {
            var result = await _recommendationService.GetRecommendations(trackId, RecommendationBLL.CalculateWeightedEulideanDisctance);
            return Ok(result);
        }

        [HttpGet("{trackId}/CosineSimilarity")]
        public async Task<IActionResult> GetRecommendedSongByCosineSimilarity(string trackId)
        {
            var result = await _recommendationService.GetRecommendations(trackId, RecommendationBLL.CalculateCosineSimilarity);
            return Ok(result);
        }
    }
}
