using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jarvis; //@todo 


public class HabitHandler : CommandHandlerBaseWithUtility
{
    public HabitHandler()
    {

    }

    protected override bool ShowHelp()
    {
        SharedLogic.StartCachingHelpText();
        SharedLogic.PrintHelp_Heading("USAGE");
        SharedLogic.PrintHelp_SubText(">habit add ", "To add a new habit");
        SharedLogic.PrintHelp_SubText(">habit ticktoday", "to tick today as completed");
        SharedLogic.PrintHelp_SubText(">habit tickyesterday", "to tick yesterday as completed");
        SharedLogic.PrintHelp_SubText(">habit ignoretoday", "to ignore today. This will not be considred as failed!");
        SharedLogic.PrintHelp_SubText(">habit ignoreyesterday", "to tick ignore yesterday. This will not be considred as failed!");

        SharedLogic.PrintHelp_SubText(">habit reset", "to reset a habit");
        SharedLogic.PrintHelp_SubText(">habit list", "to list all the habit");
        SharedLogic.PrintHelp_SubText(">habit show", "to show details of a habit");

        SharedLogic.PrintHelp_Heading("ADVANCED");
        SharedLogic.PrintHelp_SubText(">habit tick", "To mark a certain date as completed");
        SharedLogic.PrintHelp_SubText(">habit delete", "to delete a habit");
        SharedLogic.PrintHelp_SubText(">habit disable", "to disable a habit");
        SharedLogic.PrintHelp_SubText(">habit re-enable", "to re-enable a disabled habit"); 
        SharedLogic.PrintHelp_SubText(">habit edittitle", "To edit the title of a habit");

        SharedLogic.PrintHelp_Heading("NOTES"); 
        SharedLogic.PrintHelp_SubText(">habit note" , "open note for a habit");
        SharedLogic.PrintHelp_SubText(">habit printnote" , "print the notes. ( you can also use 'cat' instead of 'printnote')");
        SharedLogic.PrintHelp_SubText(">habit deletenote" , "delete note for a habit"); 

        SharedLogic.PrintHelp_Heading("HELP");
        SharedLogic.PrintHelp_SubText("All the commands have their own help section. Use the argument '--help'");
        SharedLogic.PrintHelp_SubText("Example - 'habit add --help' for more examples on how to use it. Try it!");
        SharedLogic.PrintHelp_SubText("This works for every single command! Cheers!");
        SharedLogic.FlushHelpText();
        return true;
    }

    protected override CommandHandlerBase GetSpecializedCommandHandler(Jarvis.JModel application, out List<string> argumentsForSpecializedHandler, bool printErrors)
    {
        string action = arguments_ReadOnly != null && arguments_ReadOnly.Count > 0 ? arguments_ReadOnly[0] : null;
        CommandHandlerBase selectedHander = null;

        switch (action)
        {
            case "add":
                selectedHander = new HabitAddCommand();
                break;
            case "ticktoday":
                selectedHander = new HabitStreakUpTodayCommand();
                break;
            case "tickyesterday":
                selectedHander = new HabitStreakUpYesterdayCommand();
                break;
            case "ignoretoday":
                selectedHander = new HabitIgnoreTodayCommand();
                break;
            case "ignoreyesterday":
                selectedHander = new HabitIgnoreYesterdayCommand();
                break;
            case "tick":
                selectedHander = new HabitStreakUpCommand();
                break;
            case "list":
                selectedHander = new HabitListCommand();
                break;
            case "reset":
                selectedHander = new HabitResetCommand();
                break;
            case "show":
                selectedHander = new HabitShowCommand();
                break;
            case "delete":
                selectedHander = new HabitDeleteCommand();
                break;
            case "disable":
                selectedHander = new HabitDisableCommand();
                break;
            case "re-enable":
                selectedHander = new HabitReEnableCommand();
                break;
                case "cat":
            case "printnote":
                selectedHander = new HabitCatNotesCommand();
                break;
            //case "createnote":
            //case "newnote":
            //case "addnote":
            //    selectedHander = new HabitcreatenoteCommand();
            //    break;
            case "note":
            case "opennote":
                selectedHander = new HabitEditNoteCommand();
                break;
            case "deletenote":
                selectedHander = new HabitDeleteNoteCommand();
                break;
            case "edittitle":
                selectedHander = new HabitEditTitleCommand();
                break;
            default:
                if(printErrors)
                    ConsoleWriter.Print("Invalid command. Try 'habit --help' for more information");
                break;
        }

        if (selectedHander != null)
        {
            argumentsForSpecializedHandler = new List<string>(arguments_ReadOnly);
            argumentsForSpecializedHandler.RemoveAt(0);

            Utils.Assert(selectedHander is CommandHandlerBaseWithUtility);
            (selectedHander as CommandHandlerBaseWithUtility).Init(application, notes);
        }
        else
            argumentsForSpecializedHandler = null;

        return selectedHander;
    }

