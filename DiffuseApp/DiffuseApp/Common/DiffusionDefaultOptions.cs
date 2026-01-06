using TensorStack.Python.Common;

namespace Diffuse.Common
{
    public record DiffusionDefaultOptions
    {
        public float GuidanceScale { get; set; } = 1;
        public float GuidanceScale2 { get; set; } = 1;
        public int Steps { get; set; } = 50;
        public int Steps2 { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public int Frames { get; set; }
        public float FrameRate { get; set; } = 16;
        public float Shift { get; set; } = 1;
        public SchedulerType Scheduler { get; set; }
        public SchedulerType[] Schedulers { get; set; }
    }
}
