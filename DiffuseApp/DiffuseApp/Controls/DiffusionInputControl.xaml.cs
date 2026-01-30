using Diffuse.Common;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using TensorStack.Python.Common;
using TensorStack.WPF;
using TensorStack.WPF.Controls;

namespace Diffuse.Controls
{
    /// <summary>
    /// Interaction logic for DiffusionInputControl.xaml
    /// </summary>
    public partial class DiffusionInputControl : BaseControl
    {
        private bool _isResolutionEnabled = true;
        private SizeOption _selectedResolution;
        private bool _isImageInputEnabled;
        private bool _isControlNetEnabled;
        private bool _isModelOptionsVisible;
        private bool _isSchedulerKarras;
        private bool _isSchedulerFlowMatch;
        private bool _isSchedulerMultiStep;
        private bool _isSchedulerTimeStep;
        private bool _isSchedulerStochastic;
        private bool _isSchedulerClipSample;
        private bool _isSchedulerThresholding;
        private bool _isSteps2Enabled;
        private bool _isGuidance2Enabled;

        public DiffusionInputControl()
        {
            SeedCommand = new RelayCommand<bool>(GenerateSeed);
            AddTriggerWordCommand = new AsyncRelayCommand<string>(AddTriggerWordAsync);
            InitializeComponent();
        }

        public static readonly DependencyProperty PipelineProperty = DependencyProperty.Register(nameof(Pipeline), typeof(PipelineModel), typeof(DiffusionInputControl), new PropertyMetadata<DiffusionInputControl, PipelineModel>((c, o, n) => c.OnPipelineChanged(o, n)));
        public static readonly DependencyProperty OptionsProperty = DependencyProperty.Register(nameof(Options), typeof(DiffusionInputOptions), typeof(DiffusionInputControl));
        public static readonly DependencyProperty UpscaleOptionsProperty = DependencyProperty.Register(nameof(UpscaleOptions), typeof(UpscaleInputOptions), typeof(DiffusionInputControl));
        public static readonly DependencyProperty ExtractOptionsProperty = DependencyProperty.Register(nameof(ExtractOptions), typeof(ExtractInputOptions), typeof(DiffusionInputControl));

        public ProcessType ProcessType { get; set; }
        public RelayCommand<bool> SeedCommand { get; }
        public AsyncRelayCommand<string> AddTriggerWordCommand { get; }

        public PipelineModel Pipeline
        {
            get { return (PipelineModel)GetValue(PipelineProperty); }
            set { SetValue(PipelineProperty, value); }
        }

        public DiffusionInputOptions Options
        {
            get { return (DiffusionInputOptions)GetValue(OptionsProperty); }
            set { SetValue(OptionsProperty, value); }
        }

        public UpscaleInputOptions UpscaleOptions
        {
            get { return (UpscaleInputOptions)GetValue(UpscaleOptionsProperty); }
            set { SetValue(UpscaleOptionsProperty, value); }
        }

        public ExtractInputOptions ExtractOptions
        {
            get { return (ExtractInputOptions)GetValue(ExtractOptionsProperty); }
            set { SetValue(ExtractOptionsProperty, value); }
        }

        public bool IsImageInputEnabled
        {
            get { return _isImageInputEnabled; }
            set { SetProperty(ref _isImageInputEnabled, value); }
        }

        public bool IsControlNetEnabled
        {
            get { return _isControlNetEnabled; }
            set { SetProperty(ref _isControlNetEnabled, value); }
        }

        public bool IsResolutionEnabled
        {
            get { return _isResolutionEnabled; }
            set
            {
                SetProperty(ref _isResolutionEnabled, value);
                if (_isResolutionEnabled)
                {
                    SelectedResolution = Pipeline?.DiffusionModel.Resolutions.FirstOrDefault(x => x.IsDefault);
                }
            }
        }

        public SizeOption SelectedResolution
        {
            get { return _selectedResolution; }
            set { SetProperty(ref _selectedResolution, value); }
        }

        public bool IsModelOptionsVisible
        {
            get { return _isModelOptionsVisible; }
            set { SetProperty(ref _isModelOptionsVisible, value); }
        }

        public bool IsSchedulerKarras
        {
            get { return _isSchedulerKarras; }
            set { SetProperty(ref _isSchedulerKarras, value); }
        }

        public bool IsSchedulerFlowMatch
        {
            get { return _isSchedulerFlowMatch; }
            set { SetProperty(ref _isSchedulerFlowMatch, value); }
        }

        public bool IsSchedulerMultiStep
        {
            get { return _isSchedulerMultiStep; }
            set { SetProperty(ref _isSchedulerMultiStep, value); }
        }

        public bool IsSchedulerTimeStep
        {
            get { return _isSchedulerTimeStep; }
            set { SetProperty(ref _isSchedulerTimeStep, value); }
        }

        public bool IsSchedulerStochastic
        {
            get { return _isSchedulerStochastic; }
            set { SetProperty(ref _isSchedulerStochastic, value); }
        }

        public bool IsSchedulerClipSample
        {
            get { return _isSchedulerClipSample; }
            set { SetProperty(ref _isSchedulerClipSample, value); }
        }

