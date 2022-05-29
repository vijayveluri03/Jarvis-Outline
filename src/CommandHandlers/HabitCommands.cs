using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommandLine;
using Jarvis; //@todo 


public class HabitHandler : CommandHandlerBase
{
    public HabitHandler()
    {

    }

    protected override bool ShowHelp()
    {
        ConsoleWriter.Print("USAGE : \n" +
        "Jarvis habit add  \t\t| To add a habit\n" +
        "Jarvis habit streakup \t\t| to increase the steak of a habits\n" +
        "Jarvis habit reset \t\t| to reset a habits\n" +
        "Jarvis habit list \t\t| to list all the habits\n" +
        "jarvis habit show \t\t| to show details of a habit\n" +
        "jarvis habit disable \t\t| to disable a habit\n" +
        "jarvis habit re-enable \t\t| to re-enable a disabled habit\n" + 
        "\n" +
        "NOTES\n" + 
        "jarvis habit addnotes" + "\t\t| create new notes for a habit. You can open it using opennotes\n" + 
        "jarvis habit deletenotes" + "\t\t| delete notes for a habit\n" + 
        "jarvis habit opennotes" + "\t\t| open notes for a habit. If the notes doesnt exit, try addnotes first\n" +
        "jarvis habit printnotes" + "\t\t| print the notes. ( you can also use catnotes instead of printnotes)\n"

        );

        return true;
    }

    protected override CommandHandlerBase GetSpecializedCommandHandler(Jarvis.JApplication application, out List<string> argumentsForSpecializedHandler, bool printErrors)
    {
        string action = arguments_ReadOnly != null && arguments_ReadOnly.Count > 0 ? arguments_ReadOnly[0] : null;
        CommandHandlerBase selectedHander = null;

        switch (action)
        {
            case "add":
                selectedHander = new HabitAddCommand();
                break;
            case "streakup":
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
            case "disable":
                selectedHander = new HabitDisableCommand();
                break;
            case "re-enable":
                selectedHander = new HabitReEnableCommand();
                break;
                case "catnotes":
            case "printnotes":
                selectedHander = new HabitCatNotesCommand();
                break;
            case "addnotes":
                selectedHander = new HabitAddNotesCommand();
                break;
            case "opennotes":
                selectedHander = new HabitOpenNotesCommand();
                break;
            case "deletenotes":
                selectedHander = new HabitDeleteNotesCommand();
                break;
            default:
                if(printErrors)
                    ConsoleWriter.Print("Invalid command. Try 'jarvis habit --help' for more information");
                break;
        }

        if (selectedHander != null)
        {
            argumentsForSpecializedHandler = new List<string>(arguments_ReadOnly);
            argumentsForSpecializedHandler.RemoveAt(0);
        }
        else
            argumentsForSpecializedHandler = null;

        return selectedHander;
    }

    protected override bool Run(Jarvis.JApplication application)
    {

        if (arguments_ReadOnly.Count < 1)
        {
            ConsoleWriter.Print("Invalid arguments! \n");
            ShowHelp();

            return false;
        }

        Utils.Assert(false, "Shouldnt be here");
        return true;
    }
}

public class HabitAddCommand : CommandHandlerBase
{
    public HabitAddCommand()
    {

    }

    protected override bool ShowHelp()
    {
        ConsoleWriter.Print("USAGE : \n" +
                "jarvis habit add <category> <title>\n" +
                "jarvis habit add <category> <title> --previousstreak:<count>  // This sets the streak to a base value\n" +
                "Category can be office,learn,chores,health. you can add more in the design data as per your need.\n\n"
                );
        return true;
    }
    protected override bool Run(Jarvis.JApplication application)
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
            ConsoleWriter.Print("Invalid categories.\n" +
                "Category can be office,learn,chores,health. you can add more in the design data as per your need.");
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

public class HabitListCommand : CommandHandlerBase
{
    public HabitListCommand()
    {

    }
    protected override bool ShowHelp()
    {
        ConsoleWriter.Print("USAGE : \n" +
                "jarvis habit list   // lists all the habits\n" +
                "jarvis habit list --cat:<category> // Shows only those category\n"

                //"jarvis habit list --detailed // show more details \n"
                );
        return true;
    }

