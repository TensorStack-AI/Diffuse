using TensorStack.Common;
using TensorStack.WPF;

namespace Diffuse.Common
{
    public record UpscaleInputOptions : BaseRecord
    {
        private TileMode _tileMode;
        private int _tileSize;
        private int _tileOverlap;

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
