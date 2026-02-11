using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TensorStack.Common.Common;
using TensorStack.WPF;
using TensorStack.WPF.Services;

namespace Diffuse.Common
{
    public class AudioModel : BaseModel
    {
        private bool _isValid;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int Id { get; set; }
        public BackendType Backend { get; set; }
        public string Name { get; set; }
        public bool IsDefault { get; set; }
        public bool IsGated { get; set; }
        public string Link { get; set; }
        public AudioModelType Type { get; set; }
        public string Version { get; set; }
        public int MinLength { get; set; }
        public int MaxLength { get; set; }
        public string[] Prefixes { get; set; }
        public string[] UrlPaths { get; set; }

        [JsonIgnore]
        public string Path { get; set; }

        [JsonIgnore]
        public bool IsValid
        {
            get { return _isValid; }
            private set { SetProperty(ref _isValid, value); }
        }


        public void Initialize(string modelDirectory)
        {
            var directory = System.IO.Path.Combine(modelDirectory, Name);
            var modelFiles = FileHelper.GetUrlFileMapping(UrlPaths, directory);
            if (modelFiles.Values.All(File.Exists))
            {
                IsValid = true;
                Path = directory;
            }
        }


        public async Task<bool> DownloadAsync(string modelDirectory)
        {
            var directory = System.IO.Path.Combine(modelDirectory, Name);
            if (await DialogService.DownloadAsync($"Download '{Name}' model?", UrlPaths, directory))
                Initialize(modelDirectory);

            return IsValid;
        }
    }


    public enum AudioModelType
    {
        Whisper = 0,
        Supertonic = 10
    }
}
