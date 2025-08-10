using MaelstromLauncher.Server.Globals;
using MaelstromLauncher.Server.Services;
using MaelstromLauncher.Server.DTOs;
using Microsoft.AspNetCore.Mvc;

//
// TODO:
// 1) Server controller for getting manifest, manifest info (date when it was generated to check for updates)
// 2) A controller to validate files against what's on the server's manifest
// 3) A controller for returning which exact files were modified for partial downloads
// 4) Logic wrapping all of this with proper cancellation/error handling
// 5) OpenAPI documentation
//

namespace MaelstromLauncher.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ManifestController : ControllerBase
    {
        private readonly ManifestService _manifestSerivce;
        private readonly ILogger<ManifestController> _logger;

        public ManifestController(ManifestService manifestService, ILogger<ManifestController> logger)
        {
            _manifestSerivce = manifestService;
            _logger = logger;
        }

        /// <summary>
        /// Gets the current game manifest
        /// </summary>
        /// <returns>The current manifest with all file entries and header metadata (version, date, fileentry)</returns>

        [HttpGet("manifest")]
        [ProducesResponseType(typeof(ManifestDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetManifestAsync()
        {
            try
            {
                LoggerService.Log(LogType.MANIFEST, LogType.INFORMATION, "Recieved API request to get manifest");
                var manifest = await _manifestSerivce.EnsureManifestExistsAsync();

                if (manifest == null)
                {
                    var errorMessagge = "Manifest not found";
                    LoggerService.Log(LogType.MANIFEST, LogType.ERROR, errorMessagge);
                    return Problem(errorMessagge, statusCode: StatusCodes.Status500InternalServerError);
                }

                return Ok(manifest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving manifest");
                LoggerService.Log(LogType.MANIFEST, LogType.ERROR, $"Manifest not found: {ex.Message}");
                return Problem("Manifest could not be retrieved", statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Gets the current game manifest metadata
        /// </summary>
        /// <returns>The current manifest manifest metadata, which includes version and time manifest was generated at</returns>

        [HttpGet("manifestInfo")]
        [ProducesResponseType(typeof(ManifestInfoDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetManifestInfoAsync()
        {
            try
            {
                LoggerService.Log(LogType.MANIFEST, LogType.INFORMATION, "Recieved API request to get manifest metadata");
                var manifest = await _manifestSerivce.EnsureManifestExistsAsync();

                if (manifest == null)
                {
                    var errorMessagge = "Manifest not found";
                    LoggerService.Log(LogType.MANIFEST, LogType.ERROR, errorMessagge);
                    return Problem(errorMessagge, statusCode: StatusCodes.Status500InternalServerError);
                }

                var manifestInfo = new ManifestInfoDto();

                LoggerService.Log(LogType.MANIFEST, LogType.INFORMATION, $"Successfully retrieved manifest metadata with latest version from: {manifestInfo.GeneratedAt}");

                return Ok(manifestInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving manifest metadata");
                LoggerService.Log(LogType.MANIFEST, LogType.ERROR, $"Failed to get manifest metadata: {ex.Message}");
                return Problem("Manifest metadata could not be retrieved", statusCode: StatusCodes.Status500InternalServerError);
            }
        }
    }
}
