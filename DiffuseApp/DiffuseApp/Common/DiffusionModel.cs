using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;
using TensorStack.Python.Common;
using TensorStack.WPF;

namespace Diffuse.Common
{
    public class DiffusionModel : BaseModel
    {
        private bool _isValid;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Pipeline { get; set; }
        public string Path { get; set; }
        public ModelSourceType Source { get; set; }
        public bool IsDefault { get; set; }
        public int[] MemoryModes { get; set; }
        public DataType[] DataTypes { get; set; }
        public ProcessType[] ProcessTypes { get; set; }
        public List<SizeOption> Resolutions { get; set; }
        public DiffusionDefaultOptions DefaultOptions { get; set; }


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
