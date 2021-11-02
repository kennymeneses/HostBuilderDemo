using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HostBuilderDemo.Host
{
    public class RequestHostedService : IHostedService
    {
        private readonly ILogger _logger = Log.Logger.ForContext<RequestHostedService>();
        private readonly IServiceCollection _services;
        private readonly IServiceProvider _provider;

        public RequestHostedService(IServiceCollection services, IServiceProvider provider)
        {
            _services = services;
            _provider = provider;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.Information("HostBuilderDemo.Host Start");
            string NameApp = "HostBuilderDemo-1.0";

            try
            {
                foreach (var singleton in GetSingletons(_services))
                {
                    _provider.GetServices(singleton);
                }

                var warmup = _provider.GetRequiredService<WarmupHttpRequest>();

                await warmup.StarWarmup();

                _logger
                    .ForContext("nameApp", NameApp, true)
                    .Information("Warmup Started in {nameApp}.");

            }
            catch (Exception)
            {
                _logger
                    .ForContext("nameApp", NameApp, true)
                    .Error("Warmup Started in {nameApp}.");
                throw;
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.Information("HostBuilderDemo.Host has been stopped.");

            Log.CloseAndFlush();

            return Task.CompletedTask;
        }

        private static IEnumerable<Type> GetSingletons(IServiceCollection services)
        {
            return services
                .Where(descripcion => descripcion.Lifetime == ServiceLifetime.Singleton)
                .Where(descripcion => descripcion.ServiceType.ContainsGenericParameters == false)
                .Select(descripcion => descripcion.ServiceType)
                .Distinct()
                .ToList();
        }
    }
}
