using System.Text.Json.Serialization;
using TensorStack.Python.Common;
using TensorStack.WPF;

namespace Diffuse.Common
{
    public record SchedulerInputOptions : BaseRecord
    {
        private float _shift = 1.0f;
        private bool _useDynamicShifting = false;
        private bool _stochasticSampling = false;

        public int NumTrainTimesteps { get; set; } = 1000;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int StepsOffset { get; set; } = 0;

        // IsTimestep
        public float BetaStart { get; set; } = 0.00085f;
        public float BetaEnd { get; set; } = 0.012f;
        public BetaScheduleType BetaSchedule { get; set; } = BetaScheduleType.ScaledLinear;
        public PredictionType PredictionType { get; set; } = PredictionType.Epsilon;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public VarianceType? VarianceType { get; set; }

        public TimestepSpacingType TimestepSpacing { get; set; } = TimestepSpacingType.Linspace;


        // IsClipSample
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool ClipSample { get; set; } = false;
        public float ClipSampleRange { get; set; } = 1.0f;

        //IsThreshold
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool Thresholding { get; set; } = false;
        public float DynamicThresholdingRatio { get; set; } = 0.995f;
        public float SampleMaxValue { get; set; } = 1.0f;

        // IsKarras
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool UseKarrasSigmas { get; set; } = false;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public float SigmaMin { get; set; } // 0 = null

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public float SigmaMax { get; set; } // 0 = null
        public float Rho { get; set; } = 7.0f;

        // IsMultiStep
        public int SolverOrder { get; set; } = 2; //  Usually 1–3
        public SolverType SolverType { get; set; } = SolverType.Midpoint;
        public AlgorithmType AlgorithmType { get; set; } = AlgorithmType.DPMSolverPlus;
        public bool LowerOrderFinal { get; set; } = true;

        // IsStochastic
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool StochasticSampling
        {
            get { return _stochasticSampling; }
            set { SetProperty(ref _stochasticSampling, value); }
        }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public float Eta { get; set; } = 0.0f;
        public float SNoise { get; set; } = 1.0f;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public float SChurn { get; set; } = 0.0f;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public float STmin { get; set; } = 0.0f;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public float STmax { get; set; } = 0.0f; // 0 = float.PositiveInfinity;

        // IsFlowMatch
        public float Shift
        {
            get { return _shift; }
            set { SetProperty(ref _shift, value); }
        }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool UseDynamicShifting
        {
            get { return _useDynamicShifting; }
            set { SetProperty(ref _useDynamicShifting, value); }
        }
        public float BaseShift { get; set; } = 0.5f;
        public float MaxShift { get; set; } = 1.15f;

        [JsonIgnore]
        public float FlowShift => Shift;

        public int BaseImageSeqLen { get; set; }
        public int MaxImageSeqLen { get; set; }

    }
}
