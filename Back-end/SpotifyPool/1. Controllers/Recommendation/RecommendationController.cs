﻿using BusinessLogicLayer.Implement.Services.Recommendation;
using BusinessLogicLayer.Interface.Services_Interface.Recommendation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        /// Sử dụng thuật toán Euclidean Distance với trọng số để tìm ra bài hát gợi ý
        /// </summary>
        /// <param name="trackId"></param>
        /// <returns></returns>
        [Authorize(Roles = nameof(UserRole.Customer)), HttpGet("{trackId}/Weighted-Euclidean-Distance")]
        public async Task<IActionResult> GetRecommendedSongByWeightedEulideanDisctance(string trackId)
        {
            var result = await _recommendationService.GetRecommendations(trackId, RecommendationBLL.CalculateWeightedEulideanDisctance);
            return Ok(result);
        }

        /// <summary>
        /// Sử dụng thuật toán Cosine Similarity (Không có trọng số) để tìm ra bài hát gợi ý
        /// </summary>
        /// <param name="trackId"></param>
        /// <returns></returns>
        [Authorize(Roles = nameof(UserRole.Customer)), HttpGet("{trackId}/Cosine-Similarity")]
        public async Task<IActionResult> GetRecommendedSongByCosineSimilarity(string trackId)
        {
            var result = await _recommendationService.GetRecommendations(trackId, RecommendationBLL.CalculateCosineSimilarity);
            return Ok(result);
        }
    }
}
