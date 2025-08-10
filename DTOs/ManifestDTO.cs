using MaelstromLauncher.Server.Models;

namespace MaelstromLauncher.Server.DTOs
{
    /// <summary>
    /// Data transfer object for returning full manifest to a client.
    /// </summary>
    public class ManifestDto
    {
        public required string Version { get; set; }
        public DateTime GeneratedAt { get; set; }
        public required List<FileEntry> Files { get; set; }
    }

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

    /// <summary>
    /// Data transfer object for returning only manifest metadata, so we don't have to send client full manifest.
    /// </summary>
    public class ManifestInfoDto
    {
        public required string Version { get; set; }
        public DateTime GeneratedAt { get; set; }
    }
}
