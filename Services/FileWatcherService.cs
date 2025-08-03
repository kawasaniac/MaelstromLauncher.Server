using MaelstromLauncher.Server.Globals;

namespace MaelstromLauncher.Server.Services
{
    public class FileWatcherService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly string _gameDirectoryPath;
        private FileSystemWatcher? _fileSystemWatcher;
        private readonly Timer? _debounceTimer;
        private volatile bool _refreshPending = false;

        public FileWatcherService(IServiceScopeFactory scopeFactory, IConfiguration configuration)
        {
            _scopeFactory = scopeFactory;
            _gameDirectoryPath = configuration["GameDirectory:Path"] ?? "/opt/maelstrom-launcher/files";

            _debounceTimer = new Timer(async _ => await RefreshManifestDebounced(), null, Timeout.Infinite, Timeout.Infinite);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!Directory.Exists(_gameDirectoryPath))
            {
                LoggerService.Log(LogType.MANIFEST, LogType.ERROR, $"Game directory does not exist: {_gameDirectoryPath}");
                return;
            }

            _fileSystemWatcher = new FileSystemWatcher(_gameDirectoryPath)
            {
                NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.Size,
                IncludeSubdirectories = true,
                EnableRaisingEvents = true
            };

            _fileSystemWatcher.Created += OnFileChanged;
            _fileSystemWatcher.Changed += OnFileChanged;
            _fileSystemWatcher.Deleted += OnFileChanged;
            _fileSystemWatcher.Renamed += OnFileChanged;

            LoggerService.Log(LogType.MANIFEST, LogType.INFORMATION, "File system watcher started for manifest updates");

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }

        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            if (e.Name != null && e.Name.EndsWith("manifest.json"))
                return;

            LoggerService.Log(LogType.MANIFEST, LogType.DEBUG, $"File system change detected: {e.FullPath}");

            _refreshPending = true;
            _debounceTimer?.Change(2000, Timeout.Infinite);
        }

        private async Task RefreshManifestDebounced()
        {
            if (!_refreshPending) return;

            _refreshPending = false;

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var manifestService = scope.ServiceProvider.GetRequiredService<ManifestService>();

                await manifestService.RefreshManifestAsync();
                LoggerService.Log(LogType.MANIFEST, LogType.INFORMATION, "File system triggered manifest refresh completed");
            }
            catch (Exception ex)
            {
                LoggerService.Log(LogType.MANIFEST, LogType.ERROR, $"File system triggered manifest refresh failed: {ex.Message}");
            }
        }

        public override void Dispose()
        {
            _fileSystemWatcher?.Dispose();
            _debounceTimer?.Dispose();
            base.Dispose();
        }
    }
}
