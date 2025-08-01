using System.IO;

namespace MaelstromLauncher.Server.Services
{
    public static class LoggerService
    {
        private static readonly string logPath = "server_log.txt";

        public static void Log(string category, string subCategory, string message)
        {
            string formattedMessage = $"[{DateTime.Now:dd/MM/yyyy HH:mm:ss}] [{category}] [{subCategory}] {message}";
            File.AppendAllText(logPath, formattedMessage + Environment.NewLine);
        }
    }
}
