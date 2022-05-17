using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace Jarvis
{
    public static class SharedLogic
    {
        public static Task CreateNewEntry(TaskManager taskManager)
        {
            Task ed = new Task();
            ed.id = taskManager.GetAvailableID();
            return ed;
        }

        public static Task CreateNewTask(TaskManager taskManager, string[] category, string title, Task.Type type)
        {
            Task ed = new Task();
            ed.id = taskManager.GetAvailableID();
            ed.categories = category;
            ed.title = title;
            ed.type = type;
            return ed;
        }

        public static Habit CreateNewHabit(HabitManager habitManager, string[] category, string title, int previousStreak)
        {
            Habit ed = new Habit();
            ed.id = habitManager.GetAvailableID();
            ed.categories = category;
            ed.title = title;
            ed.startDate = DateTime.Now.ZeroTime();
            ed.previousStreak = previousStreak;
            
            return ed;
        }

        public static class UI
        {

        }
    }
}
