namespace MaelstromLauncher.Server.Models
{
    public class Manifest
    {
        public string Version { get; set; } = "1.0";
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
        public List<FileEntry> Files { get; set; } = [];
    }
    public class FileEntry
    {
        public string Path { get; set; } = "";
        public long Size { get; set; }
        public string Hash { get; set; } = "";
        public string Url { get; set; } = "";
    }
}
