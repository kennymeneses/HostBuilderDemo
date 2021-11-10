using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Configuration;
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
        private readonly IConfiguration _configuration;
        private readonly IServiceCollection _services;
        private readonly IServiceProvider _provider;
        private readonly IAmazonSQS _sqs;
        private readonly List<string> list_msg;

        public ListenerService(IServiceCollection services, IServiceProvider provider, IConfiguration configuration)
        {
            _services = services;
            _provider = provider;
            _configuration = configuration;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.Information("AWS SnsService Started");

            try
            {
                var request = new ReceiveMessageRequest
                {
                    QueueUrl = _configuration["AWS-SQS-QueueURL:DemoQueue"],
                    WaitTimeSeconds = 5
                };

                var result = await _sqs.ReceiveMessageAsync(request);

                if (result.Messages.Any())
                {
                    foreach (var message in result.Messages)
                    {
                        string id = GetMessageIdFromMessage(message.Body);

                        if (!list_msg.Contains(id))
                        {
                            list_msg.Add(id);

                            count++;

                            string mnsj = GetBodyMessageFromSqsMessage(message.Body);

                            _logger
                                .ForContext("MessageBody", mnsj, true)
                                .ForContext("Now", DateTimeOffset.Now, true)
                                .Information("New Message arriving: {MessageBody} | {Now}");
                        }
                    }
                }

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
