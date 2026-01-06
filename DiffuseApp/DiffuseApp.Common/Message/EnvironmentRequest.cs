using TensorStack.Python.Config;

namespace DiffuseApp.Common.Message
{

    internal class EnvironmentRequest
    {
        public bool IsRebuild { get; set; }
        public bool IsReinstall { get; set; }
        public EnvironmentConfig Config { get; set; }
    }
}
