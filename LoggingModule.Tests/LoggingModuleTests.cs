using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Options;
using StructureMap;
using Xunit;
using System.Threading;
using System.Threading.Tasks;

namespace LoggingModule.Tests
{
    public class LoggingModuleTests
    {
        private FileLoggerProvider _fileLoggerProvider;
        private readonly string _logDir;

        public LoggingModuleTests()
        {
            _logDir = @"C:\\temp";
        }

        [Fact]
        public void ShouldDoSimpleLogging()
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

            //Act
            logger.Log(Severity.Debug, new Exception($"Simple error {guid}"));

            //Assert
            Thread.Sleep(2000);
            var lastLine = File.ReadLines(GetFullName(fileName, (DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day))).Last();
            Assert.True(lastLine.Contains(guid.ToString()));
        }

        [Fact]
        public void ShouldBeChronological_IfProcessingWithDelay()
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

            //Act
            logger.Log(Severity.Debug, new Exception($"Simple debug {guid}"));
            Thread.Sleep(1000);
            logger.Log(Severity.Info, new Exception($"Simple info {guid2}"));

            //Assert
            Thread.Sleep(5000); //to ensure that async processing of messages finished
            var lastLine = File.ReadLines(GetFullName(fileName, (DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day))).Last();
            Assert.True(lastLine.Contains(guid2.ToString()));//last info
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
            Thread.Sleep(5000);
            var lastLine = File.ReadLines(GetFullName(fileName, (DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day))).Last();
            Assert.True(lastLine.Contains(guid2.ToString()));
        }

        [Fact]
        public void ShouldConsiderPriority_ForAllMessageTypes()
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
            var guid4 = Guid.NewGuid();
            var guid5 = Guid.NewGuid();

            //Act
            logger.Log(Severity.Debug, new Exception($"Simple debug error {guid}"));
            logger.Log(Severity.Info, new Exception($"Simple info error {guid2}"));
            logger.Log(Severity.Warn, new Exception($"Simple warn error {guid3}"));
            logger.Log(Severity.Error, new Exception($"Simple error error {guid4}"));
            logger.Log(Severity.Critical, new Exception($"Simple critical error {guid5}"));

            //Assert
            Thread.Sleep(5000);
            var lastLines = File.ReadLines(GetFullName(fileName, (DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day)))
                .TakeLast(10).ToArray(); //10 - because every log puts 2 lines into log file
            Assert.True(lastLines[0].Contains(guid5.ToString()));
            Assert.True(lastLines[2].Contains(guid4.ToString()));
            Assert.True(lastLines[4].Contains(guid3.ToString()));
            Assert.True(lastLines[6].Contains(guid2.ToString()));
            Assert.True(lastLines[8].Contains(guid.ToString()));
        }

        [Fact]
        public void ShouldConsiderPriority_ForAllMessageTypes_Concurrent()
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
            var guid4 = Guid.NewGuid();
            var guid5 = Guid.NewGuid();

            //Act
            new Thread(() => logger.Log(Severity.Debug, new Exception($"Simple debug error {guid}"))).Start();
            new Thread(() => logger.Log(Severity.Info, new Exception($"Simple info error {guid2}"))).Start();
            new Thread(() => logger.Log(Severity.Error, new Exception($"Simple error error {guid4}"))).Start();
            new Thread(() => logger.Log(Severity.Critical, new Exception($"Simple critical error {guid5}"))).Start();
            new Thread(() => logger.Log(Severity.Warn, new Exception($"Simple warn error {guid3}"))).Start();

            //Assert
            Thread.Sleep(5000);
            var lastLines = File.ReadLines(GetFullName(fileName, (DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day)))
                .TakeLast(10).ToArray(); //10 - because every log puts 2 lines into log file
            Assert.True(lastLines[0].Contains(guid5.ToString()));
            Assert.True(lastLines[2].Contains(guid4.ToString()));
            Assert.True(lastLines[4].Contains(guid3.ToString()));
            Assert.True(lastLines[6].Contains(guid2.ToString()));
            Assert.True(lastLines[8].Contains(guid.ToString()));
        }

        private string GetFullName(string fileName, (int Year, int Month, int Day) group)
        {
            return Path.Combine(_logDir, $"{fileName}{group.Year:0000}{group.Month:00}{group.Day:00}.txt");
        }
    }
}
