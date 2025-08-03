using MaelstromLauncher.Server.Globals;
using MaelstromLauncher.Server.Models;
using System;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MaelstromLauncher.Server.Services
{
    public class ManifestService
    {
        private readonly string _manifestFilePath;
        public string GameDirectoryPath { get; private set; }
        public string DataPath { get; private set; }
        public string ServerUrl { get; private set; }

        private Manifest? _manifest; // Threadsafe Manifest? in case of parallel reading or writing.
        private readonly object _lockManifest = new();
        public Manifest? Manifest
        {
            get
            {
                lock (_lockManifest)
                {
                    return _manifest;
                }
            }
            set
            {
                lock (_lockManifest)
                {
                    _manifest = value;
                }
            }
        }

        public ManifestService(IConfiguration configuration)
        {
            GameDirectoryPath = configuration["GameDirectory:Path"] ?? "/opt/maelstrom-launcher/files";
            DataPath = configuration["DataDirectory:Path"] ?? "/var/lib/maelstrom-launcher/";
            ServerUrl = configuration["Server:ServerUrl"] ?? "http://localhost:5000";

            _manifestFilePath = Path.Combine(DataPath, "manifest.json");

            ValidateDirectory();
        }

        protected void ValidateDirectory()
        {
            try
            {
                Directory.CreateDirectory(GameDirectoryPath);
                Directory.CreateDirectory(DataPath);
                
                if (OperatingSystem.IsLinux())
                {
                    SetLinuxPermissions(GameDirectoryPath, "755");
                    SetLinuxPermissions(DataPath, "700");
                }
            }
            catch (Exception ex)
            {
                LoggerService.Log(LogType.FILE_CHECKER, LogType.ERROR, $"Failed to create directories: {ex.Message}");
                throw;
            }
        }

        private static void SetLinuxPermissions(string path, string? permissions)
        {
            var psi = new ProcessStartInfo()
            {
                FileName = "chmod",
                Arguments = $"{permissions} \"{path}\"",
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);

            if (process == null)
            {
                LoggerService.Log(LogType.FILE_CHECKER, LogType.ERROR, $"Failed to start chmod process for {path}");
                return;
            }

            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                var error = process.StandardError.ReadToEnd();
                LoggerService.Log(LogType.FILE_CHECKER, LogType.ERROR, $"Chmod failed for {path}: {error}");
            }
        }

        public async Task<Manifest?> LoadManifestAsync()
        {
            if (!File.Exists(_manifestFilePath))
                return null;

            LoggerService.Log(LogType.MANIFEST, LogType.INFORMATION, "Trying to load file manifest...");
            try
            {
                var json = await File.ReadAllTextAsync(_manifestFilePath, Encoding.UTF8);
                var manifest = JsonSerializer.Deserialize<Manifest>(json, GetJsonOptions());

                if (string.IsNullOrWhiteSpace(json))
                {
                    LoggerService.Log(LogType.MANIFEST, LogType.ERROR, "Manifest file is empty");
                    return null;
                }

                LoggerService.Log(LogType.MANIFEST, LogType.INFORMATION, $"Manifest successfully loaded with {manifest?.Files?.Count ?? 0} files");
                return manifest;
            }
            catch (Exception ex)
            {
                LoggerService.Log(LogType.MANIFEST, LogType.ERROR, $"Failed to load file manifest: {ex.Message}");
                return null;
            }
        }

        public async Task RefreshManifestAsync()
        {
            LoggerService.Log(LogType.MANIFEST, LogType.INFORMATION, "Refreshing manifest from files...");
            await EnsureManifestExistsAsync();

            var newManifest = new Manifest();

            if (Manifest?.Version != null)
            {
                newManifest.Version = Manifest.Version; 
            }

            var files = new List<FileEntry>();
            await ScanDirectoryAsync(GameDirectoryPath, files, string.Empty);
            newManifest.Files = files;

            Manifest = newManifest;

            try
            {
                await SaveManifestExplicitlyAsync();
                LoggerService.Log(LogType.MANIFEST, LogType.INFORMATION, "Manifest refreshed successfully");
            }
            catch (Exception ex)
            {
                LoggerService.Log(LogType.MANIFEST, LogType.ERROR, $"Failed to refresh manifest: {ex.Message}");
                throw;
            }
        }

        private async Task ScanDirectoryAsync(string directoryPath, List<FileEntry> files, string relativePath)
        {
            try
            {
                var filePaths = Directory.GetFiles(directoryPath)
                    .Where(f => !f.EndsWith("manifest.json"));

                foreach (var filePath in filePaths)
                {
                    var fileName = Path.GetFileName(filePath);
                    var relativeFilePath = string.IsNullOrEmpty(relativePath)
                        ? fileName
                        : Path.Combine(relativePath, fileName);

                    LoggerService.Log(LogType.MANIFEST, LogType.DEBUG, $"Processing file: {relativeFilePath}");

                    var fileInfo = new FileInfo(filePath);
                    var hash = await FileHashService.CalculateFileHashAsync(filePath);
                    var downloadUrl = $"{ServerUrl}/{relativeFilePath.Replace("\\", "/")}";

                    var fileEntry = new FileEntry()
                    {
                        Path = relativeFilePath.Replace("/", "\\"),
                        Size = fileInfo.Length,
                        Hash = hash,
                        Url = downloadUrl
                    };

                    files.Add(fileEntry);
                }

                foreach (var subDirectory in Directory.GetDirectories(directoryPath))
                {
                    var subDirectoryName = Path.GetFileName(subDirectory);
                    var newRelativePath = string.IsNullOrEmpty(relativePath)
                        ? subDirectoryName
                        : Path.Combine(relativePath, subDirectoryName);

                    await ScanDirectoryAsync(subDirectory, files, newRelativePath);
                }
            }
            catch (Exception ex)
            {
                LoggerService.Log(LogType.MANIFEST, LogType.ERROR, $"Error scanning directory {directoryPath}: {ex.Message}");
                throw;
            }
        }

        protected async Task SaveManifestAsync()
        {
            try
            {
                await EnsureManifestExistsAsync();

                var manifestDir = Path.GetDirectoryName(_manifestFilePath);
                var json = JsonSerializer.Serialize(Manifest, GetJsonOptions());

                if (!string.IsNullOrWhiteSpace(manifestDir))
                    Directory.CreateDirectory(manifestDir);

                LoggerService.Log(LogType.MANIFEST, LogType.INFORMATION, "Saving updated file manifest...");
                await File.WriteAllTextAsync(_manifestFilePath, json, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                LoggerService.Log(LogType.MANIFEST, LogType.ERROR, $"Failed to save manifest: {ex.Message}");
                throw;
            }
        }

        //
        // We need this method for saving file directly to avoid recursion, since
        // SaveManifestAsync() has a check and a way to create manifest.
        //
        // Do not use it for anything except
        // CreateDefaultManifestAsync() and RefreshManifestAsync(). 
        //
        internal async Task SaveManifestExplicitlyAsync()
        {
            try
            {
                var manifestDir = Path.GetDirectoryName(_manifestFilePath);
                var json = JsonSerializer.Serialize(Manifest, GetJsonOptions());

                if (!string.IsNullOrWhiteSpace(manifestDir))
                    Directory.CreateDirectory(manifestDir);

                LoggerService.Log(LogType.MANIFEST, LogType.INFORMATION, "Saving manifest directly to file...");
                await File.WriteAllTextAsync(_manifestFilePath, json, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                LoggerService.Log(LogType.MANIFEST, LogType.ERROR, $"Failed to save manifest directly: {ex.Message}");
                throw;            
            }
        }

        public async Task<Manifest> CreateDefaultManifestAsync()
        {
            if (Manifest != null)
            {
                LoggerService.Log(LogType.MANIFEST, LogType.INFORMATION, "Manifest already exists, returning existing manifest");
                return Manifest;
            }

            var defaultManifest = new Manifest();
            LoggerService.Log(LogType.MANIFEST, LogType.INFORMATION, "Creating a file manifest...");

            if (Directory.Exists(GameDirectoryPath))
            {
                var files = new List<FileEntry>();
                await ScanDirectoryAsync(GameDirectoryPath, files, string.Empty);
                defaultManifest.Files = files;

                LoggerService.Log(LogType.MANIFEST, LogType.INFORMATION, $"Scanned {files.Count} files for manifest creation");
            }

            Manifest = defaultManifest;
            await SaveManifestExplicitlyAsync();

            LoggerService.Log(LogType.MANIFEST, LogType.INFORMATION, $"Manifest created with {defaultManifest.Files.Count} entries");
            return defaultManifest;
        }

        public async Task<Manifest> EnsureManifestExistsAsync()
        {
            if (Manifest != null)
            {
                LoggerService.Log(LogType.MANIFEST, LogType.INFORMATION, "Manifest already exists");
                return Manifest;
            }

            Manifest = await LoadManifestAsync();

            if (Manifest == null)
            {
                LoggerService.Log(LogType.MANIFEST, LogType.INFORMATION, "No manifest found, creating new one");
                await CreateDefaultManifestAsync();
            }

            return Manifest!;
        }

        public async Task<Manifest> GetManifestAsync()
        {
            return await EnsureManifestExistsAsync();
        }

        public static JsonSerializerOptions GetJsonOptions()
        {
            return new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                AllowTrailingCommas = true,
                PropertyNameCaseInsensitive = true,
                NumberHandling = JsonNumberHandling.AllowReadingFromString,
                Converters = { new JsonStringEnumConverter() },
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
        }
    }
}