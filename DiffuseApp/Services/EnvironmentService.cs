using CSnakes.Runtime;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using TensorStack.Python;
using TensorStack.Python.Common;
using TensorStack.Python.Config;

namespace Diffuse.Services
{
    public class EnvironmentService : IEnvironmentService
    {
        private readonly ILogger _logger;
        private readonly Settings _settings;
        private readonly EnvironmentConfig _environmentConfig;
        private readonly PythonManager _pythonManager;
        private IPythonEnvironment _pythonEnvironment;

        /// <summary>
        /// Initializes a new instance of the <see cref="VideoService"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public EnvironmentService(Settings settings, ILogger<EnvironmentService> logger)
        {
            _logger = logger;
            _settings = settings;
            _environmentConfig = EnvironmentConfig.VendorDefault(settings.Vendor);
            _pythonManager = new PythonManager(_environmentConfig, _logger);
        }

        public bool IsLoaded => _pythonEnvironment != null;

        public async Task CreateAsync(bool isRebuild, bool isReinstall, IProgress<PipelineProgress> progressCallback)
        {
            if (_pythonEnvironment is null)
            {
                _pythonEnvironment = await _pythonManager.CreateEnvironmentAsync(isRebuild, isReinstall, progressCallback);
            }
        }

        public bool Exists()
        {
            return _pythonManager.Exists(_environmentConfig.Environment);
        }
    }


    public interface IEnvironmentService
    {
        bool IsLoaded { get; }
        bool Exists();
        Task CreateAsync(bool isRebuild, bool isReinstall, IProgress<PipelineProgress> progressCallback);
    }
}