    protected override bool Run(Jarvis.JApplication application)
    {
        if (arguments_ReadOnly.Count != 0)
        {
            ConsoleWriter.Print("Invalid arguments! \n");
            ShowHelp();
            return true;
        }

        List<Habit> habits = new List<Habit>(application.habitManager.Data.entries);

        int lineCount = 0;
        int titleArea = 40;
        int categoryArea = 15;

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

        // output Heading 
        if (habits.Count > 0)
        {
            ConsoleWriter.Print("{0, -4} {1,-" + categoryArea + "} {2,-" + titleArea + "} {3, -15} {4, -15}",
                "ID", "DEPT", "TITLE", "LAST UPDATED", "STREAK"
                );

            // Sorts based on status to keep In-Progress onces above
            habits.Sort((entry1, entry2) => { return entry1.status.CompareTo(entry2.status); });

            foreach (var habit in habits)
            {
                if (categoryFilter != string.Empty && !habit.categories.Contains(categoryFilter))
                    continue;

                ConsoleWriter.PrintInColor("{0, -4} {1,-" + categoryArea + "} {2,-" + titleArea + "} {3, -15} {4, -5} {5, -10}",
                    habit.IsDisabled ? application.DesignData.HighlightColorForText_Disabled : application.DesignData.DefaultColorForText,
                    habit.id,
                    (habit.categories != null && habit.categories.Length > 0 ? Utils.Conversions.ArrayToString(habit.categories, true).TruncateWithVisualFeedback(categoryArea - 3) : "INVALID"),
                    habit.title.TruncateWithVisualFeedback(titleArea - 7/*for the ...*/)
                        + (Utils.FileHandler.DoesFileExist(JConstants.PATH_TO_HABITS_NOTES + habit.id) ? "+(N)" : ""),
                    habit.GetLastUpdatedOn().ShortForm(),
                    habit.GetStreak(),
                    habit.IsDisabled ? "Disabled" : ""
                    );

                lineCount++;

                //@todo
                if (lineCount % 5 == 0)
                    ConsoleWriter.Print();
            }
        }
        else
            ConsoleWriter.Print("No habits found! Try adding a few using \"jarvis add\"");

        return true;
    }
}

public class HabitStreakUpCommand : CommandHandlerBase
{
    public HabitStreakUpCommand()
    {
    }

    protected override bool ShowHelp()
    {
        ConsoleWriter.Print("USAGE : \n" +
                "jarvis habit streakup <id>\n" +
                "jarvis habit streakup <id>  <--when:-1>   // How many days before ?. -1 this timelog is of yesterday. -2 for a day before that. by default, this is 0, as in the time log is created for today."
                );
        return true;
    }
    protected override bool Run(Jarvis.JApplication application)
    {
        if (arguments_ReadOnly.Count != 1)
        {
            ConsoleWriter.Print("Invalid arguments! \n");
            ShowHelp();
            return true;
        }

        bool syntaxErrorForWhenArgument = false;
        int id = Utils.Conversions.Atoi(arguments_ReadOnly[0]);
        int deltaTime = Utils.CLI.ExtractIntFromCLIParameter(optionalArguments_ReadOnly, "--when", 0, null, null, out syntaxErrorForWhenArgument);

        Habit hb = application.habitManager.GetHabit_Editable(id);

        if (hb == null)
        {
            ConsoleWriter.Print("Habit with id : {0} not found!", id);
            return true;
        }

        if( syntaxErrorForWhenArgument)
        {
            ConsoleWriter.Print("syntax invalid for --when argument. please try again");
            return true;
        }

        DateTime dateForEntry = DateTime.Now.ZeroTime().AddDays(deltaTime);
        if (hb.IsEntryOn(dateForEntry))
        {
            ConsoleWriter.Print("Habit with id: {0} already streaked up on {1}. This can only be done once a day!", id, dateForEntry.ShortForm());
            return true;
        }

        if (hb.IsDisabled)
        {
            ConsoleWriter.Print("Habit with id: {0} is disabled. You would want to re-enable it before it can be updated!", id);
            return true;
        }

        hb.AddNewEntry(dateForEntry);

        ConsoleWriter.Print("Habit with id : {0} Streaked up!", id);
        if( deltaTime != 0 )
            ConsoleWriter.Print("Entry added for date : {0}!", dateForEntry.ShortForm());

        return true;
    }
}

public class HabitDisableCommand : CommandHandlerBase
{
    public HabitDisableCommand()
    {
    }

