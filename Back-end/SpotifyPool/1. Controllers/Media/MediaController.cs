using Business_Logic_Layer.Services_Interface.InMemoryCache;
using BusinessLogicLayer.Implement.Microservices.Cloudinaries;
using BusinessLogicLayer.Interface.Microservices_Interface.Spotify;
using BusinessLogicLayer.ModelView.Service_Model_Views.Tracks.Response;
using Microsoft.AspNetCore.Mvc;

namespace SpotifyPool.Controllers.Media
{
    [Route("api/media")]
    [ApiController]
    public class MediaController(CloudinaryService cloudinaryService, ISpotify spotifyService) : ControllerBase
    {
        private readonly CloudinaryService cloudinaryService = cloudinaryService;
        private readonly ISpotify _spotifyService = spotifyService;

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

        [HttpPost("upload-image")]
        public IActionResult UploadImage(IFormFile imageFile)
        {
            var uploadResult = cloudinaryService.UploadImage(imageFile);
            return Ok(new { message = "Upload ImageResponseModel Successfully", uploadResult });
        }

        [HttpPost("upload-track")]
        public IActionResult UploadTrack(IFormFile trackFile)
        {
            var uploadResult = cloudinaryService.UploadTrack(trackFile);
            return Ok(new { message = "Upload Video Successfully", uploadResult });
        }

        [HttpGet("get-image/{publicID}")]
        public IActionResult GetImageResult(string publicID)
        {
            var getResult = cloudinaryService.GetImageResult(publicID);
            return Ok(new { message = "Get ImageResponseModel Successfully", getResult });
        }

        [HttpGet("get-track/{publicID}")]
        public IActionResult GetTrackResult(string publicID)
        {
            var getResult = cloudinaryService.GetTrackResult(publicID);
            return Ok(new { message = "Get Track Successfully", getResult });
        }

        [HttpDelete("delete-image/{publicID}")]
        public IActionResult DeleteImage(string publicID)
        {
            var deleteResult = cloudinaryService.DeleteImage(publicID);
            return Ok(new { message = $"Delete ImageResponseModel Successfully with Public ID {publicID}", deleteResult });
        }

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
            var result = await _spotifyService.GetAllTracksAsync();
            return Ok(result);
        }

        [HttpGet("spotify/authorize")]
        public IActionResult Authorize()
        {
            var result = _spotifyService.Authorize();
            return Redirect(result);
        }

        // Handle Spotify authorization callback and get the access token
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

        [HttpGet("test/lookup")]
        public async Task<IActionResult> TestLookup()
        {
            var trackArtist = await _spotifyService.TestLookup();
            return Ok(trackArtist);
        }
    }
}
