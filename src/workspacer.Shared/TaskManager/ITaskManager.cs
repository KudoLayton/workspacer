using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace workspacer.Shared.TaskManager
{
    public interface ITaskManager
    {
        public void QueueTask(Action task);
    }
}
