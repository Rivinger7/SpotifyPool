using BusinessLogicLayer.Implement.Services.Recommendation;
using BusinessLogicLayer.Interface.Services_Interface.Recommendation;
using BusinessLogicLayer.ModelView.Service_Model_Views.AudioFeatures.Request;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SetupLayer.Enum.Services.Reccomendation;
using SetupLayer.Enum.Services.User;

namespace SpotifyPool._1._Controllers.Recommendation
{
    [Route("api/v1/recommendations")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] // "Bearer"
    public class RecommendationController(IRecommendation recommendationService) : ControllerBase
    {
        private readonly IRecommendation _recommendationService = recommendationService;

        /// <summary>
        /// Botpress sẽ gửi request lên đây để lấy bài hát được recommend
        /// </summary>
        /// <param name="audioFeaturesRequest"></param>
        /// <returns></returns>
        [AllowAnonymous, HttpPost("tracks")]
        public async Task<IActionResult> GetRecommendedSongByBotpressSearching([FromBody] AudioFeaturesRequest audioFeaturesRequest)
        {
            var result = await _recommendationService.GetRecommendations(audioFeaturesRequest, RecommendationBLL.CalculateWeightedEulideanDisctance);
            return Ok(result);
        }

        /// <summary>
        /// Đề xuất bài hát dựa trên trackId
        /// </summary>
        /// <param name="trackId"></param>
        /// <param name="algorithm"></param>
        /// <returns></returns>
        [Authorize(Roles = nameof(UserRole.Customer)), HttpGet("tracks/{trackId}")]
        public async Task<IActionResult> GetRecommendedSongByTrackId([FromRoute] string trackId, [FromQuery] Algorithm algorithm = Algorithm.CosineSimilarity)
        {
            return algorithm switch
            {
                Algorithm.CosineSimilarity => Ok(await _recommendationService.GetRecommendations(trackId, RecommendationBLL.CalculateCosineSimilarity)),
                Algorithm.WeightedEuclideanDistance => Ok(await _recommendationService.GetRecommendations(trackId, RecommendationBLL.CalculateWeightedEulideanDisctance)),
                _ => BadRequest("Invalid algorithm"),
            };
        }

        /// <summary>
        /// Đề xuất bài hát dựa trên danh sách trackIds
        /// </summary>
        /// <param name="trackIds"></param>
        /// <param name="algorithm"></param>
        /// <returns></returns>
        [Authorize(Roles = nameof(UserRole.Customer)), HttpGet("tracks")]
        public async Task<IActionResult> GetRecommendedSongsByCosineSimilarity([FromQuery] IEnumerable<string> trackIds, [FromQuery] Algorithm algorithm = Algorithm.CosineSimilarity)
        {
            return algorithm switch
            {
                Algorithm.CosineSimilarity => Ok(await _recommendationService.GetManyRecommendations(trackIds, RecommendationBLL.CalculateCosineSimilarity)),
                Algorithm.WeightedEuclideanDistance => Ok(await _recommendationService.GetManyRecommendations(trackIds, RecommendationBLL.CalculateWeightedEulideanDisctance)),
                _ => BadRequest("Invalid algorithm"),
            };
        }
    }
}
