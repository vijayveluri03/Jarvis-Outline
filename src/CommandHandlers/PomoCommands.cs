using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Jarvis; //@todo 


enum PomodoroTaskType
{
    WORK = 1, 
    REST = 2,
    LONGREST = 3
};

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
        SharedLogic.PrintHelp_SubText(">pomo rest", "To start a small break!");
        SharedLogic.PrintHelp_SubText(">pomo longrest", "To start a long break!");
        
        SharedLogic.PrintHelp_SubText(">pomo status", "To list all the progress + status today");

        SharedLogic.PrintHelp_SubText(">pomo discard", "To discard the current timer!");

        SharedLogic.PrintHelp_Heading("ADVANCED");
        SharedLogic.PrintHelp_SubText(">pomo addoffline", "To add a new pomodoro timer directly!");
        SharedLogic.FlushHelpText();
        return true;
    }

    protected override CommandHandlerBase GetSpecializedCommandHandler(Jarvis.JModel model, out List<string> argumentsForSpecializedHandler, bool printErrors)
    {
        string action = arguments_ReadOnly != null && arguments_ReadOnly.Count > 0 ? arguments_ReadOnly[0] : null;
        CommandHandlerBase selectedHander = null;

        if (AreArgumentsEmpty())
        {
            argumentsForSpecializedHandler = null;
            return null;
        }

        switch (action)
        {
            case "start":
                selectedHander = new PomodoroStartCommand();
                break;
            case "rest":
                selectedHander = new PomodoroRestCommand();
                break;
            case "longrest":
                selectedHander = new PomodoroLongRestCommand();
                break;

            case "status":
                selectedHander = new PomodoroStatusCommand();
                break;
            case "discard":
                selectedHander = new PomodoroDiscardCommand();
                break;
            case "addoffline":
                selectedHander = new PomodoroAddOfflineCommand();
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
        SharedLogic.PrintHelp_SubText(">pomo start <category>", "This will start a new timer. Each is for " + JConstants.POMODORO_WORK_TIME + " mins.");
        
        SharedLogic.PrintHelp_Heading("EXAMPLE");
        SharedLogic.PrintHelp_SubText(">pomo start office", "This will start a new timer for office for " + JConstants.POMODORO_WORK_TIME  + " mins.");
        SharedLogic.PrintHelp_SubText(">pomo start health", "This will start a new timer for health for " + JConstants.POMODORO_WORK_TIME +" mins");
        //SharedLogic.PrintHelp_SubText(">pomo start health -1", "This will start a new count up timer. keeps on going up till you say 'stop'");

        SharedLogic.PrintHelp_WithHeadingAndSubText("Whats category", model.DesignData.categories.listOfCategories, "Category can be one of these following. you can add more in the Data/Design.json as per your need.");
        SharedLogic.FlushHelpText();

        return true;
    }
    protected override bool Run()
    {
        if (arguments_ReadOnly.Count != 1 )
        {
            ConsoleWriter.Print("Invalid arguments! \n");
            ShowHelp();
            return true;
        }

        if (model.UserData.IsPomodoroTaskInProgress())
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

        //int count = Utils.Conversions.Atoi( arguments_ReadOnly[1]);

        //if (count < 0 || count >5)
        //{
        //    ConsoleWriter.Print("Count is invalid. It has to be between 1 to 5. that is " + JConstants.POMODORO_WORK_TIME + " mins to " + JConstants.POMODORO_WORK_TIME * 5 + " mins!");
        //    return true;
        //}

        model.UserData.StorePomodoroStatus(DateTime.Now, category, (int)PomodoroTaskType.WORK);
        SoundPlayer.PlayPomodoroWorkStarted();

        ConsoleWriter.Print("New Pomodoro started");

        return true;
    }
}

public class PomodoroRestCommand : CommandHandlerBaseWithUtility
{
    public PomodoroRestCommand()
    {

    }

    protected override bool ShowHelp()
    {
        SharedLogic.StartCachingHelpText();
        SharedLogic.PrintHelp_Heading("USAGE");
        SharedLogic.PrintHelp_SubText(">pomo rest", "This will start a short break.");

        SharedLogic.PrintHelp_Heading("EXAMPLE");
        SharedLogic.PrintHelp_SubText(">pomo rest", "Short break;");

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

        if (model.UserData.IsPomodoroTaskInProgress())
        {
            ConsoleWriter.Print("Pomodoro in progress. Cant rest now!");
            return true;
        }

        model.UserData.StorePomodoroStatus(DateTime.Now, "", (int)PomodoroTaskType.REST);
        SoundPlayer.PlayPomodoroRestStarted();

        ConsoleWriter.Print("Break started");

        return true;
    }
}

public class PomodoroLongRestCommand : CommandHandlerBaseWithUtility
{
    public PomodoroLongRestCommand()
    {

    }

    protected override bool ShowHelp()
    {
        SharedLogic.StartCachingHelpText();
        SharedLogic.PrintHelp_Heading("USAGE");
        SharedLogic.PrintHelp_SubText(">pomo longrest", "This will start a long break.");

        SharedLogic.PrintHelp_Heading("EXAMPLE");
        SharedLogic.PrintHelp_SubText(">pomo longrest", "Long break;");

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

        if (model.UserData.IsPomodoroTaskInProgress())
        {
            ConsoleWriter.Print("Pomodoro in progress. Cant rest now!");
            return true;
        }

        model.UserData.StorePomodoroStatus(DateTime.Now, "", (int)PomodoroTaskType.LONGREST);
        SoundPlayer.PlayPomodoroRestStarted();

        ConsoleWriter.Print("Long break started");

        return true;
    }
}