        public bool IsSchedulerThresholding
        {
            get { return _isSchedulerThresholding; }
            set { SetProperty(ref _isSchedulerThresholding, value); }
        }

        public bool IsSteps2Enabled
        {
            get { return _isSteps2Enabled; }
            set { SetProperty(ref _isSteps2Enabled, value); }
        }

        public bool IsGuidance2Enabled
        {
            get { return _isGuidance2Enabled; }
            set { SetProperty(ref _isGuidance2Enabled, value); }
        }


        private Task OnPipelineChanged(PipelineModel oldPipeline, PipelineModel newPipeline)
        {
            if (newPipeline is null || newPipeline.DiffusionModel is null)
            {
                IsModelOptionsVisible = false;
                return Task.CompletedTask;
            }

            var oldModel = oldPipeline?.DiffusionModel;
            var oldOptions = oldModel?.DefaultOptions;
            var newModel = newPipeline?.DiffusionModel;
            var newOptions = newModel?.DefaultOptions;

            if (oldModel == newModel)
            {
                // TODO if has lora changed
                Options.LoraOptions = newPipeline.LoraAdapterModel?.Select(x => new LoraOptionModel { Name = x.Name, Key = x.Key, Strength = 1f }).ToList();
                return Task.CompletedTask;
            }

            var previousOptions = Options;
            Options = new DiffusionInputOptions
            {
                // Keep
                Prompt = previousOptions?.Prompt,
                NegativePrompt = previousOptions?.NegativePrompt,
                Seed = previousOptions?.Seed ?? 0,
                LoraOptions = newPipeline.LoraAdapterModel?.Select(x => new LoraOptionModel { Name = x.Name, Key = x.Key, Strength = 1f }).ToList(),
                InputImageCount = ProcessType == ProcessType.ImageEdit ? (previousOptions?.InputImageCount ?? 1) : 0,

                // Update
                Strength = ProcessType == ProcessType.ImageToImage || ProcessType == ProcessType.ControlNetImageToImage ? (previousOptions?.Strength ?? 0.7f) : 1f,
                ControlNetStrength = ProcessType == ProcessType.ControlNetImage || ProcessType == ProcessType.ControlNetImageToImage ? (previousOptions?.ControlNetStrength ?? 0.7f) : 1f,

                Steps = newOptions.Steps,
                Steps2 = newOptions.Steps2,
                Scheduler = newOptions.Scheduler,
                GuidanceScale = newOptions.GuidanceScale,
                GuidanceScale2 = newOptions.GuidanceScale2,
                SchedulerOptions = new SchedulerInputOptions
                {
                    Shift = newOptions.Shift,
                    SolverType = newOptions.SolverType,
                    PredictionType = newOptions.PredictionType,
                    BaseShift = newOptions.BaseShift,
                    BetaEnd = newOptions.BetaEnd,
                    BetaSchedule = newOptions.BetaSchedule,
                    BetaStart = newOptions.BetaStart,
                    MaxShift = newOptions.MaxShift,
                    StepsOffset = newOptions.StepsOffset,
                    TimestepSpacing = newOptions.TimestepSpacing,
                    BaseImageSeqLen = newOptions.BaseImageSeqLen,
                    MaxImageSeqLen = newOptions.MaxImageSeqLen,
                    UseDynamicShifting = newOptions.UseDynamicShifting,
                }
            };

            //Resolution
            SelectedResolution = newModel?.Resolutions.FirstOrDefault(x => x.Width == _selectedResolution?.Width && x.Height == _selectedResolution?.Height)
                              ?? newModel?.Resolutions.FirstOrDefault(x => x.IsDefault);

            // UI Flags
            IsSteps2Enabled = newPipeline.DiffusionModel.DefaultOptions.Steps2 > 0;
            IsGuidance2Enabled = newPipeline.DiffusionModel.DefaultOptions.GuidanceScale2 > 0;
            IsModelOptionsVisible = newPipeline.UpscaleModel is not null || newPipeline.ExtractModel is not null;
            return Task.CompletedTask;
        }


        private void GenerateSeed(bool random)
        {
            Options.Seed = random ? 0 : Random.Shared.Next();
        }


        private Task AddTriggerWordAsync(string triggerWord)
        {
            if (string.IsNullOrEmpty(Options.Prompt))
            {
                Options.Prompt = triggerWord;
            }
            else
            {
                Options.Prompt += $", {triggerWord}";
            }
            return Task.CompletedTask;
        }


        private void ComboBoxScheduler_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (Options is null)
                return;

            IsSchedulerTimeStep = Options.Scheduler.IsTimestep();
            IsSchedulerKarras = Options.Scheduler.IsKarras();
            IsSchedulerFlowMatch = Options.Scheduler.IsFlowMatch();
            IsSchedulerMultiStep = Options.Scheduler.IsMultiStep();
            IsSchedulerStochastic = Options.Scheduler.IsStochastic();
            IsSchedulerClipSample = Options.Scheduler.IsClipSample();
            IsSchedulerThresholding = Options.Scheduler.IsThreshold();
        }


        private void ComboBoxResolution_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (Options is null || _selectedResolution is null)
                return;

            Options.Width = _selectedResolution.Width;
            Options.Height = _selectedResolution.Height;
        }
    }
}
