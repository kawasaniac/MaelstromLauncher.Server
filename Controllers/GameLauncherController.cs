using MaelstromLauncher.Server.Globals;
using MaelstromLauncher.Server.Services;
using Microsoft.AspNetCore.Mvc;

namespace MaelstromLauncher.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GameLauncherController(GameLauncherService _gameLauncherService, ILogger<ManifestController> _logger) : ControllerBase
    {
        [HttpGet("game-launcher")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DownloadGameLauncherAsync()
        {
            try
            {
                LoggerService.Log(LogType.MAIN, LogType.INFORMATION, "Received API request to download game launcher");

                var fileStream = await _gameLauncherService.GetGameLauncherStreamAsync();
                if (fileStream == null)
                {
                    var errorMessage = "Game launcher file not found";
                    LoggerService.Log(LogType.MAIN, LogType.ERROR, errorMessage);
                    return NotFound(new { message = errorMessage });
                }

                var fileName = _gameLauncherService.GetLauncherFileName();
                LoggerService.Log(LogType.MAIN, LogType.INFORMATION, $"Serving game launcher download: {fileName}");

                return File(fileStream, "application/octet-stream", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading game launcher");
                LoggerService.Log(LogType.MAIN, LogType.ERROR, $"Error downloading game launcher: {ex.Message}");
                return Problem("Game launcher could not be downloaded", statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet("status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetGameLauncherStatus()
        {
            try
            {
                LoggerService.Log(LogType.MAIN, LogType.INFORMATION, "Received API request to check game launcher status");

                var exists = _gameLauncherService.LauncherExists();
                var response = new
                {
                    exists,
                    fileName = _gameLauncherService.GetLauncherFileName(),
                    message = exists ? "Game launcher is available" : "Game launcher not found"
                };

                LoggerService.Log(LogType.MAIN, LogType.INFORMATION, $"Game launcher status: {(exists ? "Available" : "Not found")}");
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking game launcher status");
                LoggerService.Log(LogType.MAIN, LogType.ERROR, $"Error checking game launcher status: {ex.Message}");
                return Problem("Game launcher status could not be checked", statusCode: StatusCodes.Status500InternalServerError);
            }
        }
    }
}
