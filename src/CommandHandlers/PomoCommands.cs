using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Jarvis; //@todo 


public enum PomodoroSessionType
{
    WORK = 1, 
    REST = 2,
    LONGREST = 3,
    LONGWORK = 4
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
        SharedLogic.PrintHelp_SubText(">pomo work", "To start a new WORK session!");
        SharedLogic.PrintHelp_SubText(">pomo rest", "To start a small REST session!");
        SharedLogic.PrintHelp_SubText(">pomo status", "To list all the progress + status for today");
        SharedLogic.PrintHelp_SubText(">pomo discard", "To discard the current session!");
        
        
        SharedLogic.PrintHelp_Heading("ADVANCED");
        SharedLogic.PrintHelp_SubText(">pomo longrest", "To start a long REST!");
        SharedLogic.PrintHelp_SubText(">pomo longwork", "To start a long WORK session!");

        SharedLogic.PrintHelp_SubText(">pomo addoffline", "To add an offline Work session directly!");
        SharedLogic.PrintHelp_SubText(">pomo queue", "See the sessions in queue!");
        SharedLogic.PrintHelp_SubText(">pomo clearqueue", "clears all the sessions in the queue!");
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
            case "work":
                selectedHander = new PomodoroWorkCommand();
                break;
            case "longwork":
                selectedHander = new PomodoroLongWorkCommand();
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
            case "queue":
                selectedHander = new PomodoroQueueCommand();
                break;
            case "clearqueue":
                selectedHander = new PomodoroClearQueueCommand();
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
            (selectedHander as CommandHandlerBaseWithUtility).Init(model, sharedData, noteUtility);
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

public class PomodoroWorkCommand : CommandHandlerBaseWithUtility
{
    public PomodoroWorkCommand()
    {

    }

    protected override bool ShowHelp()
    {
        SharedLogic.StartCachingHelpText();
        SharedLogic.PrintHelp_Heading("USAGE");
        SharedLogic.PrintHelp_SubText(">pomo work <category>", "to start a new WORK session. Each is for " + JConstants.POMODORO_WORK_TIME + " mins.");
        SharedLogic.PrintHelp_SubText(">pomo work <category> -q", "to queue up a new WORK session. This will start after the current session has ended!");

        SharedLogic.PrintHelp_Heading("EXAMPLE");
        SharedLogic.PrintHelp_SubText(">pomo work office", "This will start a new WORK session for office for " + JConstants.POMODORO_WORK_TIME  + " mins.");
        SharedLogic.PrintHelp_SubText(">pomo work health", "This will start a new WORK session for health for " + JConstants.POMODORO_WORK_TIME +" mins");
      
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

        bool addToQueue = optionalArguments_ReadOnly.Contains("--queue") || optionalArguments_ReadOnly.Contains("-q");

        if (model.UserData.IsPomodoroSessionInProgress() && !addToQueue)
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


        sharedData.InsertIntoPomodoroQueue(PomodoroSessionType.WORK, category);

        if (addToQueue)
            ConsoleWriter.Print("Inserted the WORK session into Queue. It will start when everything in the queue is done!");
        else
            ConsoleWriter.Print("Starting WORK");

        return true;
    }
}

public class PomodoroLongWorkCommand : CommandHandlerBaseWithUtility
{
    public PomodoroLongWorkCommand()
    {

    }

