using MaelstromLauncher.Server.Globals;
using System.Buffers;
using System.Security.Cryptography;

namespace MaelstromLauncher.Server.Services
{
    public class FileHashService
    {
        private static readonly ArrayPool<byte> _bufferPool = ArrayPool<byte>.Shared;
        private const short BufferSize = 8192;

        private static readonly SemaphoreSlim _semaphore = new(Environment.ProcessorCount * 2);

        public static async Task<string> CalculateFileHashAsync(string path)
        {
            await _semaphore.WaitAsync();

            try
            {
                FileInfo file = new(path);
                long fileLength = file.Length;

                if (File.Exists(path) && file.Length >= BufferSize)
                {
                    var buffer = _bufferPool.Rent(BufferSize);

                    try
                    {
                        using var sha256 = SHA256.Create();
                        await using var stream = File.OpenRead(path);

                        int bytesRead;
                        while ((bytesRead = await stream.ReadAsync(buffer)) > 0)
                        {
                            sha256.TransformBlock(buffer, 0, bytesRead, null, 0);
                        }

                        sha256.TransformFinalBlock([], 0, 0);
                        var hash = sha256.Hash ?? [];
                        var convertedHash = Convert.ToHexString(hash).ToLowerInvariant();

                        LoggerService.Log(LogType.FILE_HASH, LogType.INFORMATION, $"Successfully hashed: {path} with hash {convertedHash}");
                        return convertedHash;
                    }
                    finally
                    {
                        _bufferPool.Return(buffer, clearArray: true);
                    }
                }
                else
                {
                    using var sha256 = SHA256.Create();
                    await using var stream = File.OpenRead(path);

                    var hash = await sha256.ComputeHashAsync(stream);
                    var convertedHash = Convert.ToHexString(hash).ToLowerInvariant();

                    LoggerService.Log(LogType.FILE_HASH, LogType.INFORMATION, $"Successfully hashed: {path} with hash {convertedHash}");
                    return convertedHash;
                }
            }
            catch (Exception ex)
            {
                LoggerService.Log(LogType.FILE_HASH, LogType.ERROR, $"Failed to hash {path}: {ex.Message}");
                throw;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public static async Task<bool> VerifyFileHashAsync(string path, string correctHash)
        {
            try
            {
                var actualHash = await CalculateFileHashAsync(path);
                LoggerService.Log(LogType.FILE_HASH, LogType.INFORMATION, $"Successfully verified hash for: {path}");
                return string.Equals(actualHash, correctHash);
            }
            catch (Exception ex)
            {
                LoggerService.Log(LogType.FILE_HASH, LogType.ERROR, $"Failed to verify hash for {path}: {ex.Message}");
                return false;
            }
        }
    }
}
