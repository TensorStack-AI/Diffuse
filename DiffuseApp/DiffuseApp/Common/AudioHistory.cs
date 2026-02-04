using Diffuse.Views;
using System;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace Diffuse.Common
{
    public record AudioHistory : IHistoryItem
    {
        public int Version { get; init; }
        public string Id { get; init; }
        public View Source { get; init; }
        public MediaType MediaType { get; init; }
        public DateTime Timestamp { get; init; }
        public string Extension { get; init; }
        public string Model { get; init; }



        public int Channels { get; init; }
        public int SampleRate { get; init; }
        public TimeSpan Duration { get; init; }


        public AudioInputOptions Options { get; init; }

        [JsonIgnore]
        public string FilePath { get; set; }

        [JsonIgnore]
        public string MediaPath { get; set; }

        [JsonIgnore]
        public string ThumbPath { get; set; }




        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int Width { get; init; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int Height { get; init; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public float FrameRate { get; init; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int FrameCount { get; init; }
        public virtual bool Equals(UpscaleHistory other) => ReferenceEquals(this, other);
        public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);
    }
}