    protected override bool ShowHelp()
    {
        SharedLogic.StartCachingHelpText();
        SharedLogic.PrintHelp_Heading("USAGE");
        SharedLogic.PrintHelp_SubText(">pomo longwork <category>", "to start a new long WORK session. Each is for " + JConstants.POMODORO_LONG_WORK_TIME + " mins.");
        SharedLogic.PrintHelp_SubText(">pomo longwork <category> -q", "to queue up a new WORK session. This will start after the current session has ended!");

        SharedLogic.PrintHelp_Heading("EXAMPLE");
        SharedLogic.PrintHelp_SubText(">pomo longwork office", "This will start a new long WORK session for office for " + JConstants.POMODORO_LONG_WORK_TIME + " mins.");
        SharedLogic.PrintHelp_SubText(">pomo longwork health", "This will start a new long WORK session for health for " + JConstants.POMODORO_LONG_WORK_TIME + " mins");

        SharedLogic.PrintHelp_WithHeadingAndSubText("Whats category", model.DesignData.categories.listOfCategories, "Category can be one of these following. you can add more in the Data/Design.json as per your need.");
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

        bool addToQueue = optionalArguments_ReadOnly.Contains("--queue") || optionalArguments_ReadOnly.Contains("-q");

        if (model.UserData.IsPomodoroSessionInProgress() && !addToQueue)
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

        sharedData.InsertIntoPomodoroQueue(PomodoroSessionType.LONGWORK, category);

        if (addToQueue)
            ConsoleWriter.Print("Inserted the WORK session into Queue. It will start when everything in the queue is done!");
        else
            ConsoleWriter.Print("Starting WORK");

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
        SharedLogic.PrintHelp_SubText(">pomo rest", "This will start a short rest.");
        SharedLogic.PrintHelp_SubText(">pomo rest -q", "to queue up a new REST session. This will start after the current session has ended!");

        SharedLogic.PrintHelp_Heading("EXAMPLE");
        SharedLogic.PrintHelp_SubText(">pomo rest", "Short rest");

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

        bool addToQueue = optionalArguments_ReadOnly.Contains("--queue") || optionalArguments_ReadOnly.Contains("-q");

        if (model.UserData.IsPomodoroSessionInProgress() && !addToQueue)
        {
            ConsoleWriter.Print("Pomodoro in progress. Cant rest now!");
            return true;
        }

        sharedData.InsertIntoPomodoroQueue(PomodoroSessionType.REST, "");

        if (addToQueue)
            ConsoleWriter.Print("Inserted the REST session into Queue. It will start when everything in the queue is done!");
        else
            ConsoleWriter.Print("Starting REST");

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
        SharedLogic.PrintHelp_SubText(">pomo longrest", "This will start a long REST.");
        SharedLogic.PrintHelp_SubText(">pomo longrest -q", "to queue up a new REST session. This will start after the current session has ended!");

        SharedLogic.PrintHelp_Heading("EXAMPLE");
        SharedLogic.PrintHelp_SubText(">pomo longrest", "Long REST");

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

        bool addToQueue = optionalArguments_ReadOnly.Contains("--queue") || optionalArguments_ReadOnly.Contains("-q");

        if (model.UserData.IsPomodoroSessionInProgress() && !addToQueue)
        {
            ConsoleWriter.Print("Pomodoro in progress. Cant rest now!");
            return true;
        }

        sharedData.InsertIntoPomodoroQueue(PomodoroSessionType.LONGREST, "");

        if (addToQueue)
            ConsoleWriter.Print("Inserted the Pomodoro item to Queue. It will start when everything in the queue is done!");
        else
            ConsoleWriter.Print("Starting Long REST");

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
        SharedLogic.PrintHelp_SubText(">pomo addoffline <category> <count>", "To add an offline Work session directly!. Count is number of WORK sessions completed");
        
        SharedLogic.PrintHelp_Heading("EXAMPLE");
        SharedLogic.PrintHelp_SubText(">pomo addoffline office 1", "This will add an offline work session for office for " + JConstants.POMODORO_WORK_TIME + " mins.");
        SharedLogic.PrintHelp_SubText(">pomo addoffline health 2", "This will add an offline work session for health for " + JConstants.POMODORO_WORK_TIME * 2 + " mins");
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
        ConsoleWriter.Print("Added " +count +" offline WORK session(s) under category : " + category);

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
        SharedLogic.PrintHelp_SubText(">pomo discard", "This will discard the current session");
        
        SharedLogic.PrintHelp_Heading("EXAMPLE");
        SharedLogic.PrintHelp_SubText(">pomo discard");

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

        if (!model.UserData.IsPomodoroSessionInProgress())
        {
            ConsoleWriter.Print("There is no Session in progress!");
            return true;
        }

        model.UserData.ResetPomodoroStatus();

        ConsoleWriter.Print("Session discarded!");

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
        SharedLogic.PrintHelp_SubText(">pomo status");

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

        if (model.UserData.IsPomodoroSessionInProgress())
        {
            int minsRemaining = 0;
            string status = "";
            SharedLogic.GetActivePomodoroTime(model, out minsRemaining, out status);

            ConsoleWriter.Print("Session is active! " + status + " for another " + minsRemaining + " mins!");
        }
        else
            ConsoleWriter.Print("No Session is active!");

        ConsoleWriter.EmptyLine();

        var pomoEntryForToday = model.pomoManager.GetPomo_ReadOnly(Date.Today);

        if(pomoEntryForToday == null || pomoEntryForToday._entries.Count == 0)
        {
            ConsoleWriter.Print("No WORK done today!");
            return true;
        }

        ConsoleWriter.PrintInColor("{0, -15} {1}",
                model.DesignData.HighlightColorForText,
                "CATEGORY", "SESSION(S)"
                );

        int total = 0;
        foreach ( var categoryProgress in pomoEntryForToday._entries)
        {
            ConsoleWriter.Print("{0,-15} {1}", categoryProgress.Key, categoryProgress.Value);
            total += categoryProgress.Value;
        }
        ConsoleWriter.PrintInColor("{0, -15} {1}",
                model.DesignData.HighlightColorForText,
                "TOTAL", total
                );
        return true;
    }
}

public class PomodoroQueueCommand : CommandHandlerBaseWithUtility
{
    public PomodoroQueueCommand()
    {

    }

