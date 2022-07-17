using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Jarvis; //@todo 


public class PomodoroHandler : CommandHandlerBaseWithUtility
{
    public PomodoroHandler()
    {

    }

    protected override bool ShowHelp()
    {
        SharedLogic.StartCachingHelpText();
        SharedLogic.PrintHelp_Heading("USAGE");
        SharedLogic.PrintHelp_SubText(">pomo start", "To start a new pomodoro timer!");
        SharedLogic.PrintHelp_SubText(">pomo report", "To list all the progress today");
        SharedLogic.PrintHelp_SubText(">pomo discard", "To discard the current timer!");
        SharedLogic.FlushHelpText();
        return true;
    }

    protected override CommandHandlerBase GetSpecializedCommandHandler(Jarvis.JModel model, out List<string> argumentsForSpecializedHandler, bool printErrors)
    {
        string action = arguments_ReadOnly != null && arguments_ReadOnly.Count > 0 ? arguments_ReadOnly[0] : null;
        CommandHandlerBase selectedHander = null;

        switch (action)
        {
            case "start":
                selectedHander = new PomodoroStartCommand();
                break;
            case "report":
                selectedHander = new PomodoroReportCommand();
                break;
            case "discard":
                selectedHander = new PomodoroDiscardCommand();
                break;
            default:
                if(printErrors)
                    ConsoleWriter.Print("Invalid command. Try 'Pomodoro --help' for more information");
                break;
        }

        if (selectedHander != null)
        {
            argumentsForSpecializedHandler = new List<string>(arguments_ReadOnly);
            argumentsForSpecializedHandler.RemoveAt(0);

            Utils.Assert(selectedHander is CommandHandlerBaseWithUtility);
            (selectedHander as CommandHandlerBaseWithUtility).Init(model, notes);
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

public class PomodoroStartCommand : CommandHandlerBaseWithUtility
{
    public PomodoroStartCommand()
    {

    }

    protected override bool ShowHelp()
    {
        SharedLogic.StartCachingHelpText();
        SharedLogic.PrintHelp_Heading("USAGE");
        SharedLogic.PrintHelp_SubText(">pomo start <category> <count>", "This will start a new timer. Count is number of pomodoros you want to work for. Each is for " + JConstants.POMODORO_TIME + " mins.");
        //SharedLogic.PrintHelp_SubText(">pomo start <category> -1", "This will start a new timer. this will keep on going till you say 'pomodoro stop'");

        SharedLogic.PrintHelp_Heading("EXAMPLE");
        SharedLogic.PrintHelp_SubText(">pomo start office 1", "This will start a new timer for office for " + JConstants.POMODORO_TIME  + " mins.");
        SharedLogic.PrintHelp_SubText(">pomo start health 2", "This will start a new timer for health for " + JConstants.POMODORO_TIME *2 +" mins");
        //SharedLogic.PrintHelp_SubText(">pomo start health -1", "This will start a new count up timer. keeps on going up till you say 'stop'");

        SharedLogic.PrintHelp_WithHeadingAndSubText("Whats category", model.DesignData.categories.listOfCategories, "Category can be one of these following. you can add more in the Data/Design.json as per your need.");
        SharedLogic.FlushHelpText();

        return true;
    }
    protected override bool Run()
    {
        if (arguments_ReadOnly.Count != 2 )
        {
            ConsoleWriter.Print("Invalid arguments! \n");
            ShowHelp();
            return true;
        }

        if (model.UserData.IsPomodoroInProgress())
        {
            ConsoleWriter.Print("Pomodoro already in progress!");
            return true;
        }

        string category = arguments_ReadOnly[0];

        if (!model.DesignData.DoesCategoryExist(category))
        {
            ConsoleWriter.Print("Invalid categories.\n");
            SharedLogic.PrintHelp_WithHeadingAndSubText("Whats category", model.DesignData.categories.listOfCategories, "Category can be one of these following. you can add more in the Data/Design.json as per your need.");
            return true;
        }

        int count = Utils.Conversions.Atoi( arguments_ReadOnly[1]);

        if (count < 0 || count >5)
        {
            ConsoleWriter.Print("Count is invalid. It has to be between 1 to 5. that is " + JConstants.POMODORO_TIME + " mins to " + JConstants.POMODORO_TIME * 5 + " mins!");
            return true;
        }

        model.UserData.StartPomodoro(DateTime.Now, category, count);
        ConsoleWriter.Print("New Pomodoro started");

        if(count == -1 )
        {
            ConsoleWriter.Print("This is a count up timer. Will continue till a 'stop' command is issued or for a max is 5 pomodoros though!");
        }

        return true;
    }
}

public class PomodoroDiscardCommand : CommandHandlerBaseWithUtility
{
    public PomodoroDiscardCommand()
    {

    }

    protected override bool ShowHelp()
    {
        SharedLogic.StartCachingHelpText();
        SharedLogic.PrintHelp_Heading("USAGE");
        SharedLogic.PrintHelp_SubText(">pomo discard", "This will discard entire progress");
        
        SharedLogic.PrintHelp_Heading("EXAMPLE");
        SharedLogic.PrintHelp_SubText(">pomo discard", "This will discard entire progress");

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

        if (!model.UserData.IsPomodoroInProgress())
        {
            ConsoleWriter.Print("There is no Pomodoro in progress!");
            return true;
        }

        model.UserData.StopPomodoro();

        ConsoleWriter.Print("Progress discarded!");

        return true;
    }
}

public class PomodoroReportCommand : CommandHandlerBaseWithUtility
{
    public PomodoroReportCommand()
    {

    }

    protected override bool ShowHelp()
    {
        SharedLogic.StartCachingHelpText();
        SharedLogic.PrintHelp_Heading("USAGE");
        SharedLogic.PrintHelp_SubText(">pomo report", "This will show the progress for today");

        SharedLogic.PrintHelp_Heading("EXAMPLE");
        SharedLogic.PrintHelp_SubText(">pomo report", "This will show the progress for today");

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

        var pomoEntryForToday = model.pomoManager.GetPomo_ReadOnly(Date.Today);

        if(pomoEntryForToday == null || pomoEntryForToday._entries.Count == 0)
        {
            ConsoleWriter.Print("No Entries for today!");
            return true;
        }

        foreach( var categoryProgress in pomoEntryForToday._entries)
        {
            ConsoleWriter.Print("{0,-15} {1}", categoryProgress.Key, categoryProgress.Value);
        }
        return true;
    }
}

public class PomodoroObserver
{
    public void Run( JModel model, System.Func<bool> stopObserving )
    {
        while( !stopObserving())
        {
            if (model.UserData.IsPomodoroInProgress())
            {
                int totalMinsNeeded = model.UserData.GetPomodoroData().pomoCount * JConstants.POMODORO_TIME;

                //ConsoleWriter.Print("Total mins passed : " + (DateTime.Now - model.UserData.GetPomodoroStartTime()).TotalMinutes);

                if ((DateTime.Now - model.UserData.GetPomodoroStartTime()).TotalMinutes >= totalMinsNeeded)
                {
                    model.pomoManager.CreatePomoForTodayIfNecessary();
                    var pomoEntryForToday = model.pomoManager.GetPomo_Editable(Date.Today);
                    pomoEntryForToday.AddNewEntry(model.UserData.GetPomodoroData().category, model.UserData.GetPomodoroData().pomoCount);

                    model.UserData.StopPomodoro();
#if RELEASE_LOG
                    ConsoleWriter.Print(">>> Pomodoro ended >>>");
#endif
                    Console.Beep();
                    System.Media.SoundPlayer player= new System.Media.SoundPlayer( JConstants.PATH_TO_DATA + @"Notification.mp3");
                    player.Play();
                }
            }

            Thread.Sleep(1000);
        }
#if RELEASE_LOG
        ConsoleWriter.Print(">>> Observer thread ended >>>");
#endif
    }
}