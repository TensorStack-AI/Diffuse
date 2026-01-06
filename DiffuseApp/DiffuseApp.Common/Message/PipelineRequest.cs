using System.Collections.Generic;
using System.Text.Json.Serialization;
using TensorStack.Common;
using TensorStack.Common.Tensor;
using TensorStack.Python.Common;
using TensorStack.Python.Config;

namespace DiffuseApp.Common.Message
{
    internal class PipelineRequest : IPipelineMessage
    {
        public PipelineRequest() { }
        public PipelineRequest(RequestType type)
        {
            Type = type;
        }

        public PipelineRequest(PipelineConfig config)
        {
            PipelineConfig = config;
            Type = RequestType.PipelineLoad;
        }

        public PipelineRequest(EnvironmentConfig config, bool isRebuild, bool isReinstall)
        {
            Environment = new EnvironmentRequest
            {
                Config = config,
                IsRebuild = isRebuild,
                IsReinstall = isReinstall
            };
            Type = RequestType.Environment;
        }

        public PipelineRequest(PipelineOptions options)
        {
            PipelineOptions = options;
            ImageTensorCount = options.InputImages?.Count ?? 0;
            ControlNetTensorCount = options.InputControlImages?.Count ?? 0;
            Tensors = GetInputTensors(options);
            Type = RequestType.PipelineRun;
        }


        public RequestType Type { get; init; }
        public EnvironmentRequest Environment { get; set; }
        public PipelineConfig PipelineConfig { get; set; }
        public PipelineOptions PipelineOptions { get; set; }
        public int ImageTensorCount { get; set; }
        public int ControlNetTensorCount { get; set; }

        [JsonIgnore]
        public List<Tensor<float>> Tensors { get; set; }


        private static List<Tensor<float>> GetInputTensors(PipelineOptions options)
        {
            var validTensors = new List<Tensor<float>>();
            void AddTensors(List<ImageTensor> tensors)
            {
                if (tensors.IsNullOrEmpty())
                    return;

                foreach (var tensor in tensors)
                {
                    if (tensor is not null)
                        validTensors.Add(tensor.GetChannels(3).ToTensor());
                }
            }

            AddTensors(options.InputImages);
            AddTensors(options.InputControlImages);
            if (validTensors.Count == 0)
                return default;

            return validTensors;
        }
    }

}
