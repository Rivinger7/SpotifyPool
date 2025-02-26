using BusinessLogicLayer.Interface.Microservices_Interface.Spotify;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SetupLayer.Enum.Services.User;

namespace SpotifyPool._1._Controllers.Spotify
{
    [Route("api/v1/spotify")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] // "Bearer"
    public class SpotifyController(ISpotify spotifyService) : ControllerBase
    {
        private readonly ISpotify _spotifyService = spotifyService;

        /// <summary>
        /// Uỷ quyền truy cập vào Spotify API
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous, HttpGet("authorize")]
        public IActionResult Authorize()
        {
            var result = _spotifyService.Authorize();
            return Redirect(result);
        }

        /// <summary>
        /// Đổi mã authorize code lấy access token
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        [Authorize(Roles = nameof(UserRole.Admin)), HttpGet("callback")]
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

        /// <summary>
        /// Fix tracks với artist bị null
        /// </summary>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        //[Authorize(Roles = nameof(UserRole.SuperAdmin)), HttpGet("fetch/update/artists")]
        //public async Task<IActionResult> UpFetchManyArtists([FromQuery] string accessToken)
        //{
        //    if (string.IsNullOrEmpty(accessToken))
        //    {
        //        return BadRequest("Access token is required.");
        //    }
        //    await _spotifyService.FixTracksWithArtistIsNullAsync(accessToken);
        //    return Ok(new { message = "Fetched Data Successfully" });
        //}

        /// <summary>
        /// Fetch tracks từ Spotify API (Dùng khi có tracks mới được thêm vào playlist từ Spotify)
        /// </summary>
        /// <param name="accessToken"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = nameof(UserRole.Admin)), HttpGet("fetch/update/playlist/{id}/tracks")]
        public async Task<IActionResult> UpdateFetchPlaylistItems([FromQuery] string accessToken, string id)
        {
            if (string.IsNullOrEmpty(accessToken))
            {
                return BadRequest("Access token is required.");
            }

            await _spotifyService.UpdateFetchPlaylistItemsAsync(accessToken, id);
            return Ok(new { message = "Fetched Data Successfully" });
        }

        /// <summary>
        /// Fetch tracks từ Spotify API
        /// </summary>
        /// <param name="accessToken"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = nameof(UserRole.Admin)), HttpGet("fetch/playlist/{id}/tracks")]
        public async Task<IActionResult> FetchPlaylistItems([FromQuery] string accessToken, string id)
        {
            if (string.IsNullOrEmpty(accessToken))
            {
                return BadRequest("Access token is required.");
            }

            await _spotifyService.FetchPlaylistItemsAsync(accessToken, id);
            return Ok(new { message = "Fetched Data Successfully" });
        }

        /// <summary>
        /// Fetch lyrics từ Genius API
        /// </summary>
        /// <param name="accessToken">Access Token lấy từ Genius API</param>
        /// <returns></returns>
        [AllowAnonymous, HttpGet("fetch/lyrics")]
        public async Task<IActionResult> FetchLyrics([FromQuery] string accessToken)
        {
            if (string.IsNullOrEmpty(accessToken))
            {
                return BadRequest("Access token is required.");
            }

            await _spotifyService.FetchLyricsAsync(accessToken);
            return Ok(new { message = "Fetched Data Successfully" });
        }

        /// <summary>
        /// Lấy top tracks từ Spotify API
        /// </summary>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        [Authorize(Roles = nameof(UserRole.Admin)), HttpGet("top-tracks")]
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

        [AllowAnonymous, HttpGet("audio-features")]
        public async Task<IActionResult> GetSeveralAudioFeatures([FromQuery] string accessToken, string trackIds)
        {
            if (string.IsNullOrEmpty(accessToken))
            {
                return BadRequest("Access token is required.");
            }

            if (string.IsNullOrEmpty(trackIds))
            {
                return BadRequest("Track IDs are required.");
            }

            // Use the access token to get audio features for several tracks
            string audioFeatures = await _spotifyService.GetseveralAudioFeaturesAsync(accessToken, trackIds);

            return Ok(audioFeatures); // Return the audio features as the response
        }
    }
}
