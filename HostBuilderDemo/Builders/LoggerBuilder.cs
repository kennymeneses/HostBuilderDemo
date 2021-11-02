using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

namespace HostBuilderDemo.Builders
{
    public class LoggerBuilder
    {
        private readonly LoggerConfiguration _loggerConfiguration;

        public LoggerBuilder(string appName)
        {
            _loggerConfiguration = new LoggerConfiguration().MinimumLevel.Is(Serilog.Events.LogEventLevel.Verbose)
                                                            .Enrich.FromLogContext()
                                                            .Enrich.WithThreadId()
                                                            .Enrich.WithMachineName()
                                                            .Enrich.WithProperty("Application", appName);
        }

        public LoggerBuilder WithConsole()
        {
            _loggerConfiguration
                .WriteTo.Console(theme: AnsiConsoleTheme.Code);
            return this;
        }

        public ILogger Build()
        {
           return  _loggerConfiguration.CreateLogger();
        }
    }
}
