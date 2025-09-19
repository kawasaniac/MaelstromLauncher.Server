namespace MaelstromLauncher.Server.Helpers
{
    public class ServerUrlProvider(IHttpContextAccessor httpContextAccessor) : IServerUrlProvider
    {
        public string GetServerUrl()
        {
            var request = httpContextAccessor.HttpContext?.Request;

            if (request == null)
                return "http://localhost:32768"; // fallback for background tasks

            return $"{request.Scheme}://{request.Host}";
        }
    }
}
