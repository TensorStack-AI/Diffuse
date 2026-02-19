using Diffuse.Common;
using System.Collections.Generic;
using TensorStack.Common.Common;
using TensorStack.Python.Common;

namespace Diffuse.Services
{
    public class ModelManager
    {
        /// <summary>
        /// Creates the model templates.
        /// </summary>
        public static List<DiffusionModel> CreateModelTemplates()
        {
            return
            [
                 // Chroma
                 new DiffusionModel
                 {
                    Name = "Chroma_Base",
                    Pipeline = "ChromaPipeline",
                    Path = "lodestones/Chroma1-HD",
                    BaseType = DataType.Bfloat16,
                    Backend = BackendType.Pytorch,
                    Link = "https://huggingface.co/lodestones/Chroma1-HD",
                    MemoryProfile =
                    [
                        new MemoryProfile(DataType.Bfloat16, [ 4, 17, 19, 28, 30 ]),
                        new MemoryProfile(DataType.Float16, [ 4, 17, 19, 28, 30]),
                        new MemoryProfile(DataType.Float8, [ 4, 17, 16, 19, 21 ]),
                        new MemoryProfile(DataType.Int8, [ 4, 17, 16, 19, 21 ])
                    ],
                    ProcessTypes = [ProcessType.TextToImage, ProcessType.ImageToImage],
                    DefaultOptions = new DiffusionDefaultOptions
                    {
                        Steps = 30,
                        Width = 1024,
                        Height = 1024,
                        GuidanceScale = 4,
                        Scheduler = SchedulerType.FlowMatchEulerDiscrete,
                        Schedulers = [SchedulerType.FlowMatchEulerDiscrete],
                        Shift = 3,
                        UseDynamicShifting = false,
                    },
                    Resolutions =
                    [
                        new SizeOption { Width = 1024, Height = 1536 },
                        new SizeOption { Width = 768, Height = 1344 },
                        new SizeOption { Width = 832, Height = 1216 },
                        new SizeOption { Width = 1024, Height = 1024, IsDefault = true },
                        new SizeOption { Width = 1216, Height = 832 },
                        new SizeOption { Width = 1344, Height = 768 },
                        new SizeOption { Width = 1536, Height = 1024 },
                    ]
                 },

                 // CogVideoX
                 new DiffusionModel
                 {
                    Name = "CogVideoX_T2V_2B",
                    Pipeline = "CogVideoXPipeline",
                    Path = "zai-org/CogVideoX-5b-I2V",
                    BaseType = DataType.Bfloat16,
                    Backend = BackendType.Pytorch,
                    Link = "https://huggingface.co/zai-org/CogVideoX-2b",
                    MemoryProfile =
                    [
                        new MemoryProfile(DataType.Bfloat16, [ 10, 16, 54, 29, 72 ]),
                        new MemoryProfile(DataType.Float16, [ 10, 16, 54, 29, 72 ]),
                        new MemoryProfile(DataType.Float8, [ 10, 16, 54, 23, 64 ]),
                        new MemoryProfile(DataType.Int8, [ 10, 16, 54, 23, 64 ])
                    ],
                    ProcessTypes = [ ProcessType.TextToVideo ],
                    DefaultOptions = new DiffusionDefaultOptions
                    {
                        Steps = 50,
                        Width = 720,
                        Height = 480,
                        GuidanceScale = 6f,
                        Frames = 49,
                        FrameRate = 8,
                        Scheduler = SchedulerType.CogVideoXDDIM,
                        Schedulers = [SchedulerType.CogVideoXDDIM, SchedulerType.CogVideoXDPM],
                        TimestepSpacing = TimestepSpacingType.Trailing,
                        PredictionType = PredictionType.Variable,
                        FrameOptions = [ 17, 33, 49, 81, 161]
                    },
                    Resolutions =
                    [
                        new SizeOption { Width = 720, Height = 480, IsDefault = true }
                    ]
                 },
                 new DiffusionModel
                 {
                    Name = "CogVideoX_T2V_5B",
                    Pipeline = "CogVideoXPipeline",
                    Path = "zai-org/CogVideoX-5b-I2V",
                    BaseType = DataType.Bfloat16,
                    Backend = BackendType.Pytorch,
                    Link = "https://huggingface.co/zai-org/CogVideoX-5b",
                    MemoryProfile =
                    [
                        new MemoryProfile(DataType.Bfloat16, [ 8, 16, 54, 36, 72 ]),
                        new MemoryProfile(DataType.Float16, [ 8, 16, 54, 36, 72 ]),
                        new MemoryProfile(DataType.Float8, [ 8, 16, 54, 29, 72 ]),
                        new MemoryProfile(DataType.Int8, [ 8, 16, 54, 29, 72 ])
                    ],
                    ProcessTypes = [ ProcessType.TextToVideo ],
                    DefaultOptions = new DiffusionDefaultOptions
                    {
                        Steps = 50,
                        Width = 720,
                        Height = 480,
                        GuidanceScale = 6f,
                        Frames = 49,
                        FrameRate = 8,
                        Scheduler = SchedulerType.CogVideoXDDIM,
                        Schedulers = [SchedulerType.CogVideoXDDIM, SchedulerType.CogVideoXDPM],
                        TimestepSpacing = TimestepSpacingType.Trailing,
                        PredictionType = PredictionType.Variable,
                        FrameOptions = [ 17, 33, 49, 81, 161]
                    },
                    Resolutions =
                    [
                        new SizeOption { Width = 720, Height = 480, IsDefault = true }
                    ]
                 },
                 new DiffusionModel
                 {
                    Name = "CogVideoX_I2V_5B",
                    Pipeline = "CogVideoXPipeline",
                    Path = "zai-org/CogVideoX-5b-I2V",
                    BaseType = DataType.Bfloat16,
                    Backend = BackendType.Pytorch,
                    Link = "https://huggingface.co/zai-org/CogVideoX-5b-I2V",
                    MemoryProfile =
                    [
                        new MemoryProfile(DataType.Bfloat16, [ 8, 16, 54, 36, 72 ]),
                        new MemoryProfile(DataType.Float16, [ 8, 16, 54, 36, 72 ]),
                        new MemoryProfile(DataType.Float8, [ 8, 16, 54, 29, 72 ]),
                        new MemoryProfile(DataType.Int8, [ 8, 16, 54, 29, 72 ])
                    ],
                    ProcessTypes = [ProcessType.ImageToVideo ],
                    DefaultOptions = new DiffusionDefaultOptions
                    {
                        Steps = 50,
                        Width = 720,
                        Height = 480,
                        GuidanceScale = 6f,
                        Frames = 49,
                        FrameRate = 8,
                        Scheduler = SchedulerType.CogVideoXDDIM,
                        Schedulers = [SchedulerType.CogVideoXDDIM, SchedulerType.CogVideoXDPM],
                        TimestepSpacing = TimestepSpacingType.Trailing,
                        PredictionType = PredictionType.Variable,
                        FrameOptions = [ 17, 33, 49, 81, 161]
                    },
                    Resolutions =
                    [
                        new SizeOption { Width = 720, Height = 480, IsDefault = true }
                    ]
                 },
                 new DiffusionModel
                 {
                    Name = "CogVideoX_15_T2V_5B",
                    Pipeline = "CogVideoXPipeline",
                    Path = "zai-org/CogVideoX1.5-5B-I2V",
                    BaseType = DataType.Bfloat16,
                    Backend = BackendType.Pytorch,
                    Link = "https://huggingface.co/zai-org/CogVideoX1.5-5B",
                    MemoryProfile =
                    [
                        new MemoryProfile(DataType.Bfloat16, [ 8, 16, 54, 36, 72 ]),
                        new MemoryProfile(DataType.Float16, [ 8, 16, 54, 36, 72 ]),
                        new MemoryProfile(DataType.Float8, [ 8, 16, 54, 29, 72 ]),
                        new MemoryProfile(DataType.Int8, [ 8, 16, 54, 29, 72 ])
                    ],
                    ProcessTypes = [ProcessType.TextToVideo ],
                    DefaultOptions = new DiffusionDefaultOptions
                    {
                        Steps = 50,
                        Width = 1360,
                        Height = 768,
                        GuidanceScale = 5f,
                        Frames = 81,
                        FrameRate = 16,
                        Scheduler = SchedulerType.CogVideoXDDIM,
                        Schedulers = [SchedulerType.CogVideoXDDIM, SchedulerType.CogVideoXDPM],
                        TimestepSpacing = TimestepSpacingType.Trailing,
                        PredictionType = PredictionType.Variable,
                        FrameOptions = [ 17, 33, 49, 81, 161]
                    },
                    Resolutions =
                    [
                        new SizeOption { Width = 1360, Height = 768, IsDefault = true }
                    ]
                 },
                 new DiffusionModel
                 {
                    Name = "CogVideoX_15_I2V_5B",
                    Pipeline = "CogVideoXPipeline",
                    Path = "zai-org/CogVideoX1.5-5B-I2V",
                    BaseType = DataType.Bfloat16,
                    Backend = BackendType.Pytorch,
                    Link = "https://huggingface.co/zai-org/CogVideoX1.5-5B-I2V",
                    MemoryProfile =
                    [
                        new MemoryProfile(DataType.Bfloat16, [ 8, 16, 54, 36, 72 ]),
                        new MemoryProfile(DataType.Float16, [ 8, 16, 54, 36, 72 ]),
                        new MemoryProfile(DataType.Float8, [ 8, 16, 54, 29, 72 ]),
                        new MemoryProfile(DataType.Int8, [ 8, 16, 54, 29, 72 ])
                    ],
                    ProcessTypes = [ProcessType.ImageToVideo ],
                    DefaultOptions = new DiffusionDefaultOptions
                    {
                        Steps = 50,
                        Width = 1360,
                        Height = 768,
                        GuidanceScale = 5f,
                        Frames = 81,
                        FrameRate = 16,
                        Scheduler = SchedulerType.CogVideoXDDIM,
                        Schedulers = [SchedulerType.CogVideoXDDIM, SchedulerType.CogVideoXDPM],
                        TimestepSpacing = TimestepSpacingType.Trailing,
                        PredictionType = PredictionType.Variable,
                        FrameOptions = [ 17, 33, 49, 81, 161]
                    },
                    Resolutions =
                    [
                        new SizeOption { Width = 832, Height = 480, IsDefault = true }
                    ]
                 },

                 
                 // FLUX.1
                 new DiffusionModel
                 {
                    Name = "Flux1_Dev",
                    Pipeline = "FluxPipeline",
                    Path = "TensorStack/FLUX.1-schnell-ts",
                    BaseType = DataType.Bfloat16,
                    Backend = BackendType.Pytorch,
                    Link = "https://huggingface.co/TensorStack/FLUX.1-schnell-ts",
                    MemoryProfile =
                    [
                        new MemoryProfile(DataType.Bfloat16, [ 4, 21, 23, 30, 32  ]),
                        new MemoryProfile(DataType.Float16, [ 4, 21, 23, 30, 32 ]),
                        new MemoryProfile(DataType.Float8, [ 4, 14, 16, 20, 22 ]),
                        new MemoryProfile(DataType.Int8, [ 4, 14, 16, 20, 22 ])
                    ],
                    ProcessTypes = [ProcessType.TextToImage, ProcessType.ImageToImage, ProcessType.ImageInpaint, ProcessType.ControlNetImage ],
                    DefaultOptions = new DiffusionDefaultOptions
                    {
                        Steps = 30,
                        Width = 1024,
                        Height = 1024,
                        GuidanceScale = 0,
                        GuidanceScale2 = 3.5f,
                        Scheduler = SchedulerType.FlowMatchEulerDiscrete,
                        Schedulers = [SchedulerType.FlowMatchEulerDiscrete],
                        Shift = 3,
                        UseDynamicShifting = true
                    },
                    Resolutions =
                    [
                        new SizeOption { Width = 1024, Height = 1536 },
                        new SizeOption { Width = 768, Height = 1344 },
                        new SizeOption { Width = 832, Height = 1216 },
                        new SizeOption { Width = 1024, Height = 1024, IsDefault = true },
                        new SizeOption { Width = 1216, Height = 832 },
                        new SizeOption { Width = 1344, Height = 768 },
                        new SizeOption { Width = 1536, Height = 1024 },
                    ]
                 },
                 new DiffusionModel
                 {
                    Name = "Flux1_Schnell",
                    Pipeline = "FluxPipeline",
                    Path = "TensorStack/FLUX.1-dev-ts",
                    BaseType = DataType.Bfloat16,
                    Backend = BackendType.Pytorch,
                    Link = "https://huggingface.co/TensorStack/FLUX.1-dev-ts",
                    MemoryProfile =
                    [
                        new MemoryProfile(DataType.Bfloat16, [ 4, 21, 23, 30, 32  ]),
                        new MemoryProfile(DataType.Float16, [ 4, 21, 23, 30, 32 ]),
                        new MemoryProfile(DataType.Float8, [ 4, 14, 16, 20, 22 ]),
                        new MemoryProfile(DataType.Int8, [ 4, 14, 16, 20, 22 ])
                    ],
                    ProcessTypes = [ProcessType.TextToImage, ProcessType.ImageToImage, ProcessType.ImageInpaint],
                    DefaultOptions = new DiffusionDefaultOptions
                    {
                        Steps = 4,
                        Width = 1024,
                        Height = 1024,
                        GuidanceScale = 0,
                        GuidanceScale2 = 0f,
                        Scheduler = SchedulerType.FlowMatchEulerDiscrete,
                        Schedulers = [SchedulerType.FlowMatchEulerDiscrete],
                        Shift = 1,
                        UseDynamicShifting = false
                    },
                    Resolutions =
                    [
                        new SizeOption { Width = 1024, Height = 1536 },
                        new SizeOption { Width = 768, Height = 1344 },
                        new SizeOption { Width = 832, Height = 1216 },
                        new SizeOption { Width = 1024, Height = 1024, IsDefault = true },
                        new SizeOption { Width = 1216, Height = 832 },
                        new SizeOption { Width = 1344, Height = 768 },
                        new SizeOption { Width = 1536, Height = 1024 },
                    ]
                 },
                 new DiffusionModel
                 {
                    Name = "Flux1_Kontext",
                    Pipeline = "FluxPipeline",
                    Path = "TensorStack/FLUX.1-Kontext-dev-ts",
                    BaseType = DataType.Bfloat16,
                    Backend = BackendType.Pytorch,
                    Link = "https://huggingface.co/TensorStack/FLUX.1-Kontext-dev-ts",
                    MemoryProfile =
                    [
                        new MemoryProfile(DataType.Bfloat16, [ 4, 21, 23, 30, 32  ]),
                        new MemoryProfile(DataType.Float16, [ 4, 21, 23, 30, 32 ]),
                        new MemoryProfile(DataType.Float8, [ 4, 14, 16, 20, 22 ]),
                        new MemoryProfile(DataType.Int8, [ 4, 14, 16, 20, 22 ])
                    ],
                    ProcessTypes = [ProcessType.ImageEdit ],
                    DefaultOptions = new DiffusionDefaultOptions
                    {
                        Steps = 30,
                        Width = 1024,
                        GuidanceScale = 0,
                        GuidanceScale2 = 3.5f,
                        Scheduler = SchedulerType.FlowMatchEulerDiscrete,
                        Schedulers = [SchedulerType.FlowMatchEulerDiscrete],
                        Shift = 3,
                        UseDynamicShifting = true
                    },
                    Resolutions =
                    [
                        new SizeOption { Width = 1024, Height = 1536 },
                        new SizeOption { Width = 768, Height = 1344 },
                        new SizeOption { Width = 832, Height = 1216 },
                        new SizeOption { Width = 1024, Height = 1024, IsDefault = true },
                        new SizeOption { Width = 1216, Height = 832 },
                        new SizeOption { Width = 1344, Height = 768 },
                        new SizeOption { Width = 1536, Height = 1024 },
                    ]
                 },


                 // FLUX.2
                 new DiffusionModel
                 {
                    Name = "Flux2_Dev",
                    Pipeline = "Flux2Pipeline",
                    Path = "black-forest-labs/FLUX.2-dev",
                    BaseType = DataType.Bfloat16,
                    Backend = BackendType.Pytorch,
                    Link = "https://huggingface.co/black-forest-labs/FLUX.2-dev",
                    MemoryProfile =
                    [
                        new MemoryProfile(DataType.Bfloat16, [ 4, 64, 64, 128, 128 ]),
                        new MemoryProfile(DataType.Float16, [ 4, 64, 64, 128, 128 ]),
                        new MemoryProfile(DataType.Float8, [ 4, 48, 48, 80, 80]),
                        new MemoryProfile(DataType.Int8, [ 4, 48, 48, 80, 80])
                    ],
                    ProcessTypes = [ProcessType.TextToImage, ProcessType.ImageEdit],
                    DefaultOptions = new DiffusionDefaultOptions
                    {
                        Steps = 4,
                        Width = 1024,
                        Height = 1024,
                        GuidanceScale = 2.5f,
                        Scheduler = SchedulerType.FlowMatchEulerDiscrete,
                        Schedulers = [SchedulerType.FlowMatchEulerDiscrete],
                        Shift = 3,
                        UseDynamicShifting = true
                    },
                    Resolutions =
                    [
                        new SizeOption { Width = 1024, Height = 1536 },
                        new SizeOption { Width = 768, Height = 1344 },
                        new SizeOption { Width = 832, Height = 1216 },
                        new SizeOption { Width = 1024, Height = 1024, IsDefault = true },
                        new SizeOption { Width = 1216, Height = 832 },
                        new SizeOption { Width = 1344, Height = 768 },
                        new SizeOption { Width = 1536, Height = 1024 },
                    ]
                 },
                 new DiffusionModel
                 {
                    Name = "Flux2_Klein_4B",
                    Pipeline = "Flux2KleinPipeline",
                    Path = "black-forest-labs/FLUX.2-klein-base-4B",
                    BaseType = DataType.Bfloat16,
                    Backend = BackendType.Pytorch,
                    Link = "https://huggingface.co/black-forest-labs/FLUX.2-klein-base-4B",
                    MemoryProfile =
                    [
                        new MemoryProfile(DataType.Bfloat16, [4, 10, 10, 16, 18 ]),
                        new MemoryProfile(DataType.Float16, [4, 10, 10, 16, 18 ]),
                        new MemoryProfile(DataType.Float8, [ 4, 6, 6, 9, 10 ]),
                        new MemoryProfile(DataType.Int8, [ 4, 6, 6, 9, 10 ])
                    ],
                    ProcessTypes = [ProcessType.TextToImage, ProcessType.ImageToImage, ProcessType.ImageEdit],
                    DefaultOptions = new DiffusionDefaultOptions
                    {
                        Steps = 50,
                        Width = 1024,
                        Height = 1024,
                        GuidanceScale = 4f,
                        Scheduler = SchedulerType.FlowMatchEulerDiscrete,
                        Schedulers = [SchedulerType.FlowMatchEulerDiscrete],
                        Shift = 3,
                        UseDynamicShifting = true
                    },
                    Resolutions =
                    [
                        new SizeOption { Width = 1024, Height = 1536 },
                        new SizeOption { Width = 768, Height = 1344 },
                        new SizeOption { Width = 832, Height = 1216 },
                        new SizeOption { Width = 1024, Height = 1024, IsDefault = true },
                        new SizeOption { Width = 1216, Height = 832 },
                        new SizeOption { Width = 1344, Height = 768 },
                        new SizeOption { Width = 1536, Height = 1024 },
                    ]
                 },
                 new DiffusionModel
                 {
                    Name = "Flux2_Klein_4B_D",
                    Pipeline = "Flux2KleinPipeline",
                    Path = "black-forest-labs/FLUX.2-klein-base-4B",
                    BaseType = DataType.Bfloat16,
                    Backend = BackendType.Pytorch,
                    Link = "https://huggingface.co/black-forest-labs/FLUX.2-klein-4B",
                    MemoryProfile =
                    [
                        new MemoryProfile(DataType.Bfloat16, [4, 10, 10, 16, 18 ]),
                        new MemoryProfile(DataType.Float16, [4, 10, 10, 16, 18 ]),
                        new MemoryProfile(DataType.Float8, [ 4, 6, 6, 9, 10 ]),
                        new MemoryProfile(DataType.Int8, [ 4, 6, 6, 9, 10 ])
                    ],
                    ProcessTypes = [ProcessType.TextToImage, ProcessType.ImageToImage, ProcessType.ImageEdit],
                    DefaultOptions = new DiffusionDefaultOptions
                    {
                        Steps = 4,
                        Width = 1024,
                        Height = 1024,
                        GuidanceScale = 0f,
                        Scheduler = SchedulerType.FlowMatchEulerDiscrete,
                        Schedulers = [SchedulerType.FlowMatchEulerDiscrete],
                        Shift = 3,
                        UseDynamicShifting = true
                    },
                    Resolutions =
                    [
                        new SizeOption { Width = 1024, Height = 1536 },
                        new SizeOption { Width = 768, Height = 1344 },
                        new SizeOption { Width = 832, Height = 1216 },
                        new SizeOption { Width = 1024, Height = 1024, IsDefault = true },
                        new SizeOption { Width = 1216, Height = 832 },
                        new SizeOption { Width = 1344, Height = 768 },
                        new SizeOption { Width = 1536, Height = 1024 },
                    ]
                 },
                 new DiffusionModel
                 {
                    Name = "Flux2_Klein_9B",
                    Pipeline = "Flux2KleinPipeline",
                    Path = "black-forest-labs/FLUX.2-klein-base-9B",
                    BaseType = DataType.Bfloat16,
                    Backend = BackendType.Pytorch,
                    Link = "https://huggingface.co/black-forest-labs/FLUX.2-klein-base-9B",
                    MemoryProfile =
                    [
                        new MemoryProfile(DataType.Bfloat16, [4, 20, 20, 34, 35 ]),
                        new MemoryProfile(DataType.Float16, [4, 20, 20, 34, 35]),
                        new MemoryProfile(DataType.Float8, [ 4, 10, 10, 19, 20  ]),
                        new MemoryProfile(DataType.Int8, [ 4, 10, 10, 19, 20  ])
                    ],
                    ProcessTypes = [ProcessType.TextToImage, ProcessType.ImageToImage, ProcessType.ImageEdit],
                    DefaultOptions = new DiffusionDefaultOptions
                    {
                        Steps = 50,
                        Width = 1024,
                        Height = 1024,
                        GuidanceScale = 4f,
                        Scheduler = SchedulerType.FlowMatchEulerDiscrete,
                        Schedulers = [SchedulerType.FlowMatchEulerDiscrete],
                        Shift = 3,
                        UseDynamicShifting = true
                    },
                    Resolutions =
                    [
                        new SizeOption { Width = 1024, Height = 1536 },
                        new SizeOption { Width = 768, Height = 1344 },
                        new SizeOption { Width = 832, Height = 1216 },
                        new SizeOption { Width = 1024, Height = 1024, IsDefault = true },
                        new SizeOption { Width = 1216, Height = 832 },
                        new SizeOption { Width = 1344, Height = 768 },
                        new SizeOption { Width = 1536, Height = 1024 },
                    ]
                 },
                 new DiffusionModel
                 {
                    Name = "Flux2_Klein_9B_D",
                    Pipeline = "Flux2KleinPipeline",
                    Path = "black-forest-labs/FLUX.2-klein-base-9B",
                    BaseType = DataType.Bfloat16,
                    Backend = BackendType.Pytorch,
                    Link = "https://huggingface.co/black-forest-labs/FLUX.2-klein-9B",
                    MemoryProfile =
                    [
                        new MemoryProfile(DataType.Bfloat16, [4, 20, 20, 34, 35 ]),
                        new MemoryProfile(DataType.Float16, [4, 20, 20, 34, 35]),
                        new MemoryProfile(DataType.Float8, [ 4, 10, 10, 19, 20  ]),
                        new MemoryProfile(DataType.Int8, [ 4, 10, 10, 19, 20  ])
                    ],
                    ProcessTypes = [ProcessType.TextToImage, ProcessType.ImageToImage, ProcessType.ImageEdit],
                    DefaultOptions = new DiffusionDefaultOptions
                    {
                        Steps = 4,
                        Width = 1024,
                        Height = 1024,
                        GuidanceScale = 0f,
                        Scheduler = SchedulerType.FlowMatchEulerDiscrete,
                        Schedulers = [SchedulerType.FlowMatchEulerDiscrete],
                        Shift = 3,
                        UseDynamicShifting = true
                    },
                    Resolutions =
                    [
                        new SizeOption { Width = 1024, Height = 1536 },
                        new SizeOption { Width = 768, Height = 1344 },
                        new SizeOption { Width = 832, Height = 1216 },
                        new SizeOption { Width = 1024, Height = 1024, IsDefault = true },
                        new SizeOption { Width = 1216, Height = 832 },
                        new SizeOption { Width = 1344, Height = 768 },
                        new SizeOption { Width = 1536, Height = 1024 },
                    ]
                 },


                 // Kandinsky5
                 new DiffusionModel
                 {
                    Name = "Kandinsky5_T2I_Lite",
                    Pipeline = "Kandinsky5Pipeline",
                    Path = "kandinskylab/Kandinsky-5.0-I2I-Lite-pretrain-Diffusers",
                    BaseType = DataType.Bfloat16,
                    Backend = BackendType.Pytorch,
                    Link = "https://huggingface.co/kandinskylab",
                    MemoryProfile =
                    [
                        new MemoryProfile(DataType.Bfloat16, [ 8, 16, 18, 30, 32 ]),
                        new MemoryProfile(DataType.Float16, [ 8, 16, 18, 30, 32 ]),
                        new MemoryProfile(DataType.Float8, [ 8, 12, 14, 16, 18 ]),
                        new MemoryProfile(DataType.Int8, [ 8, 12, 14, 16, 18 ])
                    ],
                    ProcessTypes = [ ProcessType.TextToImage ],
                    DefaultOptions = new DiffusionDefaultOptions
                    {
                        Steps = 50,
                        Width = 1024,
                        Height = 1024,
                        GuidanceScale = 3.5f,
                        Scheduler = SchedulerType.FlowMatchEulerDiscrete,
                        Schedulers = [SchedulerType.FlowMatchEulerDiscrete],
                        Shift = 5
                    },
                    Resolutions =
                    [
                        new SizeOption { Width = 640, Height = 1408},
                        new SizeOption { Width = 768, Height = 1280},
                        new SizeOption { Width = 896, Height = 1152},
                        new SizeOption { Width = 1024, Height = 1024, IsDefault = true },
                        new SizeOption { Width = 1152, Height = 896},
                        new SizeOption { Width = 1280, Height = 768},
                        new SizeOption { Width = 1408, Height = 640},
                    ]
                 },
                 new DiffusionModel
                 {
                    Name = "Kandinsky5_I2I_Lite",
                    Pipeline = "Kandinsky5Pipeline",
                    Path = "kandinskylab/Kandinsky-5.0-I2I-Lite-pretrain-Diffusers",
                    BaseType = DataType.Bfloat16,
                    Backend = BackendType.Pytorch,
                    Link = "https://huggingface.co/kandinskylab",
                    MemoryProfile =
                    [
                        new MemoryProfile(DataType.Bfloat16, [ 8, 16, 18, 30, 32 ]),
                        new MemoryProfile(DataType.Float16, [ 8, 16, 18, 30, 32 ]),
                        new MemoryProfile(DataType.Float8, [ 8, 12, 14, 16, 18 ]),
                        new MemoryProfile(DataType.Int8, [ 8, 12, 14, 16, 18 ])
                    ],
                    ProcessTypes = [ ProcessType.ImageEdit ],
                    DefaultOptions = new DiffusionDefaultOptions
                    {
                        Steps = 50,
                        Width = 1024,
                        Height = 1024,
                        GuidanceScale = 3.5f,
                        Scheduler = SchedulerType.FlowMatchEulerDiscrete,
                        Schedulers = [SchedulerType.FlowMatchEulerDiscrete],
                        Shift = 5
                    },
                    Resolutions =
                    [
                        new SizeOption { Width = 640, Height = 1408},
                        new SizeOption { Width = 768, Height = 1280},
                        new SizeOption { Width = 896, Height = 1152},
                        new SizeOption { Width = 1024, Height = 1024, IsDefault = true },
                        new SizeOption { Width = 1152, Height = 896},
                        new SizeOption { Width = 1280, Height = 768},
                        new SizeOption { Width = 1408, Height = 640},
                    ]
                 },
                 new DiffusionModel
                 {
                    Name = "Kandinsky5_T2V_Lite",
                    Pipeline = "Kandinsky5Pipeline",
                    Path = "Kandinsky-5.0-T2V-Lite-pretrain-10s-Diffusers",
                    BaseType = DataType.Bfloat16,
                    Backend = BackendType.Pytorch,
                    Link = "https://huggingface.co/kandinskylab",
                    MemoryProfile =
                    [
                        new MemoryProfile(DataType.Bfloat16, [ 10, 32, 36, 56, 58 ]),
                        new MemoryProfile(DataType.Float16, [ 10, 32, 36, 56, 58 ]),
                        new MemoryProfile(DataType.Float8, [ 10, 21, 32, 53, 56  ]),
                        new MemoryProfile(DataType.Int8, [ 10, 21, 32, 53, 56  ])
                    ],
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
                        Shift = 5,
                        FrameOptions= [ 65, 97, 121, 137, 161, 257],
                    },
                    Resolutions =
                    [
                       new SizeOption { Width = 768, Height = 512, IsDefault = true }
                    ]
                 },
                 new DiffusionModel
                 {
                    Name = "Kandinsky5_T2V_Lite_D",
                    Pipeline = "Kandinsky5Pipeline",
                    Path = "Kandinsky-5.0-T2V-Lite-pretrain-10s-Diffusers",
                    BaseType = DataType.Bfloat16,
                    Backend = BackendType.Pytorch,
                    Link = "https://huggingface.co/kandinskylab",
                    MemoryProfile =
                    [
                        new MemoryProfile(DataType.Bfloat16, [ 10, 32, 36, 56, 58 ]),
                        new MemoryProfile(DataType.Float16, [ 10, 32, 36, 56, 58 ]),
                        new MemoryProfile(DataType.Float8, [ 10, 21, 32, 53, 56  ]),
                        new MemoryProfile(DataType.Int8, [ 10, 21, 32, 53, 56  ])
                    ],
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
                        Shift = 5,
                        FrameOptions= [ 65, 97, 121, 137, 161, 257],
                    },
                    Resolutions =
                    [
                       new SizeOption { Width = 768, Height = 512, IsDefault = true }
                    ]
                 },
                 new DiffusionModel
                 {
                    Name = "Kandinsky5_T2V_Lite_10",
                    Pipeline = "Kandinsky5Pipeline",
                    Path = "Kandinsky-5.0-T2V-Lite-pretrain-10s-Diffusers",
                    BaseType = DataType.Bfloat16,
                    Backend = BackendType.Pytorch,
                    Link = "https://huggingface.co/kandinskylab",
                    MemoryProfile =
                    [
                        new MemoryProfile(DataType.Bfloat16, [ 10, 32, 36, 56, 58 ]),
                        new MemoryProfile(DataType.Float16, [ 10, 32, 36, 56, 58 ]),
                        new MemoryProfile(DataType.Float8, [ 10, 21, 32, 53, 56  ]),
                        new MemoryProfile(DataType.Int8, [ 10, 21, 32, 53, 56  ])
                    ],
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
                        Shift = 5,
                        FrameOptions= [ 65, 97, 121, 137, 161, 241],
                    },
                    Resolutions =
                    [
                       new SizeOption { Width = 768, Height = 512, IsDefault = true }
                    ]
                 },
                 new DiffusionModel
                 {
                    Name = "Kandinsky5_T2V_Lite 10_D",
                    Pipeline = "Kandinsky5Pipeline",
                    Path = "Kandinsky-5.0-T2V-Lite-pretrain-10s-Diffusers",
                    BaseType = DataType.Bfloat16,
                    Backend = BackendType.Pytorch,
                    Link = "https://huggingface.co/kandinskylab",
                    MemoryProfile =
                    [
                        new MemoryProfile(DataType.Bfloat16, [ 10, 32, 36, 56, 68 ]),
                        new MemoryProfile(DataType.Float16, [ 10, 32, 36, 56, 68 ]),
                        new MemoryProfile(DataType.Float8, [ 10, 21, 32, 53, 58  ]),
                        new MemoryProfile(DataType.Int8, [ 10, 21, 32, 53, 58  ])
                    ],
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
                        Shift = 5,
                        FrameOptions= [ 65, 97, 121, 137, 161, 241],
                    },
                    Resolutions =
                    [
                       new SizeOption { Width = 768, Height = 512, IsDefault = true }
                    ]
                 },
                 new DiffusionModel
                 {
                    Name = "Kandinsky5_T2V_Pro",
                    Pipeline = "Kandinsky5Pipeline",
                    Path = "kandinskylab/Kandinsky-5.0-I2V-Pro-sft-5s-Diffusers",
                    BaseType = DataType.Bfloat16,
                    Backend = BackendType.Pytorch,
                    Link = "https://huggingface.co/kandinskylab",
                    MemoryProfile =
                    [
                        new MemoryProfile(DataType.Bfloat16, [10, 32, 36, 56, 68]),
                        new MemoryProfile(DataType.Float16, [10, 32, 36, 56, 68 ]),
                        new MemoryProfile(DataType.Float8, [ 10, 21, 32, 53, 64]),
                        new MemoryProfile(DataType.Int8, [ 10, 21, 32, 53, 64])
                    ],
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
                        Shift = 5,
                        FrameOptions= [ 65, 97, 121, 137, 161, 257],
                    },
                    Resolutions =
                    [
                        new SizeOption { Width = 1024, Height = 768, IsDefault = true }
                    ]
                 },
                 new DiffusionModel
                 {
                    Name = "Kandinsky5_T2V_Pro_D",
                    Pipeline = "Kandinsky5Pipeline",
                    Path = "kandinskylab/Kandinsky-5.0-I2V-Pro-sft-5s-Diffusers",
                    BaseType = DataType.Bfloat16,
                    Backend = BackendType.Pytorch,
                    Link = "https://huggingface.co/kandinskylab",
                    MemoryProfile =
                    [
                        new MemoryProfile(DataType.Bfloat16, [10, 32, 36, 56, 68]),
                        new MemoryProfile(DataType.Float16, [10, 32, 36, 56, 68 ]),
                        new MemoryProfile(DataType.Float8, [ 10, 21, 32, 53, 64]),
                        new MemoryProfile(DataType.Int8, [ 10, 21, 32, 53, 64])
                    ],
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
                        Shift = 5,
                        FrameOptions= [ 65, 97, 121, 137, 161, 257],
                    },
                    Resolutions =
                    [
                         new SizeOption { Width = 1024, Height = 768, IsDefault = true }
                    ]
                 },
                 new DiffusionModel
                 {
                    Name = "Kandinsky5_I2V_Pro",
                    Pipeline = "Kandinsky5Pipeline",
                    Path = "kandinskylab/Kandinsky-5.0-I2V-Pro-sft-5s-Diffusers",
                    BaseType = DataType.Bfloat16,
                    Backend = BackendType.Pytorch,
                    Link = "https://huggingface.co/kandinskylab",
                    MemoryProfile =
                    [
                        new MemoryProfile(DataType.Bfloat16, [10, 32, 36, 56, 68]),
                        new MemoryProfile(DataType.Float16, [10, 32, 36, 56, 68 ]),
                        new MemoryProfile(DataType.Float8, [ 10, 21, 32, 53, 64]),
                        new MemoryProfile(DataType.Int8, [ 10, 21, 32, 53, 64])
                    ],
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
                        Shift = 5,
                        FrameOptions= [ 65, 97, 121, 137, 161, 257],
                    },
                    Resolutions =
                    [
                          new SizeOption { Width = 1024, Height = 768, IsDefault = true }
                    ]
                 },
                 new DiffusionModel
                 {
                    Name = "Kandinsky5_I2V_Pro_d",
                    Pipeline = "Kandinsky5Pipeline",
                    Path = "kandinskylab/Kandinsky-5.0-I2V-Pro-sft-5s-Diffusers",
                    BaseType = DataType.Bfloat16,
                    Backend = BackendType.Pytorch,
                    Link = "https://huggingface.co/kandinskylab",
                    MemoryProfile =
                    [
                        new MemoryProfile(DataType.Bfloat16, [10, 32, 36, 56, 68]),
                        new MemoryProfile(DataType.Float16, [10, 32, 36, 56, 68 ]),
                        new MemoryProfile(DataType.Float8, [ 10, 21, 32, 53, 64]),
                        new MemoryProfile(DataType.Int8, [ 10, 21, 32, 53, 64])
                    ],
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
                        Shift = 5,
                        FrameOptions= [ 65, 97, 121, 137, 161, 257],
                    },
                    Resolutions =
                    [
                         new SizeOption { Width = 1024, Height = 768, IsDefault = true }
                    ]
                 },


                 //LTX Video
                 new DiffusionModel
                 {
                    Name = "LTX_2B",
                    Pipeline = "LTXPipeline",
                    Path = "Lightricks/LTX-Video",
                    BaseType = DataType.Bfloat16,
                    Backend = BackendType.Pytorch,
                    Link = "https://huggingface.co/Lightricks/LTX-Video",
                    MemoryProfile =
                    [
                        new MemoryProfile(DataType.Bfloat16, [ 16, 23, 32, 36, 48 ]),
                        new MemoryProfile(DataType.Float16, [ 16, 23, 32, 36, 48 ]),
                        new MemoryProfile(DataType.Float8, [ 16, 23, 23, 32, 48 ]),
                        new MemoryProfile(DataType.Int8, [ 16, 23, 23, 32, 48 ])
                    ],
                    ProcessTypes = [ProcessType.TextToVideo, ProcessType.ImageToVideo, ProcessType.VideoToVideo  ],
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
                        Shift= 1,
                        BaseShift= 0.95f,
                        MaxShift= 2.05f,
                        MaxImageSeqLen= 4096,
                        BaseImageSeqLen= 1024,
                        FrameOptions= [ 65, 97, 121, 137, 161, 257],
                    },
                    Resolutions =
                    [
                         new SizeOption { Width = 736, Height = 1280 },
                         new SizeOption { Width = 768, Height = 1152 },
                         new SizeOption { Width = 544, Height = 960 },
                         new SizeOption { Width = 512, Height = 768 },
                         new SizeOption { Width = 480, Height = 736 },
                         new SizeOption { Width = 640, Height = 640 },
                         new SizeOption { Width = 736, Height = 480 },
                         new SizeOption { Width = 768, Height = 512, IsDefault = true  },
                         new SizeOption { Width = 960, Height = 544 },
                         new SizeOption { Width = 1152, Height = 768 },
                         new SizeOption { Width = 1280, Height = 736 },
                    ]
                 },
                 new DiffusionModel
                 {
                    Name = "LTX_2B_D",
                    Pipeline = "LTXPipeline",
                    Path = "Lightricks/LTX-Video",
                    BaseType = DataType.Bfloat16,
                    Backend = BackendType.Pytorch,
                    Link = "https://huggingface.co/Lightricks/LTX-Video",
                    MemoryProfile =
                    [
                        new MemoryProfile(DataType.Bfloat16, [ 16, 23, 32, 36, 48 ]),
                        new MemoryProfile(DataType.Float16, [ 16, 23, 32, 36, 48 ]),
                        new MemoryProfile(DataType.Float8, [ 16, 23, 23, 32, 48 ]),
                        new MemoryProfile(DataType.Int8, [ 16, 23, 23, 32, 48 ])
                    ],
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
                        Shift= 1,
                        BaseShift= 0.95f,
                        MaxShift= 2.05f,
                        MaxImageSeqLen= 4096,
                        BaseImageSeqLen= 1024,
                        FrameOptions= [ 65, 97, 121, 137, 161, 257],
                    },
                    Resolutions =
                    [
                         new SizeOption { Width = 736, Height = 1280 },
                         new SizeOption { Width = 768, Height = 1152 },
                         new SizeOption { Width = 544, Height = 960 },
                         new SizeOption { Width = 512, Height = 768 },
                         new SizeOption { Width = 480, Height = 736 },
                         new SizeOption { Width = 640, Height = 640 },
                         new SizeOption { Width = 736, Height = 480 },
                         new SizeOption { Width = 768, Height = 512, IsDefault = true  },
                         new SizeOption { Width = 960, Height = 544 },
                         new SizeOption { Width = 1152, Height = 768 },
                         new SizeOption { Width = 1280, Height = 736 },
                    ]
                 },
                 new DiffusionModel
                 {
                    Name = "LTX_13B",
                    Pipeline = "LTXPipeline",
                    Path = "Lightricks/LTX-Video",
                    BaseType = DataType.Bfloat16,
                    Backend = BackendType.Pytorch,
                    Link = "https://huggingface.co/Lightricks/LTX-Video",
                    MemoryProfile =
                    [
                        new MemoryProfile(DataType.Bfloat16, [ 16, 23, 32, 36, 48 ]),
                        new MemoryProfile(DataType.Float16, [ 16, 23, 32, 36, 48 ]),
                        new MemoryProfile(DataType.Float8, [ 16, 23, 23, 32, 48 ]),
                        new MemoryProfile(DataType.Int8, [ 16, 23, 23, 32, 48 ])
                    ],
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
                        Shift= 1,
                        BaseShift= 0.95f,
                        MaxShift= 2.05f,
                        MaxImageSeqLen= 4096,
                        BaseImageSeqLen= 1024,
                        FrameOptions= [ 65, 97, 121, 137, 161, 257]
                    },
                    Resolutions =
                    [
                         new SizeOption { Width = 736, Height = 1280 },
                         new SizeOption { Width = 768, Height = 1152 },
                         new SizeOption { Width = 544, Height = 960 },
                         new SizeOption { Width = 512, Height = 768 },
                         new SizeOption { Width = 480, Height = 736 },
                         new SizeOption { Width = 640, Height = 640 },
                         new SizeOption { Width = 736, Height = 480 },
                         new SizeOption { Width = 768, Height = 512, IsDefault = true  },
                         new SizeOption { Width = 960, Height = 544 },
                         new SizeOption { Width = 1152, Height = 768 },
                         new SizeOption { Width = 1280, Height = 736 },
                    ]
                 },
                 new DiffusionModel
                 {
                    Name = "LTX_13B_D",
                    Pipeline = "LTXPipeline",
                    Path = "Lightricks/LTX-Video",
                    BaseType = DataType.Bfloat16,
                    Backend = BackendType.Pytorch,
                    Link = "https://huggingface.co/Lightricks/LTX-Video",
                    MemoryProfile =
                    [
                        new MemoryProfile(DataType.Bfloat16, [ 16, 23, 32, 36, 48 ]),
                        new MemoryProfile(DataType.Float16, [ 16, 23, 32, 36, 48 ]),
                        new MemoryProfile(DataType.Float8, [ 16, 23, 23, 32, 48 ]),
                        new MemoryProfile(DataType.Int8, [ 16, 23, 23, 32, 48 ])
                    ],
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
                        Shift= 1,
                        BaseShift= 0.95f,
                        MaxShift= 2.05f,
                        MaxImageSeqLen= 4096,
                        BaseImageSeqLen= 1024,
                        FrameOptions= [ 65, 97, 121, 137, 161, 257],
                    },
                    Resolutions =
                    [
                         new SizeOption { Width = 736, Height = 1280 },
                         new SizeOption { Width = 768, Height = 1152 },
                         new SizeOption { Width = 544, Height = 960 },
                         new SizeOption { Width = 512, Height = 768 },
                         new SizeOption { Width = 480, Height = 736 },
                         new SizeOption { Width = 640, Height = 640 },
                         new SizeOption { Width = 736, Height = 480 },
                         new SizeOption { Width = 768, Height = 512, IsDefault = true  },
                         new SizeOption { Width = 960, Height = 544 },
                         new SizeOption { Width = 1152, Height = 768 },
                         new SizeOption { Width = 1280, Height = 736 },
                    ]
                 },
                 new DiffusionModel
                 {
                    Name = "LTX2_19B",
                    Pipeline = "LTX2Pipeline",
                    Path = "Lightricks/LTX-2",
                    BaseType = DataType.Bfloat16,
                    Backend = BackendType.Pytorch,
                    Link = "https://huggingface.co/Lightricks/LTX-2",
                    MemoryProfile =
                    [
                        new MemoryProfile(DataType.Bfloat16, [ 12, 42, 42, 42, 92 ]),
                        new MemoryProfile(DataType.Float16, [ 12, 42, 42, 42, 92 ]),
                        new MemoryProfile(DataType.Float8, [ 12, 23, 35, 35, 74]),
                        new MemoryProfile(DataType.Int8, [ 12, 23, 35, 35, 74])
                    ],
                    ProcessTypes = [ProcessType.TextToVideo, ProcessType.ImageToVideo ],
                    DefaultOptions = new DiffusionDefaultOptions
                    {
                        Steps = 40,
                        Width = 768,
                        Height = 512,
                        GuidanceScale = 4f,
                        Frames = 121,
                        FrameRate = 25,
                        Scheduler = SchedulerType.FlowMatchEulerDiscrete,
                        Schedulers = [SchedulerType.FlowMatchEulerDiscrete],
                        Shift= 1,
                        BaseShift= 0.95f,
                        MaxShift= 2.05f,
                        MaxImageSeqLen= 4096,
                        BaseImageSeqLen= 1024,
                        UseDynamicShifting=true,
                        FrameOptions= [ 65, 97, 121, 137, 161, 257],
                    },
                    Resolutions =
                    [
                         new SizeOption { Width = 2176, Height = 3840 },
                         new SizeOption { Width = 864, Height = 2048 },
                         new SizeOption { Width = 1088, Height = 1920 },
                         new SizeOption { Width = 736, Height = 1280 },
                         new SizeOption { Width = 768, Height = 1152 },
                         new SizeOption { Width = 544, Height = 960 },
                         new SizeOption { Width = 512, Height = 768 },
                         new SizeOption { Width = 480, Height = 736 },
                         new SizeOption { Width = 640, Height = 640 },
                         new SizeOption { Width = 736, Height = 480 },
                         new SizeOption { Width = 768, Height = 512, IsDefault = true  },
                         new SizeOption { Width = 960, Height = 544 },
                         new SizeOption { Width = 1152, Height = 768 },
                         new SizeOption { Width = 1280, Height = 736 },
                         new SizeOption { Width = 1920, Height = 1088 },
                         new SizeOption { Width = 2048, Height = 864 },
                         new SizeOption { Width = 3840, Height = 2176 }
                    ]
                 },
                 new DiffusionModel
                 {
                    Name = "LTX2_19B_D",
                    Pipeline = "LTX2Pipeline",
                    Path = "Lightricks/LTX-2",
                    BaseType = DataType.Bfloat16,
                    Backend = BackendType.Pytorch,
                    Link = "https://huggingface.co/Lightricks/LTX-2",
                    MemoryProfile =
                    [
                        new MemoryProfile(DataType.Bfloat16, [ 12, 42, 42, 42, 92 ]),
                        new MemoryProfile(DataType.Float16, [ 12, 42, 42, 42, 92 ]),
                        new MemoryProfile(DataType.Float8, [ 12, 23, 35, 35, 74]),
                        new MemoryProfile(DataType.Int8, [ 12, 23, 35, 35, 74])
                    ],
                    ProcessTypes = [ProcessType.TextToVideo, ProcessType.ImageToVideo ],
                    DefaultOptions = new DiffusionDefaultOptions
                    {
                        Steps = 8,
                        Width = 768,
                        Height = 512,
                        GuidanceScale = 0f,
                        Frames = 121,
                        FrameRate = 25,
                        Scheduler = SchedulerType.FlowMatchEulerDiscrete,
                        Schedulers = [SchedulerType.FlowMatchEulerDiscrete],
                        Shift= 1,
                        BaseShift= 0.95f,
                        MaxShift= 2.05f,
                        MaxImageSeqLen= 4096,
                        BaseImageSeqLen= 1024,
                        UseDynamicShifting=true,
                        FrameOptions= [ 65, 97, 121, 137, 161, 257],
                    },
                    Resolutions =
                    [
                         new SizeOption { Width = 2176, Height = 3840 },
                         new SizeOption { Width = 864, Height = 2048 },
                         new SizeOption { Width = 1088, Height = 1920 },
                         new SizeOption { Width = 736, Height = 1280 },
                         new SizeOption { Width = 768, Height = 1152 },
                         new SizeOption { Width = 544, Height = 960 },
                         new SizeOption { Width = 512, Height = 768 },
                         new SizeOption { Width = 480, Height = 736 },
                         new SizeOption { Width = 640, Height = 640 },
                         new SizeOption { Width = 736, Height = 480 },
                         new SizeOption { Width = 768, Height = 512, IsDefault = true  },
                         new SizeOption { Width = 960, Height = 544 },
                         new SizeOption { Width = 1152, Height = 768 },
                         new SizeOption { Width = 1280, Height = 736 },
                         new SizeOption { Width = 1920, Height = 1088 },
                         new SizeOption { Width = 2048, Height = 864 },
                         new SizeOption { Width = 3840, Height = 2176 }
                    ]
                 },


                 // Qwen Image
                 new DiffusionModel
                 {
                    Name = "Qwen_Base",
                    Pipeline = "QwenImagePipeline",
                    Path = "Qwen/Qwen-Image-2512",
                    BaseType = DataType.Bfloat16,
                    Backend = BackendType.Pytorch,
                    Link = "https://huggingface.co/Qwen/Qwen-Image-2512",
                    MemoryProfile =
                    [
                        new MemoryProfile(DataType.Bfloat16, [ 5, 38, 40, 58, 60 ]),
                        new MemoryProfile(DataType.Float16, [ 5, 38, 40, 58, 60  ]),
                        new MemoryProfile(DataType.Float8, [ 5, 22, 25, 30, 32 ]),
                        new MemoryProfile(DataType.Int8, [ 5, 22, 25, 30, 32 ])
                    ],
                    ProcessTypes = [ProcessType.TextToImage, ProcessType.ImageToImage, ProcessType.ImageInpaint, ProcessType.ControlNetImage],
                    DefaultOptions = new DiffusionDefaultOptions
                    {
                        Steps = 30,
                        Width = 1328,
                        Height = 1328,
                        GuidanceScale = 1f,
                        GuidanceScale2 = 1f,
                        Scheduler = SchedulerType.FlowMatchEulerDiscrete,
                        Schedulers = [SchedulerType.FlowMatchEulerDiscrete],
                        MaxShift= 0.9f,
                        MaxImageSeqLen= 8192,
                        UseDynamicShifting= true
                    },
                    Resolutions =
                    [
                        new SizeOption { Width = 1664, Height = 928},
                        new SizeOption { Width = 1584, Height = 1056},
                        new SizeOption { Width = 1472, Height = 1104},
                        new SizeOption { Width = 1328, Height = 1328, IsDefault = true },
                        new SizeOption { Width = 1104, Height = 1472},
                        new SizeOption { Width = 1056, Height = 1584},
                        new SizeOption { Width = 928, Height = 1664}
                    ]
                 },
                 new DiffusionModel
                 {
                    Name = "Qwen_Edit",
                    Pipeline = "QwenImagePipeline",
                    Path = "Qwen/Qwen-Image-Edit-2512",
                    BaseType = DataType.Bfloat16,
                    Backend = BackendType.Pytorch,
                    Link = "https://huggingface.co/Qwen/Qwen-Image-Edit-2512",
                    MemoryProfile =
                    [
                        new MemoryProfile(DataType.Bfloat16, [ 5, 38, 40, 58, 60 ]),
                        new MemoryProfile(DataType.Float16, [ 5, 38, 40, 58, 60  ]),
                        new MemoryProfile(DataType.Float8, [ 5, 22, 25, 30, 32 ]),
                        new MemoryProfile(DataType.Int8, [ 5, 22, 25, 30, 32 ])
                    ],
                    ProcessTypes = [ProcessType.ImageEdit],
                    DefaultOptions = new DiffusionDefaultOptions
                    {
                        Steps = 30,
                        Width = 1024,
                        Height = 1024,
                        GuidanceScale = 1f,
                        GuidanceScale2 = 1f,
                        Scheduler = SchedulerType.FlowMatchEulerDiscrete,
                        Schedulers = [SchedulerType.FlowMatchEulerDiscrete],
                        MaxShift= 0.9f,
                        MaxImageSeqLen= 8192,
                        UseDynamicShifting= true
                    },
                    Resolutions =
                    [
                        new SizeOption { Width = 1664, Height = 928},
                        new SizeOption { Width = 1584, Height = 1056},
                        new SizeOption { Width = 1472, Height = 1104},
                        new SizeOption { Width = 1328, Height = 1328, IsDefault = true },
                        new SizeOption { Width = 1104, Height = 1472},
                        new SizeOption { Width = 1056, Height = 1584},
                        new SizeOption { Width = 928, Height = 1664}
                    ]
                 },

                 //StableDiffusionXL
                 new DiffusionModel
                 {
                    Name = "StableDiffusionXL_Base",
                    Pipeline = "StableDiffusionXLPipeline",
                    Path = "stabilityai/stable-diffusion-xl-base-1.0",
                    BaseType = DataType.Bfloat16,
                    Backend = BackendType.Pytorch,
                    Link = "https://huggingface.co/stabilityai/stable-diffusion-xl-base-1.0",
                    MemoryProfile =
                    [
                        new MemoryProfile(DataType.Bfloat16, [2, 6, 8, 14, 16 ]),
                        new MemoryProfile(DataType.Float16, [2, 6, 8, 14, 16 ]),
                        new MemoryProfile(DataType.Float8, [2, 4, 6, 8, 10]),
                        new MemoryProfile(DataType.Int8, [2, 4, 6, 8, 10])
                    ],
                    ProcessTypes = [ProcessType.TextToImage, ProcessType.ImageToImage, ProcessType.ImageInpaint, ProcessType.ControlNetImage, ProcessType.ControlNetImageToImage],
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
                        new SizeOption { Width = 640, Height = 1536 },
                        new SizeOption { Width = 768, Height = 1344 },
                        new SizeOption { Width = 832, Height = 1280 },
                        new SizeOption { Width = 896, Height = 1152 },
                        new SizeOption { Width = 768, Height = 768 },
                        new SizeOption { Width = 1024, Height = 1024, IsDefault = true },
                        new SizeOption { Width = 1152, Height = 896 },
                        new SizeOption { Width = 1280, Height = 832 },
                        new SizeOption { Width = 1344, Height = 768 },
                        new SizeOption { Width = 1536, Height = 640 },
                    ]
                 },
                 new DiffusionModel
                 {
                    Name = "StableDiffusionXL_Turbo",
                    Pipeline = "StableDiffusionXLPipeline",
                    Path = "stabilityai/sdxl-turbo",
                    BaseType = DataType.Bfloat16,
                    Backend = BackendType.Pytorch,
                    Link = "https://huggingface.co/stabilityai/sdxl-turbo",
                    MemoryProfile =
                    [
                        new MemoryProfile(DataType.Bfloat16, [2, 6, 8, 14, 16 ]),
                        new MemoryProfile(DataType.Float16, [2, 6, 8, 14, 16 ]),
                        new MemoryProfile(DataType.Float8, [2, 4, 6, 8, 10]),
                        new MemoryProfile(DataType.Int8, [2, 4, 6, 8, 10])
                    ],
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
                        new SizeOption { Width = 648, Height = 1152 },
                        new SizeOption { Width = 768, Height = 1024 },
                        new SizeOption { Width = 512, Height = 896 },
                        new SizeOption { Width = 1152, Height = 768 },
                        new SizeOption { Width = 576, Height = 768 },
                        new SizeOption { Width = 512, Height = 512, IsDefault = true },
                        new SizeOption { Width = 768, Height = 768 },
                        new SizeOption { Width = 768, Height = 576 },
                        new SizeOption { Width = 768, Height = 1152 },
                        new SizeOption { Width = 896, Height = 512 },
                        new SizeOption { Width = 1024, Height = 768 },
                        new SizeOption { Width = 1152, Height = 648 },
                    ]
                 },
                 new DiffusionModel
                 {
                    Name = "StableDiffusionXL_Lightning",
                    Pipeline = "StableDiffusionXLPipeline",
                    Path = "stabilityai/stable-diffusion-xl-base-1.0",
                    BaseType = DataType.Bfloat16,
                    Backend = BackendType.Pytorch,
                    Link = "https://huggingface.co/stabilityai/stable-diffusion-xl-base-1.0",
                    MemoryProfile =
                    [
                        new MemoryProfile(DataType.Bfloat16, [2, 6, 8, 14, 16 ]),
                        new MemoryProfile(DataType.Float16, [2, 6, 8, 14, 16 ]),
                        new MemoryProfile(DataType.Float8, [2, 4, 6, 8, 10]),
                        new MemoryProfile(DataType.Int8, [2, 4, 6, 8, 10])
                    ],
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
                        new SizeOption { Width = 640, Height = 1536 },
                        new SizeOption { Width = 768, Height = 1344 },
                        new SizeOption { Width = 832, Height = 1280 },
                        new SizeOption { Width = 896, Height = 1152 },
                        new SizeOption { Width = 768, Height = 768 },
                        new SizeOption { Width = 1024, Height = 1024, IsDefault = true },
                        new SizeOption { Width = 1152, Height = 896 },
                        new SizeOption { Width = 1280, Height = 832 },
                        new SizeOption { Width = 1344, Height = 768 },
                        new SizeOption { Width = 1536, Height = 640 },
                    ]
                 },

                  // WAN 2.1
                 new DiffusionModel
                 {
                    Name = "Wan21_T2V_1B",
                    Pipeline = "WanPipeline",
                    Path = "Wan-AI/Wan2.1-I2V-14B-480P-Diffusers",
                    BaseType = DataType.Bfloat16,
                    Backend = BackendType.Pytorch,
                    Link = "https://huggingface.co/Wan-AI",
                    MemoryProfile =
                    [
                        new MemoryProfile(DataType.Bfloat16, [ 8, 12, 17, 19, 31 ]),
                        new MemoryProfile(DataType.Float16, [ 8, 12, 17, 19, 31 ]),
                        new MemoryProfile(DataType.Float8, [ 6, 10, 17, 14, 26 ]),
                        new MemoryProfile(DataType.Int8, [ 6, 10, 17, 14, 26 ])
                    ],
                    ProcessTypes = [ProcessType.TextToVideo ],
                    DefaultOptions = new DiffusionDefaultOptions
                    {
                        Steps = 50,
                        Width = 832,
                        Height = 480,
                        GuidanceScale = 5f,
                        Frames = 81,
                        FrameRate = 16,
                        Scheduler = SchedulerType.UniPC,
                        Schedulers = [SchedulerType.UniPC, SchedulerType.FlowMatchEulerDiscrete],
                        BetaStart= 0.0001f,
                        BetaEnd= 0.02f,
                        BetaSchedule= BetaScheduleType.Linear,
                        TimestepSpacing= TimestepSpacingType.Linspace,
                        PredictionType=  PredictionType.FlowPrediction,
                        SolverType=  SolverType.BH2,
                        StepsOffset= 0,
                        Shift= 3,
                        BaseShift= 0.5f,
                        MaxShift= 1.15f,
                        FrameOptions = [ 17, 33, 49, 81, 161]
                    },
                    Resolutions =
                    [
                        new SizeOption { Width = 832, Height = 480, IsDefault = true }
                    ]
                 },
                 new DiffusionModel
                 {
                    Name = "Wan21_T2V_14B",
                    Pipeline = "WanPipeline",
                    Path = "Wan-AI/Wan2.1-I2V-14B-480P-Diffusers",
                    BaseType = DataType.Bfloat16,
                    Backend = BackendType.Pytorch,
                    Link = "https://huggingface.co/Wan-AI",
                    MemoryProfile =
                    [
                        new MemoryProfile(DataType.Bfloat16, [ 8, 17, 34, 40, 52  ]),
                        new MemoryProfile(DataType.Float16, [ 8, 17, 34, 40, 52  ]),
                        new MemoryProfile(DataType.Float8, [ 8, 21, 21, 32, 38]),
                        new MemoryProfile(DataType.Int8, [ 8, 21, 21, 32, 38])
                    ],
                    ProcessTypes = [ProcessType.TextToVideo ],
                    DefaultOptions = new DiffusionDefaultOptions
                    {
                        Steps = 50,
                        Width = 832,
                        Height = 480,
                        GuidanceScale = 5f,
                        Frames = 81,
                        FrameRate = 16,
                        Scheduler = SchedulerType.UniPC,
                        Schedulers = [SchedulerType.UniPC, SchedulerType.FlowMatchEulerDiscrete],
                        BetaStart= 0.0001f,
                        BetaEnd= 0.02f,
                        BetaSchedule= BetaScheduleType.Linear,
                        TimestepSpacing= TimestepSpacingType.Linspace,
                        PredictionType=  PredictionType.FlowPrediction,
                        SolverType=  SolverType.BH2,
                        StepsOffset= 0,
                        Shift= 3,
                        BaseShift= 0.5f,
                        MaxShift= 1.15f,
                        FrameOptions = [ 17, 33, 49, 81, 161]
                    },
                    Resolutions =
                    [
                        new SizeOption { Width = 832, Height = 480, IsDefault = true },
                        new SizeOption { Width = 1280, Height = 720 }
                    ]
                 },
                 new DiffusionModel
                 {
                    Name = "Wan21_I2V_14B_480",
                    Pipeline = "WanPipeline",
                    Path = "Wan-AI/Wan2.1-I2V-14B-480P-Diffusers",
                    BaseType = DataType.Bfloat16,
                    Backend = BackendType.Pytorch,
                    Link = "https://huggingface.co/Wan-AI",
                    MemoryProfile =
                    [
                        new MemoryProfile(DataType.Bfloat16, [ 8, 17, 34, 40, 52  ]),
                        new MemoryProfile(DataType.Float16, [ 8, 17, 34, 40, 52  ]),
                        new MemoryProfile(DataType.Float8, [ 8, 21, 21, 32, 38]),
                        new MemoryProfile(DataType.Int8, [ 8, 21, 21, 32, 38])
                    ],
                    ProcessTypes = [ProcessType.ImageToVideo ],
                    DefaultOptions = new DiffusionDefaultOptions
                    {
                        Steps = 50,
                        Width = 832,
                        Height = 480,
                        GuidanceScale = 5f,
                        Frames = 81,
                        FrameRate = 16,
                        Scheduler = SchedulerType.UniPC,
                        Schedulers = [SchedulerType.UniPC, SchedulerType.FlowMatchEulerDiscrete],
                        BetaStart= 0.0001f,
                        BetaEnd= 0.02f,
                        BetaSchedule= BetaScheduleType.Linear,
                        TimestepSpacing= TimestepSpacingType.Linspace,
                        PredictionType=  PredictionType.FlowPrediction,
                        SolverType=  SolverType.BH2,
                        StepsOffset= 0,
                        Shift= 3,
                        BaseShift= 0.5f,
                        MaxShift= 1.15f,
                        FrameOptions = [ 17, 33, 49, 81, 161]
                    },
                    Resolutions =
                    [
                        new SizeOption { Width = 832, Height = 480, IsDefault = true }
                    ]
                 },
                 new DiffusionModel
                 {
                    Name = "Wan21_I2V_14B_720",
                    Pipeline = "WanPipeline",
                    Path = "Wan-AI/Wan2.1-I2V-14B-480P-Diffusers",
                    BaseType = DataType.Bfloat16,
                    Backend = BackendType.Pytorch,
                    Link = "https://huggingface.co/Wan-AI",
                    MemoryProfile =
                    [
                        new MemoryProfile(DataType.Bfloat16, [ 8, 17, 34, 40, 52  ]),
                        new MemoryProfile(DataType.Float16, [ 8, 17, 34, 40, 52  ]),
                        new MemoryProfile(DataType.Float8, [ 8, 21, 21, 32, 38]),
                        new MemoryProfile(DataType.Int8, [ 8, 21, 21, 32, 38])
                    ],
                    ProcessTypes = [ProcessType.ImageToVideo ],
                    DefaultOptions = new DiffusionDefaultOptions
                    {
                        Steps = 50,
                        Width = 1280,
                        Height = 720,
                        GuidanceScale = 5f,
                        Frames = 81,
                        FrameRate = 16,
                        Scheduler = SchedulerType.UniPC,
                        Schedulers = [SchedulerType.UniPC, SchedulerType.FlowMatchEulerDiscrete],
                        BetaStart= 0.0001f,
                        BetaEnd= 0.02f,
                        BetaSchedule= BetaScheduleType.Linear,
                        TimestepSpacing= TimestepSpacingType.Linspace,
                        PredictionType=  PredictionType.FlowPrediction,
                        SolverType=  SolverType.BH2,
                        StepsOffset= 0,
                        Shift= 3,
                        BaseShift= 0.5f,
                        MaxShift= 1.15f,
                        FrameOptions = [ 17, 33, 49, 81, 161]
                    },
                    Resolutions =
                    [
                        new SizeOption { Width = 1280, Height = 720, IsDefault = true }
                    ]
                 },


                 // Z-Image Turbo
                 new DiffusionModel
                 {
                    Name = "ZImage_Turbo",
                    Pipeline = "ZImagePipeline",
                    Path = "Tongyi-MAI/Z-Image-Turbo",
                    BaseType = DataType.Bfloat16,
                    Backend = BackendType.Pytorch,
                    Link = "https://huggingface.co/Tongyi-MAI/Z-Image-Turbo",
                    MemoryProfile =
                    [
                        new MemoryProfile(DataType.Bfloat16, [ 4, 12, 14, 22, 24 ]),
                        new MemoryProfile(DataType.Float16, [ 4, 12, 14, 22, 24 ]),
                        new MemoryProfile(DataType.Float8, [ 4, 8, 10, 14, 16 ]),
                        new MemoryProfile(DataType.Int8, [ 4, 8, 10, 14, 16 ])
                    ],
                    ProcessTypes = [ProcessType.TextToImage, ProcessType.ImageToImage, ProcessType.ImageInpaint, ProcessType.ControlNetImage],
                    DefaultOptions = new DiffusionDefaultOptions
                    {
                        Steps = 9,
                        Width = 1024,
                        Height = 1024,
                        GuidanceScale = 0,
                        Scheduler = SchedulerType.FlowMatchEulerDiscrete,
                        Schedulers = [SchedulerType.FlowMatchEulerDiscrete],
                        Shift = 3
                    },
                    Resolutions =
                    [
                        new SizeOption { Width = 1664, Height = 928},
                        new SizeOption { Width = 1584, Height = 1056},
                        new SizeOption { Width = 1472, Height = 1104},
                        new SizeOption { Width = 1024, Height = 1024, IsDefault = true },
                        new SizeOption { Width = 1328, Height = 1328},
                        new SizeOption { Width = 1408, Height = 1408 },
                        new SizeOption { Width = 1104, Height = 1472},
                        new SizeOption { Width = 1056, Height = 1584},
                        new SizeOption { Width = 928, Height = 1664}
                    ]
                 },
             ];
        }


        /// <summary>
        /// Creates the wizard model templates.
        /// </summary>
        /// <returns>System.Collections.Generic.List&lt;Diffuse.Common.WizardItemModel&gt;.</returns>
        public static List<WizardItemModel> CreateWizardModelTemplates()
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
                        new WizardOptionModel{ Name = "T2V 5B",      Template = "CogVideoX_T2V_5B"},
                        new WizardOptionModel{ Name = "I2V 5B",      Template = "CogVideoX_I2V_5B"},
                        new WizardOptionModel{ Name = "v1.5 T2V 5B", Template = "CogVideoX_15_T2V_5B"},
                        new WizardOptionModel{ Name = "v1.5 I2V 5B", Template = "CogVideoX_15_I2V_5B"}
                    ]
                },
                new WizardItemModel
                {
                    Name = "FLUX.1",
                    Options =
                    [
                        new WizardOptionModel{ Name = "Dev",     Template = "Flux1_Dev"},
                        new WizardOptionModel{ Name = "Schnell", Template = "Flux1_Schnell"},
                        new WizardOptionModel{ Name = "Kontext", Template = "Flux1_Kontext"}
                    ]
                },
                new WizardItemModel
                {
                    Name = "FLUX.2",
                    Options =
                    [
                        new WizardOptionModel{ Name = "Dev",            Template = "Flux2_Dev"},
                        new WizardOptionModel{ Name = "Klein 4B",       Template = "Flux2_Klein_4B_D"},
                        new WizardOptionModel{ Name = "Klein 4B Base",  Template = "Flux2_Klein_4B"},
                        new WizardOptionModel{ Name = "Klein 9B",       Template = "Flux2_Klein_9B_D"},
                        new WizardOptionModel{ Name = "Klein 9B Base",  Template = "Flux2_Klein_9B"},
                    ]
                },
                new WizardItemModel
                {
                    Name = "Kandinsky5",
                    Options =
                    [
                        new WizardOptionModel{ Name = "T2I Lite",                 Template = "Kandinsky5_T2I_Lite"},
                        new WizardOptionModel{ Name = "I2I Lite",                 Template = "Kandinsky5_I2I_Lite"},
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
                        new WizardOptionModel{ Name = "2B",              Template = "LTX_2B"},
                        new WizardOptionModel{ Name = "2B (distilled)",  Template = "LTX_2B_D"},
                        new WizardOptionModel{ Name = "13B",             Template = "LTX_13B"},
                        new WizardOptionModel{ Name = "13B (distilled)", Template = "LTX_13B_D"}
                    ]
                },
                new WizardItemModel
                {
                    Name = "LTX-2",
                    Options =
                    [
                        new WizardOptionModel{ Name = "19B",              Template = "LTX2_19B"},
                        new WizardOptionModel{ Name = "19B (distilled)",  Template = "LTX2_19B_D"}
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
                        new WizardOptionModel{ Name = "Base",       Template = "StableDiffusionXL_Base"},
                        new WizardOptionModel{ Name = "Turbo",      Template = "StableDiffusionXL_Turbo"},
                        new WizardOptionModel{ Name = "Lightning",  Template = "StableDiffusionXL_Lightning"}
                    ]
                },
                new WizardItemModel
                {
                    Name = "WAN 2.1",
                    Options =
                    [
                        new WizardOptionModel{ Name = "T2V (1B)",       Template = "Wan21_T2V_1B"},
                        new WizardOptionModel{ Name = "T2V (14B)",      Template = "Wan21_T2V_14B"},
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
    }
}
