using System.Collections.Generic;
using TensorStack.Common.Tensor;

namespace DiffuseApp.Common.Message
{
    internal interface IPipelineMessage
    {
        List<Tensor<float>> Tensors { get; set; }
    }

}
