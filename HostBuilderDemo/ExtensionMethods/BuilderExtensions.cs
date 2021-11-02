using HostBuilderDemo.Builders;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace HostBuilderDemo.ExtensionMethods
{
    public static class BuilderExtensions
    {
        public static IServiceCollection AddLoging(this IServiceCollection services)
        {
            Log.Logger = new LoggerBuilder("HostServiceDemo-1.0")
                         .WithConsole()
                         .Build();

            return services;
        }
    }
}
