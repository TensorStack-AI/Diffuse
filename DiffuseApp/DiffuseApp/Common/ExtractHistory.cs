using Diffuse.Views;
using System;
using System.Text.Json.Serialization;

namespace Diffuse.Common
{
    public record ExtractHistory : IHistoryItem
    {
        public int Version { get; init; }
        public string Id { get; init; }
        public View Source { get; init; }
        public MediaType MediaType { get; init; }
        public DateTime Timestamp { get; init; }
        public string Extension { get; init; }

        public int Width { get; init; }
        public int Height { get; init; }
        public string Model { get; init; }
        public ExtractInputOptions Options { get; init; }


        [JsonIgnore]
        public string FilePath { get; set; }

        [JsonIgnore]
        public string MediaPath { get; set; }

        [JsonIgnore]
        public string ThumbPath { get; set; }
    }
}