    protected override bool ShowHelp()
    {
        ConsoleWriter.Print("USAGE : \n" +
                "jarvis habit disable <id>"
                );
        return true;
    }
    protected override bool Run(Jarvis.JApplication application)
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

public class HabitReEnableCommand : CommandHandlerBase
{
    public HabitReEnableCommand()
    {
    }

    protected override bool ShowHelp()
    {
        ConsoleWriter.Print("USAGE : \n" +
                "jarvis habit re-enable <id>"
                );
        return true;
    }
    protected override bool Run(Jarvis.JApplication application)
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

public class HabitResetCommand : CommandHandlerBase
{
    public HabitResetCommand()
    {
    }

    protected override bool ShowHelp()
    {
        ConsoleWriter.Print("USAGE : \n" +
                "jarvis habit reset <id> // This will reset the streak of a habit!"
                );
        return true;
    }
    protected override bool Run(Jarvis.JApplication application)
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

        if (hb.IsEntryOn(DateTime.Now.ZeroTime()))
        {
            ConsoleWriter.Print("Habit with id: {0} is already updated today. try again tomorrow!", id);
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

public class HabitShowCommand : CommandHandlerBase
{
    public HabitShowCommand()
    {
    }

    protected override bool ShowHelp()
    {
        ConsoleWriter.Print("USAGE : \n" +
                "jarvis habit show <id> // This will show all the details of a habit!"
                );
        return true;
    }
    protected override bool Run(Jarvis.JApplication application)
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

        // Print Task details
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

            ConsoleWriter.Print("STREAK : {0, -15}\n" +
                "STATUS : {1}\n" + 
                "NOTES : {2}\n" +
                "LAST COMPLETED ON : {3}\n" +
                "AVG IN LAST 7 DAYS : {4,15}\n" +
                "AVG IN LAST MONTH : {5,-15}",
                hb.GetStreak(),
                hb.StatusStr,
                (Utils.FileHandler.DoesFileExist(JConstants.PATH_TO_HABITS_NOTES + hb.id) ? "YES" : "NO"),
                hb.GetLastUpdatedOn().ShortFormWithDay(),
                (hb.GetStreak() >= 7 && hb.GetEntryCount() >= 7 ? hb.GetEntryCount(7 - 1) / 7.0f : "Need more data to show this"),
                (hb.GetStreak() >= 30 && hb.GetEntryCount() >= 30 ? hb.GetEntryCount(28 - 1) / 7.0f : "Need more data to show this")
                );
        }

        ConsoleWriter.Print();

        {
            ConsoleWriter.PrintInColor("LAST 30 DAY ACTIVITY : ", application.DesignData.HighlightColorForText);
            ConsoleWriter.Print("'+' - streak up entry present on that day | '-' - No entry available on that day | weekends are coloured.\n");
            for (int day = 0; day <= 30; day++)
            {
                DateTime date = DateTime.Now.ZeroTime().AddDays(-1 * day);
                bool isWeekend = date.DayOfWeek == DayOfWeek.Sunday || date.DayOfWeek == DayOfWeek.Saturday;
                bool isEntryAvailable = hb.IsEntryOn(date);

                ConsoleWriter.PrintWithColorWithOutLineBreak(isEntryAvailable ? " + " : " - ", isWeekend ? application.DesignData.HighlightColorForText_3 : application.DesignData.DefaultColorForText);
            }
            ConsoleWriter.EmptyLine();
        }

        return true;
    }
}

public class HabitCatNotesCommand : CommandHandlerBase
{
    public HabitCatNotesCommand()
    {

    }

    protected override bool ShowHelp()
    {
        ConsoleWriter.Print("USAGE : \n" +
                "jarvis habit catnotes <habitID> \t\t| Prints the notes of a habit. You can also use printnotes instead of catnotes\n" +
                "jarvis habit printnotes <habitID> \t\t| Same as catnotes\n"
                );
        return true;
    }

    protected override bool Run(Jarvis.JApplication application)
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
            if( !Utils.FileHandler.DoesFileExist(JConstants.PATH_TO_HABITS_NOTES + id) )
                ConsoleWriter.Print("Notes not found for the habit with id : {0}", id);
            else 
            {
                ConsoleWriter.PrintInColor("NOTES :", application.DesignData.HighlightColorForText);
                ConsoleWriter.PrintText(Utils.FileHandler.Read(JConstants.PATH_TO_HABITS_NOTES + id));
            }
        }
        else
            ConsoleWriter.Print("habit not found with id : " + id);

