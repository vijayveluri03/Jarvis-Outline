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
            public PomodoroQueueItem(PomodoroTaskType type, string category) { this.type = type; this.category = category; }

            public string category;
            public PomodoroTaskType type;
        };

        public Queue<PomodoroQueueItem> _pomodoroTaskQueue = new Queue<PomodoroQueueItem>();
        private System.Object _lock = new Object();

        public bool IsPomodoroQueueEmpty()
        {
            lock(_lock)
            {
                return _pomodoroTaskQueue.Count == 0;
            }
        }

        public void InsertIntoPomodoroQueue(PomodoroTaskType type, string category)
        {
            lock (_lock)
            {
                _pomodoroTaskQueue.Enqueue(new PomodoroQueueItem(type, category));
            }
        }
        public PomodoroQueueItem PopPomodoroQueue()
        {
            lock (_lock)
            {
                return _pomodoroTaskQueue.Dequeue();
            }
        }

        public void ClearPomodoroQueue()
        {
            lock(this._lock)
            {
                _pomodoroTaskQueue.Clear();
            }
        }
    }
}