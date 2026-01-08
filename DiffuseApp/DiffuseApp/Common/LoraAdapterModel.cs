using System.IO;
using System.Text.Json.Serialization;
using TensorStack.WPF;

namespace Diffuse.Common
{
    public class LoraAdapterModel : BaseModel
    {
        private bool _isValid;

        public int Id { get; set; }
        public string Name { get; set; }
        public string Key { get; set; }
        public string Path { get; set; }
        public string Weights { get; set; }
        public string Pipeline { get; set; }
        public string[] Triggers { get; set; }
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
            IsValid = Directory.Exists(System.IO.Path.Combine(Path, Weights)) 
                   || Directory.Exists(System.IO.Path.Combine(modelDirectory, modelId));
        }
    }
}
