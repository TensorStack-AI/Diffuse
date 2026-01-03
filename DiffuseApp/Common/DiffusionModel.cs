using System.Collections.Generic;
using TensorStack.Python.Common;
using TensorStack.WPF;

namespace Diffuse.Common
{
    public class DiffusionModel : BaseModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Pipeline { get; set; }
        public string ModelUrl { get; set; }
        public bool IsDefault { get; set; }
        public int[] MemoryModes { get; set; }
        public DataType[] DataTypes { get; set; }
        public ProcessType[] ProcessTypes { get; set; }
        public List<SizeOption> Resolutions { get; set; }
        public DiffusionDefaultOptions DefaultOptions { get; set; }
    }
}
