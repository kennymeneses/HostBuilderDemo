using Amazon.SQS;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace HostBuilderDemo.Host.ExtensionsMethods
{
    public static class ServicesExtensions
    {
        public static IServiceCollection AddSnsService(this IServiceCollection services,
            Func<IServiceProvider, IAmazonSQS> funcAWSSQS,
            string connectionString)
        {
            return services
                .AddSingleton<SnsService>(x => new SnsService(funcAWSSQS(x)) { queueUrl = connectionString });
        }
    }
}
