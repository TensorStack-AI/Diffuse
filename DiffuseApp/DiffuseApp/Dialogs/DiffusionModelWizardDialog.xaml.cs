// Copyright (c) TensorStack. All rights reserved.
// Licensed under the Apache 2.0 License.
using Diffuse.Common;
using DiffuseApp.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TensorStack.Python.Common;
using TensorStack.WPF;
using TensorStack.WPF.Controls;

namespace Diffuse.Dialogs
{
    /// <summary>
    /// Interaction logic for DiffusionModelWizardDialog.xaml
    /// </summary>
    public partial class DiffusionModelWizardDialog : DialogControl
    {
        private WizardOptionModel _selectedOption;
        private string _selectedName;
        private string _selectedModelPath;
        private ModelSourceType _selectedSource;
        private DataType _selectedDataType;
        private WizardItemModel _selectedItem;

        public DiffusionModelWizardDialog(Settings settings)
        {
            Settings = settings;
            Templates = CreateTemplates();
            Items = CreateItems();

            Errors = new ObservableCollection<string>();
            CancelCommand = new AsyncRelayCommand(CancelAsync);
            SaveCommand = new AsyncRelayCommand(SaveAsync, CanExecuteSave);

            SelectedItem = Items[0];
            SelectedDataType = DataType.Bfloat16;
            SelectedSource = ModelSourceType.HuggingFace;
            InitializeComponent();
        }

        public Settings Settings { get; }
        public AsyncRelayCommand SaveCommand { get; }
        public AsyncRelayCommand CancelCommand { get; }
        public ObservableCollection<string> Errors { get; }
        public List<DiffusionModel> Templates { get; }
        public List<WizardItemModel> Items { get; }

