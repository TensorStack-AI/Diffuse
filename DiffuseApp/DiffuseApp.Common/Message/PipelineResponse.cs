using System.Collections.Generic;
using System.Text.Json.Serialization;
using TensorStack.Common.Tensor;

namespace DiffuseApp.Common.Message
{
    internal class PipelineResponse : IPipelineMessage
    {
        public string Error { get; init; }

        [JsonIgnore]
        public List<Tensor<float>> Tensors { get; set; }


        [JsonIgnore]
        public bool IsError => !string.IsNullOrEmpty(Error);
    }
}
