using MaelstromLauncher.Server.Globals;
using MaelstromLauncher.Server.Models;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MaelstromLauncher.Server.Services
{
    public class ManifestService
    {
        private readonly string _manifestFilePath;
        private readonly FileHashService _fileHashService;

        public string GameDirectoryPath { get; private set; }
        public string DataPath { get; private set; }
        public string ServerUrl { get; private set; }
        public Manifest? Manifest { get; protected set; }

        public ManifestService(IConfiguration configuration, FileHashService fileHashService)
        {
            _fileHashService = fileHashService;

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
                var error = process.StandardOutput.ReadToEnd();
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
            Manifest ??= await LoadManifestAsync();

            var newManifest = new Manifest();

            if (Manifest?.Version != null)
            {
                newManifest.Version = Manifest.Version; 
            }

            //TODO: Should scan the directory for new files somewhere to refresh the manifest with new data

            Manifest = newManifest;

            try
            {
                await SaveManifestAsync();
                LoggerService.Log(LogType.MANIFEST, LogType.INFORMATION, "Manifest refreshed successfully");
            }
            catch (Exception ex)
            {
                LoggerService.Log(LogType.MANIFEST, LogType.ERROR, $"Failed to refresh manifest: {ex.Message}");
                throw;
            }
        }

        protected async Task SaveManifestAsync()
        {
            try
            {
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

        public async Task<Manifest> CreateDefaultManifestAsync()
        {
            var defaultManifest = new Manifest();
            LoggerService.Log(LogType.MANIFEST, LogType.INFORMATION, "Creating a file manifest...");

            if (Directory.Exists(GameDirectoryPath))
            {
                var files = Directory.GetFiles(GameDirectoryPath, "*", SearchOption.AllDirectories);

                foreach (var filePath in files)
                {
                    try
                    {
                        var fileInfo = new FileInfo(filePath);
                        var relativePath = Path.GetRelativePath(GameDirectoryPath, filePath);
                        var hash = await FileHashService.CalculateFileHashAsync(filePath);

                        var fileEntry = new FileEntry()
                        {
                            Path = relativePath.Replace("/", "\\"),
                            Size = fileInfo.Length,
                            Hash = hash,
                            Url = $"ServerURL" // TODO: Add server URLs
                        };

                        defaultManifest.Files.Add(fileEntry);
                        LoggerService.Log(LogType.MANIFEST, LogType.INFORMATION, $"Created manifest entry for file {filePath} with size equal to {fileInfo.Length} bytes");
                    }
                    catch (Exception ex)
                    {
                        LoggerService.Log(LogType.MANIFEST, LogType.ERROR, $"Failed to process file {filePath}: {ex.Message}");
                    }
                }
            }

            Manifest = defaultManifest;
            await SaveManifestAsync();

            LoggerService.Log(LogType.MANIFEST, LogType.INFORMATION, $"Manifest created with {defaultManifest.Files.Count} entries");
            return defaultManifest;
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