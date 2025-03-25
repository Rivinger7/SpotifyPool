using Business_Logic_Layer.Services_Interface.Users;
using BusinessLogicLayer.Implement.Services.Users;
using BusinessLogicLayer.Interface.Services_Interface.ContentManagers;
using BusinessLogicLayer.Interface.Services_Interface.Playlists.Custom;
using BusinessLogicLayer.ModelView.Service_Model_Views.Tracks.Request;
using BusinessLogicLayer.ModelView.Service_Model_Views.Users.Request;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SetupLayer.Enum.Services.User;

namespace SpotifyPool._1._Controllers.ContentManager
{
    [Route("api/v1/content-managers")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] // "Bearer"
    [Authorize(Roles = nameof(UserRole.ContentManager))]
    public class ContentManagerController(IContentManager contentManagerService, IUser userBLL) : Controller
    {
        private readonly IContentManager _contentManager = contentManagerService;
        private readonly IUser _userBLL = userBLL;

        /// <summary>
        /// Thông tin cá nhân (Tên, Ảnh)
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = $"{nameof(UserRole.ContentManager)},{nameof(UserRole.Admin)}"), HttpGet("me/profile")]
        public async Task<IActionResult> GetProfileAsync()
        {
            var user = await _userBLL.GetProfileAsync();
            return Ok(user);
        }

        /// <summary>
        /// Chỉnh sửa thông tin cá nhân (Tên,Ảnh)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize(Roles = nameof(UserRole.ContentManager)), HttpPatch("me/profile")]
        public async Task<IActionResult> EditProfileAsync([FromForm] EditProfileRequestModel request)
        {
            await _userBLL.EditProfileAsync(request);
            return Ok("Update profile successfully!");
        }


        /// <summary>
        /// Cập nhật Restriction của Track (vd: khi duyệt track mới đc upload, hoặc cấm vì vi phạm...)
        /// </summary>
        /// <param name="id"> Id của track </param>
        /// <param name="model"></param>
        /// <returns></returns>
        [Authorize(Roles = nameof(UserRole.ContentManager)), HttpPut("{id}/track-restriction-change")]
        public async Task<IActionResult> UpdateAlbumAsync(string id, [FromForm] TrackRestrictionRequestModel model)
        {
            await _contentManager.ChangeTrackRestrictionAsync(id, model);
            return Ok(new { Message = "Update Track Restriction Successfully" });
        }
    }
}
