using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TensorStack.Common;
using TensorStack.Common.Common;
using TensorStack.WPF;
using TensorStack.WPF.Services;

namespace Diffuse.Common
{
    public class ExtractorModel : BaseModel
    {
        private bool _isValid;

        public int Id { get; init; }
        public string Name { get; set; }
        public bool IsDefault { get; set; }
        public ExtractorType Type { get; set; }
        public int Channels { get; set; }
        public int SampleSize { get; set; }
        public Normalization Normalization { get; set; } = Normalization.ZeroToOne;
        public Normalization OutputNormalization { get; set; } = Normalization.OneToOne;
        public int OutputChannels { get; set; }
        public bool IsDynamicOutput { get; set; }
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
                Path = modelFiles.Values.First(x => x.EndsWith(".onnx"));
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

    public enum ExtractorType
    {
        Default = 0,
        Background = 1,
        Pose = 2
    }
}
