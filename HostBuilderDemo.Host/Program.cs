using HostBuilderDemo.ExtensionMethods;
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
                    Console.Title = "HostBuilderTest Demo 1.0";
                    var applicationsOptions = context.Configuration.GetSection("options").Get<ApplicationOptions>();
                    services
                        .AddLoging()
                        .AddSingleton<WarmupHttpRequest>()
                        .AddHostedService<ClassHealthCheks>()
                        .AddHostedService<RequestHostedService>()
                        .TryAddSingleton(services);
                })
                .Build();

            await host.RunAsync();
        }
    }
}
