using MaelstromLauncher.Server.DTOs;
using MaelstromLauncher.Server.Globals;
using MaelstromLauncher.Server.Services;
using Microsoft.AspNetCore.Mvc;

namespace MaelstromLauncher.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ManifestController(ManifestService manifestService, ILogger<ManifestController> logger) : ControllerBase
    {
        private readonly ManifestService _manifestService = manifestService ?? throw new ArgumentNullException(nameof(manifestService));
        private readonly ILogger<ManifestController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        [HttpGet]
        [ProducesResponseType(typeof(ManifestDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetManifestAsync()
        {
            try
            {
                LoggerService.Log(LogType.MANIFEST, LogType.INFORMATION, "Received API request to get manifest");

                var manifest = await _manifestService.EnsureManifestExistsAsync();
                if (manifest == null)
                {
                    const string errorMessage = "Manifest not found";
                    LoggerService.Log(LogType.MANIFEST, LogType.ERROR, errorMessage);
                    return NotFound(errorMessage);
                }

                return Ok(manifest);
            }
            catch (TaskCanceledException)
            {
                LoggerService.Log(LogType.MANIFEST, LogType.ERROR, "API request to get manifest request was canceled by the client");
                _logger.LogWarning("GetManifestAsync request was canceled by the client");
                return StatusCode(499); // Client Closed Request
            }
            catch (Exception ex)
            {
                const string errorMessage = "Manifest could not be retrieved";
                LoggerService.Log(LogType.MANIFEST, LogType.ERROR, $"{errorMessage}: {ex.Message}");
                _logger.LogError(ex, errorMessage);
                return Problem(errorMessage, statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet("info")]
        [ProducesResponseType(typeof(ManifestInfoDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetManifestInfoAsync()
        {
            try
            {
                LoggerService.Log(LogType.MANIFEST, LogType.INFORMATION, "Received API request to get manifest metadata");

                var manifest = await _manifestService.EnsureManifestExistsAsync();
                if (manifest == null)
                {
                    const string errorMessage = "Manifest not found";
                    LoggerService.Log(LogType.MANIFEST, LogType.ERROR, errorMessage);
                    return NotFound(errorMessage);
                }

                var manifestInfo = new ManifestInfoDto
                {
                    Version = manifest.Version,
                    GeneratedAt = manifest.GeneratedAt
                };

                LoggerService.Log(LogType.MANIFEST, LogType.INFORMATION, $"Successfully retrieved manifest metadata with latest version from: {manifestInfo.GeneratedAt}");
                return Ok(manifestInfo);
            }
            catch (TaskCanceledException)
            {
                LoggerService.Log(LogType.MANIFEST, LogType.ERROR, "API request to get manifest info request was canceled by the client");
                _logger.LogWarning("GetManifestInfoAsync request was canceled by the client");
                return StatusCode(499);
            }
            catch (Exception ex)
            {
                const string errorMessage = "Manifest metadata could not be retrieved";
                LoggerService.Log(LogType.MANIFEST, LogType.ERROR, $"{errorMessage}: {ex.Message}");
                _logger.LogError(ex, errorMessage);
                return Problem(errorMessage, statusCode: StatusCodes.Status500InternalServerError);
            }
        }
    }
}