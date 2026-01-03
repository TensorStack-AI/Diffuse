using TensorStack.WPF;

namespace Diffuse.Common
{
    public class LoraAdapterModel : BaseModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Key { get; set; }
        public string Path { get; set; }
        public string Weights { get; set; }
        public string Pipeline { get; set; }
        public string[] Triggers { get; set; }
        public bool IsDefault { get; set; }
    }
}
