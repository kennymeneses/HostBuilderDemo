using Amazon.SQS;
using Amazon.SQS.Model;
using HostBuilderDemo.Host.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace HostBuilderDemo.Host
{
    public class SqsService : BackgroundService
    {
        private readonly ILogger _logger = Log.Logger.ForContext<SqsService>();
        private readonly IConfiguration _configuration;
        private readonly IAmazonSQS _sqs;
        private QueueMessage _queueInstance;

        public SqsService(IConfiguration configuration, IAmazonSQS sqs, QueueMessage queueMessage)
        {
            _configuration = configuration;
            _sqs = sqs;
            _queueInstance = queueMessage;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            int count = 0;

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

                    if (result.Messages.Any())
                    {
                        foreach (var message in result.Messages)
                        {
                            string id = GetMessageIdFromMessage(message.Body);

                            if (!_queueInstance.listQueue.Contains(id))
                            {
                                _queueInstance.listQueue.Add(id);

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
                catch (Exception e)
                {
                    _logger
                        .ForContext("error", e.Message, true)
                        .Error("Error in a SQS message: {error}");
                }
            }
            _logger.Information("Sns Service running at: {time}", DateTimeOffset.Now);
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
