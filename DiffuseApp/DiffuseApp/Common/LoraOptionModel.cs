using TensorStack.WPF;

namespace Diffuse.Common
{
    public class LoraOptionModel : BaseModel
    {
        public string Name { get; init; }
        public string Key { get; init; }
        public float Strength { get; set; }
    }
}
