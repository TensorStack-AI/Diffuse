using System.IO;
using System.Text.Json.Serialization;
using TensorStack.WPF;

namespace Diffuse.Common
{
    public class LoraAdapterModel : BaseModel
    {
        private bool _isValid;
        private string _weights;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Key { get; set; }
        public string Path { get; set; }
        public string Weights
        {
            get { return _weights; }
            set { SetProperty(ref _weights, value); }
        }
        public string Pipeline { get; set; }
        public ModelSourceType Source { get; set; }
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
            if (Source == ModelSourceType.Folder)
                IsValid = Directory.Exists(Path);
            else if (Source == ModelSourceType.SingleFile)
                IsValid = File.Exists(Path);
            else if (Source == ModelSourceType.HuggingFace)
                IsValid = Directory.Exists(System.IO.Path.Combine(modelDirectory, Utils.GetHuggingFaceCacheId(Path)));
        }
    }
}
