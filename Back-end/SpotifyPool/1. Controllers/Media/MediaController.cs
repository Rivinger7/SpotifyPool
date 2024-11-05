using BusinessLogicLayer.Implement.Microservices.Cloudinaries;
using BusinessLogicLayer.Interface.Microservices_Interface.Spotify;
using BusinessLogicLayer.Interface.Services_Interface.Playlists.Favorites;
using BusinessLogicLayer.Interface.Services_Interface.Tracks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SetupLayer.Enum.Microservices.Cloudinary;
using SetupLayer.Enum.Services.User;

namespace SpotifyPool.Controllers.Media
{
    [Route("api/media")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] // "Bearer"
    public class MediaController(CloudinaryService cloudinaryService, ISpotify spotifyService, ITrack trackService, IFavoritesPlaylist favoritesPlaylistService) : ControllerBase
    {
        private readonly CloudinaryService cloudinaryService = cloudinaryService;
        private readonly ISpotify _spotifyService = spotifyService;
        private readonly ITrack _trackService = trackService;
        private readonly IFavoritesPlaylist _favoritesPlaylistService = favoritesPlaylistService;

        /// <summary>
        /// FOR BACK-END
        /// </summary>
        /// <param name="formFile"></param>
        /// <returns></returns>
        #region Cloudinary
        [HttpPost("test-content-type-file")]
        public IActionResult GetContentTypeFile(IFormFile formFile)
        {
            if (formFile == null)
            {
                return BadRequest("ehhh");
            }

            var contentType = formFile.ContentType;

            string[] type = contentType.Split("/");

            return Ok(new { ContentType = contentType, Type = type });
        }

        /// <summary>
        /// FOR BACK-END
        /// </summary>
        /// <param name="imageFile"></param>
        /// <param name="imageTag"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("upload-image")]
        public IActionResult UploadImage(IFormFile imageFile, ImageTag imageTag)
        {
            var uploadResult = cloudinaryService.UploadImage(imageFile, imageTag);
            return Ok(new { message = "Upload ImageResponseModel Successfully", uploadResult });
        }

        /// <summary>
        /// FOR BACK-END
        /// </summary>
        /// <param name="trackFile"></param>
        /// <param name="audioTagParent"></param>
        /// <param name="audioTagChild"></param>
        /// <returns></returns>
        [HttpPost("upload-track")]
        public IActionResult UploadTrack(IFormFile trackFile, AudioTagParent audioTagParent, AudioTagChild audioTagChild)
        {
            var uploadResult = cloudinaryService.UploadTrack(trackFile, audioTagParent, audioTagChild);
            return Ok(new { message = "Upload Video Successfully", uploadResult });
        }

        /// <summary>
        /// FOR BACK-END
        /// </summary>
        /// <param name="publicID"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("get-image/{publicID}")]
        public IActionResult GetImageResult(string publicID)
        {
            var getResult = cloudinaryService.GetImageResult(publicID);
            return Ok(new { message = "Get ImageResponseModel Successfully", getResult });
        }

        /// <summary>
        /// FOR BACK-END
        /// </summary>
        /// <param name="publicID"></param>
        /// <returns></returns>
        [HttpGet("get-track/{publicID}")]
        public IActionResult GetTrackResult(string publicID)
        {
            var getResult = cloudinaryService.GetTrackResult(publicID);
            return Ok(new { message = "Get Track Successfully", getResult });
        }

        /// <summary>
        /// FOR BACK-END
        /// </summary>
        /// <param name="publicID"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpDelete("delete-image/{publicID}")]
        public IActionResult DeleteImage(string publicID)
        {
            var deleteResult = cloudinaryService.DeleteImage(publicID);
            return Ok(new { message = $"Delete ImageResponseModel Successfully with Public ID {publicID}", deleteResult });
        }

        /// <summary>
        /// FOR BACK-END
        /// </summary>
        /// <param name="publicID"></param>
        /// <returns></returns>
        [HttpDelete("delete-track/{publicID}")]
        public IActionResult DeleteTrack(string publicID)
        {
            var deleteResult = cloudinaryService.DeleteTrack(publicID);
            return Ok(new { message = $"Delete Track Successfully with Public ID {publicID}", deleteResult });
        }
        #endregion

        #region Spotify
        [HttpGet("tracks")]
        public async Task<IActionResult> GetAllTracksAsync()
        {
            var result = await _trackService.GetAllTracksAsync();
            return Ok(result);
        }

        /// <summary>
        /// Tìm theo tên bài hát hoặc nghệ sĩ
        /// </summary>
        /// <param name="searchTerm"></param>
        /// <returns></returns>
        [Authorize(Roles = nameof(UserRole.Customer))]
        [HttpGet("tracks/search")]
        public async Task<IActionResult> SearchTracksAsync([FromQuery] string searchTerm)
        {
            var result = await _trackService.SearchTracksAsync(searchTerm);
            return Ok(result);
        }

