using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Options;
using StructureMap;
using Xunit;

namespace LoggingModule.Tests
{
    public class LoggingModuleTests
    {
        private FileLoggerProvider _fileLoggerProvider;
        private readonly string _logDir;
        private readonly string _fileName;

        public LoggingModuleTests()
        {
            _fileName = "testlog.log";
            _logDir = @"C:\\temp";
            var logDir = _logDir;
            _fileLoggerProvider = new FileLoggerProvider(new OptionsManager<FileLoggerOptions>(new List<IConfigureOptions<FileLoggerOptions>>(){new ConfigureOptions<FileLoggerOptions>(
                (x) =>
                {
                    x.FileName = _fileName;
                    x.LogDirectory = logDir;
                })
            }));
        }

        [Fact]
        public void SimpleLogMessage()
        {
            //Arrange
            var logger = _fileLoggerProvider.CreateLogger();
            var guid = Guid.NewGuid();

            //Act
            logger.Log(Severity.Debug, new Exception($"Simple error {guid}"));

            //Assert
            var lastLine = File.ReadLines(GetFullName((DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day))).Last();
            Assert.True(lastLine.Contains(guid.ToString()));
        }
       
        private string GetFullName((int Year, int Month, int Day) group)
        {
            return Path.Combine(_logDir, $"{_fileName}{group.Year:0000}{group.Month:00}{group.Day:00}.txt");
        }
    }
}
