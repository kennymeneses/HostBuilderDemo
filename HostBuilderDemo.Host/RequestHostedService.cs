using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Configuration;
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
    public class RequestHostedService : BackgroundService, IHostedService 
    {
        private readonly ILogger _logger = Log.Logger.ForContext<RequestHostedService>();
        private readonly IServiceCollection _services;
        private readonly IServiceProvider _provider;
        private readonly IConfiguration _configuration;
        private readonly IAmazonSQS _sqs;

        public RequestHostedService(IServiceCollection services, IServiceProvider provider, IConfiguration configuration, IAmazonSQS sqs)
        {
            _services = services;
            _provider = provider;
            _configuration = configuration;
            _sqs = sqs;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
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

                var snsService = _provider.GetRequiredService<SnsService>();

                await warmup.StarWarmup();

                await snsService.HandleMessage();

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

        public override Task StopAsync(CancellationToken cancellationToken)
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

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var request = new ReceiveMessageRequest
                    {
                        QueueUrl = _configuration["AWS-SQS-QueueURL:DemoQueue"],
                        WaitTimeSeconds = 5
                    };

                    var result = await _sqs.ReceiveMessageAsync(request);

                    if( result.Messages.Any())
                    {
                        foreach (var message in result.Messages)
                        {
                            _logger
                                .ForContext("MessageBody", message.Body, true)
                                .ForContext("Now", DateTimeOffset.Now, true)
                                .Information("New Message arriving: {MessageBody} | {Now}");
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger
                        .ForContext("error", e.Message, true)
                        .Error("Error in a SQS message: {error}");
                }
            }

            _logger.Information("Sns Service running at: {time}", DateTimeOffset.Now);
        }
    }
}
