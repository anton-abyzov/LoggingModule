using System.Collections.Concurrent;

namespace LoggingModule
{
    public class ConcurrentPriorityQueue<TMessage, TSeverity> : ConcurrentQueue<LogMessage> 
    {
        public ConcurrentPriorityQueue()
        {
            
        }
    }
}