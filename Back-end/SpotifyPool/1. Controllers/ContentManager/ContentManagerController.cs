using BusinessLogicLayer.Interface.Services_Interface.ContentManagers;
using BusinessLogicLayer.ModelView.Service_Model_Views.Tracks.Request;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SetupLayer.Enum.Services.User;

namespace SpotifyPool._1._Controllers.ContentManager
{
    [Route("api/v1/content-managers")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] // "Bearer"
    [Authorize(Roles = nameof(UserRole.ContentManeger))]
    public class ContentManagerController(IContentManager contentManagerService) : Controller
    {
        private readonly IContentManager _contentManager = contentManagerService;
        /// <summary>
        /// Cập nhật Restriction của Track (vd: khi duyệt track mới đc upload, hoặc cấm vì vi phạm...)
        /// </summary>
        /// <param name="id"> Id của track </param>
        /// <param name="model"></param>
        /// <returns></returns>
        [Authorize(Roles = nameof(UserRole.ContentManeger)), HttpPut("{id}")]
        public async Task<IActionResult> UpdateAlbumAsync(string id, [FromForm] TrackRestrictionRequestModel model)
        {
            await _contentManager.ChangeTrackRestrictionAsync(id, model);
            return Ok(new { Message = "Update Track Restriction Successfully" });
        }
    }
}
