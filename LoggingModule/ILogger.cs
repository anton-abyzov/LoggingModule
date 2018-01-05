using System;

namespace LoggingModule
{
    public interface ILogger
    {
        void Log(Severity severity, Exception exception, Func<Exception, string> formatter);
        void Log(Severity severity, Exception exception);
    }
}