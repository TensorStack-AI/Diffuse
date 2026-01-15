namespace DiffuseApp.Common.Config
{
    public static class ServerConfig
    {
        public const int ChunkSize = 8 * 1024 * 1024; // 8 MB
        public const string Name = "DiffuseServer";
        public const string Executable = "DiffuseServer.exe";
        public const string ChannelCommand = "DiffuseApp.Command";
        public const string ChannelPipeName = "DiffuseApp.PipeName";
        public const string ChannelProgress = "DiffuseApp.Progress";
    }
}
