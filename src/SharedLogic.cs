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
            ed.id = outlineManager.GetAvailableID();
            return ed;
        }

        public static Task CreateNewEntry(TaskManager taskManager, string[] category, string title, Task.Type type)
        {
            Task ed = new Task();
            ed.id = taskManager.GetAvailableID();
            ed.categories = category;
            ed.title = title;
            ed.type = type;
            return ed;
        }

        public static class UI
        {

        }
    }
}
