using MaelstromLauncher.Server.Models;

namespace MaelstromLauncher.Server.DTOs
{
    /// <summary>
    /// Data transfer object for returning full manifest to a client.
    /// </summary>
    public class ManifestDto
    {
        public string Version { get; set; }
        public DateTime GeneratedAt { get; set; }
        public List<FileEntry> Files { get; set; }
    }

    /// <summary>
    /// Data transfer object for returning single file from a manifest for partial downloads.
    /// </summary>
    public class FileEntryDto
    {
        public string Path { get; set; }
        public long Size { get; set; }
        public string Hash { get; set; }
        public string Url { get; set; }
    }

    /// <summary>
    /// Data transfer object for returning only manifest metadata, so we don't have to send client full manifest.
    /// </summary>
    public class ManifestInfoDto
    {
        public string Version { get; set; }
        public DateTime GeneratedAt { get; set; }
    }
}
