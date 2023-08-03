
## 1.�����MyLoggerFactoryʵ��SkyApm.Logging.ILoggerFactory����������Ҳ������ã��뿴����5

```
using Serilog.Events;
using Serilog;
using SkyApm.Config;
using SkyApm.Utilities.Logging;
using MSLoggerFactory = Microsoft.Extensions.Logging.LoggerFactory;

namespace ApmLogger
{
    public class MyLoggerFactory : SkyApm.Logging.ILoggerFactory
    {
        private const string outputTemplate =
            @"{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{ServiceName}] [{Level}] {SourceContext} : {Message}{NewLine}{Exception}";

        private readonly MSLoggerFactory _loggerFactory;
        private readonly LoggingConfig _loggingConfig;

        public MyLoggerFactory(IConfigAccessor configAccessor)
        {
            _loggingConfig = configAccessor.Get<LoggingConfig>();
            _loggerFactory = new MSLoggerFactory();
            var instrumentationConfig = configAccessor.Get<InstrumentConfig>();

            var level = EventLevel(_loggingConfig.Level);

            _loggerFactory.AddSerilog(new LoggerConfiguration().MinimumLevel.Verbose().Enrich
                .WithProperty("SourceContext", null).Enrich
                .WithProperty(nameof(instrumentationConfig.ServiceName),
                    instrumentationConfig.ServiceName).Enrich
                .FromLogContext().WriteTo.Async(o =>
                    o.File(string.Format(_loggingConfig.FilePath, DateTime.Now.ToString("yyyyMM")), level, outputTemplate, flushToDiskInterval: TimeSpan.FromMilliseconds(500), rollingInterval: RollingInterval.Infinite))
                .CreateLogger());
        }

        public SkyApm.Logging.ILogger CreateLogger(Type type)
        {
            return new MyLogger(_loggerFactory.CreateLogger(type));
        }

        private static LogEventLevel EventLevel(string level)
        {
            return Enum.TryParse<LogEventLevel>(level, out var logEventLevel)
                ? logEventLevel
                : LogEventLevel.Error;
        }
    }
}
```


## 2.�����MyLoggerʵ��SkyApm.Logging.ILogger

```
using MSLogger = Microsoft.Extensions.Logging.ILogger;

namespace GrpcClient.ApmLogger
{
    public class MyLogger : SkyApm.Logging.ILogger
    {
        private readonly MSLogger _readLogger;
        public MyLogger(MSLogger readLogger)
        {
            _readLogger = readLogger;
        }

        public void Debug(string message)
        {
            _readLogger.LogDebug(message);
        }

        public void Error(string message, Exception exception)
        {
            _readLogger.LogError(message + Environment.NewLine + exception);
        }

        public void Information(string message)
        {
            _readLogger.LogInformation(message);
        }

        public void Trace(string message)
        {
            _readLogger.LogTrace(message);
        }

        public void Warning(string message)
        {
            _readLogger.LogWarning(message);
        }
    }
}
```

## 3.Program.cs�����滻����
```
builder.Services.Replace(ServiceDescriptor.Singleton<SkyApm.Logging.ILoggerFactory, MyLoggerFactory>());
```

## 4.�޸���skyapm.json�ļ��е�Logging:FilePath����
```
    "Logging": {
      "Level": "Information",
      "FilePath": "logs\\{0}\\skyapm.log"
    },
```

## 5.�����Ŀ�Ҳ������ã��������������
```
	<PackageReference Include="Serilog.Sinks.Async" Version="1.5.0" />
	<PackageReference Include="Serilog.Sinks.File" Version="5.0.0" />
```