    protected override bool Run()
    {
        ConsoleWriter.Print("Invalid arguments! \n");
        ShowHelp();
        return true;
    }
}

public class HabitAddCommand : CommandHandlerBaseWithUtility
{
    public HabitAddCommand()
    {

    }

    protected override bool ShowHelp()
    {
        SharedLogic.StartCachingHelpText();
        SharedLogic.PrintHelp_Heading("USAGE");
        SharedLogic.PrintHelp_SubText(">habit add <category> <title>");
        SharedLogic.PrintHelp_SubText(">habit add <category> <title> --previousstreak:<count>", "This sets the starting streak for a habit. if you have already been doing this habit for a while, you can use this, to start the tick count from there ");

        SharedLogic.PrintHelp_Heading("EXAMPLES");
        SharedLogic.PrintHelp_SubText(">habit add health \"Wake up at 6AM everyday!\"");
        SharedLogic.PrintHelp_SubText(">habit add health \"Walk daily\" --previousstreak:20", "In this case, the tick count starts from 20!");

        SharedLogic.PrintHelp_WithHeadingAndSubText("Whats category", application.DesignData.categories.listOfCategories, "Category can be one of these following. you can add more in the Data/Design.json as per your need.");
        SharedLogic.FlushHelpText();
        return true;
    }
    protected override bool Run()
    {
        if (arguments_ReadOnly.Count != 2)
        {
            ConsoleWriter.Print("Invalid arguments! \n");
            ShowHelp();
            return true;
        }

        string[] categories = arguments_ReadOnly[0].Split(',');
        string title = arguments_ReadOnly[1];

        if (!application.DesignData.DoesCategoryExist(categories))
        {
            ConsoleWriter.Print("Invalid categories.");
            SharedLogic.PrintHelp_WithHeadingAndSubText("Whats category", application.DesignData.categories.listOfCategories, "Category can be one of these following. you can add more in the Data/Design.json as per your need.");
            return true;
        }

        bool syntaxErrorInCount = false;
        int previousStreak = Utils.CLI.ExtractIntFromCLIParameter(optionalArguments_ReadOnly, "--previousstreak", 0, null, null, out syntaxErrorInCount);
        if (syntaxErrorInCount)
        {
            ConsoleWriter.Print("Invalid syntax for --previousstreak argument.");
            return true;
        }


        var entry = SharedLogic.CreateNewHabit(application.habitManager, categories, title, previousStreak);
        application.habitManager.AddHabit(entry);

        ConsoleWriter.Print("New Habit added with id : " + entry.id);
        return true;
    }
}

public class HabitListCommand : CommandHandlerBaseWithUtility
{
    public HabitListCommand()
    {

    }
    protected override bool ShowHelp()
    {
        SharedLogic.StartCachingHelpText();
        SharedLogic.PrintHelp_Heading("USAGE");
        SharedLogic.PrintHelp_SubText(">habit list", "Lists the habits which are in-progress");
        SharedLogic.PrintHelp_SubText(">habit list --allstatus", "Lists all the habits");

        SharedLogic.PrintHelp_SubText(">habit list --cat:<category>", "Lists all the habits under this category");

        SharedLogic.PrintHelp_Heading("EXAMPLES");
        SharedLogic.PrintHelp_SubText(">habit list");
        SharedLogic.PrintHelp_SubText(">habit list --allstatus", "Shows even the disabled habits!");
        SharedLogic.FlushHelpText();
        return true;
    }

