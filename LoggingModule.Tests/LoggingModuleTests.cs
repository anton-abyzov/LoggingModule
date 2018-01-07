using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Options;
using StructureMap;
using Xunit;
using System.Threading;

namespace LoggingModule.Tests
{
    public class LoggingModuleTests
    {
        private FileLoggerProvider _fileLoggerProvider;
        private readonly string _logDir;
        private readonly TimeSpan _delayInterval = TimeSpan.FromSeconds(1);

        public LoggingModuleTests()
        {
            _logDir = @"C:\\temp";
        }

        [Fact]
        public void SimpleLogMessage()
        {
            //Arrange
            var fileName = $"testlog{Guid.NewGuid()}";
            var logDir = _logDir;
            _fileLoggerProvider = new FileLoggerProvider(new OptionsManager<FileLoggerOptions>(new List<IConfigureOptions<FileLoggerOptions>>(){new ConfigureOptions<FileLoggerOptions>(
                (x) =>
                {
                    x.FileName = fileName;
                    x.LogDirectory = logDir;
                    x.DelayInterval = _delayInterval;
                })
            }));
            var logger = _fileLoggerProvider.CreateLogger();
            var guid = Guid.NewGuid();

            //Act
            logger.Log(Severity.Debug, new Exception($"Simple error {guid}"));

            //Assert
            Thread.Sleep(2000);
            var lastLine = File.ReadLines(GetFullName(fileName, (DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day))).Last();
            Assert.True(lastLine.Contains(guid.ToString()));
        }

        [Fact]
        public void ShouldConsiderPriority()
        {
            //Arrange
            var fileName = $"testlog{Guid.NewGuid()}";
            var logDir = _logDir;
            _fileLoggerProvider = new FileLoggerProvider(new OptionsManager<FileLoggerOptions>(new List<IConfigureOptions<FileLoggerOptions>>(){new ConfigureOptions<FileLoggerOptions>(
                (x) =>
                {
                    x.FileName = fileName;
                    x.LogDirectory = logDir;
                })
            }));
            var logger = _fileLoggerProvider.CreateLogger();
            var guid = Guid.NewGuid();
            var guid2 = Guid.NewGuid();
            var guid3 = Guid.NewGuid();

            //Act
            logger.Log(Severity.Debug, new Exception($"Simple debug error {guid}"));
            logger.Log(Severity.Debug, new Exception($"Simple debug error2 {guid2}"));
            logger.Log(Severity.Critical, new Exception($"Simple critical error {guid3}"));

            //Assert
            Thread.Sleep(2000);
            var lastLine = File.ReadLines(GetFullName(fileName, (DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day))).Last();
            Assert.True(lastLine.Contains(guid2.ToString()));
        }

        private string GetFullName(string fileName, (int Year, int Month, int Day) group)
        {
            return Path.Combine(_logDir, $"{fileName}{group.Year:0000}{group.Month:00}{group.Day:00}.txt");
        }
    }
}