        public WizardItemModel SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                SetProperty(ref _selectedItem, value);
                SelectedOption = _selectedItem.Options?.FirstOrDefault();
            }
        }

        public WizardOptionModel SelectedOption
        {
            get { return _selectedOption; }
            set { SetProperty(ref _selectedOption, value); }
        }

        public string SelectedName
        {
            get { return _selectedName; }
            set { SetProperty(ref _selectedName, value); }
        }

        public ModelSourceType SelectedSource
        {
            get { return _selectedSource; }
            set { SetProperty(ref _selectedSource, value); GenerateName(); }
        }

        public DataType SelectedDataType
        {
            get { return _selectedDataType; }
            set { SetProperty(ref _selectedDataType, value); }
        }

        public string SelectedModelPath
        {
            get { return _selectedModelPath; }
            set { SetProperty(ref _selectedModelPath, value); GenerateName(); }
        }


        protected override Task SaveAsync()
        {
            var diffusionTemplate = Templates.FirstOrDefault(x => x.Name == SelectedOption.Template);

            diffusionTemplate.Id = GetNextModelId();
            diffusionTemplate.Name = SelectedName;
            diffusionTemplate.Source = _selectedSource;
            diffusionTemplate.DataTypes = _selectedDataType == DataType.Float8_e5m2 || _selectedDataType == DataType.Float8_e4m3fn
                ? [DataType.Bfloat16, DataType.Float16, _selectedDataType]
                : [DataType.Bfloat16, DataType.Float16];
            diffusionTemplate.Path = _selectedModelPath;
            if (_selectedSource == ModelSourceType.HuggingFace && Utils.TryParseHuggingFaceRepo(_selectedModelPath, out var huggingfacePath))
                diffusionTemplate.Path = huggingfacePath;

            diffusionTemplate.Initialize(Settings.DirectoryModel);
            Settings.DiffusionModels.Add(diffusionTemplate);
            return base.SaveAsync();
        }


        protected override bool CanExecuteSave()
        {
            Errors.Clear();
            foreach (var inputError in GetValidationErrors())
                Errors.Add(inputError);

            return Errors.Count == 0 && base.CanExecuteSave();
        }


        protected override Task CancelAsync()
        {
            return base.CancelAsync();
        }


        protected override async Task CloseAsync()
        {
            await base.CloseAsync();
        }


        private IEnumerable<string> GetValidationErrors()
        {
            if (string.IsNullOrWhiteSpace(_selectedName))
                yield return "Name cannot be empty";
            if (!string.IsNullOrWhiteSpace(_selectedName) && Settings.DiffusionModels.Any(x => x.Name.Equals(_selectedName, StringComparison.OrdinalIgnoreCase)))
                yield return $"Model with name '{_selectedName}' already exists";

            if (string.IsNullOrWhiteSpace(_selectedModelPath))
                yield return string.Empty;
            if (!string.IsNullOrWhiteSpace(_selectedModelPath))
            {
                if (_selectedSource == ModelSourceType.Folder && !Directory.Exists(_selectedModelPath))
                    yield return "Model folder not found";
                else if (_selectedSource == ModelSourceType.SingleFile && !File.Exists(_selectedModelPath))
                    yield return "Model file not found";
                else if (_selectedSource == ModelSourceType.HuggingFace && !Utils.TryParseHuggingFaceRepo(_selectedModelPath, out _))
                    yield return "HuggingFace repository not found";
            }
        }



        private int GetNextModelId()
        {
            return Math.Max(Utils.FixedIdRange, Settings.DiffusionModels.Max(x => x.Id)) + 1;
        }


        private void GenerateName()
        {
            if (!string.IsNullOrWhiteSpace(_selectedModelPath))
            {
                if (File.Exists(_selectedModelPath) || Directory.Exists(_selectedModelPath))
                {
                    SelectedName = Path.GetFileNameWithoutExtension(_selectedModelPath);
                }
                else
                {
                    SelectedName = Utils.TryParseHuggingFaceRepo(_selectedModelPath, out var huggingfaceRepo)
                        ? huggingfaceRepo
                        : Path.GetFileNameWithoutExtension(_selectedModelPath.Split('/', '\\').LastOrDefault());
                }
            }
        }

        private List<WizardItemModel> CreateItems()
        {
            return
            [
                new WizardItemModel
                {
                    Name = "Chroma",
                    Options =
                    [
                        new WizardOptionModel{ Name = "Base", Template = "Chroma_Base"}
                    ]
                },
                new WizardItemModel
                {
                    Name = "CogVideoX",
                    Options =
                    [
                        new WizardOptionModel{ Name = "T2V 2B",      Template = "CogVideoX_T2V_2B"},
                        new WizardOptionModel{ Name = "T2V 5B",     Template = "CogVideoX_T2V_5B"},
                        new WizardOptionModel{ Name = "I2V 5B", Template = "CogVideoX_I2V_5B"},
                        new WizardOptionModel{ Name = "v1.5 T2V 5B", Template = "CogVideoX_15_T2V_5B"},
                        new WizardOptionModel{ Name = "v1.5 I2V 5B", Template = "CogVideoX_15_I2V_5B"}
                    ]
                },
                new WizardItemModel
                {
                    Name = "FLUX.1",
                    Options =
                    [
                        new WizardOptionModel{ Name = "Dev", Template = "Flux1_Dev"},
                        new WizardOptionModel{ Name = "Schnell", Template = "Flux1_Schnell"},
                        new WizardOptionModel{ Name = "Kontext", Template = "Flux1_Kontext"}
                    ]
                },
                new WizardItemModel
                {
                    Name = "FLUX.2",
                    Options =
                    [
                        new WizardOptionModel{ Name = "Dev", Template = "Flux2_Dev"},
                    ]
                },
                new WizardItemModel
                {
                    Name = "Kandinsky5",
                    Options =
                    [
                        new WizardOptionModel{ Name = "T2I Lite", Template = "Kandinsky5_T2I_Lite"},
                        new WizardOptionModel{ Name = "I2I Lite", Template = "Kandinsky5_I2I_Lite"},
                        new WizardOptionModel{ Name = "T2V Lite",                 Template = "Kandinsky5_T2V_Lite"},
                        new WizardOptionModel{ Name = "T2V Lite (distilled)",     Template = "Kandinsky5_T2V_Lite_D"},
                        new WizardOptionModel{ Name = "T2V Lite 10s",             Template = "Kandinsky5_T2V_Lite_10"},
                        new WizardOptionModel{ Name = "T2V Lite 10s (distilled)", Template = "Kandinsky5_T2V_Lite 10_D"},
                        new WizardOptionModel{ Name = "T2V Pro",                  Template = "Kandinsky5_T2V_Pro"},
                        new WizardOptionModel{ Name = "T2V Pro (distilled)",      Template = "Kandinsky5_T2V_Pro_D"},
                        new WizardOptionModel{ Name = "I2V Pro",                  Template = "Kandinsky5_I2V_Pro"},
                        new WizardOptionModel{ Name = "I2V Pro (distilled)",      Template = "Kandinsky5_I2V_Pro_D"},
                    ]
                },
                new WizardItemModel
                {
                    Name = "LTX",
                    Options =
                    [
                        new WizardOptionModel{ Name = "2B", Template = "LTX_2B"},
                        new WizardOptionModel{ Name = "2B (distilled)", Template = "LTX_2B_D"},
                        new WizardOptionModel{ Name = "13B", Template = "LTX_13B"},
                        new WizardOptionModel{ Name = "13B (distilled)", Template = "LTX_13B_D"}
                    ]
                },

                new WizardItemModel
                {
                    Name = "Qwen Image",
                    Options =
                    [
                        new WizardOptionModel{ Name = "Base", Template = "Qwen_Base"},
                        new WizardOptionModel{ Name = "Edit", Template = "Qwen_Edit"}
                    ]
                },
                new WizardItemModel
                {
                    Name = "StableDiffusion XL",
                    Options =
                    [
                        new WizardOptionModel{ Name = "Base", Template = "StableDiffusionXL_Base"},
                        new WizardOptionModel{ Name = "Turbo", Template = "StableDiffusionXL_Turbo"},
                        new WizardOptionModel{ Name = "Lightning", Template = "StableDiffusionXL_Lightning"}
                    ]
                },
                new WizardItemModel
                {
                    Name = "WAN 2.1",
                    Options =
                    [
                        new WizardOptionModel{ Name = "T2V (1B)",      Template = "Wan21_T2V_1B"},
                        new WizardOptionModel{ Name = "T2V (14B)",     Template = "Wan21_T2V_14B"},
                        new WizardOptionModel{ Name = "I2V (14B) 480p", Template = "Wan21_I2V_14B_480"},
                        new WizardOptionModel{ Name = "I2V (14B) 720p", Template = "Wan21_I2V_14B_720"}
                    ]
                },
                new WizardItemModel
                {
                    Name = "Z-Image",
                    Options =
                    [
                        new WizardOptionModel{ Name = "Turbo", Template = "ZImage_Turbo"}
                    ]
                }
            ];

        }

        private List<DiffusionModel> CreateTemplates()
        {
            return
            [
                 // Chroma
                 new DiffusionModel
                 {
                    Name = "Chroma_Base",
                    Pipeline = "ChromaPipeline",
                    MemoryModes = [ 2, 2, 19, 30 ],
                    ProcessTypes = [ProcessType.TextToImage, ProcessType.ImageToImage],
                    DefaultOptions = new DiffusionDefaultOptions
                    {
                        Steps = 30,
                        Width = 1024,
                        Height = 1024,
                        GuidanceScale = 4,
                        Scheduler = SchedulerType.FlowMatchEulerDiscrete,
                        Schedulers = [SchedulerType.FlowMatchEulerDiscrete],
                    },
                    Resolutions =
                    [
                        new SizeOption {  Width = 1024, Height = 1536 },
                        new SizeOption {  Width = 768, Height = 1344 },
                        new SizeOption {  Width = 832, Height = 1216 },
                        new SizeOption {  Width = 1024, Height = 1024, IsDefault = true },
                        new SizeOption {  Width = 1216 , Height = 832 },
                        new SizeOption {  Width = 1344, Height = 768 },
                        new SizeOption {  Width = 1536, Height = 1024 },
                    ]
                 },

                 // CogVideoX
                 new DiffusionModel
                 {
                    Name = "CogVideoX_T2V_2B",
                    Pipeline = "CogVideoXPipeline",
                    MemoryModes =  [8, 16, 54, 64  ],
                    ProcessTypes = [ ProcessType.TextToVideo ],
                    DefaultOptions = new DiffusionDefaultOptions
                    {
                        Steps = 50,
                        Width = 720,
                        Height = 480,
                        GuidanceScale = 6f,
                        Frames = 48,
                        FrameRate = 8,
                        Scheduler = SchedulerType.DDIM,
                        Schedulers = [SchedulerType.DDIM, SchedulerType.DDPM],
                    },
                    Resolutions =
                    [
                        new SizeOption {  Width = 720, Height = 480, IsDefault = true }
                    ]
                 },
                 new DiffusionModel
                 {
                    Name = "CogVideoX_T2V_5B",
                    Pipeline = "CogVideoXPipeline",
                    MemoryModes =  [ 8, 16, 54, 72  ],
                    ProcessTypes = [ ProcessType.TextToVideo ],
                    DefaultOptions = new DiffusionDefaultOptions
                    {
                        Steps = 50,
                        Width = 720,
                        Height = 480,
                        GuidanceScale = 6f,
                        Frames = 48,
                        FrameRate = 8,
                        Scheduler = SchedulerType.DDIM,
                        Schedulers = [SchedulerType.DDIM, SchedulerType.DDPM],
                    },
                    Resolutions =
                    [
                        new SizeOption {  Width = 720, Height = 480, IsDefault = true }
                    ]
                 },
                 new DiffusionModel
                 {
                    Name = "CogVideoX_I2V_5B",
                    Pipeline = "CogVideoXPipeline",
                    MemoryModes =  [ 8, 18, 54, 72  ],
                    ProcessTypes = [ProcessType.ImageToVideo ],
                    DefaultOptions = new DiffusionDefaultOptions
                    {
                        Steps = 50,
                        Width = 720,
                        Height = 480,
                        GuidanceScale = 6f,
                        Frames = 49,
                        FrameRate = 8,
                        Scheduler = SchedulerType.DDIM,
                        Schedulers = [SchedulerType.DDIM, SchedulerType.DDPM],
                    },
                    Resolutions =
                    [
                        new SizeOption {  Width = 720, Height = 480, IsDefault = true }
                    ]
                 },
                 new DiffusionModel
                 {
                    Name = "CogVideoX_15_T2V_5B",
                    Pipeline = "CogVideoXPipeline",
                    MemoryModes =  [8, 18, 54, 72   ],
                    ProcessTypes = [ProcessType.TextToVideo ],
                    DefaultOptions = new DiffusionDefaultOptions
                    {
                        Steps = 50,
                        Width = 1360,
                        Height = 768,
                        GuidanceScale = 5f,
                        Frames = 81,
                        FrameRate = 16,
                        Scheduler = SchedulerType.DDIM,
                        Schedulers = [SchedulerType.DDIM, SchedulerType.DDPM],
                    },
                    Resolutions =
                    [
                        new SizeOption {  Width = 1360, Height = 768, IsDefault = true }
                    ]
                 },
                 new DiffusionModel
                 {
                    Name = "CogVideoX_15_I2V_5B",
                    Pipeline = "CogVideoXPipeline",
                    MemoryModes =  [ 8, 18, 54, 72 ],
                    ProcessTypes = [ProcessType.ImageToVideo ],
                    DefaultOptions = new DiffusionDefaultOptions
                    {
                        Steps = 50,
                        Width = 1360,
                        Height = 768,
                        GuidanceScale = 5f,
                        Frames = 81,
                        FrameRate = 16,
                        Scheduler = SchedulerType.DDIM,
                        Schedulers = [SchedulerType.DDIM, SchedulerType.DDPM],
                    },
                    Resolutions =
                    [
                        new SizeOption {  Width = 832, Height = 480, IsDefault = true }
                    ]
                 },

                 
                 // FLUX.1
                 new DiffusionModel
                 {
                    Name = "Flux1_Dev",
                    Pipeline = "FluxPipeline",
                    MemoryModes =  [ 4, 8, 23, 32 ],
                    ProcessTypes = [ProcessType.TextToImage, ProcessType.ImageToImage , ProcessType.ControlNetImage ],
                    DefaultOptions = new DiffusionDefaultOptions
                    {
                        Steps = 30,
                        Width = 1024,
                        Height = 1024,
                        GuidanceScale = 3.5f,
                        Scheduler = SchedulerType.FlowMatchEulerDiscrete,
                        Schedulers = [SchedulerType.FlowMatchEulerDiscrete],
                    },
                    Resolutions =
                    [
                        new SizeOption {  Width = 1024, Height = 1536 },
                        new SizeOption {  Width = 768, Height = 1344 },
                        new SizeOption {  Width = 832, Height = 1216 },
                        new SizeOption {  Width = 1024, Height = 1024, IsDefault = true },
                        new SizeOption {  Width = 1216 , Height = 832 },
                        new SizeOption {  Width = 1344, Height = 768 },
                        new SizeOption {  Width = 1536, Height = 1024 },
                    ]
                 },
                 new DiffusionModel
                 {
                    Name = "Flux1_Schnell",
                    Pipeline = "FluxPipeline",
                    MemoryModes =  [ 4, 8, 23, 32 ],
                    ProcessTypes = [ProcessType.TextToImage, ProcessType.ImageToImage],
                    DefaultOptions = new DiffusionDefaultOptions
                    {
                        Steps = 4,
                        Width = 1024,
                        Height = 1024,
                        GuidanceScale = 0,
                        Scheduler = SchedulerType.FlowMatchEulerDiscrete,
                        Schedulers = [SchedulerType.FlowMatchEulerDiscrete],
                    },
                    Resolutions =
                    [
                        new SizeOption {  Width = 1024, Height = 1536 },
                        new SizeOption {  Width = 768, Height = 1344 },
                        new SizeOption {  Width = 832, Height = 1216 },
                        new SizeOption {  Width = 1024, Height = 1024, IsDefault = true },
                        new SizeOption {  Width = 1216 , Height = 832 },
                        new SizeOption {  Width = 1344, Height = 768 },
                        new SizeOption {  Width = 1536, Height = 1024 },
                    ]
                 },
                 new DiffusionModel
                 {
                    Name = "Flux1_Kontext",
                    Pipeline = "FluxPipeline",
                    MemoryModes =  [ 4, 8, 23, 32 ],
                    ProcessTypes = [ProcessType.ImageEdit ],
                    DefaultOptions = new DiffusionDefaultOptions
                    {
                        Steps = 30,
                        Width = 1024,
                        Height = 1024,
                        GuidanceScale = 3.5f,
                        Scheduler = SchedulerType.FlowMatchEulerDiscrete,
                        Schedulers = [SchedulerType.FlowMatchEulerDiscrete],
                    },
                    Resolutions =
                    [
                        new SizeOption {  Width = 1024, Height = 1536 },
                        new SizeOption {  Width = 768, Height = 1344 },
                        new SizeOption {  Width = 832, Height = 1216 },
                        new SizeOption {  Width = 1024, Height = 1024, IsDefault = true },
                        new SizeOption {  Width = 1216 , Height = 832 },
                        new SizeOption {  Width = 1344, Height = 768 },
                        new SizeOption {  Width = 1536, Height = 1024 },
                    ]
                 },


                 // FLUX.2
                 new DiffusionModel
                 {
                    Name = "Flux2_Dev",
                    Pipeline = "Flux2Pipeline",
                    MemoryModes= [ 4, 8, 64, 128 ],
                    ProcessTypes = [ProcessType.TextToImage, ProcessType.ImageEdit],
                    DefaultOptions = new DiffusionDefaultOptions
                    {
                        Steps = 4,
                        Width = 1024,
                        Height = 1024,
                        GuidanceScale = 2.5f,
                        Scheduler = SchedulerType.FlowMatchEulerDiscrete,
                        Schedulers = [SchedulerType.FlowMatchEulerDiscrete],
                    },
                    Resolutions =
                    [
                        new SizeOption {  Width = 1024, Height = 1536 },
                        new SizeOption {  Width = 768, Height = 1344 },
                        new SizeOption {  Width = 832, Height = 1216 },
                        new SizeOption {  Width = 1024, Height = 1024, IsDefault = true },
                        new SizeOption {  Width = 1216 , Height = 832 },
                        new SizeOption {  Width = 1344, Height = 768 },
                        new SizeOption {  Width = 1536, Height = 1024 },
                    ]
                 },


                 // Kandinsky5
                 new DiffusionModel
                 {
                    Name = "Kandinsky5_T2I_Lite",
                    Pipeline = "Kandinsky5Pipeline",
                    MemoryModes =  [ 8, 16, 18, 32  ],
                    ProcessTypes = [ ProcessType.TextToImage ],
                    DefaultOptions = new DiffusionDefaultOptions
                    {
                        Steps = 50,
                        Width = 1024,
                        Height = 1024,
                        GuidanceScale = 3.5f,
                        Scheduler = SchedulerType.FlowMatchEulerDiscrete,
                        Schedulers = [SchedulerType.FlowMatchEulerDiscrete],
                    },
                    Resolutions =
                    [
                        new SizeOption {  Width = 640, Height = 1408},
                        new SizeOption {  Width = 768, Height = 1280},
                        new SizeOption {  Width = 896, Height = 1152},
                        new SizeOption {  Width = 1024, Height = 1024, IsDefault = true },
                        new SizeOption {  Width = 1152, Height = 896},
                        new SizeOption {  Width = 1280, Height = 768},
                        new SizeOption {  Width = 1408, Height = 640},
                    ]
                 },
                 new DiffusionModel
                 {
                    Name = "Kandinsky5_I2I_Lite",
                    Pipeline = "Kandinsky5Pipeline",
                    MemoryModes =  [ 8, 16, 54, 72  ],
                    ProcessTypes = [ ProcessType.ImageEdit ],
                    DefaultOptions = new DiffusionDefaultOptions
                    {
                        Steps = 50,
                        Width = 1024,
                        Height = 1024,
                        GuidanceScale = 3.5f,
                        Scheduler = SchedulerType.FlowMatchEulerDiscrete,
                        Schedulers = [SchedulerType.FlowMatchEulerDiscrete],
                    },
                    Resolutions =
                    [
                        new SizeOption {  Width = 640, Height = 1408},
                        new SizeOption {  Width = 768, Height = 1280},
                        new SizeOption {  Width = 896, Height = 1152},
                        new SizeOption {  Width = 1024, Height = 1024, IsDefault = true },
                        new SizeOption {  Width = 1152, Height = 896},
                        new SizeOption {  Width = 1280, Height = 768},
                        new SizeOption {  Width = 1408, Height = 640},
                    ]
                 },
                 new DiffusionModel
                 {
                    Name = "Kandinsky5_T2V_Lite",
                    Pipeline = "Kandinsky5Pipeline",
                    MemoryModes =  [ 16, 32, 32, 64],
                    ProcessTypes = [ProcessType.TextToVideo ],
                    DefaultOptions = new DiffusionDefaultOptions
                    {
                        Steps = 50,
                        Width = 768,
                        Height = 512,
                        GuidanceScale = 5f,
                        Frames = 121,
                        FrameRate = 24,
                        Scheduler = SchedulerType.FlowMatchEulerDiscrete,
                        Schedulers = [SchedulerType.FlowMatchEulerDiscrete],
                    },
                    Resolutions =
                    [
                       new SizeOption {  Width = 768, Height = 512, IsDefault = true }
                    ]
                 },
                 new DiffusionModel
                 {
                    Name = "Kandinsky5_T2V_Lite_D",
                    Pipeline = "Kandinsky5Pipeline",
                    MemoryModes =  [ 16, 32, 32, 64],
                    ProcessTypes = [ProcessType.TextToVideo ],
                    DefaultOptions = new DiffusionDefaultOptions
                    {
                        Steps = 16,
                        Width = 768,
                        Height = 512,
                        GuidanceScale = 5f,
                        Frames = 121,
                        FrameRate = 24,
                        Scheduler = SchedulerType.FlowMatchEulerDiscrete,
                        Schedulers = [SchedulerType.FlowMatchEulerDiscrete],
                    },
                    Resolutions =
                    [
                       new SizeOption {  Width = 768, Height = 512, IsDefault = true }
                    ]
                 },
                 new DiffusionModel
                 {
                    Name = "Kandinsky5_T2V_Lite_10",
                    Pipeline = "Kandinsky5Pipeline",
                    MemoryModes =  [ 16, 32, 32, 64],
                    ProcessTypes = [ProcessType.TextToVideo ],
                    DefaultOptions = new DiffusionDefaultOptions
                    {
                        Steps = 50,
                        Width = 768,
                        Height = 512,
                        GuidanceScale = 5f,
                        Frames = 241,
                        FrameRate = 24,
                        Scheduler = SchedulerType.FlowMatchEulerDiscrete,
                        Schedulers = [SchedulerType.FlowMatchEulerDiscrete],
                    },
                    Resolutions =
                    [
                       new SizeOption {  Width = 768, Height = 512, IsDefault = true }
                    ]
                 },
                 new DiffusionModel
                 {
                    Name = "Kandinsky5_T2V_Lite 10_D",
                    Pipeline = "Kandinsky5Pipeline",
                    MemoryModes =  [ 16, 32, 32, 64],
                    ProcessTypes = [ProcessType.TextToVideo ],
                    DefaultOptions = new DiffusionDefaultOptions
                    {
                        Steps = 16,
                        Width = 768,
                        Height = 512,
                        GuidanceScale = 5f,
                        Frames = 241,
                        FrameRate = 24,
                        Scheduler = SchedulerType.FlowMatchEulerDiscrete,
                        Schedulers = [SchedulerType.FlowMatchEulerDiscrete],
                    },
                    Resolutions =
                    [
                       new SizeOption {  Width = 768, Height = 512, IsDefault = true }
                    ]
                 },
                 new DiffusionModel
                 {
                    Name = "Kandinsky5_T2V_Pro",
                    Pipeline = "Kandinsky5Pipeline",
                    MemoryModes =  [16, 32, 32, 64   ],
                    ProcessTypes = [ProcessType.TextToVideo ],
                    DefaultOptions = new DiffusionDefaultOptions
                    {
                        Steps = 50,
                        Width = 1024,
                        Height = 768,
                        GuidanceScale = 5f,
                        Frames = 121,
                        FrameRate = 24,
                        Scheduler = SchedulerType.FlowMatchEulerDiscrete,
                        Schedulers = [SchedulerType.FlowMatchEulerDiscrete],
                    },
                    Resolutions =
                    [
                        new SizeOption {  Width = 1024, Height = 768, IsDefault = true }
                    ]
                 },
                 new DiffusionModel
                 {
                    Name = "Kandinsky5_T2V_Pro_D",
                    Pipeline = "Kandinsky5Pipeline",
                    MemoryModes =  [16, 32, 32, 64   ],
                    ProcessTypes = [ProcessType.TextToVideo ],
                    DefaultOptions = new DiffusionDefaultOptions
                    {
                        Steps = 16,
                        Width = 1024,
                        Height = 768,
                        GuidanceScale = 5f,
                        Frames = 121,
                        FrameRate = 24,
                        Scheduler = SchedulerType.FlowMatchEulerDiscrete,
                        Schedulers = [SchedulerType.FlowMatchEulerDiscrete],
                    },
                    Resolutions =
                    [
                         new SizeOption {  Width = 1024, Height = 768, IsDefault = true }
                    ]
                 },
                 new DiffusionModel
                 {
                    Name = "Kandinsky5_I2V_Pro",
                    Pipeline = "Kandinsky5Pipeline",
                    MemoryModes =  [16, 32, 32, 64   ],
                    ProcessTypes = [ProcessType.ImageToVideo ],
                    DefaultOptions = new DiffusionDefaultOptions
                    {
                        Steps = 50,
                        Width = 1024,
                        Height = 768,
                        GuidanceScale = 5f,
                        Frames = 121,
                        FrameRate = 24,
                        Scheduler = SchedulerType.FlowMatchEulerDiscrete,
                        Schedulers = [SchedulerType.FlowMatchEulerDiscrete],
                    },
                    Resolutions =
                    [
                          new SizeOption {  Width = 1024, Height = 768, IsDefault = true }
                    ]
                 },
                 new DiffusionModel
                 {
                    Name = "Kandinsky5_I2V_Pro_d",
                    Pipeline = "Kandinsky5Pipeline",
                    MemoryModes =  [16, 32, 32, 64   ],
                    ProcessTypes = [ProcessType.ImageToVideo ],
                    DefaultOptions = new DiffusionDefaultOptions
                    {
                        Steps = 16,
                        Width = 1024,
                        Height = 768,
                        GuidanceScale = 5f,
                        Frames = 121,
                        FrameRate = 24,
                        Scheduler = SchedulerType.FlowMatchEulerDiscrete,
                        Schedulers = [SchedulerType.FlowMatchEulerDiscrete],
                    },
                    Resolutions =
                    [
                         new SizeOption {  Width = 1024, Height = 768, IsDefault = true }
                    ]
                 },


                 //LTX Video
                 new DiffusionModel
                 {
                    Name = "LTX_2B",
                    Pipeline = "LTXPipeline",
                    MemoryModes =  [16, 23, 32, 48 ],
                    ProcessTypes = [ProcessType.TextToVideo , ProcessType.ImageToVideo, ProcessType.VideoToVideo  ],
                    DefaultOptions = new DiffusionDefaultOptions
                    {
                        Steps = 50,
                        Width = 704,
                        Height = 480,
                        GuidanceScale = 3f,
                        Frames = 161,
                        FrameRate = 24,
                        Scheduler = SchedulerType.FlowMatchEulerDiscrete,
                        Schedulers = [SchedulerType.FlowMatchEulerDiscrete],
                    },
                    Resolutions =
                    [
                         new SizeOption {  Width = 720, Height = 1280 },
                         new SizeOption {  Width = 480, Height = 832 },
                         new SizeOption {  Width = 768, Height = 512, IsDefault = true },
                         new SizeOption {  Width = 512, Height = 768 },
                         new SizeOption {  Width = 832, Height = 480},
                         new SizeOption {  Width = 1280, Height = 720 }
                    ]
                 },
                 new DiffusionModel
                 {
                    Name = "LTX_2B_D",
                    Pipeline = "LTXPipeline",
                    MemoryModes =  [16, 23, 32, 48 ],
                    ProcessTypes = [ProcessType.TextToVideo, ProcessType.ImageToVideo, ProcessType.VideoToVideo   ],
                    DefaultOptions = new DiffusionDefaultOptions
                    {
                        Steps = 50,
                        Width = 704,
                        Height = 480,
                        GuidanceScale = 3f,
                        Frames = 161,
                        FrameRate = 24,
                        Scheduler = SchedulerType.FlowMatchEulerDiscrete,
                        Schedulers = [SchedulerType.FlowMatchEulerDiscrete],
                    },
                    Resolutions =
                    [
                         new SizeOption {  Width = 720, Height = 1280 },
                         new SizeOption {  Width = 480, Height = 832 },
                         new SizeOption {  Width = 768, Height = 512, IsDefault = true },
                         new SizeOption {  Width = 512, Height = 768 },
                         new SizeOption {  Width = 832, Height = 480},
                         new SizeOption {  Width = 1280, Height = 720 }
                    ]
                 },
                 new DiffusionModel
                 {
                    Name = "LTX_13B",
                    Pipeline = "LTXPipeline",
                    MemoryModes =  [16, 23, 32, 48 ],
                    ProcessTypes = [ProcessType.TextToVideo, ProcessType.ImageToVideo, ProcessType.VideoToVideo   ],
                    DefaultOptions = new DiffusionDefaultOptions
                    {
                        Steps = 50,
                        Width = 704,
                        Height = 480,
                        GuidanceScale = 3f,
                        Frames = 161,
                        FrameRate = 24,
                        Scheduler = SchedulerType.FlowMatchEulerDiscrete,
                        Schedulers = [SchedulerType.FlowMatchEulerDiscrete],
                    },
                    Resolutions =
                    [
                         new SizeOption {  Width = 720, Height = 1280 },
                         new SizeOption {  Width = 480, Height = 832 },
                         new SizeOption {  Width = 768, Height = 512, IsDefault = true },
                         new SizeOption {  Width = 512, Height = 768 },
                         new SizeOption {  Width = 832, Height = 480},
                         new SizeOption {  Width = 1280, Height = 720 }
                    ]
                 },
                 new DiffusionModel
                 {
                    Name = "LTX_13B_D",
                    Pipeline = "LTXPipeline",
                    MemoryModes =  [16, 23, 32, 48 ],
                    ProcessTypes = [ProcessType.TextToVideo, ProcessType.ImageToVideo, ProcessType.VideoToVideo   ],
                    DefaultOptions = new DiffusionDefaultOptions
                    {
                        Steps = 50,
                        Width = 704,
                        Height = 480,
                        GuidanceScale = 3f,
                        Frames = 161,
                        FrameRate = 24,
                        Scheduler = SchedulerType.FlowMatchEulerDiscrete,
                        Schedulers = [SchedulerType.FlowMatchEulerDiscrete],
                    },
                    Resolutions =
                    [
                         new SizeOption {  Width = 720, Height = 1280 },
                         new SizeOption {  Width = 480, Height = 832 },
                         new SizeOption {  Width = 768, Height = 512, IsDefault = true },
                         new SizeOption {  Width = 512, Height = 768 },
                         new SizeOption {  Width = 832, Height = 480},
                         new SizeOption {  Width = 1280, Height = 720 }
                    ]
                 },


                 // Qwen Image
                 new DiffusionModel
                 {
                    Name = "Qwen_Base",
                    Pipeline = "QwenImagePipeline",
                    MemoryModes = [ 5, 5, 40, 60 ],
                    ProcessTypes = [ProcessType.TextToImage, ProcessType.ImageToImage, ProcessType.ControlNetImage],
                    DefaultOptions = new DiffusionDefaultOptions
                    {
                        Steps = 30,
                        Width = 1328,
                        Height = 1328,
                        GuidanceScale = 4,
                        Scheduler = SchedulerType.FlowMatchEulerDiscrete,
                        Schedulers = [SchedulerType.FlowMatchEulerDiscrete],
                    },
                    Resolutions =
                    [
                        new SizeOption {  Width = 1664, Height = 928},
                        new SizeOption {  Width = 1584, Height = 1056},
                        new SizeOption {  Width = 1472, Height = 1104},
                        new SizeOption {  Width = 1328, Height = 1328, IsDefault = true },
                        new SizeOption {  Width = 1104, Height = 1472},
                        new SizeOption {  Width = 1056, Height = 1584},
                        new SizeOption {  Width = 928, Height = 1664}
                    ]
                 },
                 new DiffusionModel
                 {
                    Name = "Qwen_Edit",
                    Pipeline = "QwenImagePipeline",
                    MemoryModes = [ 5, 5, 40, 60 ],
                    ProcessTypes = [ProcessType.ImageEdit],
                    DefaultOptions = new DiffusionDefaultOptions
                    {
                        Steps = 30,
                        Width = 1024,
                        Height = 1024,
                        GuidanceScale = 4,
                        Scheduler = SchedulerType.FlowMatchEulerDiscrete,
                        Schedulers = [SchedulerType.FlowMatchEulerDiscrete],
                    },
                    Resolutions =
                    [
                        new SizeOption {  Width = 1664, Height = 928},
                        new SizeOption {  Width = 1584, Height = 1056},
                        new SizeOption {  Width = 1472, Height = 1104},
                        new SizeOption {  Width = 1328, Height = 1328, IsDefault = true },
                        new SizeOption {  Width = 1104, Height = 1472},
                        new SizeOption {  Width = 1056, Height = 1584},
                        new SizeOption {  Width = 928, Height = 1664}
                    ]
                 },

                 //StableDiffusionXL
                 new DiffusionModel
                 {
                    Name = "StableDiffusionXL_Base",
                    Pipeline = "StableDiffusionXLPipeline",
                    MemoryModes = [2, 2, 8, 16],
                    ProcessTypes = [ProcessType.TextToImage, ProcessType.ImageToImage, ProcessType.ControlNetImage, ProcessType.ControlNetImageToImage],
                    DefaultOptions = new DiffusionDefaultOptions
                    {
                        Steps = 30,
                        Width = 1024,
                        Height = 1024,
                        GuidanceScale = 7.5f,
                        Scheduler = SchedulerType.DDPM,
                        Schedulers = [SchedulerType.LMS, SchedulerType.Euler, SchedulerType.EulerAncestral, SchedulerType.DDPM, SchedulerType.DDIM, SchedulerType.KDPM2, SchedulerType.KDPM2Ancestral, SchedulerType.PNDM, SchedulerType.Heun, SchedulerType.UniPC, SchedulerType.DPMM, SchedulerType.DPMS, SchedulerType.DPMSDE, SchedulerType.DEISM ],
                    },
                    Resolutions =
                    [
                        new SizeOption {  Width = 640, Height = 1536 },
                        new SizeOption {  Width = 768, Height = 1344 },
                        new SizeOption {  Width = 832, Height = 1280 },
                        new SizeOption {  Width = 896, Height = 1152 },
                        new SizeOption {  Width = 768, Height = 768 },
                        new SizeOption {  Width = 1024, Height = 1024, IsDefault = true },
                        new SizeOption {  Width = 1152, Height = 896 },
                        new SizeOption {  Width = 1280, Height = 832 },
                        new SizeOption {  Width = 1344, Height = 768 },
                        new SizeOption {  Width = 1536, Height = 640 },
                    ]
                 },
                 new DiffusionModel
                 {
                    Name = "StableDiffusionXL_Turbo",
                    Pipeline = "StableDiffusionXLPipeline",
                    MemoryModes = [2, 2, 8, 16],
                    ProcessTypes = [ProcessType.TextToImage],
                    DefaultOptions = new DiffusionDefaultOptions
                    {
                        Steps = 4,
                        Width = 512,
                        Height = 512,
                        GuidanceScale = 0f,
                        Scheduler = SchedulerType.EulerAncestral,
                        Schedulers = [SchedulerType.LMS, SchedulerType.Euler, SchedulerType.EulerAncestral, SchedulerType.DDPM, SchedulerType.DDIM, SchedulerType.KDPM2, SchedulerType.KDPM2Ancestral, SchedulerType.PNDM, SchedulerType.Heun, SchedulerType.UniPC, SchedulerType.DPMM, SchedulerType.DPMS, SchedulerType.DPMSDE, SchedulerType.DEISM],
                    },
                    Resolutions =
                    [
                        new SizeOption {  Width = 648 , Height = 1152 },
                        new SizeOption {  Width = 768 , Height = 1024 },
                        new SizeOption {  Width = 512, Height = 896 },
                        new SizeOption {  Width = 1152 , Height = 768 },
                        new SizeOption {  Width = 576 , Height = 768 },
                        new SizeOption {  Width = 512, Height = 512, IsDefault = true },
                        new SizeOption {  Width = 768, Height = 768 },
                        new SizeOption {  Width = 768 , Height = 576 },
                        new SizeOption {  Width = 768 , Height = 1152 },
                        new SizeOption {  Width = 896, Height = 512 },
                        new SizeOption {  Width = 1024 , Height = 768 },
                        new SizeOption {  Width = 1152, Height = 648 },
                    ]
                 },
                 new DiffusionModel
                 {
                    Name = "StableDiffusionXL_Lightning",
                    Pipeline = "StableDiffusionXLPipeline",
                    MemoryModes = [2, 2, 8, 16],
                    ProcessTypes = [ProcessType.TextToImage, ProcessType.ImageToImage, ProcessType.ControlNetImage, ProcessType.ControlNetImageToImage],
                    DefaultOptions = new DiffusionDefaultOptions
                    {
                        Steps = 8,
                        Width = 1024,
                        Height = 1024,
                        GuidanceScale = 2f,
                        Scheduler = SchedulerType.DDPM,
                        Schedulers = [SchedulerType.LMS, SchedulerType.Euler, SchedulerType.EulerAncestral, SchedulerType.DDPM, SchedulerType.DDIM, SchedulerType.KDPM2, SchedulerType.KDPM2Ancestral, SchedulerType.PNDM, SchedulerType.Heun, SchedulerType.UniPC, SchedulerType.DPMM, SchedulerType.DPMS, SchedulerType.DPMSDE, SchedulerType.DEISM],
                    },
                    Resolutions =
                    [
                        new SizeOption {  Width = 640, Height = 1536 },
                        new SizeOption {  Width = 768, Height = 1344 },
                        new SizeOption {  Width = 832, Height = 1280 },
                        new SizeOption {  Width = 896, Height = 1152 },
                        new SizeOption {  Width = 768, Height = 768 },
                        new SizeOption {  Width = 1024, Height = 1024, IsDefault = true },
                        new SizeOption {  Width = 1152, Height = 896 },
                        new SizeOption {  Width = 1280, Height = 832 },
                        new SizeOption {  Width = 1344, Height = 768 },
                        new SizeOption {  Width = 1536, Height = 640 },
                    ]
                 },

                  // WAN 2.1
                 new DiffusionModel
                 {
                    Name = "Wan21_T2V_1B",
                    Pipeline = "WanPipeline",
                    MemoryModes =  [ 8, 16, 18, 32 ],
                    ProcessTypes = [ProcessType.TextToVideo ],
                    DefaultOptions = new DiffusionDefaultOptions
                    {
                        Steps = 50,
                        Width = 832,
                        Height = 480,
                        GuidanceScale = 5f,
                        Frames = 81,
                        FrameRate = 16,
                        Scheduler = SchedulerType.FlowMatchEulerDiscrete,
                        Schedulers = [SchedulerType.FlowMatchEulerDiscrete],
                    },
                    Resolutions =
                    [
                        new SizeOption {  Width = 832, Height = 480, IsDefault = true }
                    ]
                 },
                 new DiffusionModel
                 {
                    Name = "Wan21_T2V_14B",
                    Pipeline = "WanPipeline",
                    MemoryModes =  [ 8, 16, 18, 32 ],
                    ProcessTypes = [ProcessType.TextToVideo ],
                    DefaultOptions = new DiffusionDefaultOptions
                    {
                        Steps = 50,
                        Width = 832,
                        Height = 480,
                        GuidanceScale = 5f,
                        Frames = 81,
                        FrameRate = 16,
                        Scheduler = SchedulerType.FlowMatchEulerDiscrete,
                        Schedulers = [SchedulerType.FlowMatchEulerDiscrete],
                    },
                    Resolutions =
                    [
                        new SizeOption {  Width = 832, Height = 480, IsDefault = true },
                        new SizeOption {  Width = 1280, Height = 720 }
                    ]
                 },
                 new DiffusionModel
                 {
                    Name = "Wan21_I2V_14B_480",
                    Pipeline = "WanPipeline",
                    MemoryModes =  [ 8, 16, 18, 32 ],
                    ProcessTypes = [ProcessType.ImageToVideo ],
                    DefaultOptions = new DiffusionDefaultOptions
                    {
                        Steps = 50,
                        Width = 832,
                        Height = 480,
                        GuidanceScale = 5f,
                        Frames = 81,
                        FrameRate = 16,
                        Scheduler = SchedulerType.FlowMatchEulerDiscrete,
                        Schedulers = [SchedulerType.FlowMatchEulerDiscrete],
                    },
                    Resolutions =
                    [
                        new SizeOption {  Width = 832, Height = 480, IsDefault = true }
                    ]
                 },
                 new DiffusionModel
                 {
                    Name = "Wan21_I2V_14B_720",
                    Pipeline = "WanPipeline",
                    MemoryModes =  [ 8, 16, 18, 32 ],
                    ProcessTypes = [ProcessType.ImageToVideo ],
                    DefaultOptions = new DiffusionDefaultOptions
                    {
                        Steps = 50,
                        Width = 1280,
                        Height = 720,
                        GuidanceScale = 5f,
                        Frames = 81,
                        FrameRate = 16,
                        Scheduler = SchedulerType.FlowMatchEulerDiscrete,
                        Schedulers = [SchedulerType.FlowMatchEulerDiscrete],
                    },
                    Resolutions =
                    [
                        new SizeOption {  Width = 1280, Height = 720, IsDefault = true }
                    ]
                 },


                 // Z-Image Turbo
                 new DiffusionModel
                 {
                    Name = "ZImage_Turbo",
                    Pipeline = "ZImagePipeline",
                    MemoryModes = [ 2, 2, 14, 24 ],
                    ProcessTypes = [ProcessType.TextToImage, ProcessType.ImageToImage],
                    DefaultOptions = new DiffusionDefaultOptions
                    {
                        Steps = 9,
                        Width = 1024,
                        Height = 1024,
                        Shift = 3,
                        GuidanceScale = 0,
                        Scheduler = SchedulerType.FlowMatchEulerDiscrete,
                        Schedulers = [SchedulerType.FlowMatchEulerDiscrete],
                    },
                    Resolutions =
                    [
                        new SizeOption {  Width = 1664, Height = 928},
                        new SizeOption {  Width = 1584, Height = 1056},
                        new SizeOption {  Width = 1472, Height = 1104},
                        new SizeOption {  Width = 1024, Height = 1024, IsDefault = true },
                        new SizeOption {  Width = 1328, Height = 1328},
                        new SizeOption {  Width = 1408, Height = 1408 },
                        new SizeOption {  Width = 1104, Height = 1472},
                        new SizeOption {  Width = 1056, Height = 1584},
                        new SizeOption {  Width = 928, Height = 1664}
                    ]
                 },
             ];
        }

    }
}