        return true;
    }
}

public class HabitOpenNotesCommand : CommandHandlerBase
{
    public HabitOpenNotesCommand()
    {

    }

    protected override bool ShowHelp()
    {
        ConsoleWriter.Print("USAGE : \n" +
                "jarvis habit opennotes <habitID> \t\t| Opens notes for a habit. If notes doesnt exist, you might want to try addnotes first!\n" + 
                "jarvis habit opennotes <habitID> --ext:<editorname> \t\t| provide external editor program name of your choice. Example : code or vim\n" + 
                "You can change the default editor in the DesignData.json under 'defaultExternalEditor'\n"
                );
        return true;
    }

    protected override bool Run(Jarvis.JApplication application)
    {
        if (arguments_ReadOnly.Count != 1)
        {
            ConsoleWriter.Print("Invalid arguments! \n");
            ShowHelp();
            return true;
        }

        int id = Utils.Conversions.Atoi(arguments_ReadOnly[0]);
        bool syntaxError = false;
        string externalProgram = Utils.CLI.ExtractStringFromCLIParameter(optionalArguments_ReadOnly, "--ext", string.Empty, null, null, out syntaxError );

        if ( syntaxError ) 
        {
            ConsoleWriter.Print("Invalid syntax for --ext argument."); 
            return true;
        }

        if (application.habitManager.DoesHabitExist(id))
        {
            if( !Utils.FileHandler.DoesFileExist(JConstants.PATH_TO_HABITS_NOTES + id) )
                ConsoleWriter.Print("Notes not found for the habit with id : {0}", id);
            else 
            {
                ConsoleWriter.Print("Opening Notes");
                Utils.OpenAFileInEditor(
                    JConstants.PATH_TO_HABITS_NOTES + id, 
                    externalProgram.IsEmpty() ? application.DesignData.defaultExternalEditor : externalProgram,
                    true /* wait for the program to end*/);
                ConsoleWriter.Print("Closing Notes");
            }
        }
        else
            ConsoleWriter.Print("habit not found with id : " + id);
        return true;
    }
}

public class HabitAddNotesCommand : CommandHandlerBase
{
    public HabitAddNotesCommand()
    {

    }

    protected override bool ShowHelp()
    {
        ConsoleWriter.Print("USAGE : \n" +
                "jarvis habit addnotes <habitID> \t\t| Creates new notes for a habit. You can try opennotes after this!\n"
                );
        return true;
    }

    protected override bool Run(Jarvis.JApplication application)
    {
        if (arguments_ReadOnly.Count != 1)
        {
            ConsoleWriter.Print("Invalid arguments! \n");
            ShowHelp();
            return true;
        }

        int id = Utils.Conversions.Atoi(arguments_ReadOnly[0]);
        bool syntaxError = false;
        string externalProgram = Utils.CLI.ExtractStringFromCLIParameter(optionalArguments_ReadOnly, "--ext", string.Empty, null, null, out syntaxError );

        if ( syntaxError ) 
        {
            ConsoleWriter.Print("Invalid syntax for --ext argument."); 
            return true;
        }

        if (application.habitManager.DoesHabitExist(id))
        {
            if( !Utils.FileHandler.DoesFileExist(JConstants.PATH_TO_HABITS_NOTES + id) )
            {
                Utils.FileHandler.Create(JConstants.PATH_TO_HABITS_NOTES + id);
                ConsoleWriter.Print("Notes created");
            }
            else
                ConsoleWriter.Print("Notes already exists for habit with id : " + id);
        }
        else
            ConsoleWriter.Print("habit not found with id : " + id);

        return true;
    }
}

public class HabitDeleteNotesCommand : CommandHandlerBase
{
    public HabitDeleteNotesCommand()
    {

    }

    protected override bool ShowHelp()
    {
        ConsoleWriter.Print("USAGE : \n" +
                "jarvis habit deletenotes <habitID> \t\t| deletes the notes.\n"
                );
        return true;
    }

    protected override bool Run(Jarvis.JApplication application)
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
            if( Utils.FileHandler.DoesFileExist(JConstants.PATH_TO_HABITS_NOTES + id) )
            {
                Utils.FileHandler.DoesFileExist(JConstants.PATH_TO_HABITS_NOTES + id);
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