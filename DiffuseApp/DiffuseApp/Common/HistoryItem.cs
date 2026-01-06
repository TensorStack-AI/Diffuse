using Diffuse.Views;
using System;
using System.Text.Json.Serialization;

namespace Diffuse.Common
{
    public class HistoryItem
    {
        public string Id { get; set; }
        public View Source { get; init; }
        public MediaType MediaType { get; init; }
        public DateTime Timestamp { get; init; }
        public string Extension { get; set; }


        [JsonIgnore]
        public string FilePath { get; set; }

        [JsonIgnore]
        public string MediaPath { get; set; }

        [JsonIgnore]
        public string ThumbPath { get; set; }
    }


    public enum MediaType
    {
        Image = 0,
        Video = 1,
        Audio = 2,
        Text = 3
    }

    public class RecentHistory : HistoryItem
    {

    }

    public class GenerateHistory : HistoryItem
    {
        public GenerateOptions Options { get; set; }
    }
}
