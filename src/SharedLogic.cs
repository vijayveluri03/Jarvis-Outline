using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace Jarvis
{
    public static class SharedLogic
    {
        public static Task CreateNewEntry(TaskManager outlineManager)
        {
            Task ed = new Task();
            ed.id = outlineManager.GetNewID();
            return ed;
        }

        public static Task CreateNewEntry( TaskManager taskManager, string[] category, string title )
        {
            Task ed = new Task();
            ed.id = taskManager.GetNewID();
            ed.categories = category;
            ed.title = title;
            return ed;
        }
    }
}
