using Amazon.Runtime;
using Amazon.SimpleNotificationService;
using Amazon.SQS;
using HostBuilderDemo.ExtensionMethods;
using HostBuilderDemo.Host.ExtensionsMethods;
using HostBuilderDemo.Host.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Threading.Tasks;

namespace HostBuilderDemo.Host
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            if (string.IsNullOrWhiteSpace(environment))
                environment = "Development";

            var host = new HostBuilder()
                .UseConsoleLifetime()
                .UseEnvironment(environment)
                .ConfigureAppConfiguration(builder =>
                {
                    builder.SetBasePath(Directory.GetCurrentDirectory());
                    builder.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                    builder.AddCommandLine(args);
                    builder.AddEnvironmentVariables();
                    var settings = builder.Build();
                })
                .ConfigureServices((context, services) =>
                {
                    //add urlSQS by parameter in an extension Method
                    string urlSQS = context.Configuration["AWS-SQS-QueueURL:DemoQueue"];
                    //var SqsUrl = context.Configuration.GetSection("AWS-SQS-QueueURL").Get<AwsSqs>();

                    Console.Title = "HostBuilderTest Demo 1.0";
                    var awsOption = context.Configuration.GetAWSOptions();
                    awsOption.Credentials = new BasicAWSCredentials(context.Configuration["AWS-IAM:AccessKey"], context.Configuration["AWS-IAM:SecretKey"]);
                    var applicationsOptions = context.Configuration.GetSection("options").Get<ApplicationOptions>();
                    services
                        .AddLoging()
                        .AddDefaultAWSOptions(awsOption)
                        .AddAWSService<IAmazonSimpleNotificationService>()
                        .AddAWSService<IAmazonSQS>()
                        .AddSingleton<WarmupHttpRequest>()
                        .AddSingleton<QueueMessage>()
                        //.AddSingleton<SnsService>()
                        .AddSnsService(x => x.GetRequiredService<IAmazonSQS>(), urlSQS)
                        .AddHostedService<ListenerService>()
                        .AddHostedService<SqsService>()
                        //.AddHostedService<RequestHostedService>()
                        .TryAddSingleton(services);
                })
                .Build();

            await host.RunAsync();
        }
    }
}
