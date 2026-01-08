using Diffuse.Common;
using System;
using System.Collections.Generic;
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
        private SchedulerType[] _schedulers;

        public DiffusionInputControl()
        {
            SeedCommand = new RelayCommand<bool>(GenerateSeed);
            AddTriggerWordCommand = new AsyncRelayCommand<string>(AddTriggerWordAsync);
            // DefaultResolutions = [.. Enumerable.Range(4, 24).Select(x => 64 * x)];
            InitializeComponent();
        }



        public static readonly DependencyProperty PipelineProperty = DependencyProperty.Register(nameof(Pipeline), typeof(PipelineModel), typeof(DiffusionInputControl), new PropertyMetadata<DiffusionInputControl, PipelineModel>((c, o, n) => c.OnPipelineChanged(o, n)));
        public static readonly DependencyProperty OptionsProperty = DependencyProperty.Register(nameof(Options), typeof(DiffusionInputOptions), typeof(DiffusionInputControl), new PropertyMetadata<DiffusionInputControl, DiffusionInputOptions>((c, o, n) => c.OnOptionsChanged(o, n)));
        public static readonly DependencyProperty UpscaleOptionsProperty = DependencyProperty.Register(nameof(UpscaleOptions), typeof(UpscaleInputOptions), typeof(DiffusionInputControl));
        public static readonly DependencyProperty ExtractOptionsProperty = DependencyProperty.Register(nameof(ExtractOptions), typeof(ExtractInputOptions), typeof(DiffusionInputControl));
        private HashSet<int> _defaultResolutions;


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




        //public HashSet<int> DefaultResolutions
        //{
        //    get { return _defaultResolutions; }
        //    set { SetProperty(ref _defaultResolutions, value); }
        //}

        public SchedulerType[] Schedulers
        {
            get { return _schedulers; }
            set { SetProperty(ref _schedulers, value); }
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
                if (Options != null && _isResolutionEnabled)
                {
                    SelectedResolution = Pipeline.DiffusionModel.Resolutions.FirstOrDefault(x => x.Width == Options.Width && x.Height == Options.Height)
                                      ?? Pipeline.DiffusionModel.Resolutions.FirstOrDefault(x => x.IsDefault);
                }
            }
        }

        public SizeOption SelectedResolution
        {
            get { return _selectedResolution; }
            set
            {
                _selectedResolution = value;
                if (Options != null && _selectedResolution != null)
                {
                    Options.Width = _selectedResolution.Width;
                    Options.Height = _selectedResolution.Height;
                }
                NotifyPropertyChanged();
            }
        }





        private Task OnPipelineChanged(PipelineModel oldPipeline, PipelineModel newPipeline)
        {
            if (newPipeline is null || newPipeline.DiffusionModel is null)
                return Task.CompletedTask;

            var oldModel = oldPipeline?.DiffusionModel;
            var newModel = newPipeline?.DiffusionModel;

            if (oldModel is not null && oldModel.Pipeline == newModel.Pipeline)
                return Task.CompletedTask;

            Schedulers = Enum.GetValues<SchedulerType>().ToArray();
            SelectedResolution = newModel.Resolutions.FirstOrDefault(x => x.IsDefault);

            //var resolutions = new SortedSet<int>([.. Enumerable.Range(4, 24).Select(x => 64 * x)]);
            //foreach (var preset in newModel.Resolutions)
            //{
            //    resolutions.Add(preset.Width);
            //    resolutions.Add(preset.Height);
            //}
            //DefaultResolutions = [.. resolutions];

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
            }

            if (IsControlNetEnabled)
            {
                newOptions.Strength = 1f;
            }

            //if(_selectedResolution is not null)
            //{
            //    newOptions.Width = _selectedResolution.Width;
            //    newOptions.Height = _selectedResolution.Height;
            //}

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
    }
}
