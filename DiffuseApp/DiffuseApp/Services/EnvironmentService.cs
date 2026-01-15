using Diffuse.Common;
using DiffuseApp.Common;
using DiffuseApp.Common.Config;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TensorStack.Common.Common;
using TensorStack.Python.Common;
using TensorStack.Python.Config;

namespace Diffuse.Services
{
    public sealed class EnvironmentService : IEnvironmentService
    {
        private readonly ILogger _logger;
        private readonly Settings _settings;

        /// <summary>
        /// Initializes a new instance of the <see cref="VideoService"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public EnvironmentService(Settings settings, ILogger<EnvironmentService> logger)
        {
            _logger = logger;
            _settings = settings;
        }


        public async Task<PipelineClient> CreateClientAsync(PipelineModel pipeline, PipelineConfig pipelineConfig, IProgress<PipelineProgress> progressCallback, CancellationToken cancellationToken = default)
        {
            var environment = await GetAsync(pipeline);
            var pipelineClientConfig = new ClientConfig
            {
                Environment = environment,
                ServerPath = App.DirectoryServer,
                IsDebugMode = environment.IsDebug,
            };

            var diffusionPipeline = new PipelineClient(pipelineClientConfig, progressCallback, _logger);

            try
            {
                await diffusionPipeline.LoadAsync(pipelineConfig, cancellationToken);
                return diffusionPipeline;
            }
            catch (Exception)
            {
                diffusionPipeline?.Dispose();
                throw;
            }
        }


        public Task<EnvironmentConfig> GetAsync(PipelineModel pipeline)
        {
            var environment = GetEnvironment(pipeline);
            return Task.FromResult(FromModel(environment, _settings.IsServerDebugEnabled));
        }


        public Task<EnvironmentConfig> GetAsync(EnvironmentModel environment)
        {
            return Task.FromResult(FromModel(environment, _settings.IsServerDebugEnabled));
        }


        public async Task CreateAsync(PipelineModel pipeline, IProgress<PipelineProgress> progressCallback, CancellationToken cancellationToken = default)
        {
            var environment = GetEnvironment(pipeline);
            await CreateInternalAsync(environment, false, progressCallback, cancellationToken);
        }


        public async Task CreateAsync(EnvironmentModel environment, IProgress<PipelineProgress> progressCallback, CancellationToken cancellationToken = default)
        {
            await CreateInternalAsync(environment, false, progressCallback, cancellationToken);
        }


        public async Task RebuildAsync(EnvironmentModel environment, IProgress<PipelineProgress> progressCallback, CancellationToken cancellationToken = default)
        {
            await CreateInternalAsync(environment, true, progressCallback, cancellationToken);
        }


        public Task DeleteAsync(EnvironmentModel environment)
        {
            FileHelper.DeleteDirectory(GetPath(environment));
            return Task.CompletedTask;
        }


        public bool Exists(PipelineModel pipeline)
        {
            var environment = GetEnvironment(pipeline);
            return Exists(environment);
        }


        public bool Exists(EnvironmentModel environment)
        {
            return Directory.Exists(GetPath(environment));
        }


        private string GetPath(EnvironmentModel environment)
        {
            return Path.Combine(App.DirectoryPython, "Pipelines", $".{environment.Environment}");
        }


        public async Task CreateInternalAsync(EnvironmentModel environment, bool isRebuild, IProgress<PipelineProgress> progressCallback, CancellationToken cancellationToken = default)
        {
            using var pipelineClient = new PipelineClient(new ClientConfig
            {
                IsDebugMode = false,
                IsRebuild = isRebuild,
                Environment = FromModel(environment, _settings.IsServerDebugEnabled),
                ServerPath = App.DirectoryServer,
            }, progressCallback, _logger);
            await pipelineClient.StartAsync(cancellationToken);
        }


        private EnvironmentModel GetEnvironment(PipelineModel pipeline)
        {
            var pipelineEnvironment = _settings.Environments
                .Where(x => x.Vendor == pipeline.Device.Vendor && x.Type == EnvironmentType.Pipeline && x.Pipeline == pipeline.DiffusionModel.Pipeline)
                .OrderByDescending(x => x.IsDefault)
                .FirstOrDefault();
            if (pipelineEnvironment != null)
                return pipelineEnvironment;

            var deviceEnvironment = _settings.Environments
                .Where(x => x.Vendor == pipeline.Device.Vendor && x.Type == EnvironmentType.Device && x.Device == pipeline.Device.HardwareID)
                .OrderByDescending(x => x.IsDefault)
                .FirstOrDefault();
            if (deviceEnvironment != null)
                return deviceEnvironment;

            var vendorEnvironment = _settings.Environments
                .Where(x => x.Vendor == pipeline.Device.Vendor && x.Type == EnvironmentType.Vendor)
                .OrderByDescending(x => x.IsDefault)
                .FirstOrDefault();
            if (vendorEnvironment != null)
                return vendorEnvironment;

            return _settings.Environments.First();
        }


        private static EnvironmentConfig FromModel(EnvironmentModel environment, bool isDebugEnabled)
        {
            return new EnvironmentConfig
            {
                IsDebug = isDebugEnabled,
                Directory = App.DirectoryPython,
                Variables = environment.Variables,
                Environment = environment.Environment,
                Requirements = environment.Requirements
            };
        }
    }


    public interface IEnvironmentService
    {
        Task<EnvironmentConfig> GetAsync(PipelineModel pipeline);
        Task<EnvironmentConfig> GetAsync(EnvironmentModel environment);
        Task<PipelineClient> CreateClientAsync(PipelineModel pipeline, PipelineConfig pipelineConfig, IProgress<PipelineProgress> progressCallback, CancellationToken cancellationToken = default);

        bool Exists(PipelineModel pipeline);
        bool Exists(EnvironmentModel environment);
        Task CreateAsync(PipelineModel pipeline, IProgress<PipelineProgress> progressCallback, CancellationToken cancellationToken = default);
        Task CreateAsync(EnvironmentModel environment, IProgress<PipelineProgress> progressCallback, CancellationToken cancellationToken = default);
        Task RebuildAsync(EnvironmentModel environment, IProgress<PipelineProgress> progressCallback, CancellationToken cancellationToken = default);
        Task DeleteAsync(EnvironmentModel environment);
    }
}
