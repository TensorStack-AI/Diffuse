using TensorStack.WPF;

namespace Diffuse.Common
{
    public class UpscaleInputOptions : BaseModel
    {
        private bool _isTileEnabled;
        private int _tileSize;
        private int _tileOverlap;

        public bool IsTileEnabled
        {
            get { return _isTileEnabled; }
            set { SetProperty(ref _isTileEnabled, value); }
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
