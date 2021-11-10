using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HostBuilderDemo.Host
{
    public class ListenerService : IHostedService
    {
        private readonly ILogger _logger = Log.Logger.ForContext<ListenerService>();
        private readonly IServiceCollection _services;
        private readonly IServiceProvider _provider;

        public ListenerService(IServiceCollection services, IServiceProvider provider)
        {
            _services = services;
            _provider = provider;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.Information("AWS SnsService Started");

            try
            {
                foreach (var singleton in GetSingletons(_services))
                {
                    _provider.GetServices(singleton);
                }

                var listenerService = _provider.GetRequiredService<SnsService>();

                await listenerService.HandleMessage();
            }
            catch (Exception ex)
            {
                _logger
                    .ForContext("error", ex.Message, true)
                    .Error("Error in SnsService was beacuse: {error}.");
                throw;
            }         
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.Information("The Listener Service Stop");

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
