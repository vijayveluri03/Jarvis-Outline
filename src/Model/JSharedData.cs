using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jarvis
{
    public class JSharedData
    {
        public class PomodoroQueueItem
        {
            public PomodoroQueueItem() { }
            public PomodoroQueueItem(PomodoroSessionType type, string category) { this.type = type; this.category = category; }

            public string category;
            public PomodoroSessionType type;
        };

        public Queue<PomodoroQueueItem> _pomodoroSessionQueue = new Queue<PomodoroQueueItem>();
        private System.Object _lock = new Object();

        public bool IsPomodoroQueueEmpty()
        {
            lock(_lock)
            {
                return _pomodoroSessionQueue.Count == 0;
            }
        }

        public void InsertIntoPomodoroQueue(PomodoroSessionType type, string category)
        {
            lock (_lock)
            {
                _pomodoroSessionQueue.Enqueue(new PomodoroQueueItem(type, category));
            }
        }
        public PomodoroQueueItem PopPomodoroQueue()
        {
            lock (_lock)
            {
                return _pomodoroSessionQueue.Dequeue();
            }
        }

        public void ClearPomodoroQueue()
        {
            lock(this._lock)
            {
                _pomodoroSessionQueue.Clear();
            }
        }
    }
}