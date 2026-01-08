using TensorStack.Common;
using TensorStack.Extractors.Common;
using TensorStack.WPF;

namespace Diffuse.Common
{
    public record ExtractInputOptions : BaseRecord
    {
        // Default
        public TileMode TileMode { get; set; }
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
    }
}
