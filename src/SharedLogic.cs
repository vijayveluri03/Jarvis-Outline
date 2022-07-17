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

        public static Habit CreateNewHabit(HabitManager habitManager, string[] category, string title, int previousTickCount)
        {
            Habit ed = new Habit();
            ed.id = habitManager.GetAvailableID();
            ed.categories = category;
            ed.title = title;
            ed._startDate = Date.Today; // @todo , using private member here
            ed.IsDirty = true;
            ed._previousTickCount = previousTickCount; //@todo, using private member here
            
            return ed;
        }

        public static NotebookEntry CreateNewNotebookEntry(NotebookManager notebookManager, string[] tags, string title)
        {
            NotebookEntry ed = new NotebookEntry();
            ed.id = notebookManager.GetAvailableID();
            ed.title = title;
            ed.tags = tags;
            ed.loggedDate = DateTime.Now.ZeroTime();
            return ed;
        }

        public static void TryAddHabitEntry(JModel model, int[] ids, Date date, HabitStatus status)
        {
            foreach( var id in ids )
                TryAddHabitEntry(model, id, date, status);
        }
        public static void TryAddHabitEntry(JModel model, int id, Date date, HabitStatus status)
        {
            Habit hb = model.habitManager.GetHabit_Editable(id);

            if (hb == null)
            {
                ConsoleWriter.Print("Habit with id : {0} not found!", id);
                return;
            }

            Date dateForEntry = date;
            if (hb.IsEntryOn(dateForEntry))
            {
                ConsoleWriter.Print("Habit with id: {0} already updated for {1}. This can only be done once a day!", id, dateForEntry.ShortForm());
                return;
            }

            if (hb.IsDisabled)
            {
                ConsoleWriter.Print("Habit with id: {0} is disabled. You would want to re-enable it before it can be updated!", id);
                return;
            }

            string error = "";
            if (!hb.IsNewEntryValid(dateForEntry, out error))
            {
                ConsoleWriter.Print("Invalid date! -> " + error);
                return;
            }

            int previousSuccess = hb.GetSuccessRate();

            hb.AddNewEntry(dateForEntry, status);

            if (status == HabitStatus.Completed)
                ConsoleWriter.Print("Habit with id : {0} ticked on {1}! Success rate {2} -> {3}", id, date.ShortForm(), previousSuccess, hb.GetSuccessRate());
            else if (status == HabitStatus.Ignored)
                ConsoleWriter.Print("Habit with id : {0} ticked on {1}! Success rate {2} -> {3}", id, date.ShortForm(), previousSuccess, hb.GetSuccessRate());
            else
                Utils.Assert(false, "This condition is not programmed");

            {
                ConsoleWriter.EmptyLine();
                SharedLogic.PrintMonth(model, dateForEntry, hb);
                ConsoleWriter.EmptyLine();
            }

            ConsoleWriter.Print("You can also try 'show' command for more details. Cheers!");
        }

        #region HABITS AND CALENDAR SUPPORT 

        public static void PrintMonth( JModel model, Date month, Habit hb)
        {
            ConsoleWriter.PrintInColor(string.Format("Calendar for month : {0} {1}".ToUpper(), month.ToString("MMMM"), month.Year), model.DesignData.HighlightColorForText);

            ConsoleWriter.PushIndent();
            ConsoleWriter.PrintInColor(string.Format("{0} {1} {2} {3} {4} {5} {6}", "M", "T", "W", "T", "F", "S", "S"), model.DesignData.HighlightColorForText_2);

            Date currentDate = new Date(month.Year, month.Month, 1);
            int emptyBlocksAtStart = GetNumberOfEmptySpacesAtTheStartOfTheMonth(currentDate);
            int totalDaysInMonth = DateTime.DaysInMonth(month.Year, month.Month);

            ConsoleWriter.IndentWithOutLineBreak();
            while (emptyBlocksAtStart > 0)
            {
                ConsoleWriter.PrintWithOutLineBreak("{0} ", "-");   // the space after {0} is intentional, to match the header of the calendar 
                emptyBlocksAtStart--;
            }

            for (int day = 1; day <= totalDaysInMonth; day++)
            {
                currentDate = new Date(month.Year, month.Month, day);
                bool ticked = hb.IsTickedOn(currentDate);
                bool isIgnored = hb.IsIgnoredOn(currentDate);
                ConsoleColor color = ConsoleColor.Green;
                string text = "";
                if (currentDate >= Date.Today)
                {
                    text = "-";
                    color = model.DesignData.DefaultColorForText;
                }
                else if (currentDate < hb._startDate)// @todo, using private member directly
                {
                    text = "-";
                    color = model.DesignData.DefaultColorForText;
                }
                else if (ticked)
                {
                    text = "Y";
                    color = ConsoleColor.Green;
                }
                else if(isIgnored)
                {
                    text = "I";
                    color = model.DesignData.DefaultColorForText;
                }
                else
                {
                    text = "X";
                    color = ConsoleColor.Red;
                }

                ConsoleWriter.PrintWithColorWithOutLineBreak(String.Format("{0} ", text), color);

                if (currentDate.DayOfWeek == DayOfWeek.Sunday)
                {
                    if (day < totalDaysInMonth) // dont worry about indentation if this is last day anyway.
                    {
                        ConsoleWriter.EmptyLine();
                        ConsoleWriter.IndentWithOutLineBreak();
                    }
                }
            }

            ConsoleWriter.InsertNewLineIfAvoidingLineBreaks();  // to flush the consolewriter with out line breaks

            ConsoleWriter.PopIndent();
        }

        private static int GetNumberOfEmptySpacesAtTheStartOfTheMonth(Date startDate)
        {
            DayOfWeek dayOfWeek = startDate.DayOfWeek;

            // This order is based on my custom calendar which has days in this order "M", "T", "W", "T", "F", "S", "S"
            switch (dayOfWeek)
            {
                case DayOfWeek.Monday:
                    return 0;
                case DayOfWeek.Tuesday:
                    return 1;
                case DayOfWeek.Wednesday:
                    return 2;
                case DayOfWeek.Thursday:
                    return 3;
                case DayOfWeek.Friday:
                    return 4;
                case DayOfWeek.Saturday:
                    return 5;
                case DayOfWeek.Sunday:
                    return 6;
            }
            Utils.Assert(false);
            return -1;
        }

        #endregion

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

        public static void PrintHelp_Heading(string statement, string comments = "", bool addToCache = true, int reservedSpaceForStatement = DEFAULT_RESERVED_SPACE_FOR_HELP, int fallbackReserveSpaceIfOverflowing = FALLBACK_RESERVED_SPACE_FOR_HELP)
        {
            PrintHelp( statement.ToUpper(), comments, addToCache, reservedSpaceForStatement, fallbackReserveSpaceIfOverflowing);
        }

        public static void PrintHelp_SubText(string statement, string comments = "", bool addToCache = true, int reservedSpaceForStatement = DEFAULT_RESERVED_SPACE_FOR_HELP, int fallbackReserveSpaceIfOverflowing = FALLBACK_RESERVED_SPACE_FOR_HELP)
        {
            PrintHelp("   " /* tiny indentation */ + statement, comments, addToCache, reservedSpaceForStatement, fallbackReserveSpaceIfOverflowing);
        }

        /* 
         * Param AutoCaching - Will start caching if its not enabled by default. If the caching had to be enable, then it will also flush and disable at the end.
         */
        public static void PrintHelp_WithHeadingAndSubText(string statement, string[] subtext, string comments = "",  bool addToCache = true, int reservedSpaceForStatement = DEFAULT_RESERVED_SPACE_FOR_HELP, int fallbackReserveSpaceIfOverflowing = FALLBACK_RESERVED_SPACE_FOR_HELP, bool autoCaching = true)
        {
            bool flushAtEnd = false;
            if (autoCaching)
            {
                if (!isHelpTextCachingStarted)
                {
                    // If caching is enabled, it will also disable it by the end of this method. s
                    StartCachingHelpText();
                    flushAtEnd = true;
                }
            }
            PrintHelp_Heading(statement, comments, addToCache, reservedSpaceForStatement, fallbackReserveSpaceIfOverflowing);
            foreach( var sub in subtext )
                PrintHelp_SubText(sub, "", addToCache, reservedSpaceForStatement, fallbackReserveSpaceIfOverflowing);
        
            if(flushAtEnd)
            {
                FlushHelpText();
            }
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

        public void OpenNote(JModel model, int id, string externalProgram, bool waitForTheProgramToEnd, bool feedback = true)
        {
            if (feedback)
                ConsoleWriter.Print("Opening Notes");
            Utils.OpenAFileInEditor(
                path + id,
                externalProgram.IsEmpty() ? model.DesignData.defaultExternalEditor : externalProgram,
                waitForTheProgramToEnd);

            if (feedback)
                ConsoleWriter.Print("Closing Notes");
        }

        private string path;
    }

}

