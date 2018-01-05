namespace LoggingModule
{
    public class FileLoggerOptions : LoggerRunOptions
    {
        public string LogDirectory { get; set; }
        public string FileName { get; set; }
    }
}