    protected override bool Run()
    {
        if (arguments_ReadOnly.Count != 0)
        {
            ConsoleWriter.Print("Invalid arguments! \n");
            ShowHelp();
            return true;
        }

        List<Habit> habits = new List<Habit>(application.habitManager.Data.entries);

        int lineCount = 0;
        int titleArea = (optionalArguments_ReadOnly.Contains("-e") || optionalArguments_ReadOnly.Contains("--expand")) ? 
            application.DesignData.GetIntegerProperty("view.habit.titleWidthExtended") : 
            application.DesignData.GetIntegerProperty("view.habit.titleWidthDefault");
        int categoryArea = 15;
        int newLineAfter = 5;

        bool syntaxErrorInCategoryFilter = false;
        string categoryFilter = Utils.CLI.ExtractStringFromCLIParameter(optionalArguments_ReadOnly, "--cat", string.Empty, null, null, out syntaxErrorInCategoryFilter);
        if (syntaxErrorInCategoryFilter)
        {
            ConsoleWriter.Print("Invalid syntax for --cat argument.");
            categoryFilter = string.Empty;
            return true;
        }
        else if (categoryFilter != string.Empty && !application.DesignData.DoesCategoryExist(categoryFilter))
        {
            ConsoleWriter.Print("Invalid category");
            categoryFilter = string.Empty;
            return true;
        }

        bool isAll = optionalArguments_ReadOnly.Contains("--allstatus") || optionalArguments_ReadOnly.Contains("-a");

        // output Heading 
        if (habits.Count > 0)
        {
            ConsoleWriter.PrintInColor("{0, -4} {1,-" + categoryArea + "} {2,-" + titleArea + "} {3, -15} {4, -10} {5, -15}",
                application.DesignData.HighlightColorForText,
                "ID", "DEPT", "TITLE", "LAST UPDATED", "COUNT", "SUCCESS %"
                );

            // Sorts based on status to keep In-Progress onces above
            habits.Sort((entry1, entry2) => { return entry1.status.CompareTo(entry2.status); });

            foreach (var habit in habits)
            {
                if(isAll)
                {
                    // No code
                }
                else
                {
                    if (habit.status != Habit.Status.In_Progress)
                        continue;
                }

                if (categoryFilter != string.Empty && !habit.categories.Contains(categoryFilter))
                    continue;

                ConsoleWriter.PrintInColor("{0, -4} {1,-" + categoryArea + "} {2,-" + titleArea + "} {3, -15} {4, -10} {5, -5} {6, -10}",
                    habit.IsDisabled ? application.DesignData.HighlightColorForText_Disabled : application.DesignData.DefaultColorForText,
                    habit.id,
                    (habit.categories != null && habit.categories.Length > 0 ? Utils.Conversions.ArrayToString(habit.categories, true).TruncateWithVisualFeedback(categoryArea - 3) : "INVALID"),
                    habit.title.TruncateWithVisualFeedback(titleArea - 7/*for the ...*/)
                        + (notes.DoesNoteExist(habit.id) ? "+(N)" : ""),
                    (habit.GetLastUpdatedOn().IsThisMinDate() ? "Never" : habit.GetLastUpdatedOn().ShortForm()),
                    habit.GetAllEntryCount( HabitStatus.Completed, true),
                    habit.GetSuccessRate(),
                    habit.IsDisabled ? "Disabled" : ""
                    );

                lineCount++;

                if (lineCount % newLineAfter == 0)
                    ConsoleWriter.Print();
            }
        }
        else
            ConsoleWriter.Print("No habits found! Try adding a few using \"habit add\"");

        return true;
    }
}


public class HabitStreakUpTodayCommand : CommandHandlerBaseWithUtility
{
    public HabitStreakUpTodayCommand()
    {
    }

    protected override bool ShowHelp()
    {
        SharedLogic.StartCachingHelpText();
        SharedLogic.PrintHelp_Heading("USAGE");
        SharedLogic.PrintHelp_SubText(">habit ticktoday <id>", "tick a habit as done, for today");

        SharedLogic.PrintHelp_Heading("EXAMPLES");
        SharedLogic.PrintHelp_SubText(">habit ticktoday 1", "tick a habit with id 1 as done, for today");
        SharedLogic.FlushHelpText();

        return true;
    }
    protected override bool Run()
    {
        if (arguments_ReadOnly.Count != 1)
        {
            ConsoleWriter.Print("Invalid arguments! \n");
            ShowHelp();
            return true;
        }

        int[] id = Utils.Conversions.SplitAndAtoi(arguments_ReadOnly[0]);
        Date date = Date.Today;

        SharedLogic.TryAddHabitEntry(application, id, date, HabitStatus.Completed);

        return true;
    }
}

// @todo - lot of code repeatition here. club all the three classes into one.
public class HabitStreakUpYesterdayCommand : CommandHandlerBaseWithUtility
{
    public HabitStreakUpYesterdayCommand()
    {
    }

