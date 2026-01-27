using TensorStack.Common;
using TensorStack.Python.Common;
using TensorStack.WPF;

namespace Diffuse.Common
{
    public class PipelineModel : BaseModel
    {
        private Device _device;
        private DiffusionModel _diffusionModel;
        private ControlNetModel _controlNetModel;
        private ExtractModel _extractModel;
        private LoraAdapterModel[] _loraAdapterModel;
        private UpscaleModel _upscaleModel;
        private MemoryMode _memoryMode;
        private ProcessType _processType;
        private DataType _dataType;

        public Device Device
        {
            get { return _device; }
            set { SetProperty(ref _device, value); }
        }

        public DiffusionModel DiffusionModel
        {
            get { return _diffusionModel; }
            set { SetProperty(ref _diffusionModel, value); }
        }

        public ControlNetModel ControlNetModel
        {
            get { return _controlNetModel; }
            set { SetProperty(ref _controlNetModel, value); }
        }

        public ExtractModel ExtractModel
        {
            get { return _extractModel; }
            set { SetProperty(ref _extractModel, value); }
        }

        public LoraAdapterModel[] LoraAdapterModel
        {
            get { return _loraAdapterModel; }
            set { SetProperty(ref _loraAdapterModel, value); }
        }

        public UpscaleModel UpscaleModel
        {
            get { return _upscaleModel; }
            set { SetProperty(ref _upscaleModel, value); }
        }

        public ProcessType ProcessType
        {
            get { return _processType; }
            set { SetProperty(ref _processType, value); }
        }

        public MemoryMode MemoryMode
        {
            get { return _memoryMode; }
            set { SetProperty(ref _memoryMode, value); }
        }

        public DataType DataType
        {
            get { return _dataType; }
            set { SetProperty(ref _dataType, value); }
        }


        public bool IsReloadRequired(PipelineModel pipeline)
        {
            return pipeline is null
                || pipeline.DiffusionModel != _diffusionModel
                || pipeline.LoraAdapterModel != _loraAdapterModel
                || pipeline.ControlNetModel != _controlNetModel
                || pipeline.MemoryMode != _memoryMode
                || pipeline.DataType != _dataType
                || pipeline.ProcessType != _processType;
        }
    }
}
