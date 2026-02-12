using TensorStack.Python.Config;

namespace DiffuseApp.Common.Config
{
    public class ClientConfig
    {
        public bool IsDebugMode { get; set; }
        public string ServerPath { get; set; }
        public EnvironmentConfig Environment { get; set; }
    }
}
