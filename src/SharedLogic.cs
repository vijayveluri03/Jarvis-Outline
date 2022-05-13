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

        public static Task CreateNewEntry(TaskManager taskManager, string[] category, string title)
        {
            Task ed = new Task();
            ed.id = taskManager.GetNewID();
            ed.categories = category;
            ed.title = title;
            return ed;
        }

        public static class UI
        {
            //public static void PrintTask( Task entry, TaskCollectioTaskTimeManagementn int titleArea, bool isInProgress)
            //{
            //    int timeInProgress = isInProgress ? (int)(DateTime.Now - application.UserData.taskProgress.startTime).TotalMinutes : 0;

            //    Console.Out.WriteLine("{0, -4} {1,-15} {2,-" + titleArea + "} {3, -15} {4, -15}",
            //        entry.id,
            //        (entry.categories != null && entry.categories.Length > 0 ? Utils.ArrayToString(entry.categories, true) : "INVALID"),
            //        entry.title.TruncateWithVisualFeedback(titleArea - 3/*for the ...*/),
            //        (isInProgress ? "In Progress" : entry.StatusString),
            //        (isInProgress ? timeInProgress + " + " : "") + ("(" + application.logManager.GetTotalTimeSpentToday(entry.id) + "," + application.logManager.GetTotalTimeSpent(entry.id) + ")")
            //        );

            //}

        }
    }
}
