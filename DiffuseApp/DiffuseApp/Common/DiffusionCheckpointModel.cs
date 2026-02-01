using System.Text.Json.Serialization;
using TensorStack.WPF;

namespace Diffuse.Common
{
    public sealed class DiffusionCheckpointModel : BaseModel
    {
        private string _checkpoint;
        private string _modelCheckpoint;
        private string _vaeCheckpoint;
        private string _textEncoderCheckpoint;

        /// <summary>
        /// Gets or sets the SingleFile checkpoint.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Checkpoint
        {
            get { return _checkpoint; }
            set { SetProperty(ref _checkpoint, value == string.Empty ? null : value); }
        }

        /// <summary>
        /// Gets or sets the Transformer/Unet checkpoint.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string ModelCheckpoint
        {
            get { return _modelCheckpoint; }
            set { SetProperty(ref _modelCheckpoint, value == string.Empty ? null : value); }
        }

        /// <summary>
        /// Gets or sets the Vae checkpoint.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string VaeCheckpoint
        {
            get { return _vaeCheckpoint; }
            set { SetProperty(ref _vaeCheckpoint, value == string.Empty ? null : value); }
        }


        /// <summary>
        /// Gets or sets the TextEncoder checkpoint.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string TextEncoderCheckpoint
        {
            get { return _textEncoderCheckpoint; }
            set { SetProperty(ref _textEncoderCheckpoint, value == string.Empty ? null : value); }
        }
    }
}
