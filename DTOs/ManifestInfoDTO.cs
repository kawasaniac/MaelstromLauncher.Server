namespace MaelstromLauncher.Server.DTOs
{
    /// <summary>
    /// Data transfer object for returning only manifest metadata, so we don't have to send client full manifest.
    /// </summary>
    public class ManifestInfoDto
    {
        public required string Version { get; set; }
        public DateTime GeneratedAt { get; set; }
    }
}
}
