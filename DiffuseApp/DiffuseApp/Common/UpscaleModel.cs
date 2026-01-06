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

    public class UpscaleModel : BaseModel
    {
        private bool _isValid;

        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsDefault { get; set; }
        public int Channels { get; set; } = 3;
        public int SampleSize { get; set; }
        public int ScaleFactor { get; set; } = 1;
        public Normalization Normalization { get; set; } = Normalization.ZeroToOne;
        public Normalization OutputNormalization { get; set; } = Normalization.OneToOne;
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
}
