using Diffuse;
using Diffuse.Common;
using Diffuse.Views;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using TensorStack.Common.Common;
using TensorStack.Image;
using TensorStack.Video;

namespace Diffuse.Services
{
    public class HistoryService : IHistoryService
    {
        private readonly Settings _settings;
        private readonly ObservableCollection<HistoryItem> _historyCollection;

        /// <summary>
        /// Initializes a new instance of the <see cref="HistoryService"/> class.
        /// </summary>
        /// <param name="settings">The settings.</param>
        public HistoryService(Settings settings)
        {
            _settings = settings;
            _historyCollection = [];
        }

        /// <summary>
        /// Gets the history collection.
        /// </summary>
        public ObservableCollection<HistoryItem> HistoryCollection => _historyCollection;


        public async Task InitializeAsync()
        {
            var historyFiles = Directory.EnumerateFiles(_settings.DirectoryHistory, "*.json", SearchOption.TopDirectoryOnly)
                .Select(x => new FileInfo(x))
                .OrderByDescending(x => x.CreationTimeUtc)
                .Take(_settings.MaxHistory)
                .ToList();
            foreach (var historyFile in historyFiles)
            {
                var historyItem = default(HistoryItem);
                if (historyFile.Name.StartsWith("Recent_"))
                    historyItem = await Json.LoadAsync<RecentHistory>(historyFile.FullName);
                if (historyFile.Name.StartsWith("GenerateImage_"))
                    historyItem = await Json.LoadAsync<GenerateHistory>(historyFile.FullName);
                if (historyFile.Name.StartsWith("GenerateVideo_"))
                    historyItem = await Json.LoadAsync<GenerateHistory>(historyFile.FullName);
                if (historyItem == null)
                    continue;

                historyItem.FilePath = historyFile.FullName;
                historyItem.MediaPath = Path.Combine(historyFile.DirectoryName, historyFile.Name.Replace(".json", $".{historyItem.Extension}"));
                historyItem.ThumbPath = Path.Combine(historyFile.DirectoryName, historyFile.Name.Replace(".json", ".png"));
                if (!File.Exists(historyItem.MediaPath))
                    continue;

                _historyCollection.Add(historyItem);
            }
        }


        public Task DeleteAsync(HistoryItem historyItem)
        {
            _historyCollection.Remove(historyItem);
            FileHelper.DeleteFiles(historyItem.FilePath, historyItem.MediaPath, historyItem.ThumbPath);
            return Task.CompletedTask;
        }


        public async Task AddRecentAsync(ImageInput image)
        {
            if (_settings.MaxHistory <= 0)
                return;

            var key = GetRandomName();
            var history = new RecentHistory
            {
                Id = key,
                Extension = "png",
                MediaType = MediaType.Image,
                Timestamp = DateTime.Now,
                Source = View.History,
                FilePath = Path.Combine(_settings.DirectoryHistory, $"Recent_{key}.json"),
                MediaPath = image.SourceFile,
            };

            await Json.SaveAsync(history.FilePath, history);
            AddHistoryItem(history);
        }


        public async Task AddRecentAsync(VideoInputStream videoStream)
        {
            if (_settings.MaxHistory <= 0)
                return;

            var key = GetRandomName();
            var history = new RecentHistory
            {
                Id = key,
                Extension = "mp4",
                MediaType = MediaType.Video,
                Timestamp = DateTime.Now,
                Source = View.History,
                FilePath = Path.Combine(_settings.DirectoryHistory, $"Recent_{key}.json"),
                MediaPath = videoStream.SourceFile,
                ThumbPath = Path.Combine(_settings.DirectoryHistory, $"Recent_{key}.png")
            };

            await videoStream.Thumbnail.SaveAsync(history.ThumbPath);
            await Json.SaveAsync(history.FilePath, history);
            AddHistoryItem(history);
        }


        public async Task<ImageInput> AddAsync(ImageInput image, View source, GenerateOptions options)
        {
            if (_settings.MaxHistory <= 0)
                return image;

            var key = GetRandomName();
            var history = new GenerateHistory
            {
                Id = key,
                Extension = "png",
                MediaType = MediaType.Image,
                Timestamp = DateTime.Now,
                Source = source,
                FilePath = Path.Combine(_settings.DirectoryHistory, $"GenerateImage_{key}.json"),
                MediaPath = Path.Combine(_settings.DirectoryHistory, $"GenerateImage_{key}.png"),
                ThumbPath = Path.Combine(_settings.DirectoryHistory, $"GenerateImage_{key}.png"),
                Options = options,
            };

            await image.SaveAsync(history.MediaPath);
            await Json.SaveAsync(history.FilePath, history);
            AddHistoryItem(history);
            return image;
        }


        public async Task<VideoInputStream> AddAsync(VideoInputStream videoStream, View source, GenerateOptions options)
        {
            if (_settings.MaxHistory <= 0)
                return videoStream;

            var key = GetRandomName();
            var history = new GenerateHistory
            {
                Id = key,
                Extension = "mp4",
                MediaType = MediaType.Video,
                Timestamp = DateTime.Now,
                Source = source,
                FilePath = Path.Combine(_settings.DirectoryHistory, $"GenerateVideo_{key}.json"),
                MediaPath = Path.Combine(_settings.DirectoryHistory, $"GenerateVideo_{key}.mp4"),
                ThumbPath = Path.Combine(_settings.DirectoryHistory, $"GenerateVideo_{key}.png"),
                Options = options
            };

            var newStream = await videoStream.MoveAsync(history.MediaPath);
            await videoStream.Thumbnail.SaveAsync(history.ThumbPath);
            await Json.SaveAsync(history.FilePath, history);
            AddHistoryItem(history);
            return newStream;
        }


        private string GetRandomName()
        {
            return Path.GetFileNameWithoutExtension(Path.GetRandomFileName());
        }


        private void AddHistoryItem(HistoryItem historyItem)
        {
            while (_historyCollection.Count > Math.Max(0, _settings.MaxHistory))
            {
                _historyCollection.RemoveAt(_historyCollection.Count - 1);
            }
            _historyCollection.Add(historyItem);
        }

    }


    public interface IHistoryService
    {
        ObservableCollection<HistoryItem> HistoryCollection { get; }

        Task InitializeAsync();
        Task<ImageInput> AddAsync(ImageInput image, View source, GenerateOptions options);
        Task<VideoInputStream> AddAsync(VideoInputStream videoStream, View source, GenerateOptions options);
        Task AddRecentAsync(ImageInput image);
        Task AddRecentAsync(VideoInputStream videoStream);
        Task DeleteAsync(HistoryItem historyItem);
    }

}
