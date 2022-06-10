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

        public static Task CreateNewTask(TaskManager taskManager, string[] category, string title, Task.Type type, Task.Status status = Task.Status.Open)
        {
            Task ed = new Task();
            ed.id = taskManager.GetAvailableID();
            ed.categories = category;
            ed.title = title;
            ed.type = type;
            ed.SetStatus(status);
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

        public static JournalEntry CreateNewJournalEntry(JournalManager journalManager, string title)
        {
            JournalEntry ed = new JournalEntry();
            ed.id = journalManager.GetAvailableID();
            ed.title = title;
            ed.loggedDate = DateTime.Now.ZeroTime();
            return ed;
        }

        public static void PrintHelp( string statement, string comments = "", int reservedSpaceForStatement = 30, int fallbackReserveSpaceIfOverflowing = 60 ) 
        {
            if ( !comments.IsEmpty())
                comments = "| " + comments;

            if ( statement.Length >= reservedSpaceForStatement )
                reservedSpaceForStatement = fallbackReserveSpaceIfOverflowing;

            ConsoleWriter.Print("{0," + -reservedSpaceForStatement + "} {1}", statement, comments);
        }

        public static class UI
        {

        }
    }
}
