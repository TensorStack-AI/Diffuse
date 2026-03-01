namespace DiffuseApp.Common.Config
{
    public class ChannelConfig
    {
        public int ChunkSize { get; } = 32 * 1024 * 1024; // 32 MB
        public string Name { get; init; }
        public string Executable { get; init; }
        public string ChannelCommand { get; init; }
        public string ChannelPipeName { get; init; }
        public string ChannelProgress { get; init; }


        public readonly static ChannelConfig PipelineConfig = new ChannelConfig
        {
            Name = "DiffuseServer",
            Executable = "DiffuseServer.exe",
            ChannelCommand = "DiffuseApp.Command",
            ChannelPipeName = "DiffuseApp.PipeName",
            ChannelProgress = "DiffuseApp.Progress"
        };


        public readonly static ChannelConfig DownloadConfig = new ChannelConfig
        {
            Name = "DiffuseDownload",
            Executable = "DiffuseDownloader.exe",
            ChannelCommand = "DiffuseDownload.Command",
            ChannelPipeName = "DiffuseDownload.PipeName",
            ChannelProgress = "DiffuseDownload.Progress"
        };
    }
}
