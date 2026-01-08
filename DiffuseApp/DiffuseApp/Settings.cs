using Diffuse.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TensorStack.Common;
using TensorStack.Providers;
using TensorStack.WPF;

namespace Diffuse
{
    public class Settings : IUIConfiguration
    {
        public string DirectoryTemp { get; set; }
        public string DirectoryModel { get; set; }
        public string DirectoryCache { get; set; }
        public string DirectoryHistory { get; set; }
        public string SecureToken { get; set; }
        public int ReadBuffer { get; set; } = 32;
        public int WriteBuffer { get; set; } = 32;
        public string VideoCodec { get; set; } = "mp4v";
        public ObservableCollection<EnvironmentModel> Environments { get; set; }
        public ObservableCollection<UpscaleModel> UpscaleModels { get; set; }
        public ObservableCollection<DiffusionModel> DiffusionModels { get; set; }
        public ObservableCollection<LoraAdapterModel> LoraAdapterModels { get; set; }
        public ObservableCollection<ControlNetModel> ControlNetModels { get; set; }
        public ObservableCollection<ExtractModel> ExtractModels { get; set; }

        [JsonIgnore]
        public Device DefaultDevice { get; set; }

        [JsonIgnore]
        public List<Device> Devices { get; set; }
        public bool IsLegacyDeviceDetection { get; set; }


        public int MaxHistory { get; set; } = 500;


        public void Initialize(string directoryData)
        {
            if (string.IsNullOrEmpty(DirectoryTemp) || !Path.Exists(DirectoryTemp))
                DirectoryTemp = Path.Combine(directoryData, "Temp");
            if (string.IsNullOrEmpty(DirectoryModel) || !Path.Exists(DirectoryModel))
                DirectoryModel = Path.Combine(directoryData, "Models");
            if (string.IsNullOrEmpty(DirectoryHistory) || !Path.Exists(DirectoryHistory))
                DirectoryHistory = Path.Combine(directoryData, "History");
            if (string.IsNullOrEmpty(DirectoryCache) || !Path.Exists(DirectoryCache))
            {
                var huggingfaceCache = Environment.GetEnvironmentVariable("HUGGINGFACE_HUB_CACHE");
                if (!Directory.Exists(huggingfaceCache))
                    huggingfaceCache = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".cache", "huggingface", "hub");

                DirectoryCache = Directory.Exists(huggingfaceCache)
                    ? huggingfaceCache
                    : Path.Combine(directoryData, "Models");
            }

            Directory.CreateDirectory(DirectoryTemp);
            Directory.CreateDirectory(DirectoryModel);
            Directory.CreateDirectory(DirectoryCache);
            Directory.CreateDirectory(DirectoryHistory);

            Provider.Initialize();
            Devices = Provider.GetDevices()
                .Where(x => x.Type == DeviceType.GPU && !string.IsNullOrEmpty(x.HardwareVendor))
                .ToList();
            DefaultDevice = Devices.FirstOrDefault();

            ScanModels();
            Json.Save(App.FileSettings, this);
        }


        public async Task SetDefaultsAsync(PipelineModel pipeline)
        {
            if (pipeline.DiffusionModel != null)
            {
                var defaultModel = DiffusionModels.FirstOrDefault(x => x.IsDefault);
                if (defaultModel is not null)
                    defaultModel.IsDefault = false;

                pipeline.DiffusionModel.IsDefault = true;
            }
            if (pipeline.UpscaleModel != null)
            {
                var defaultModel = UpscaleModels.FirstOrDefault(x => x.IsDefault);
                if (defaultModel is not null)
                    defaultModel.IsDefault = false;

                pipeline.UpscaleModel.IsDefault = true;
            }
            if (pipeline.ExtractModel != null)
            {
                var defaultModel = ExtractModels.FirstOrDefault(x => x.IsDefault);
                if (defaultModel is not null)
                    defaultModel.IsDefault = false;

                pipeline.ExtractModel.IsDefault = true;
            }

            await Json.SaveAsync(App.FileSettings, this);
        }


        public HashSet<string> GetPipelines()
        {
            var pipelines = new HashSet<string>(
            [
                "ChromaPipeline",
                "CogVideoXPipeline",
                "FluxPipeline",
                "Flux2Pipeline",
                "Kandinsky5Pipeline",
                "LTXPipeline",
                "QwenImagePipeline",
                "StableDiffusionXLPipeline",
                "WanPipeline",
                "ZImagePipeline"
            ]);

            foreach (var pipeline in DiffusionModels.Select(x => x.Pipeline).Distinct())
            {
                pipelines.Add(pipeline);
            }
            return pipelines;
        }


        public void ScanModels()
        {
            var upscaleDirectory = Path.Combine(DirectoryModel, "Upscale");
            foreach (var upscaleModel in UpscaleModels)
                upscaleModel.Initialize(upscaleDirectory);
            var extractDirectory = Path.Combine(DirectoryModel, "Extract");
            foreach (var extractModel in ExtractModels)
                extractModel.Initialize(extractDirectory);
            foreach (var diffusionModel in DiffusionModels)
                diffusionModel.Initialize(DirectoryCache);
            foreach (var loraAdapterModel in LoraAdapterModels)
                loraAdapterModel.Initialize(DirectoryCache);
            foreach (var controlNetModel in ControlNetModels)
                controlNetModel.Initialize(DirectoryCache);
        }
    }
}