    protected override bool ShowHelp()
    {
        SharedLogic.StartCachingHelpText();
        SharedLogic.PrintHelp_Heading("USAGE");
        SharedLogic.PrintHelp_SubText(">habit tickyesterday <id>", "tick a habit as done, for yesterday");

        SharedLogic.PrintHelp_Heading("EXAMPLES");
        SharedLogic.PrintHelp_SubText(">habit tickyesterday 1", "tick a habit with id 1 as done, for yesterday");
        SharedLogic.FlushHelpText();

        return true;
    }
    protected override bool Run()
    {
        if (arguments_ReadOnly.Count != 1)
        {
            ConsoleWriter.Print("Invalid arguments! \n");
            ShowHelp();
            return true;
        }

        int[] id = Utils.Conversions.SplitAndAtoi(arguments_ReadOnly[0]);
        Date date = Date.Today - 1;

        SharedLogic.TryAddHabitEntry(application, id, date, HabitStatus.Completed);

        return true;
    }
}

public class HabitIgnoreTodayCommand : CommandHandlerBaseWithUtility
{
    public HabitIgnoreTodayCommand()
    {
    }

    protected override bool ShowHelp()
    {
        SharedLogic.StartCachingHelpText();
        SharedLogic.PrintHelp_Heading("USAGE");
        SharedLogic.PrintHelp_SubText(">habit ignoretoday <id>", "Mark this habit as ignore for today. Not be considered as failed!");

        SharedLogic.PrintHelp_Heading("EXAMPLES");
        SharedLogic.PrintHelp_SubText(">habit ignoretoday 1", "ignores habit with id 1, for today");
        SharedLogic.FlushHelpText();

        return true;
    }
    protected override bool Run()
    {
        if (arguments_ReadOnly.Count != 1)
        {
            ConsoleWriter.Print("Invalid arguments! \n");
            ShowHelp();
            return true;
        }

        int[] id = Utils.Conversions.SplitAndAtoi(arguments_ReadOnly[0]);
        Date date = Date.Today;

        SharedLogic.TryAddHabitEntry(application, id, date, HabitStatus.Ignored);

        return true;
    }
}

// @todo - lot of code repeatition here. club all the three classes into one.
public class HabitIgnoreYesterdayCommand : CommandHandlerBaseWithUtility
{
    public HabitIgnoreYesterdayCommand()
    {
    }

    protected override bool ShowHelp()
    {
        SharedLogic.StartCachingHelpText();
        SharedLogic.PrintHelp_Heading("USAGE");
        SharedLogic.PrintHelp_SubText(">habit ignoreyesterday <id>", "Mark this habit as ignore for yesterday. Not be considered as failed!");

        SharedLogic.PrintHelp_Heading("EXAMPLES");
        SharedLogic.PrintHelp_SubText(">habit ignoreyesterday 1", "ignores habit with id 1, yesterday");
        SharedLogic.FlushHelpText();

        return true;
    }
    protected override bool Run()
    {
        if (arguments_ReadOnly.Count != 1)
        {
            ConsoleWriter.Print("Invalid arguments! \n");
            ShowHelp();
            return true;
        }

        int[] id = Utils.Conversions.SplitAndAtoi(arguments_ReadOnly[0]);
        Date date = Date.Today - 1;

        SharedLogic.TryAddHabitEntry(application, id, date, HabitStatus.Ignored);

        return true;
    }
}

public class HabitStreakUpCommand : CommandHandlerBaseWithUtility
{
    public HabitStreakUpCommand()
    {
    }

    protected override bool ShowHelp()
    {
        SharedLogic.StartCachingHelpText();
        SharedLogic.PrintHelp_Heading("USAGE");
        SharedLogic.PrintHelp_SubText(">habit tick <id> dd/mm/yy", "tick a habit as done, on a specific date");

        SharedLogic.PrintHelp_Heading("EXAMPLES");
        SharedLogic.PrintHelp_SubText(">habit tick 1 01/01/2022", "tick a habit with id 1 on jan 1st 2022");
        SharedLogic.FlushHelpText();

        return true;
    }
    protected override bool Run()
    {
        if (arguments_ReadOnly.Count != 2)
        {
            ConsoleWriter.Print("Invalid arguments! \n");
            ShowHelp();
            return true;
        }

        int[] id = Utils.Conversions.SplitAndAtoi(arguments_ReadOnly[0]);
        Date date = new Date();
                
        if ( !Date.TryParse(arguments_ReadOnly[1], new System.Globalization.CultureInfo("es-ES"), out date) )
        {
            ConsoleWriter.Print("Date mentioned is invalid. it has to be in the format dd/mm/yy");
            return true;
        }

        SharedLogic.TryAddHabitEntry(application, id, date, HabitStatus.Completed);

        return true;
    }
}
public class HabitDeleteCommand : CommandHandlerBaseWithUtility
{
    public HabitDeleteCommand()
    {
    }