        /// <summary>
        /// Thêm bài hát vào danh sách yêu thích
        /// </summary>
        /// <param name="trackID"></param>
        /// <returns></returns>
        [AllowAnonymous, HttpPost("add-to-favorite-list")]
        public async Task<IActionResult> AddToFavoriteListAsync([FromBody] string trackID)
        {
            await _favoritesPlaylistService.AddToPlaylistAsync(trackID);
            return Ok(new {Message = "Add to Favorite Song Successfully"});
        }

        /// <summary>
        /// Danh sách các bài hát yêu thích
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous, HttpGet("playlist/favorite-songs")]
        public async Task<IActionResult> GetFavoriteSongs()
        {
            var result = await _favoritesPlaylistService.GetPlaylistAsync();
            return Ok(result);
        }

        /// <summary>
        /// Xóa bài hát ra khỏi danh sách yêu thích
        /// </summary>
        /// <param name="trackID"></param>
        /// <returns></returns>
        [AllowAnonymous, HttpDelete("playlist/favorite-songs/{trackID}")]
        public async Task<IActionResult> RemoveTrackFromFavoriteSongs(string trackID)
        {
            await _favoritesPlaylistService.RemoveFromPlaylistAsync(trackID);
            return Ok(new { Message = "Remove Favorite Song Successfully" });
        }

        /// <summary>
        /// FOR BACK-END
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("spotify/authorize")]
        public IActionResult Authorize()
        {
            var result = _spotifyService.Authorize();
            return Redirect(result);
        }

        // Handle Spotify authorization callback and get the access token
        /// <summary>
        /// FOR BACK-END
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("callback")]
        public async Task<IActionResult> Callback([FromQuery] string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return BadRequest("Authorization code not found.");
            }

            // Exchange the authorization code for an access token
            var (accessToken, refreshToken) = await _spotifyService.GetAccessTokenAsync(code);

            // Store the access token in the session or return it to the client
            // For simplicity, we'll just return it as a response here
            return Ok(new
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            });
        }

        // Get the top tracks
        /// <summary>
        /// FOR BACK-END
        /// </summary>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("spotify/top-tracks")]
        public async Task<IActionResult> GetTopTracks([FromQuery] string accessToken)
        {
            if (string.IsNullOrEmpty(accessToken))
            {
                return BadRequest("Access token is required.");
            }

            // Use the access token to get top tracks
            string topTracks = await _spotifyService.GetTopTracksAsync(accessToken);

            return Ok(topTracks); // Return the top tracks as the response
        }

        /// <summary>
        /// FOR BACK-END
        /// </summary>
        /// <param name="accessToken"></param>
        /// <param name="limit"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("spotify/fetch/saved-tracks")]
        public async Task<IActionResult> GetUserSaveTracks([FromQuery] string accessToken, [FromQuery] int limit, [FromQuery] int offset)
        {
            if (string.IsNullOrEmpty(accessToken))
            {
                return BadRequest("Access token is required.");
            }

            await _spotifyService.FetchUserSaveTracksAsync(accessToken, limit, offset);
            return Ok(new {message = "Fetched Data Successfully"});
        }

        /// <summary>
        /// Fetch playlist items from Spotify API
        /// </summary>
        /// <param name="accessToken"></param>
        /// <param name="limit"></param>
        /// <param name="playlistID"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("spotify/fetch/playlist/{playlistID}/tracks")]
        public async Task<IActionResult> GetPlaylistTracks([FromQuery] string accessToken, [FromQuery] int limit, string playlistID)
        {
            if (string.IsNullOrEmpty(accessToken))
            {
                return BadRequest("Access token is required.");
            }

            await _spotifyService.FetchPlaylistItemsAsync(accessToken, playlistID, limit);
            return Ok(new { message = "Fetched Data Successfully" });
        }

        /// <summary>
        /// FOR BACK-END
        /// </summary>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        [HttpGet("spotify/genre-seeds")]
        public async Task<IActionResult> GetAllGenreSeeds([FromQuery] string accessToken)
        {
            if (string.IsNullOrEmpty(accessToken))
            {
                return BadRequest("Access token is required.");
            }

            await _spotifyService.GetAllGenreSeedsAsync(accessToken);
            return Ok();
        }

        /// <summary>
        /// FOR BACK-END
        /// </summary>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        [HttpGet("spotify/markets")]
        public async Task<IActionResult> GetAllMarkets([FromQuery] string accessToken)
        {
            if (string.IsNullOrEmpty(accessToken))
            {
                return BadRequest("Access token is required.");
            }

            await _spotifyService.GetAllMarketsAsync(accessToken);
            return Ok();
        }
        #endregion
    }
}
