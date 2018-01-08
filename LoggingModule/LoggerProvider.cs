using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace LoggingModule
{
    public abstract class LoggerProvider
    {
        private readonly List<LogMessage> _currentBatch = new List<LogMessage>();
        private readonly TimeSpan _interval;
        private readonly BlockingCollection<LogMessage> _messageQueue = new BlockingCollection<LogMessage>(new ConcurrentPriorityQueue<Severity, LogMessage>());
        private Task _outputTask;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        protected LoggerProvider(IOptions<LoggerRunOptions> options)
        {
            _interval = options.Value.DelayInterval;
            _outputTask = Task.Factory.StartNew<Task>(ProcessLogQueue, null, TaskCreationOptions.LongRunning);
        }

        // implemented in derived classes to actually write the messages out
        protected abstract Task WriteMessagesAsync(IEnumerable<LogMessage> messages, CancellationToken token);

        // take messages from concurrent queue and write them out
        private async Task ProcessLogQueue(object state)
        {
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                await Task.Delay(300); // little delay to collect several messages and only then start processing it
                while (_messageQueue.TryTake(out var message))
                {
                    _currentBatch.Add(message);
                }
                
                await WriteMessagesAsync(_currentBatch, _cancellationTokenSource.Token);
                _currentBatch.Clear();
                
                await Task.Delay(_interval, _cancellationTokenSource.Token);
            }
        }
        
        internal void AddMessage(DateTimeOffset timestamp, string message, Severity severity)
        {
            if (!_messageQueue.IsAddingCompleted)
            {
                _messageQueue.Add(new LogMessage { Message = message, Timestamp = timestamp, Severity = severity}, _cancellationTokenSource.Token);
            }
        }
      
        // Create an instance of an ILogger, which is used to actually write the logs
        public ILogger CreateLogger()
        {
            return new Logger(this);
        }

    }
}
