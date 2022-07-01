using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace Jarvis
{
    public static class SharedLogic
    {

        public static Task CreateNewTask(TaskManager taskManager, string[] category, string title, Task.Type type, string status)
        {
            Task ed = new Task();
            ed.id = taskManager.GenerateNextAvailableID();

#if RELEASE_LOG
            if ( ed.id > 1000 )
            {
                ConsoleWriter.Print("Looks like you have a lot of tasks. Considering doing some maintenance!");
            }
#endif

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

        #region UTILS TO PRINT HELP TEXT

        public static void StartCachingHelpText()
        {
#if RELEASE_LOG
            if ( cachedHelpText.Count > 0)
            {
                ConsoleWriter.Print("You have unflushed messages in the queue!");
            }
#endif
            currentReservedLengthForHelpText = DEFAULT_RESERVED_SPACE_FOR_HELP;
            isHelpTextCachingStarted = true;
        }

        public static void FlushHelpText()
        {
            foreach(Pair<string, string> pair in cachedHelpText)
            {
                ConsoleWriter.Print("{0," + -currentReservedLengthForHelpText + "} {1}", pair.First, pair.Second);
            }
            cachedHelpText.Clear();
            isHelpTextCachingStarted = false;
        }

        public static void PrintHelp( string statement, string comments = "", bool addToCache = true, int reservedSpaceForStatement = DEFAULT_RESERVED_SPACE_FOR_HELP, int fallbackReserveSpaceIfOverflowing = FALLBACK_RESERVED_SPACE_FOR_HELP ) 
        {
            if ( !comments.IsEmpty())
                comments = "| " + comments;

            if (addToCache)
            {
#if RELEASE_LOG
                if (!isHelpTextCachingStarted)
                    ConsoleWriter.Print("Warning: Help Text caching is not started!");
#endif
                if (!comments.IsEmpty() && statement.Length >= currentReservedLengthForHelpText && currentReservedLengthForHelpText < fallbackReserveSpaceIfOverflowing)
                    currentReservedLengthForHelpText = statement.Length + 1;

                cachedHelpText.Add(new Pair<string, string>(statement, comments));
            }
            else
            {
                if (statement.Length >= reservedSpaceForStatement)
                    reservedSpaceForStatement = fallbackReserveSpaceIfOverflowing;

                ConsoleWriter.Print("{0," + -reservedSpaceForStatement + "} {1}", statement, comments);
            }
        }


        private static List<Pair<string, string>> cachedHelpText = new List<Pair<string, string>>();
        private const int DEFAULT_RESERVED_SPACE_FOR_HELP = 20;
        private const int FALLBACK_RESERVED_SPACE_FOR_HELP = 60;
        private static int currentReservedLengthForHelpText = 0;
        private static bool isHelpTextCachingStarted = false;

        #endregion // UTILS TO PRINT HELP TEXT
    }

    public class NotesUtility
    {
        public NotesUtility(string path)
        {
            this.path = path;
        }
        public void CreateNoteIfUnavailable(int id, bool feedback = true)
        {
            if (!Utils.FileHandler.DoesFileExist(path + id))
            {
                Utils.FileHandler.Create(path + id);
                if (feedback)
                    ConsoleWriter.Print("new note created");
            }
        }

        public bool DoesNoteExist(int id)
        {
            return Utils.FileHandler.DoesFileExist(path + id);
        }

        public string GetNoteContent(int id)
        {
            return Utils.FileHandler.Read(path + id);
        }

        public void AppendToNote(int id, string text)
        {
            Utils.AppendToFile(path + id, text);
        }

        public void RemoveNote(int id)
        {
            Utils.FileHandler.Remove(path + id);
        }

        public void OpenNote(JApplication application, int id, string externalProgram, bool waitForTheProgramToEnd, bool feedback = true)
        {
            if (feedback)
                ConsoleWriter.Print("Opening Notes");
            Utils.OpenAFileInEditor(
                path + id,
                externalProgram.IsEmpty() ? application.DesignData.defaultExternalEditor : externalProgram,
                waitForTheProgramToEnd);

            if (feedback)
                ConsoleWriter.Print("Closing Notes");
        }

        private string path;
    }

}

