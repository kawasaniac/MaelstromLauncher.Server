namespace MaelstromLauncher.Server.DTOs
{
    /// <summary>
    /// Data transfer object for returning single file from a manifest for partial downloads.
    /// </summary>
    public class FileEntryDto
    {
        public required string Path { get; set; }
        public long Size { get; set; }
        public required string Hash { get; set; }
        public required string Url { get; set; }
    }
}
