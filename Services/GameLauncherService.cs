using MaelstromLauncher.Server.Globals;

namespace MaelstromLauncher.Server.Services
{
    public class GameLauncherService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;
        private readonly string _launcherFileName;
        private readonly string _launcherPath;

        public GameLauncherService(
            IWebHostEnvironment environment,
            IConfiguration configuration)
        {
            _environment = environment;
            _configuration = configuration;
            _launcherFileName = _configuration["GameLauncher:FileName"] ?? "GameLauncher.exe";
            _launcherPath = Path.Combine(_environment.WebRootPath, _launcherFileName); ;
        }

        public async Task<FileStream?> GetGameLauncherStreamAsync()
        {
            try
            {
                if (!File.Exists(_launcherPath))
                {
                    LoggerService.Log(LogType.MAIN, LogType.ERROR, "Game launcher file not found for download");
                    return null;
                }

                var fileStream = await Task.Run(() => new FileStream(_launcherPath, FileMode.Open, FileAccess.Read, FileShare.Read));

                LoggerService.Log(LogType.MAIN, LogType.INFORMATION, "Opening game launcher file stream for download");
                return fileStream;
            }
            catch (Exception ex)
            {
                LoggerService.Log(LogType.MAIN, LogType.ERROR, $"Error opening game launcher stream: {ex.Message}");
                return null;
            }
        }

        public string GetLauncherFileName()
        {
            return _launcherFileName;
        }

        public string GetLauncherPath()
        {
            return _launcherPath;
        }

        public bool LauncherExists()
        {
            return File.Exists(_launcherPath);
        }
    }
}