    protected override bool ShowHelp()
    {
        SharedLogic.StartCachingHelpText();
        SharedLogic.PrintHelp_Heading("USAGE");
        SharedLogic.PrintHelp_SubText(">habit delete <id>", "To delete a habit");

        SharedLogic.PrintHelp_Heading("EXAMPLES");
        SharedLogic.PrintHelp_SubText(">habit deelte 1", "deletes a habit with id 1");
        SharedLogic.FlushHelpText();
        return true;
    }
    protected override bool Run()
    {
        if (arguments_ReadOnly.Count != 1)
        {
            ConsoleWriter.Print("Invalid arguments! \n");
            ShowHelp();
            return true;
        }

        int id = Utils.Conversions.Atoi(arguments_ReadOnly[0]);

        Habit hb = application.habitManager.GetHabit_Editable(id);

        if (hb == null)
        {
            ConsoleWriter.Print("Habit with id : {0} not found!", id);
            return true;
        }

        application.habitManager.RemoveHabit(hb);

        ConsoleWriter.Print("Habit with id : {0} removed!", id);
        return true;
    }
}


public class HabitDisableCommand : CommandHandlerBaseWithUtility
{
    public HabitDisableCommand()
    {
    }

    protected override bool ShowHelp()
    {
        SharedLogic.StartCachingHelpText();
        SharedLogic.PrintHelp_Heading("USAGE");
        SharedLogic.PrintHelp_SubText(">habit disable <id>", "To disable a habit");

        SharedLogic.PrintHelp_Heading("EXAMPLES");
        SharedLogic.PrintHelp_SubText(">habit disable 1", "Disable a habit with id 1");

        SharedLogic.FlushHelpText();
        return true;
    }
    protected override bool Run()
    {
        if (arguments_ReadOnly.Count != 1)
        {
            ConsoleWriter.Print("Invalid arguments! \n");
            ShowHelp();
            return true;
        }

        int id = Utils.Conversions.Atoi(arguments_ReadOnly[0]);

        Habit hb = application.habitManager.GetHabit_Editable(id);

        if (hb == null)
        {
            ConsoleWriter.Print("Habit with id : {0} not found!", id);
            return true;
        }

        if (hb.IsDisabled)
        {
            ConsoleWriter.Print("Habit with id: {0} is already disabled. ", id);
            return true;
        }

        hb.SetStatus(Habit.Status.Disabled); ;

        ConsoleWriter.Print("Habit with id : {0} disabled!", id);
        return true;
    }
}

public class HabitReEnableCommand : CommandHandlerBaseWithUtility
{
    public HabitReEnableCommand()
    {
    }

    protected override bool ShowHelp()
    {
        SharedLogic.StartCachingHelpText();
        SharedLogic.PrintHelp_Heading("USAGE");
        SharedLogic.PrintHelp_SubText(">habit re-enable <id>", "To re-enable a disabled habit");

        SharedLogic.PrintHelp_Heading("EXAMPLES");
        SharedLogic.PrintHelp_SubText(">habit re-enable 1", "Re-enable a habit with id 1");

        SharedLogic.FlushHelpText();

        return true;
    }
    protected override bool Run()
    {
        if (arguments_ReadOnly.Count != 1)
        {
            ConsoleWriter.Print("Invalid arguments! \n");
            ShowHelp();
            return true;
        }

        int id = Utils.Conversions.Atoi(arguments_ReadOnly[0]);

        Habit hb = application.habitManager.GetHabit_Editable(id);

        if (hb == null)
        {
            ConsoleWriter.Print("Habit with id : {0} not found!", id);
            return true;
        }

        if (hb.IsEnabled)
        {
            ConsoleWriter.Print("Habit with id: {0} is already enabled. ", id);
            return true;
        }

        hb.SetStatus(Habit.Status.In_Progress); ;

        ConsoleWriter.Print("Habit with id : {0} is enabled now!", id);
        return true;
    }
}

public class HabitResetCommand : CommandHandlerBaseWithUtility
{
    public HabitResetCommand()
    {
    }

