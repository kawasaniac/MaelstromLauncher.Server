using System.ComponentModel.DataAnnotations;

namespace MaelstromLauncher.Server.Models
{
    public class Manifest
    {
        [Required]
        public string Version { get; set; } = "1.0";
        [Required]
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
        [Required]
        public List<FileEntry> Files { get; set; } = [];
    }
    public class FileEntry
    {
        [Required]
        public string Path { get; set; } = "";
        [Range(0, long.MaxValue)]
        public long Size { get; set; }
        [Required]
        public string Hash { get; set; } = "";
        [Required]
        [Url]
        public string Url { get; set; } = "";
    }
}
