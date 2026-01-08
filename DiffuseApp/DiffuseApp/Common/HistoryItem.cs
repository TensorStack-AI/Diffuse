using Diffuse.Views;
using System;
using System.Text.Json.Serialization;

namespace Diffuse.Common
{
    public interface IHistoryItem
    {
        int Version { get; init; }
        string Id { get; init; }
        View Source { get; init; }
        MediaType MediaType { get; init; }
        DateTime Timestamp { get; init; }
        string Extension { get; init; }
        int Width { get; init; }
        int Height { get; init; }

        string FilePath { get; set; }
        string MediaPath { get; set; }
        string ThumbPath { get; set; }
    }


    public record RecentHistory : IHistoryItem
    {
        public int Version { get; init; }
        public string Id { get; init; }

        public View Source { get; init; }
        public MediaType MediaType { get; init; }
        public DateTime Timestamp { get; init; }
        public string Extension { get; init; }
        public int Width { get; init; }
        public int Height { get; init; }

        [JsonIgnore]
        public string FilePath { get; set; }

        [JsonIgnore]
        public string MediaPath { get; set; }

        [JsonIgnore]
        public string ThumbPath { get; set; }
    }
}