    protected override bool ShowHelp()
    {
        SharedLogic.StartCachingHelpText();
        SharedLogic.PrintHelp_Heading("USAGE");
        SharedLogic.PrintHelp_SubText(">habit reset <id>", "This will reset the streak of a habit! For a fresh start!");

        SharedLogic.PrintHelp_Heading("EXAMPLES");
        SharedLogic.PrintHelp_SubText(">habit reset 1", "Resets a habit with ID 1. This will reset all the streak!");

        SharedLogic.FlushHelpText();
        return true;
    }
    protected override bool Run()
    {
        if (arguments_ReadOnly.Count != 1)
        {
            ConsoleWriter.Print("Invalid arguments! \n");
            ShowHelp();
            return true;
        }

        int id = Utils.Conversions.Atoi(arguments_ReadOnly[0]);

        Habit hb = application.habitManager.GetHabit_Editable(id);

        if (hb == null)
        {
            ConsoleWriter.Print("Habit with id : {0} not found!", id);
            return true;
        }

        if (hb.IsDisabled)
        {
            ConsoleWriter.Print("Habit with id: {0} is disabled. You would want to re-enable it before it can be updated!", id);
            return true;
        }

        hb.Reset();

        ConsoleWriter.Print("Habit with id : {0} is resetted. Streak back to 0 :(", id);
        return true;
    }
}

public class HabitShowCommand : CommandHandlerBaseWithUtility
{
    public HabitShowCommand()
    {
    }

    protected override bool ShowHelp()
    {
        SharedLogic.StartCachingHelpText();
        SharedLogic.PrintHelp_Heading("USAGE");
        SharedLogic.PrintHelp_SubText(">habit show <id>", "This will show all the details of a habit!");

        SharedLogic.PrintHelp_Heading("EXAMPLES");
        SharedLogic.PrintHelp_SubText(">habit show 1", "Show more details for habit with id 1");

        SharedLogic.FlushHelpText();
        return true;
    }
    protected override bool Run()
    {
        if (arguments_ReadOnly.Count != 1)
        {
            ConsoleWriter.Print("Invalid arguments! \n");
            ShowHelp();
            return true;
        }

        int id = Utils.Conversions.Atoi(arguments_ReadOnly[0]);

        Habit hb = application.habitManager.GetHabit_ReadOnly(id);

        if (hb == null)
        {
            ConsoleWriter.Print("Habit with id : {0} not found!", id);
            return true;
        }

        // Print Habit details
        {
            // Heading
            ConsoleWriter.PrintInColor("{0, -4} {1,-15} {2}",
                application.DesignData.HighlightColorForText,
                "ID", "DEPT", "TITLE"
                );

            ConsoleWriter.Print("{0, -4} {1,-15} {2}",
                hb.id,
                (hb.categories != null && hb.categories.Length > 0 ? Utils.Conversions.ArrayToString(hb.categories, true) : "INVALID"),
                hb.title);

            ConsoleWriter.Print();

            ConsoleWriter.Print(
                "TICK COUNT : {0}\n\n" + 

                "SUCCESS RATE IN THE LAST 3 DAYS : {1}\n" +
                "SUCCESS RATE IN THE LAST 7 DAYS : {2}\n" +
                "SUCCESS RATE IN THE LAST MONTH : {3}\n\n" +
                                                
                "START DATE : {4}\n" +
                "LAST UPDATED ON : {5}\n\n" + 

                "STATUS : {6}\n" +
                "NOTES ? : {7}",

                hb.GetAllEntryCount(HabitStatus.Completed, true),

                (hb.GetNumberOfDaysFromTheStart() >= 3 ? hb.GetSuccessRateForDuration(Date.Today - 3, Date.Today - 1) + " %" : "Need more data. Keep updating the habit daily!"),
                (hb.GetNumberOfDaysFromTheStart() >= 7 ? hb.GetSuccessRateForDuration(Date.Today - 7, Date.Today - 1) + " %" : "Need more data. Keep updating the habit daily!"),
                (hb.GetNumberOfDaysFromTheStart() >= 30 ? hb.GetSuccessRateForDuration(Date.Today - 30, Date.Today - 1) + " %" : "Need more data. Keep updating the habit daily!"),

                hb._startDate.ShortFormWithDay(),
                (hb.GetLastUpdatedOn().IsThisMinDate() ? "Never" : hb.GetLastUpdatedOn().ShortFormWithDay()),

                hb.StatusStr,
                (notes.DoesNoteExist(hb.id) ? "YES" : "NO")
                );
        }

        ConsoleWriter.Print();

        ConsoleWriter.Print("DATES WHEN ITS TICKED >>");
        ConsoleWriter.PushIndent();

        if (hb._entries == null || hb._entries.Count == 0)
            ConsoleWriter.Print("No records found! Try to tick(complete) the habit first!");
        else
        {
            ConsoleWriter.EmptyLine();
            for ( Date month = Date.Today; ShouldPrintMonth( month, hb._entries ); month = month.AddMonths(-1) )
            {
                SharedLogic.PrintMonth(application, month, hb);
                ConsoleWriter.EmptyLine();
            }
        }

        ConsoleWriter.ClearIndent();

        return true;
    }

