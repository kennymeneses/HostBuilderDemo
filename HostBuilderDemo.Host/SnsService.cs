using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Serilog;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HostBuilderDemo.Host
{
    public class SnsService
    {
        private readonly ILogger _logger = Log.Logger.ForContext<RequestHostedService>();
        private IAmazonSQS _SqsClient;
        public string queueUrl { get; set; }

        public SnsService(IAmazonSQS SqsClient)
        {
            _SqsClient = SqsClient;
        }

        public async Task HandleMessage()
        {
            //Grant permission to this app from SQS-DemoQueue

            var messageQueue = await _SqsClient.ReceiveMessageAsync(
                 new ReceiveMessageRequest
                 {
                     QueueUrl = queueUrl,
                     AttributeNames = new List<string> { "All" },
                     MaxNumberOfMessages = 10,                     
                 });

            if (messageQueue.Messages.Count >0)
            {
                _logger.Information(messageQueue.Messages.Count.ToString());

                foreach (var message in messageQueue.Messages)
                {
                    var rpta = JsonConvert.DeserializeObject(message.Body);

                    _logger.Information(rpta.ToString());
                }
            }
        }
    }
}
