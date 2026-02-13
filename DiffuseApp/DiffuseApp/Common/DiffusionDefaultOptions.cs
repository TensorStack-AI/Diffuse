using System.Runtime.CompilerServices;
using TensorStack.Python.Common;

namespace Diffuse.Common
{
    public record DiffusionDefaultOptions
    {
        public float GuidanceScale { get; set; } = 0;
        public float GuidanceScale2 { get; set; } = 0;
        public int Steps { get; set; } = 50;
        public int Steps2 { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public int Frames { get; set; }
        public float FrameRate { get; set; } = 16;
        public int FrameChunk { get; set; }
        public int FrameChunkOverlap { get; set; }
        public int NoiseCondition { get; set; }

        public SchedulerType Scheduler { get; set; }
        public SchedulerType[] Schedulers { get; set; }


        public float BetaStart { get; set; } = 0.00085f;
        public float BetaEnd { get; set; } = 0.012f;
        public BetaScheduleType BetaSchedule { get; set; } = BetaScheduleType.ScaledLinear;
        public TimestepSpacingType TimestepSpacing { get; set; } = TimestepSpacingType.Linspace;
        public PredictionType PredictionType { get; set; } = PredictionType.Epsilon;
        public SolverType SolverType { get; set; }
        public int StepsOffset { get; set; }
        public float Shift { get; set; } = 1;
        public float BaseShift { get; set; } = 1.15f;
        public float MaxShift { get; set; } = 0.5f;
        public int BaseImageSeqLen { get; set; } = 256;
        public int MaxImageSeqLen { get; set; } = 4096;
        public bool UseDynamicShifting { get; set; }
        public int SampleRate { get; set; } = 24000;
        public int FramesMin { get; set; }
        public int FramesMax { get; set; }
        public bool IsStochasticSampling { get; set; }

        public virtual bool Equals(DiffusionDefaultOptions other) => ReferenceEquals(this, other);
        public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);
    }
}
