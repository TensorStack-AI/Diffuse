using TensorStack.WPF;

namespace Diffuse.Common
{
    public class ControlNetModel : BaseModel
    {
        public int Id { get; init; }
        public string Name { get; init; }
        public string Path { get; init; }
        public string Pipeline { get; init; }
        public bool IsDefault { get; set; }
    }
}