    private bool ShouldPrintMonth( Date month, List<HabitEntry> entries )
    {
        foreach( var entry in entries )
        {
            if (month.Month.Equals(entry.date.Month) && month.Year.Equals(entry.date.Year))
                return true;
        }
        return false;
    }

}

public class HabitEditTitleCommand : CommandHandlerBaseWithUtility
{
    public HabitEditTitleCommand()
    {
    }

    protected override bool ShowHelp()
    {
        SharedLogic.StartCachingHelpText();
        SharedLogic.PrintHelp_Heading("USAGE");
        SharedLogic.PrintHelp_SubText(">habit edittitle <id> <new title>", "This is to rename the habit title!");

        SharedLogic.PrintHelp_Heading("EXAMPLES");
        SharedLogic.PrintHelp_SubText(">habit edittitle 1 \"Wake up at 7 AM\"", "rename the title of habit : 1 to 'Wake up at 7 AM'");
        SharedLogic.FlushHelpText();
        return true;
    }
    protected override bool Run()
    {
        if (arguments_ReadOnly.Count != 2)
        {
            ConsoleWriter.Print("Invalid arguments! \n");
            ShowHelp();
            return true;
        }

        int id = Utils.Conversions.Atoi(arguments_ReadOnly[0]);
        string title = arguments_ReadOnly[1];

        Habit hb = application.habitManager.GetHabit_ReadOnly(id);

        if (hb == null)
        {
            ConsoleWriter.Print("Habit with id : {0} not found!", id);
            return true;
        }

        application.habitManager.GetHabit_Editable(id).title = title;
        ConsoleWriter.Print("Habit with id : {0} renamed to - {1}", id, title);

        return true;
    }
}

public class HabitCatNotesCommand : CommandHandlerBaseWithUtility
{
    public HabitCatNotesCommand()
    {

    }

    protected override bool ShowHelp()
    {
        SharedLogic.StartCachingHelpText();
        SharedLogic.PrintHelp_Heading("USAGE");
        SharedLogic.PrintHelp_SubText(">habit cat <habitID>", "Prints the notes of a habit. You can also use 'printnote' instead of cat");
        SharedLogic.PrintHelp_SubText(">habit printnote <habitID>", "Same as cat");

        SharedLogic.PrintHelp_Heading("EXAMPLES");
        SharedLogic.PrintHelp_SubText(">habit cat 1", "Prints the notes for habit with id 1");
        SharedLogic.FlushHelpText();
        return true;
    }

    protected override bool Run()
    {
        if (arguments_ReadOnly.Count != 1)
        {
            ConsoleWriter.Print("Invalid arguments! \n");
            ShowHelp();
            return true;
        }

        int id = Utils.Conversions.Atoi(arguments_ReadOnly[0]);

        
        if (application.habitManager.DoesHabitExist(id))
        {
            if (!notes.DoesNoteExist(id))
                ConsoleWriter.Print("Notes not found for the habit with id : {0}", id);
            else
            {
                ConsoleWriter.PrintInColor("NOTES :", application.DesignData.HighlightColorForText);
                ConsoleWriter.PrintText(notes.GetNoteContent(id));
            }
        }
        else
            ConsoleWriter.Print("habit not found with id : " + id);

        return true;
    }
}

public class HabitEditNoteCommand : CommandHandlerBaseWithUtility
{
    public HabitEditNoteCommand()
    {

    }

