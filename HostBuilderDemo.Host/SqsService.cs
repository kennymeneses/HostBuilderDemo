using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
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
        private readonly List<string> list_msg;

        public SqsService(IConfiguration configuration, IAmazonSQS sqs)
        {
            _configuration = configuration;
            _sqs = sqs;
            list_msg = new List<string>();
        }

        //public override async Task StartAsync(CancellationToken stoppingToken)
        //{
        //    var messageQueue = await _sqs.ReceiveMessageAsync(
        //     new ReceiveMessageRequest
        //     {
        //         QueueUrl = _configuration["AWS-SQS-QueueURL:DemoQueue"],
        //         AttributeNames = new List<string> { "All" },
        //         MaxNumberOfMessages = 10,
        //     });

        //    Thread.Sleep(2000);

        //    if (messageQueue.Messages.Any())
        //    {
        //        string idMessage = string.Empty;

        //        foreach (var message in messageQueue.Messages)
        //        {
        //            idMessage = GetMessageIdFromMessage(message.Body);

        //            list_msg.Add(idMessage);

        //            var body = JsonConvert.DeserializeObject(message.Body);

        //            _logger
        //                .ForContext("body",body.ToString(), true)
        //                .Information("A message already stored: {body}");
        //        }
        //    }
        //}

        //public override Task StopAsync(CancellationToken cancellationToken)
        //{
        //    _logger.Information("AWS SQS Listener ends succesfully");

        //    Log.CloseAndFlush();

        //    return Task.CompletedTask;
        //}

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            int count = 0;
            int timer = 0;

            var task = Task.Run(async () => {
                for (; ; )
                {
                    timer++;
                    await Task.Delay(1000);
                    Console.WriteLine(timer.ToString() + " seconds.");
                }
            });

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

                            //if (!list_msg.Contains(id))
                            //{
                            //    _logger
                            //    .ForContext("MessageBody", message.Body, true)
                            //    .ForContext("Now", DateTimeOffset.Now, true)
                            //    .Information("New Message arriving: {MessageBody} | {Now}");
                            //}

                            count = count + 1;

                            string mnsj = GetBodyMessageFromSqsMessage(message.Body);

                            _logger
                                .ForContext("MessageBody", mnsj, true)
                                .ForContext("Now", DateTimeOffset.Now, true)
                                .Information("New Message arriving: {MessageBody} | {Now}");
                        }

                        _logger
                            .ForContext("items", count, true)
                            .Information("the number messages is: {items}");
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

        public void TickTack()
        {
            var task = Task.Run(async () => {
                for (; ; )
                {
                    await Task.Delay(1000);
                    Console.WriteLine("Hello World after 10 seconds");
                }
            });
        }
    }
}
