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
        public required List<FileEntryDto> Files { get; set; }
    }
}
