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

        public DiffusionInputControl()
        {
            SeedCommand = new RelayCommand<bool>(GenerateSeed);
            AddTriggerWordCommand = new AsyncRelayCommand<string>(AddTriggerWordAsync);
            InitializeComponent();
        }

        public static readonly DependencyProperty PipelineProperty = DependencyProperty.Register(nameof(Pipeline), typeof(PipelineModel), typeof(DiffusionInputControl), new PropertyMetadata<DiffusionInputControl, PipelineModel>((c, o, n) => c.OnPipelineChanged(o, n)));
        public static readonly DependencyProperty OptionsProperty = DependencyProperty.Register(nameof(Options), typeof(DiffusionInputOptions), typeof(DiffusionInputControl), new PropertyMetadata<DiffusionInputControl, DiffusionInputOptions>((c, o, n) => c.OnOptionsChanged(o, n)));
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


        private Task OnPipelineChanged(PipelineModel oldPipeline, PipelineModel newPipeline)
        {
            if (newPipeline is null || newPipeline.DiffusionModel is null)
                return Task.CompletedTask;

            var oldModel = oldPipeline?.DiffusionModel;
            var newModel = newPipeline?.DiffusionModel;

            if (oldModel is not null && oldModel.Pipeline == newModel.Pipeline)
                return Task.CompletedTask;

            IsModelOptionsVisible = newPipeline.UpscaleModel is not null || newPipeline.ExtractModel is not null;
            return Task.CompletedTask;
        }


        private Task OnOptionsChanged(DiffusionInputOptions oldOptions, DiffusionInputOptions newOptions)
        {
            if (newOptions is null)
                return Task.CompletedTask;

            if (oldOptions != null)
            {
                newOptions.Seed = oldOptions.Seed;
                newOptions.Prompt = oldOptions.Prompt;
                newOptions.NegativePrompt = oldOptions.NegativePrompt;

                newOptions.Steps = oldOptions.Steps;
                newOptions.Steps2 = oldOptions.Steps2;
                newOptions.GuidanceScale = oldOptions.GuidanceScale;
                newOptions.GuidanceScale2 = oldOptions.GuidanceScale2;
                newOptions.InputImageCount = oldOptions.InputImageCount;

                newOptions.Strength = oldOptions.Strength;
                newOptions.LoraStrength = oldOptions.LoraStrength;
                newOptions.ControlNetStrength = oldOptions.ControlNetStrength;

                if (Pipeline.DiffusionModel.DefaultOptions.Schedulers.Contains(oldOptions.Scheduler))
                {
                    newOptions.Scheduler = oldOptions.Scheduler;
                    newOptions.SchedulerOptions = oldOptions.SchedulerOptions;
                }

                SelectedResolution = Pipeline?.DiffusionModel.Resolutions.FirstOrDefault(x => x.Width == _selectedResolution?.Width && x.Height == _selectedResolution?.Height)
                                  ?? Pipeline?.DiffusionModel.Resolutions.FirstOrDefault(x => x.IsDefault);
            }
            else
            {
                SelectedResolution = Pipeline?.DiffusionModel.Resolutions.FirstOrDefault(x => x.IsDefault);
            }

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
