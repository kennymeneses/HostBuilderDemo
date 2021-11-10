using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

namespace HostBuilderDemo.Host
{
    public class ClassHealthCheks : IHostedService
    {
        private readonly ILogger _logger = Log.Logger.ForContext<ClassHealthCheks>();

        public Task StartAsync(CancellationToken cancellationToken)
        {
            string NameApp = "HostBuilderDemo-1.0";

            _logger
                .ForContext("nameApp", NameApp,true)
                .Information("HostBuilderDemo.Host Start {nameApp}.");

            return WebHost.CreateDefaultBuilder()
                          .UseStartup<Startup>()
                          .Build()
                          .StartAsync(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.Information("HostBuilderDemo.Host Stop");
            return Task.CompletedTask;
        }
    }
}
