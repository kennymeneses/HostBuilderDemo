using Amazon.SQS;
using Amazon.SQS.Model;
using HostBuilderDemo.Host.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace HostBuilderDemo.Host
{
    public class ListenerService : IHostedService
    {
        private readonly ILogger _logger = Log.Logger.ForContext<ListenerService>();
        private readonly IConfiguration _configuration;
        private readonly IAmazonSQS _sqs;
        private QueueMessage _queueInstance;

        public ListenerService(IConfiguration configuration,IAmazonSQS sqs, QueueMessage queueMessage)
        {
            _configuration = configuration;
            _sqs = sqs;
            _queueInstance = queueMessage;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.Information("AWS SqsService Started");

            try
            {
                var request = new ReceiveMessageRequest
                {
                    QueueUrl = _configuration["AWS-SQS-QueueURL:DemoQueue"],
                    WaitTimeSeconds = 5,
                    MessageAttributeNames = new List<string> { "All" }
                };

                var request2 = new GetQueueAttributesRequest
                {
                    QueueUrl = _configuration["AWS-SQS-QueueURL:DemoQueue"],
                    AttributeNames = new List<string> { "All" }
                };

                var r = await _sqs.GetQueueAttributesAsync(request2);

                _logger.Information(r.ApproximateNumberOfMessages.ToString()+" Messages founded");

                for (int i = 0; i < r.ApproximateNumberOfMessages; i++)
                {
                    var _request = new ReceiveMessageRequest
                    {
                        QueueUrl = _configuration["AWS-SQS-QueueURL:DemoQueue"],
                        WaitTimeSeconds = 5,
                        MessageAttributeNames = new List<string> { "All" }
                    };

                    var resultado = await _sqs.ReceiveMessageAsync(_request);

                    string id = GetMessageIdFromMessage(resultado.Messages[0].Body);

                    _queueInstance.listQueue.Add(id);

                    string mnsj = GetBodyMessageFromSqsMessage(resultado.Messages[0].Body);

                    _logger
                        .ForContext("MessageBody", mnsj, true)
                        .ForContext("Now", DateTimeOffset.Now, true)
                        .Information("A Message has been stored: {MessageBody} | {Now}");

                }

                _logger.Information("Number messages stored: " + r.ApproximateNumberOfMessages.ToString() +" stored");
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

        public string GetMessageIdFromMessage(string body)
        {
            JsonDocument json = JsonDocument.Parse(body);
            JsonElement root = json.RootElement;
            JsonElement mssgId = root.GetProperty("MessageId");

            return mssgId.ToString();
        }

        public string GetBodyMessageFromSqsMessage(string body)
        {
            JsonDocument json = JsonDocument.Parse(body);
            JsonElement root = json.RootElement;
            JsonElement mssg = root.GetProperty("Message");

            return mssg.ToString();
        }
    }
}
