using TensorStack.Common;
using TensorStack.WPF;

namespace Diffuse.Common
{
    public class UpscaleOptions : BaseModel
    {
        private TileMode _tileMode = TileMode.ClipBlend;
        private int _tileSize = 512;
        private int _tileOverlap = 16;

        public TileMode TileMode
        {
            get { return _tileMode; }
            set { SetProperty(ref _tileMode, value); }
        }

        public int TileSize
        {
            get { return _tileSize; }
            set { SetProperty(ref _tileSize, value); }
        }

        public int TileOverlap
        {
            get { return _tileOverlap; }
            set { SetProperty(ref _tileOverlap, value); }
        }
    }
}
