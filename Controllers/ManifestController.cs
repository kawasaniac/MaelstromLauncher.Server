using MaelstromLauncher.Server.Globals;
using MaelstromLauncher.Server.Services;
using MaelstromLauncher.Server.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace MaelstromLauncher.Server.Controllers
{
    [ApiController]
    [Route("api/manifest/[controller]")]
    [Produces("application/json")]
    public class ManifestController(ManifestService manifestService, ILogger<ManifestController> logger) : ControllerBase
    {

        /// <summary>
        /// Gets the current game manifest
        /// </summary>
        /// <returns>The current manifest with all file entries and header metadata (version, date, fileentry).</returns>

        [HttpGet]
        [ProducesResponseType(typeof(ManifestDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetManifestAsync()
        {
            try
            {
                LoggerService.Log(LogType.MANIFEST, LogType.INFORMATION, "Recieved API request to get manifest");
                var manifest = await manifestService.EnsureManifestExistsAsync();

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
                logger.LogError(ex, "Error retrieving manifest");
                LoggerService.Log(LogType.MANIFEST, LogType.ERROR, $"Manifest not found: {ex.Message}");
                return Problem("Manifest could not be retrieved", statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Gets the current game manifest metadata
        /// </summary>
        /// <returns>The current manifest manifest metadata, which includes version and time manifest was generated at.</returns>

        [HttpGet("info")]
        [ProducesResponseType(typeof(ManifestInfoDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetManifestInfoAsync()
        {
            try
            {
                LoggerService.Log(LogType.MANIFEST, LogType.INFORMATION, "Recieved API request to get manifest metadata");
                var manifest = await manifestService.EnsureManifestExistsAsync();

                if (manifest == null)
                {
                    var errorMessagge = "Manifest not found";
                    LoggerService.Log(LogType.MANIFEST, LogType.ERROR, errorMessagge);
                    return Problem(errorMessagge, statusCode: StatusCodes.Status500InternalServerError);
                }

                var manifestInfo = new ManifestInfoDto()
                {
                    Version = manifest.Version
                };
                LoggerService.Log(LogType.MANIFEST, LogType.INFORMATION, $"Successfully retrieved manifest metadata with latest version from: {manifestInfo.GeneratedAt}");

                return Ok(manifestInfo);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving manifest metadata");
                LoggerService.Log(LogType.MANIFEST, LogType.ERROR, $"Failed to get manifest metadata: {ex.Message}");
                return Problem("Manifest metadata could not be retrieved", statusCode: StatusCodes.Status500InternalServerError);
            }
        }
    }
}
