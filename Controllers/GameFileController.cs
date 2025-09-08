using MaelstromLauncher.Server.Globals;
using MaelstromLauncher.Server.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace MaelstromLauncher.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GameFileController(IConfiguration configuration) : ControllerBase
    {
        private readonly IConfiguration _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        private readonly FileExtensionContentTypeProvider _contentTypeProvider = new();

        [HttpGet("{**filePath}")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult DownloadFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                const string errorMessage = "File path cannot be empty";
                LoggerService.Log(LogType.MAIN, LogType.ERROR, errorMessage);
                return BadRequest(new { message = errorMessage });
            }

            filePath = filePath.Replace('\\', '/').Trim('/');
            if (filePath.Contains("..") || Path.IsPathRooted(filePath))
            {
                const string errorMessage = "Invalid file path";
                LoggerService.Log(LogType.MAIN, LogType.ERROR, $"Potential directory traversal attempt: {filePath}");
                return BadRequest(new { message = errorMessage });
            }

            var gameDirectory = _configuration["GameDirectory:Path"];
            if (string.IsNullOrEmpty(gameDirectory))
            {
                const string errorMessage = "Game directory not configured";
                LoggerService.Log(LogType.MAIN, LogType.ERROR, errorMessage);
                return Problem(errorMessage, statusCode: StatusCodes.Status500InternalServerError);
            }

            var fullPath = Path.Combine(gameDirectory, filePath);
            var gameDirectoryInfo = new DirectoryInfo(gameDirectory);
            var fileInfo = new FileInfo(fullPath);

            if (!fileInfo.FullName.StartsWith(gameDirectoryInfo.FullName, StringComparison.OrdinalIgnoreCase))
            {
                LoggerService.Log(LogType.MAIN, LogType.ERROR, $"Path traversal blocked: {filePath}");
                return Forbid();
            }

            if (!fileInfo.Exists)
            {
                var errorMessage = $"File not found: {filePath}";
                LoggerService.Log(LogType.MAIN, LogType.ERROR, errorMessage);
                return NotFound(new { message = errorMessage });
            }

            LoggerService.Log(LogType.MAIN, LogType.INFORMATION, $"Serving file: {filePath}");

            if (!_contentTypeProvider.TryGetContentType(fullPath, out var contentType))
            {
                contentType = "application/octet-stream";
            }

            var fileName = Path.GetFileName(fullPath);

            var fileStream = new FileStream(
                fullPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 4096,
                useAsync: true
            );

            return File(fileStream, contentType, fileName, enableRangeProcessing: true);
        }
    }
}