using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace LoggingModule
{
    public class FileLoggerProvider : BatchingLoggerProvider
    {
        private readonly string _path;
        private readonly string _fileName;

        public FileLoggerProvider(IOptions<FileLoggerOptions> options) : base(options)
        {
            var loggerOptions = options.Value;
            _path = loggerOptions.LogDirectory;
            _fileName = loggerOptions.FileName;
        }

        // Write the provided messages to the file system
        protected override async Task WriteMessagesAsync(IEnumerable<LogMessage> messages, CancellationToken cancellationToken)
        {
            Directory.CreateDirectory(_path);

            // Group messages by log date
            foreach (var group in messages.GroupBy(GetGrouping))
            {
                var fullName = GetFullName(group.Key);
                
                using (var streamWriter = File.AppendText(fullName))
                {
                    foreach (var item in group)
                    {
                        await streamWriter.WriteAsync(item.Message);
                    }
                }
            }
        }

        // Get the file name
        private string GetFullName((int Year, int Month, int Day) group)
        {
            return Path.Combine(_path, $"{_fileName}{group.Year:0000}{group.Month:00}{group.Day:00}.txt");
        }

        private (int Year, int Month, int Day) GetGrouping(LogMessage message)
        {
            return (message.Timestamp.Year, message.Timestamp.Month, message.Timestamp.Day);
        }

    }
}