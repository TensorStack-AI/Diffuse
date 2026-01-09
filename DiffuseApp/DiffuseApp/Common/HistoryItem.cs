using Diffuse.Views;
using System;
using System.Runtime.CompilerServices;
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
        string Model { get; init; }
        int Width { get; init; }
        int Height { get; init; }
        float FrameRate { get; init; }
        int FrameCount { get; init; }

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
        public string Model { get; init; }
        public int Width { get; init; }
        public int Height { get; init; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public float FrameRate { get; init; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int FrameCount { get; init; }


        [JsonIgnore]
        public string FilePath { get; set; }

        [JsonIgnore]
        public string MediaPath { get; set; }

        [JsonIgnore]
        public string ThumbPath { get; set; }

        public virtual bool Equals(RecentHistory other) => ReferenceEquals(this, other);
        public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);
    }
}
