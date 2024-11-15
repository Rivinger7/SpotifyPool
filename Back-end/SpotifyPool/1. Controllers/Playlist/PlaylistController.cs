using BusinessLogicLayer.Interface.Services_Interface.Playlists.Favorites;
using BusinessLogicLayer.Interface.Services_Interface.Playlists.Own;
using BusinessLogicLayer.ModelView.Service_Model_Views.Playlists.Request;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SetupLayer.Enum.Services.User;

namespace SpotifyPool._1._Controllers.Playlist
{
	[Route("api/v1/playlists")]
	[ApiController]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] // "Bearer"
	[Authorize(Roles = nameof(UserRole.Customer))]
	public class PlaylistController(IFavoritesPlaylist favoritesPlaylistService, IOwnPlaylist ownPlaylist) : ControllerBase
	{
		private readonly IFavoritesPlaylist _favoritesPlaylistService = favoritesPlaylistService;
		private readonly IOwnPlaylist _ownPlaylist = ownPlaylist;

		/// <summary>
		/// Lấy danh sách các track yêu thích
		/// </summary>
		/// <returns></returns>
		[Authorize(Roles = nameof(UserRole.Customer)), HttpGet("favorite-songs")]
		public async Task<IActionResult> GetFavoriteSongs()
		{
			var result = await _favoritesPlaylistService.GetPlaylistAsync();
			return Ok(result);
		}

		/// <summary>
		/// Thêm track vào danh sách yêu thích
		/// </summary>
		/// <param name="trackID"></param>
		/// <returns></returns>
		[Authorize(Roles = nameof(UserRole.Customer)), HttpPost("add-to-favorite-list")]
		public async Task<IActionResult> AddToFavoriteListAsync([FromBody] string trackID)
		{
			await _favoritesPlaylistService.AddToPlaylistAsync(trackID);
			return Ok(new { Message = "Add to Favorite Song Successfully" });
		}

		/// <summary>
		/// Xóa track ra khỏi danh sách yêu thích
		/// </summary>
		/// <param name="trackID"></param>
		/// <returns></returns>
		[Authorize(Roles = nameof(UserRole.Customer)), HttpDelete("playlist/favorite-songs/{trackID}")]
		public async Task<IActionResult> RemoveTrackFromFavoriteSongs(string trackID)
		{
			await _favoritesPlaylistService.RemoveFromPlaylistAsync(trackID);
			return Ok(new { Message = "Remove Favorite Song Successfully" });
		}



		[HttpGet("get-all")]
		public async Task<IActionResult> GetAllPlaylist()
		{
			await _ownPlaylist.GetAllPlaylistAsync();
			return Ok(new { Message = "Get all playlist successfully" });
		}


		/// <summary>
		/// Thêm một plalist mới
		/// </summary>
		/// <returns></returns>
		[HttpPost("add-new")]
		public async Task<IActionResult> AddNewPlaylist()
		{
			await _ownPlaylist.AddNewPlaylistAsync();
			return Ok(new { Message = "Add new playlist successfully" });
		}


		/// <summary>
		/// Cập nhật thông tin playlist
		/// </summary>
		/// <param name="playlistID"></param>
		/// <returns></returns>
		[HttpPut("update/{playlistID}")]
		public async Task<IActionResult> UpdatePlaylist(string playlistID,[FromBody] UpdatePlaylistRequestModel request)
		{
			await _ownPlaylist.UpdatePlaylistAsync(playlistID, request);
			return Ok(new { Message = "Update playlist successfully" });
		}


		/// <summary>
		/// Ghim playlist
		/// </summary>
		/// <param name="playlistID"></param>
		/// <returns></returns>
		[HttpPatch("pin/{playlistID}")]
		public async Task<IActionResult> PinPlaylist(string playlistID)
		{
			await _ownPlaylist.PinPlaylistAsync(playlistID);
			return Ok(new { Message = "Pin playlist successfully" });
		}
	}
}
