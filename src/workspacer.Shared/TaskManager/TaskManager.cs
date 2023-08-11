using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace workspacer.Shared.TaskManager
{
    public class TaskManager : ITaskManager
    {
        private ConcurrentQueue<Action> tasks = new ConcurrentQueue<Action>();

        public void QueueTask(Action task)
        {
            tasks.Enqueue(task);
        }

        public void RunTasks()
        {
            while (tasks.TryDequeue(out Action task))
            {
                task();
            }
        }
    }
}
