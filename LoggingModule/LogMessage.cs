using System;

namespace LoggingModule
{
    public class LogMessage
    {
        public string Message { get; set; }

        public DateTimeOffset Timestamp { get; set; }

        public Severity Severity { get; set; }
    }
}