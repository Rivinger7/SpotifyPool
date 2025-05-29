using BusinessLogicLayer.Implement.Services.Fingerprint;
using Microsoft.AspNetCore.Mvc;

namespace SpotifyPool._1._Controllers.AudioFingerprint
{
    [Route("api/[controller]")]
    [ApiController]
    public class AudioFingerprintController(FingerprintCustomService fingerprintCustomService) : ControllerBase
    {
        private readonly FingerprintCustomService _fingerprintCustomService = fingerprintCustomService;

        [HttpPost("comparation")]
        public async Task<IActionResult> CompareAudioFile(IFormFile audioFile)
        {
            if (audioFile == null || audioFile.Length == 0)
            {
                return BadRequest("No audio file uploaded.");
            }
            try
            {
                // Call the service to save the fingerprint to MongoDB
                var result = await _fingerprintCustomService.CompareWithDatabase(audioFile);
                return Ok(new { Confidence = $"{Math.Round(result)}%" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> UploadAudioFile(IFormFile audioFile)
        {
            if (audioFile == null || audioFile.Length == 0)
            {
                return BadRequest("No audio file uploaded.");
            }
            try
            {
                // Call the service to save the fingerprint to MongoDB
                await _fingerprintCustomService.SaveFingerprintToMongo(audioFile);
                return Ok("Audio fingerprint saved successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
