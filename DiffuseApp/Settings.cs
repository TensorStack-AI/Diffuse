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
        public VendorType Vendor { get; set; }
        public string DirectoryTemp { get; set; }
        public string DirectoryModel { get; set; }
        public string DirectoryCache { get; set; }
        public string DirectoryHistory { get; set; }
        public string SecureToken { get; set; }
        public int ReadBuffer { get; set; } = 32;
        public int WriteBuffer { get; set; } = 32;
        public string VideoCodec { get; set; } = "mp4v";
        public ObservableCollection<UpscaleModel> UpscaleModels { get; set; }
        public ObservableCollection<DiffusionModel> DiffusionModels { get; set; }
        public ObservableCollection<LoraAdapterModel> LoraAdapterModels { get; set; }
        public ObservableCollection<ControlNetModel> ControlNetModels { get; set; }
        public ObservableCollection<ExtractorModel> ExtractorModels { get; set; }

        [JsonIgnore]
        public Device DefaultDevice { get; set; }

        [JsonIgnore]
        public List<Device> Devices { get; set; }
        public bool IsLegacyDeviceDetection { get; set; }
        public int ProcessId { get; set; } = Environment.ProcessId;

        public int MaxHistory { get; set; } = 500;

        public void Initialize()
        {
            Directory.CreateDirectory(DirectoryModel);
            Directory.CreateDirectory(DirectoryTemp);
            Directory.CreateDirectory(DirectoryHistory);


            Provider.Initialize();
            Devices = Provider.GetDevices()
                .Where(x => x.Type == DeviceType.GPU && x.HardwareVendorId == ((int)Vendor))
                .ToList();
            DefaultDevice = Devices.FirstOrDefault();

            ScanModels();
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
            if (pipeline.ExtractorModel != null)
            {
                var defaultModel = ExtractorModels.FirstOrDefault(x => x.IsDefault);
                if (defaultModel is not null)
                    defaultModel.IsDefault = false;

                pipeline.ExtractorModel.IsDefault = true;
            }

            await Json.SaveAsync<Settings>("Settings.json", this);
        }


        private void ScanModels()
        {
            var upscaleDirectory = Path.Combine(DirectoryModel, "Upscale");
            foreach (var upscaleModel in UpscaleModels)
                upscaleModel.Initialize(upscaleDirectory);


            var diffusionDirectory = Path.Combine(DirectoryModel, "Diffusion");
        }
    }
}