public class PomodoroAddOfflineCommand : CommandHandlerBaseWithUtility
{
    public PomodoroAddOfflineCommand()
    {

    }

    protected override bool ShowHelp()
    {
        SharedLogic.StartCachingHelpText();
        SharedLogic.PrintHelp_Heading("USAGE");
        SharedLogic.PrintHelp_SubText(">pomo addoffline <category> <count>", "This will add a new timer directly (with out starting it). Count is number of pomodoros");
        
        SharedLogic.PrintHelp_Heading("EXAMPLE");
        SharedLogic.PrintHelp_SubText(">pomo addoffline office 1", "This will add an offline timer for office for " + JConstants.POMODORO_WORK_TIME + " mins.");
        SharedLogic.PrintHelp_SubText(">pomo addoffline health 2", "This will add an offline timer for health for " + JConstants.POMODORO_WORK_TIME * 2 + " mins");
        //SharedLogic.PrintHelp_SubText(">pomo addoffline health -1", "This will start a new count up timer. keeps on going up till you say 'stop'");

        SharedLogic.PrintHelp_WithHeadingAndSubText("Whats category", model.DesignData.categories.listOfCategories, "Category can be one of these following. you can add more in the Data/Design.json as per your need.");
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

        string category = arguments_ReadOnly[0];

        if (!model.DesignData.DoesCategoryExist(category))
        {
            ConsoleWriter.Print("Invalid categories.\n");
            SharedLogic.PrintHelp_WithHeadingAndSubText("Whats category", model.DesignData.categories.listOfCategories, "Category can be one of these following. you can add more in the Data/Design.json as per your need.");
            return true;
        }

        int count = Utils.Conversions.Atoi(arguments_ReadOnly[1]);

        if (count < 0 || count > 5)
        {
            ConsoleWriter.Print("Count is invalid. It has to be between 1 to 5. that is " + JConstants.POMODORO_WORK_TIME + " mins to " + JConstants.POMODORO_WORK_TIME * 5 + " mins!");
            return true;
        }

        model.pomoManager.CreatePomoForTodayIfNecessary();
        model.pomoManager.GetPomo_EditableForToday().AddNewEntry(category, count);
        ConsoleWriter.Print("Added " +count +" offline Pomodoro(s) under category : " + category);

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

        if (!model.UserData.IsPomodoroTaskInProgress())
        {
            ConsoleWriter.Print("There is no Pomodoro in progress!");
            return true;
        }

        model.UserData.ResetPomodoroStatus();

        ConsoleWriter.Print("Progress discarded!");

        return true;
    }
}

public class PomodoroStatusCommand : CommandHandlerBaseWithUtility
{
    public PomodoroStatusCommand()
    {

    }

    protected override bool ShowHelp()
    {
        SharedLogic.StartCachingHelpText();
        SharedLogic.PrintHelp_Heading("USAGE");
        SharedLogic.PrintHelp_SubText(">pomo status", "This will show the status for today");

        SharedLogic.PrintHelp_Heading("EXAMPLE");
        SharedLogic.PrintHelp_SubText(">pomo status", "This will show the status for today");

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

        if (model.UserData.IsPomodoroTaskInProgress())
        {
            int minsRemaining = 0;
            string status = "";
            SharedLogic.GetActivePomodoroTime(model, out minsRemaining, out status);

            ConsoleWriter.Print("Pomodoro task is active! " + status + " for another " + minsRemaining + " mins!");
        }
        else
            ConsoleWriter.Print("No pomodoro task is active!");

        ConsoleWriter.EmptyLine();

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
            if (model.UserData.IsPomodoroTaskInProgress())
            {
                int totalMinsNeeded = 0;

                var data = model.UserData.GetPomodoroData();
                if (data.taskType == (int)PomodoroTaskType.WORK)
                    totalMinsNeeded = JConstants.POMODORO_WORK_TIME;
                else if (data.taskType == (int)PomodoroTaskType.REST)
                    totalMinsNeeded = JConstants.POMODORO_REST_TIME;
                else if (data.taskType == (int)PomodoroTaskType.LONGREST)
                    totalMinsNeeded = JConstants.POMODORO_LONG_REST_TIME;
                else
                    Utils.Assert(false);

                //ConsoleWriter.Print("Total mins passed : " + (DateTime.Now - model.UserData.GetPomodoroStartTime()).TotalMinutes);

                if ((DateTime.Now - data.startTime).TotalMinutes >= totalMinsNeeded)
                {
                    model.pomoManager.CreatePomoForTodayIfNecessary();
                    var pomoEntryForToday = model.pomoManager.GetPomo_Editable(Date.Today);


                    if (data.taskType == (int)PomodoroTaskType.WORK)
                    {
                        pomoEntryForToday.AddNewEntry(model.UserData.GetPomodoroData().category, 1);
#if RELEASE_LOG
                        ConsoleWriter.Print(">>> Pomodoro work ended >>>");
#endif
                        SoundPlayer.PlayPomodoroWorkComplete();
                        // play sound
                    }
                    else if (data.taskType == (int)PomodoroTaskType.REST)
                    {
                        SoundPlayer.PlayPomodoroRestComplete();
                        // todo - play sound; do nothing;
                    }
                    else if (data.taskType == (int)PomodoroTaskType.LONGREST)
                    {
                        SoundPlayer.PlayPomodoroRestComplete();
                        // todo - play sound; do nothing;
                    }
                    else
                        Utils.Assert(false);

                    model.UserData.ResetPomodoroStatus();
                }
            }

            Thread.Sleep(1000);
        }
#if RELEASE_LOG
        ConsoleWriter.Print(">>> Observer thread ended >>>");
#endif
    }
}