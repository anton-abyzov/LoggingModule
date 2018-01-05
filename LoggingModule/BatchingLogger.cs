using System;
using System.Text;

namespace LoggingModule
{
    public class BatchingLogger : ILogger
    {
        private readonly BatchingLoggerProvider _provider;
        private readonly Func<Exception, string> _formatter = (x) => x.Message;

        public BatchingLogger(BatchingLoggerProvider loggerProvider)
        {
            _provider = loggerProvider;
        }
       
        // Write a log message
        private void Log(DateTimeOffset timestamp, Severity severity, Exception exception, Func<Exception, string> formatter)
        {
            var builder = new StringBuilder();
            builder.Append(timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff zzz"));
            builder.Append(" [");
            builder.Append(severity);
            builder.Append("] ");
            builder.Append(": ");
            builder.AppendLine(formatter(exception));

            if (exception != null)
            {
                builder.AppendLine(exception.ToString());
            }

            _provider.AddMessage(timestamp, builder.ToString());
        }

        public void Log(Severity severity, Exception exception, Func<Exception, string> formatter)
        {
            Log(DateTimeOffset.Now, severity, exception, formatter);
        }

        public void Log(Severity severity, Exception exception)
        {
            Log(DateTimeOffset.Now, severity, exception, _formatter);
        }

    }
}