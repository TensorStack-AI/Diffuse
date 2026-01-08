using System.IO;
using System.Text.Json.Serialization;
using TensorStack.WPF;

namespace Diffuse.Common
{
    public class ControlNetModel : BaseModel
    {
        private bool _isValid;

        public int Id { get; init; }
        public string Name { get; init; }
        public string Path { get; init; }
        public string Pipeline { get; init; }
        public bool IsDefault { get; set; }


        [JsonIgnore]
        public bool IsValid
        {
            get { return _isValid; }
            private set { SetProperty(ref _isValid, value); }
        }

        public void Initialize(string modelDirectory)
        {
            var modelId = $"models--{Path.Replace("/", "--")}";
            IsValid = Directory.Exists(Path)
                   || Directory.Exists(System.IO.Path.Combine(modelDirectory, modelId));
        }
    }
}
