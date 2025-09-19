namespace MaelstromLauncher.Server.Globals
{
    public static class Settings
    {
        public static readonly string PORTAL_KEY = "CONNECTION_STRING";

        public static readonly string PORTAL = "localhost";

        public static readonly string PORTAL_SCHEME = "http://";

        public static readonly string GAME_ACCOUNT = "WoW1";

        public static readonly string DEFAULT_LAUNCH_ARGUMENTS = "";

        public static readonly Uri STATIC_SERVER_URL = new(PORTAL_SCHEME + PORTAL + ":32768");
    }
}