    protected override bool ShowHelp()
    {
        SharedLogic.StartCachingHelpText();
        SharedLogic.PrintHelp_Heading("USAGE");
        SharedLogic.PrintHelp_SubText(">habit note <habitID>", "Opens notes for a habit");
        SharedLogic.PrintHelp_SubText(">habit note <habitID> --ext:<editorname>", "Provide external editor name of your choice, to open the notes in. Example : code or vim");
        SharedLogic.PrintHelp_SubText(">habit note <habitID> --append:<Message>", "Appends a message directly to the note");
        SharedLogic.PrintHelp_SubText(">habit note <habitID> --appendlog:<Message>", "Appends a message directly to a note, with a timestamp");

        SharedLogic.PrintHelp_Heading("ADVANCED");
        SharedLogic.PrintHelp_SubText("You can change the default editor (to open your notes in) in the Data/Design.json under 'defaultExternalEditor'");
        SharedLogic.PrintHelp_SubText("you can use '--nowait' to have jarvis not wait for the notes to be closed.");

        SharedLogic.PrintHelp_Heading("EXAMPLES");
        SharedLogic.PrintHelp_SubText(">habit note 1", "Edit the notes for habit : 1");
        SharedLogic.PrintHelp_SubText(">habit note 1 --ext:code", "Edit the notes for habit : 1, within the visual studio code");
        SharedLogic.PrintHelp_SubText(">habit note 1 --append:\"Buy milk\"", "Add 'buy milk' to the notes!");
        SharedLogic.FlushHelpText();
        return true;
    }

    protected override bool Run()
    {
        if (arguments_ReadOnly.Count != 1)
        {
            ConsoleWriter.Print("Invalid arguments! \n");
            ShowHelp();
            return true;
        }

        int id = Utils.Conversions.Atoi(arguments_ReadOnly[0]);
        bool syntaxError = false;
        bool waitForTheProgramToEnd = !optionalArguments_ReadOnly.Contains("--nowait");

        string externalProgram = Utils.CLI.ExtractStringFromCLIParameter(optionalArguments_ReadOnly, "--ext", string.Empty, null, null, out syntaxError );

        if ( syntaxError ) 
        {
            ConsoleWriter.Print("Invalid syntax for --ext argument."); 
            return true;
        }

        syntaxError = false;
        string appendMessage = Utils.CLI.ExtractStringFromCLIParameter(optionalArguments_ReadOnly, "--append", string.Empty, null, null, out syntaxError);

        if (syntaxError)
        {
            ConsoleWriter.Print("Invalid syntax for --append argument.");
            return true;
        }

        string appendLogMessage = Utils.CLI.ExtractStringFromCLIParameter(optionalArguments_ReadOnly, "--appendlog", string.Empty, null, null, out syntaxError);

        if (syntaxError)
        {
            ConsoleWriter.Print("Invalid syntax for --appendlog argument.");
            return true;
        }

        if ( !appendLogMessage.IsEmpty() )
        {
            appendMessage = "Log on " +  DateTime.Now.ToShortDateString() + " " + appendLogMessage;
        }

        if (application.habitManager.DoesHabitExist(id))
        {
            notes.CreateNoteIfUnavailable(id);

            if (!appendMessage.IsEmpty())
            {
                ConsoleWriter.Print("Message appended to the notes");
                notes.AppendToNote(id, appendMessage);
            }
            else
            {
                notes.OpenNote(application, id, externalProgram, waitForTheProgramToEnd, true);
            }
        }
        else
            ConsoleWriter.Print("habit not found with id : " + id);
        return true;
    }
}


public class HabitDeleteNoteCommand : CommandHandlerBaseWithUtility
{
    public HabitDeleteNoteCommand()
    {

    }

    protected override bool ShowHelp()
    {
        SharedLogic.StartCachingHelpText();
        SharedLogic.PrintHelp_Heading("USAGE");
        SharedLogic.PrintHelp_SubText(">habit deletenote <habitID>", "Deletes the notes");
        SharedLogic.FlushHelpText();
        return true;
    }

    protected override bool Run()
    {
        if (arguments_ReadOnly.Count != 1)
        {
            ConsoleWriter.Print("Invalid arguments! \n");
            ShowHelp();
            return true;
        }

        int id = Utils.Conversions.Atoi(arguments_ReadOnly[0]);

        if (application.habitManager.DoesHabitExist(id))
        {
            if (notes.DoesNoteExist(id))
            {
                notes.RemoveNote(id);
                ConsoleWriter.Print("Notes deleted");
            }
            else
                ConsoleWriter.Print("Notes doesnt exit for habit with id : " + id);
        }
        else
            ConsoleWriter.Print("habit not found with id : " + id);

        return true;
    }
}