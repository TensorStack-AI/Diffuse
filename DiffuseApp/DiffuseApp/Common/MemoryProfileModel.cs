using TensorStack.WPF;

namespace Diffuse.Common
{
    public class MemoryProfileModel : BaseModel
    {
        private int _memoryGB;
        private MemoryMode _memoryMode;

        public MemoryMode MemoryMode
        {
            get { return _memoryMode; }
            set { SetProperty(ref _memoryMode, value); }
        }
        public int MemoryGB
        {
            get { return _memoryGB; }
            set { SetProperty(ref _memoryGB, value); }
        }
    }


}
