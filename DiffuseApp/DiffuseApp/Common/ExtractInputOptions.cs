using TensorStack.Extractors.Common;
using TensorStack.WPF;

namespace Diffuse.Common
{
    public class ExtractInputOptions : BaseModel
    {
        private bool _isTileEnabled;

        // Default
        public bool IsTileEnabled
        {
            get { return _isTileEnabled; }
            set { SetProperty(ref _isTileEnabled, value); }
        }
        public int TileSize { get; set; }
        public int TileOverlap { get; set; }
        public bool IsInverted { get; set; }
        public bool MergeInput { get; set; }
        public bool IsTransparent { get; set; }

        // Background
        public BackgroundMode Mode { get; set; }


        // Pose
        public int Detections { get; set; }
        public float BodyConfidence { get; set; }
        public float JointConfidence { get; set; }
        public float ColorAlpha { get; set; }
        public float JointRadius { get; set; }
        public float BoneRadius { get; set; }
        public float BoneThickness { get; set; }

        public bool IsTileSupported { get; set; }
    }
}
