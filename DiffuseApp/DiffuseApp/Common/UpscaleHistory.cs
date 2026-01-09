using Diffuse.Views;
using System;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace Diffuse.Common
{
    public record UpscaleHistory : IHistoryItem
    {
        public int Version { get; init; }
        public string Id { get; init; }
        public View Source { get; init; }
        public MediaType MediaType { get; init; }
        public DateTime Timestamp { get; init; }
        public string Extension { get; init; }

        public string Model { get; init; }

        public int Width { get; init; }
        public int Height { get; init; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public float FrameRate { get; init; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int FrameCount { get; init; }

        public int ScaleFactor { get; init; }
        public int OriginalWidth { get; init; }
        public int OriginalHeight { get; init; }
      
        public UpscaleInputOptions Options { get; init; }

      
        [JsonIgnore]
        public string FilePath { get; set; }

        [JsonIgnore]
        public string MediaPath { get; set; }

        [JsonIgnore]
        public string ThumbPath { get; set; }

        public virtual bool Equals(UpscaleHistory other) => ReferenceEquals(this, other);
        public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);
    }
}
