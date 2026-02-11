using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using TensorStack.Common.Tensor;
using TensorStack.Python.Common;
using TensorStack.WPF;

namespace Diffuse.Common
{
    public record DiffusionInputOptions : BaseRecord
    {
        private int _width;
        private int _height;
        private int _seed;
        private SchedulerType _scheduler;
        private float _guidanceScale = 1;
        private float _guidanceScale2 = 1;
        private string _prompt;
        private string _negativePrompt;
        private int _steps;
        private int _steps2;
        private float _strength = 1;
        private float _controlNetStrength = 1;
        private int _inputImageCount = 0;
        private SchedulerInputOptions _schedulerOptions = new SchedulerInputOptions();
        private List<LoraOptionModel> _loraOptions;
        private int _frames;
        private float _frameRate;

        public int Width
        {
            get { return _width; }
            set { SetProperty(ref _width, value); }
        }

        public int Height
        {
            get { return _height; }
            set { SetProperty(ref _height, value); }
        }

        public int Seed
        {
            get { return _seed; }
            set { SetProperty(ref _seed, value); }
        }

        public SchedulerType Scheduler
        {
            get { return _scheduler; }
            set { SetProperty(ref _scheduler, value); }
        }

        public float GuidanceScale
        {
            get { return _guidanceScale; }
            set { SetProperty(ref _guidanceScale, value); }
        }

        public float GuidanceScale2
        {
            get { return _guidanceScale2; }
            set { SetProperty(ref _guidanceScale2, value); }
        }

        public string Prompt
        {
            get { return _prompt; }
            set { SetProperty(ref _prompt, value); }
        }

        public string NegativePrompt
        {
            get { return _negativePrompt; }
            set { SetProperty(ref _negativePrompt, value); }
        }
        public int Steps
        {
            get { return _steps; }
            set { SetProperty(ref _steps, value); }
        }

        public int Steps2
        {
            get { return _steps2; }
            set { SetProperty(ref _steps2, value); }
        }

        public float Strength
        {
            get { return _strength; }
            set { SetProperty(ref _strength, value); }
        }

        public List<LoraOptionModel> LoraOptions
        {
            get { return _loraOptions; }
            set { SetProperty(ref _loraOptions, value); }
        }

        public float ControlNetStrength
        {
            get { return _controlNetStrength; }
            set { SetProperty(ref _controlNetStrength, value); }
        }

        public int InputImageCount
        {
            get { return _inputImageCount; }
            set { SetProperty(ref _inputImageCount, value); }
        }

        public SchedulerInputOptions SchedulerOptions
        {
            get { return _schedulerOptions; }
            set { SetProperty(ref _schedulerOptions, value); }
        }

        public int Frames
        {
            get { return _frames; }
            set { SetProperty(ref _frames, value); }
        }

        public float FrameRate
        {
            get { return _frameRate; }
            set { SetProperty(ref _frameRate, value); }
        }



        [JsonIgnore]
        public ImageTensor InputImage
        {
            get { return InputImages.FirstOrDefault(); }
            set
            {
                if (InputImages.Count == 0)
                {
                    InputImages.Add(value);
                }
                else
                {
                    InputImages[0] = value;
                }
                NotifyPropertyChanged();
            }
        }

        [JsonIgnore]
        public ImageTensor InputControlImage
        {
            get { return InputControlImages.FirstOrDefault(); }
            set
            {
                if (InputControlImages.Count == 0)
                {
                    InputControlImages.Add(value);
                }
                else
                {
                    InputControlImages[0] = value;
                }
                NotifyPropertyChanged();
            }
        }


        [JsonIgnore]
        public List<ImageTensor> InputImages { get; set; } = [];

        [JsonIgnore]
        public List<ImageTensor> InputControlImages { get; set; } = [];

    }
}
