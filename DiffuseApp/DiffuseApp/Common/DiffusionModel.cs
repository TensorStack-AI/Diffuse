using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;
using TensorStack.Common.Common;
using TensorStack.Python.Common;
using TensorStack.WPF;

namespace Diffuse.Common
{
    public class DiffusionModel : BaseModel
    {
        private bool _isValid;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int Id { get; set; }
        public BackendType Backend { get; set; }
        public string Name { get; set; }
        public string Pipeline { get; set; }
        public string Path { get; set; }
        public string Variant { get; set; }
        public ModelSourceType Source { get; set; }
        public bool IsDefault { get; set; }
        public bool IsGated { get; set; }
        public string Link { get; set; }
        public MemoryProfile[] MemoryProfile { get; set; }
        public DataType BaseType { get; set; }
        public ProcessType[] ProcessTypes { get; set; }
        public List<SizeOption> Resolutions { get; set; }
        public DiffusionDefaultOptions DefaultOptions { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public DiffusionCheckpointModel Checkpoint { get; set; }


        [JsonIgnore]
        public bool IsValid
        {
            get { return _isValid; }
            private set { SetProperty(ref _isValid, value); }
        }


        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public MemoryMode? UserMemoryMode { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public DataType? UserDataType { get; set; }


        public void Initialize(string modelDirectory)
        {
            if (Source == ModelSourceType.Folder)
                IsValid = Directory.Exists(Path);
            else if (Source == ModelSourceType.HuggingFace)
                IsValid = Directory.Exists(System.IO.Path.Combine(modelDirectory, Utils.GetHuggingFaceCacheId(Path)));
            else if (Source == ModelSourceType.SingleFile)
            {
                IsValid = Checkpoint is not null && Utils.IsCheckpointInstalled(modelDirectory, Checkpoint.Checkpoint);
            }
            else if (Source == ModelSourceType.Checkpoint)
            {
                IsValid = Checkpoint is not null
                    && Utils.TryParseHuggingFaceRepo(Path, out _)
                    && (string.IsNullOrEmpty(Checkpoint.VaeCheckpoint) || Utils.IsCheckpointInstalled(modelDirectory, Checkpoint.VaeCheckpoint))
                    && (string.IsNullOrEmpty(Checkpoint.ModelCheckpoint) || Utils.IsCheckpointInstalled(modelDirectory, Checkpoint.ModelCheckpoint))
                    && (string.IsNullOrEmpty(Checkpoint.TextEncoderCheckpoint) || Utils.IsCheckpointInstalled(modelDirectory, Checkpoint.TextEncoderCheckpoint));
            }
        }

    }

    public sealed class MemoryProfile : BaseModel
    {
        public MemoryProfile() { }
        public MemoryProfile(DataType dataType, int[] memoryModes)
        {
            DataType = dataType;
            MemoryModes = memoryModes;
        }

        public DataType DataType { get; set; }
        public int[] MemoryModes { get; set; }
    }
}
