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
                "Jarvis habit add  // To add a habit\n" +
                "Jarvis habit streakup // to increase the steak of a habits\n" +
                "Jarvis habit reset // to reset a habits\n" +
                "Jarvis habit list // to list all the habits\n");

        return true;
    }

    protected override CommandHandlerBase GetSpecializedCommandHandler(Jarvis.JApplication application, out List<string> argumentsForSpecializedHandler)
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
                selectedHander = null;
                ConsoleWriter.Print("NYI"); // @todo
                break;

            default:
                break;
        }

        if ( selectedHander != null )
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
        int previousStreak = Utils.ExtractIntFromArgument(optionalArguments_ReadOnly, "--previousstreak", 0, null, null, out syntaxErrorInCount );
        if( syntaxErrorInCount )
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

        int lineCount = 0;
        int titleArea = 40;
        int categoryArea = 15;

        bool syntaxErrorInCategoryFilter = false;
        string categoryFilter = Utils.ExtractStringFromArgument(optionalArguments_ReadOnly, "--cat", string.Empty, null, null, out syntaxErrorInCategoryFilter );
        if( syntaxErrorInCategoryFilter )
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
        if (application.habitManager.Data.entries.Count() > 0)
        {
            ConsoleWriter.Print("{0, -4} {1,-" + categoryArea + "} {2,-" + titleArea + "} {3, -15} {4, -15}",
                "ID", "DEPT", "TITLE", "LAST UPDATED", "STREAK"
                );


            foreach (var entry in application.habitManager.Data.entries)
            {
                if (categoryFilter != string.Empty && !entry.categories.Contains(categoryFilter))
                    continue;

                ConsoleWriter.Print("{0, -4} {1,-" + categoryArea +"} {2,-" + titleArea + "} {3, -15} {4, -15}",
                    entry.id,
                    (entry.categories != null && entry.categories.Length > 0 ? Utils.ArrayToString(entry.categories, true).TruncateWithVisualFeedback(categoryArea - 3) : "INVALID"),
                    entry.title.TruncateWithVisualFeedback(titleArea - 6/*for the ...*/) + (entry.notes.Count > 0 ? "+(" + entry.notes.Count + ")" : ""),
                    entry.GetLastUpdatedOn().ShortForm(),
                    entry.GetStreak()
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
                "jarvis habit streakup <id>"    
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

        int id = Utils.Atoi(arguments_ReadOnly[0]);

        Habit hb = application.habitManager.GetHabit_Editable(id);

        if ( hb == null )
        {
            ConsoleWriter.Print("Habit with id : {0} not found!", id);
            return true;
        }
        
        if( hb.IsEntryOn(DateTime.Now.ZeroTime()))
        {
            ConsoleWriter.Print("Habit with id: {0} already streaked up today. This can be only done once a day!", id);
            return true;
        }

        hb.AddNewEntry(DateTime.Now.ZeroTime());

        ConsoleWriter.Print("Habit with id : {0} Streaked up!", id);
        return true;
    }
}