    protected override bool ShowHelp()
    {
        SharedLogic.StartCachingHelpText();
        SharedLogic.PrintHelp_Heading("USAGE");
        SharedLogic.PrintHelp_SubText(">pomo queue", "See all the pending Sessions in queue");

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

        // cloning list for thread safety
        List<JSharedData.PomodoroQueueItem> items = new List<JSharedData.PomodoroQueueItem>(sharedData._pomodoroSessionQueue);

        if(items.Count == 0)
        {
            ConsoleWriter.Print("Queue is empty! You can add to the queue by using '-q' with your work or rest commands!");
            return true;
        }

        ConsoleWriter.PrintInColor("{0,-6} {1, -15} {2}",
                model.DesignData.HighlightColorForText,
                "ORDER", "SESSION TYPE", "CATEGORY"
                );

        int order = 1;
        foreach (var item in items)
        {
            ConsoleWriter.Print("{0, -6} {1,-15} {2}", order, item.type, string.IsNullOrEmpty(item.category) ? "NA" : item.category);
            order++;
        }
        return true;
    }
}

public class PomodoroClearQueueCommand : CommandHandlerBaseWithUtility
{
    public PomodoroClearQueueCommand()
    {

    }

    protected override bool ShowHelp()
    {
        SharedLogic.StartCachingHelpText();
        SharedLogic.PrintHelp_Heading("USAGE");
        SharedLogic.PrintHelp_SubText(">pomo clearqueue", "Clears all the sessions in queue");

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

        sharedData.ClearPomodoroQueue();
        ConsoleWriter.Print("Queue cleared!");
        return true;
    }
}


public class PomodoroObserver
{
    public void Run( JModel model, JSharedData sharedData, System.Func<bool> stopObserving )
    {
        DateTime waitTill = DateTime.Now;
        while( !stopObserving())
        {
            if (DateTime.Now < waitTill)
            { 
            }
            else if (model.UserData.IsPomodoroSessionInProgress())
            {
                int totalMinsNeeded = 0;

                var data = model.UserData.GetPomodoroData();
                if (data.sessionType == (int)PomodoroSessionType.WORK)
                    totalMinsNeeded = JConstants.POMODORO_WORK_TIME;
                else if (data.sessionType == (int)PomodoroSessionType.LONGWORK)
                    totalMinsNeeded = JConstants.POMODORO_LONG_WORK_TIME;
                else if (data.sessionType == (int)PomodoroSessionType.REST)
                    totalMinsNeeded = JConstants.POMODORO_REST_TIME;
                else if (data.sessionType == (int)PomodoroSessionType.LONGREST)
                    totalMinsNeeded = JConstants.POMODORO_LONG_REST_TIME;
                else
                    Utils.Assert(false);

                //ConsoleWriter.Print("Total mins passed : " + (DateTime.Now - model.UserData.GetPomodoroStartTime()).TotalMinutes);

                if ((DateTime.Now - data.startTime).TotalMinutes >= totalMinsNeeded)
                {
                    model.pomoManager.CreatePomoForTodayIfNecessary();
                    var pomoEntryForToday = model.pomoManager.GetPomo_Editable(Date.Today);


                    if (data.sessionType == (int)PomodoroSessionType.WORK)
                    {
#if RELEASE_LOG
                        ConsoleWriter.Print(">>> Pomodoro WORK session ended >>>");
#endif
                        pomoEntryForToday.AddNewEntry(model.UserData.GetPomodoroData().category, 1);
                        SoundPlayer.PlayPomodoroWorkComplete();
                    }
                    else if (data.sessionType == (int)PomodoroSessionType.LONGWORK)
                    {
#if RELEASE_LOG
                        ConsoleWriter.Print(">>> Pomodoro long WORK session ended >>>");
# endif
                        pomoEntryForToday.AddNewEntry(model.UserData.GetPomodoroData().category, 2/* todo hardcode*/);
                        SoundPlayer.PlayPomodoroRestComplete();
                    }
                    else if (data.sessionType == (int)PomodoroSessionType.REST)
                    {
#if RELEASE_LOG
                        ConsoleWriter.Print(">>> Pomodoro REST session ended >>>");
#endif
                        SoundPlayer.PlayPomodoroRestComplete();
                    }
                    else if (data.sessionType == (int)PomodoroSessionType.LONGREST)
                    {
#if RELEASE_LOG
                        ConsoleWriter.Print(">>> Pomodoro long REST session ended >>>");
#endif
                        SoundPlayer.PlayPomodoroRestComplete();
                    }
                    else
                        Utils.Assert(false);

                    model.UserData.ResetPomodoroStatus();
                }
            }
            else if( !sharedData.IsPomodoroQueueEmpty())
            {
                var queueItem = sharedData.PopPomodoroQueue();
                Utils.Assert(queueItem != null);

                if (queueItem.type == PomodoroSessionType.WORK)
                {
                    model.UserData.StorePomodoroStatus(DateTime.Now, queueItem.category, (int)PomodoroSessionType.WORK);
                    SoundPlayer.PlayPomodoroWorkStarted();

                    ConsoleWriter.Print(">>> New Pomodoro WORK session started >>>");
                }
                else if (queueItem.type == PomodoroSessionType.LONGWORK)
                {
                    model.UserData.StorePomodoroStatus(DateTime.Now, queueItem.category, (int)PomodoroSessionType.LONGWORK);
                    SoundPlayer.PlayPomodoroWorkStarted();

                    ConsoleWriter.Print(">>> New Pomodoro LONG WORK session started >>>");
                }
                else if (queueItem.type == PomodoroSessionType.REST)
                {
                    model.UserData.StorePomodoroStatus(DateTime.Now, "", (int)PomodoroSessionType.REST);
                    SoundPlayer.PlayPomodoroRestStarted();

                    ConsoleWriter.Print(">>> New Pomodoro REST session started >>>");
                }
                else if (queueItem.type == PomodoroSessionType.LONGREST)
                {
                    model.UserData.StorePomodoroStatus(DateTime.Now, "", (int)PomodoroSessionType.LONGREST);
                    SoundPlayer.PlayPomodoroRestStarted();

                    ConsoleWriter.Print(">>> New Pomodoro Long REST session started >>>");
                }
                else
                    Utils.Assert(false);
            }
            

            Thread.Sleep(3000);
        }
#if RELEASE_LOG
        ConsoleWriter.Print(">>> Observer thread ended >>>");
#endif
    }
}