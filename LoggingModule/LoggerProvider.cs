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
        //private BlockingCollection<LogMessage> _messageQueue = new BlockingCollection<LogMessage>(new PriorityQueue<LogMessage>());
        private readonly BlockingCollection<LogMessage> _messageQueue = new BlockingCollection<LogMessage>(new PriorityQueue<LogMessage>());
        private Task _outputTask;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        protected LoggerProvider(IOptions<LoggerRunOptions> options)
        {
            // save options etc
            _interval = options.Value.DelayInterval;
            // start the background task
            _outputTask = Task.Factory.StartNew<Task>(ProcessLogQueue, null, TaskCreationOptions.LongRunning);
        }

        // Implemented in derived classes to actually write the messages out
        protected abstract Task WriteMessagesAsync(IEnumerable<LogMessage> messages, CancellationToken token);

        // Take messages from concurrent queue and write them out
        private async Task ProcessLogQueue(object state)
        {
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                // Add pending messages to the current batch
                while (_messageQueue.TryTake(out var message))
                {
                    _currentBatch.Add(message);
                }

                // Write the current batch out
                await WriteMessagesAsync(_currentBatch, _cancellationTokenSource.Token);
                _currentBatch.Clear();

                // Wait before writing the next batch
                await Task.Delay(_interval, _cancellationTokenSource.Token);
            }
        }

        // Add a message to the concurrent queue
        internal void AddMessage(DateTimeOffset timestamp, string message)
        {
            if (!_messageQueue.IsAddingCompleted)
            {
                _messageQueue.Add(new LogMessage { Message = message, Timestamp = timestamp }, _cancellationTokenSource.Token);
            }
        }

        public void Dispose()
        {
            // Finish writing messages out etc
        }

        // Create an instance of an ILogger, which is used to actually write the logs
        public ILogger CreateLogger()
        {
            return new Logger(this);
        }


    }
}
