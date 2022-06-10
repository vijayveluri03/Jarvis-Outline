using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommandLine;
using Jarvis; //@todo 


public class TaskHandler : CommandHandlerBase
{
    public TaskHandler()
    {

    }

    protected override bool ShowHelp()
    {
        SharedLogic.PrintHelp("USAGE");
        SharedLogic.PrintHelp("Jarvis task add ", "To add a task");
        SharedLogic.PrintHelp("Jarvis task list", "to list all the tasks");

        SharedLogic.PrintHelp("Jarvis task delete", "to remove a task");
        SharedLogic.PrintHelp("Jarvis task complete", "to complete a task");
        SharedLogic.PrintHelp("Jarvis task discard", "to discard a task");
        SharedLogic.PrintHelp("Jarvis task archieve", "to archieve a task.");
        SharedLogic.PrintHelp("Jarvis task open", "to re-open a task");

        SharedLogic.PrintHelp("\nLOG TIME");
        SharedLogic.PrintHelp("Jarvis task start", "to start time log");
        SharedLogic.PrintHelp("Jarvis task stop", "to stop time log");
        SharedLogic.PrintHelp("Jarvis task active", "to show if any time record is in progress");
        SharedLogic.PrintHelp("Jarvis task recordtimelog", "to record an offline task");

        SharedLogic.PrintHelp("\nADVANCED");
        SharedLogic.PrintHelp("Jarvis task show", "to show more details of a task");
        SharedLogic.PrintHelp("Jarvis task report", "to show all the work done in the last day/week");
        SharedLogic.PrintHelp("Jarvis task edittitle", "To edit the title of a task");

        SharedLogic.PrintHelp("\nNOTES");
        SharedLogic.PrintHelp("Jarvis task note", "open note for a task");
        SharedLogic.PrintHelp("Jarvis task printnote", "print the notes. ( you can also use cat instead of printnote");
        SharedLogic.PrintHelp("Jarvis task deletenote", "delete note for a task");
        return true;
    }

    protected override CommandHandlerBase GetSpecializedCommandHandler(Jarvis.JApplication application, out List<string> argumentsForSpecializedHandler, bool printErrors)
    {
        string action = arguments_ReadOnly != null && arguments_ReadOnly.Count > 0 ? arguments_ReadOnly[0] : null;
        CommandHandlerBase selectedHander = null;

        switch (action)
        {
            case "add":
                selectedHander = new TaskAddCommand();
                break;
            case "list":
                selectedHander = new TaskListCommand();
                break;
            case "delete":
            case "remove":
                selectedHander = new TaskRemoveCommand();
                break;
            case "start":
                selectedHander = new TaskStartCommand();
                break;
            case "stop":
                selectedHander = new TaskStopCommand();
                break;
            case "active":
                selectedHander = new TaskActiveCommand();
                break;
            case "show":
                selectedHander = new TaskShowCommand();
                break;
            case "recordtimelog":
                selectedHander = new TaskRecordTimeLogCommand();
                break;
            case "complete":
                selectedHander = new TaskSetStatusCommand(Task.Status.Complete);
                break;
            case "discard":
                selectedHander = new TaskSetStatusCommand(Task.Status.Discard);
                break;
            case "open":
                selectedHander = new TaskSetStatusCommand(Task.Status.Open);
                break;
            case "archieve":
                selectedHander = new TaskSetStatusCommand(Task.Status.Archieve);
                break;
            case "report":
                selectedHander = new TaskReportCommand();
                break;
            case "cat":
            case "printnote":
                selectedHander = new TaskCatNotesCommand();
                break;
            //case "createnote":
            //case "newnote":
            //case "addnote":
            //    selectedHander = new TaskcreatenoteCommand();
            //    break;
            case "note":
            case "opennote":
                selectedHander = new TaskEditNoteCommand();
                break;
            case "deletenote":
                selectedHander = new TaskDeleteNoteCommand();
                break;
            case "edittitle":
                selectedHander = new TaskEditTitleCommand();
                break;
            default:
                if (printErrors)
                    ConsoleWriter.Print("Invalid command. Try 'jarvis task --help' for more information.");
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
            return true;
        }

        Utils.Assert(false, "Shouldnt be here");
        return true;
    }
}


public class TaskAddCommand : CommandHandlerBase
{
    public TaskAddCommand()
    {

    }

    protected override bool ShowHelp()
    {
        SharedLogic.PrintHelp("USAGE");
        SharedLogic.PrintHelp("Jarvis task add <category> <title>");
        SharedLogic.PrintHelp("Jarvis task add <category> <title> --story", "A story is collection of tasks" );
        SharedLogic.PrintHelp("Jarvis task add <category> <title> --collection", "Creates a collection instead of a task). Collections are to club many simple tasks. Like Grocery list. Easy to keep them in one place");
        SharedLogic.PrintHelp("Jarvis task add <category> <title> --archieve", "To directly send it to archieve");
        
        SharedLogic.PrintHelp("\nMORE INFO");
        SharedLogic.PrintHelp("Category can be office,learn,chores,health. you can add more in the DesignData.json as per your need.");
        SharedLogic.PrintHelp("use --story or -s to create a story (instead of a task)");
        SharedLogic.PrintHelp("use --collection or -c to create a collection (instead of a task)");
        SharedLogic.PrintHelp("use --archieve to send it straight to archieve category");
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
        bool isStory = optionalArguments_ReadOnly.Contains("--story") || optionalArguments_ReadOnly.Contains("-s");
        bool isCollection = optionalArguments_ReadOnly.Contains("--collection") || optionalArguments_ReadOnly.Contains("-c");
        bool isArchieve = optionalArguments_ReadOnly.Contains("--archieve");

        if (!application.DesignData.DoesCategoryExist(categories))
        {
            ConsoleWriter.Print("Invalid categories.\n" +
                "Category can be office,learn,chores,health. you can add more in the design data as per your need.");
            return true;
        }

        var entry = SharedLogic.CreateNewTask(
            application.taskManager,
            categories,
            title,
            isStory ? Task.Type.Story : (isCollection ? Task.Type.Collection : Task.Type.Task),
            isArchieve ? Task.Status.Archieve : Task.Status.Open);

        application.taskManager.AddTask(entry);

        ConsoleWriter.Print("New task added with id : " + entry.id);
        return true;
    }
}


public class TaskRemoveCommand : CommandHandlerBase
{
    public TaskRemoveCommand()
    {

    }

    protected override bool ShowHelp()
    {
        SharedLogic.PrintHelp("USAGE");
        SharedLogic.PrintHelp("Jarvis task remove <taskID> ", "Task id is the ID of the task you are trying to remove");
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

        string[] ids = arguments_ReadOnly[0].Split(',');

        foreach (var idStr in ids)
        {
            int id = Utils.Conversions.Atoi(idStr, -1);
            if (application.taskManager.RemoveTaskIfExists(id))
            {
                application.logManager.RemoveAllEntries(id);
                ConsoleWriter.Print("Task removed with id : " + id);
            }
            else
                ConsoleWriter.Print("Task not found with id : " + id);
        }

        return true;
    }
}

public class TaskStartCommand : CommandHandlerBase
{
    public TaskStartCommand()
    {

    }

    protected override bool ShowHelp()
    {
        SharedLogic.PrintHelp("USAGE :");
        SharedLogic.PrintHelp("Jarvis task start <taskID>", "Start time tracking");
                
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

        if (application.UserData.IsTaskInProgress())
        {
            ConsoleWriter.Print("There is already a task with ID : {0} in progress.", application.UserData.taskProgress.taskIDInProgress);
            return true;
        }

        if (application.taskManager.DoesTaskExist(id))
        {
            application.UserData.StartTask(id, DateTime.Now);
            ConsoleWriter.Print("Started progress on Task with id : {0} -> {1}", id, application.taskManager.GetTask_ReadOnly(id).title);
        }
        else
            ConsoleWriter.Print("Task not found with id : " + id);

        return true;
    }
}

public class TaskStopCommand : CommandHandlerBase
{
    public TaskStopCommand()
    {
    }

    protected override bool ShowHelp()
    {
        SharedLogic.PrintHelp("USAGE");
        SharedLogic.PrintHelp("Jarvis task stop" , "Stops time tracking a task");
        SharedLogic.PrintHelp("Jarvis task stop <comments>");
        SharedLogic.PrintHelp("Jarvis task stop --discard", "to ignore the recording alltogether");

        return true;
    }

    protected override bool Run(Jarvis.JApplication application)
    {
        if (arguments_ReadOnly.Count > 1)
        {
            ConsoleWriter.Print("Invalid arguments! \n");
            ShowHelp();
            return true;
        }

        if (!application.UserData.IsTaskInProgress())
        {
            ConsoleWriter.Print("There is no task in progress.");
            return true;
        }

        string comments = arguments_ReadOnly != null && arguments_ReadOnly.Count > 0 ? arguments_ReadOnly[0] : string.Empty;
        int id = application.UserData.taskProgress.taskIDInProgress;
        int timeTakenInMinutes = (int)(DateTime.Now - application.UserData.taskProgress.startTime).TotalMinutes;
        bool discard = optionalArguments_ReadOnly.Contains("--discard");
        application.UserData.StopTask();

        if (discard)
        {
            ConsoleWriter.Print("Stopped and discarded progress on Task with id : {0} -> {1} ", id, application.taskManager.GetTask_ReadOnly(id).title);
            return true;
        }

        // Add record to log manager
        {
            LogEntry le = new LogEntry();
            le.id = id;
            le.date = DateTime.Now;
            le.comment = comments;
            le.timeTakenInMinutes = timeTakenInMinutes;

            application.logManager.AddEntry(le);
        }

        ConsoleWriter.Print("Stopped progress on Task with id : {0} -> {1} ", id, application.taskManager.GetTask_ReadOnly(id).title);
        ConsoleWriter.Print("Total time recorded : {0}", Utils.Time.MinutesToHoursString(timeTakenInMinutes));

        return true;
    }
}

public class TaskEditTitleCommand : CommandHandlerBase
{
    public TaskEditTitleCommand()
    {

    }

    protected override bool ShowHelp()
    {
        SharedLogic.PrintHelp("USAGE");
        SharedLogic.PrintHelp("Jarvis task edittitle <taskID> <title>", "rename the title");
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

        int id = Utils.Conversions.Atoi(arguments_ReadOnly[0]);
        string title = arguments_ReadOnly[1];

        if (application.taskManager.DoesTaskExist(id))
        {
            application.taskManager.GetTask_Editable(id).title = title;
            ConsoleWriter.Print("Task with id : {0} renamed to - {1}", id, title);
        }
        else
            ConsoleWriter.Print("Task not found with id : " + id);

        return true;
    }
}

public class TaskActiveCommand : CommandHandlerBase
{
    public TaskActiveCommand()
    {
    }

    protected override bool ShowHelp()
    {
        SharedLogic.PrintHelp("USAGE");
        SharedLogic.PrintHelp("Jarvis task active", "Shows if any time tracking is active");
        return true;
    }

    protected override bool Run(Jarvis.JApplication application)
    {
        if (arguments_ReadOnly.Count > 1)
        {
            ConsoleWriter.Print("Invalid arguments! \n");
            ShowHelp();
            return true;
        }

        if (!application.UserData.IsTaskInProgress())
        {
            ConsoleWriter.Print("There is no task in progress.");
            return true;
        }

        int id = application.UserData.taskProgress.taskIDInProgress;
        int timeTakenInMinutes = (int)(DateTime.Now - application.UserData.taskProgress.startTime).TotalMinutes;

        ConsoleWriter.Print("Task in progress, with id : {0} -> {1} ", id, application.taskManager.GetTask_ReadOnly(id).title);
        ConsoleWriter.Print("Total time recorded : {0}", Utils.Time.MinutesToHoursString(timeTakenInMinutes));
        return true;
    }
}

public class TaskListCommand : CommandHandlerBase
{
    public TaskListCommand()
    {

    }
    protected override bool ShowHelp()
    {
        SharedLogic.PrintHelp("USAGE");
        SharedLogic.PrintHelp("Jarvis task list", "lists all the tasks which are open");
        SharedLogic.PrintHelp("Jarvis task list --all", "Shows all the tasks including archieved, discarded and completed");
        SharedLogic.PrintHelp("Jarvis task list --archieve", "Shows all the tasks archieved");
        SharedLogic.PrintHelp("Jarvis task list --open", "Shows only the open tasks ( this is also the default setting )");
        SharedLogic.PrintHelp("Jarvis task list --complete", "Shows only the tasks completed");
        SharedLogic.PrintHelp("Jarvis task list --discard", "Shows only the tasks discard");
        SharedLogic.PrintHelp("Jarvis task list --story", "Shows only stories");
        SharedLogic.PrintHelp("Jarvis task list --collection", "Shows only collections");
        SharedLogic.PrintHelp("Jarvis task list --task", "Shows only tasks");
        SharedLogic.PrintHelp("Jarvis task list --cat:<category>", "Shows only those category");
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

        bool isArchieved = optionalArguments_ReadOnly.Contains("--archieve");
        bool isCompleted = optionalArguments_ReadOnly.Contains("--complete");
        bool isDiscarded = optionalArguments_ReadOnly.Contains("--discard");
        bool isOpen = optionalArguments_ReadOnly.Contains("--open");

        bool isStory = optionalArguments_ReadOnly.Contains("--story");
        bool isCollection = optionalArguments_ReadOnly.Contains("--collection");
        bool isTask = optionalArguments_ReadOnly.Contains("--task");

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

        // By default all are shown
        if (!isStory && !isTask && !isCollection)
            isStory = isTask = isCollection = true;

        // by default only open ones are shown, unless specified in which case open ones are not shown. 
        bool showOpenByDefault = true;

        if (isArchieved || isCompleted || isDiscarded)
            showOpenByDefault = false;
        if (isOpen)
            showOpenByDefault = true;

        if (optionalArguments_ReadOnly.Contains("--all") || optionalArguments_ReadOnly.Contains("-a"))
        {
            isArchieved = isCompleted = isDiscarded = isOpen = true;
        }

        Dictionary<string, List<Task>> filteredTasks = new Dictionary<string, List<Task>>();

        foreach (var task in application.taskManager.Data.entries)
        {
            if (task.IsOpen && !showOpenByDefault)
                continue;

            if (task.IsComplete && !isCompleted)
                continue;

            if (task.IsDiscarded && !isDiscarded)
                continue;

            if (task.IsArchieved && !isArchieved)
                continue;

            if (task.IsTask && !isTask)
                continue;

            if (task.IsStory && !isStory)
                continue;

            if (task.IsCollection && !isCollection)
                continue;

            if (categoryFilter != string.Empty && !task.categories.Contains(categoryFilter))
                continue;

            foreach (var category in task.categories)
            {
                if (!filteredTasks.ContainsKey(category))
                    filteredTasks[category] = new List<Task>();

                filteredTasks[category].Add(task);
            }
        }


        // output Heading 
        if (filteredTasks.Count() > 0)
        {
            ConsoleWriter.Print("{0, -4} {1,-" + categoryArea + "} {2,-" + titleArea + "} {3, -15} {4, -15}",
                "ID", "DEPT", "TITLE", "STATUS", "TIME SPENT"
                );

            foreach (var categoryFilteredTasks in filteredTasks)
            {
                ConsoleWriter.Print("\n--- {0} --- ", categoryFilteredTasks.Key.ToUpper());
                lineCount = 0;

                foreach (var task in categoryFilteredTasks.Value)
                {
                    bool isInProgress = application.UserData.IsTaskInProgress() && application.UserData.taskProgress.taskIDInProgress == task.id;
                    int timeInProgress = isInProgress ? (int)(DateTime.Now - application.UserData.taskProgress.startTime).TotalMinutes : 0;

                    ConsoleColor textColor = application.DesignData.DefaultColorForText;
                    if (task.IsStory)
                        textColor = application.DesignData.HighlightColorForText_2;
                    else if (task.IsCollection)
                        textColor = application.DesignData.HighlightColorForText_3;
                    else if (task.IsTask)
                        textColor = application.DesignData.DefaultColorForText;
                    else
                        Utils.Assert(false);

                    ConsoleWriter.PrintInColor("{0, -4} {1,-" + categoryArea + "} {2,-" + titleArea + "} {3, -15} {4, -15}",
                        textColor,
                        task.id,
                        (task.categories != null && task.categories.Length > 0 ? Utils.Conversions.ArrayToString(task.categories, true).TruncateWithVisualFeedback(categoryArea - 3) : "INVALID"),
                        task.title.TruncateWithVisualFeedback(titleArea - 7/*for the ...*/)
                            + (Utils.FileHandler.DoesFileExist(JConstants.PATH_TO_TASKS_NOTE + task.id) ? "+(N)" : ""),
                        (isInProgress ? "In Progress" : task.StatusString),
                        (isInProgress ? Utils.Time.MinutesToHoursString(timeInProgress) + " + " : "") + ("( " + Utils.Time.MinutesToHoursString(application.logManager.GetTotalTimeSpentToday(task.id)) + " , " + Utils.Time.MinutesToHoursString(application.logManager.GetTotalTimeSpentInMins(task.id)) + " )")
                        ); ;

                    lineCount++;

                    //@todo
                    if (lineCount % 5 == 0)
                        ConsoleWriter.Print();
                }
            }
        }
        else
            ConsoleWriter.Print("No tasks found! Try adding a few using \"jarvis add\"");

        return true;
    }
}


public class TaskShowCommand : CommandHandlerBase
{
    public TaskShowCommand()
    {

    }

    protected override bool ShowHelp()
    {
        SharedLogic.PrintHelp("USAGE");
        SharedLogic.PrintHelp("Jarvis task show <taskID>", "Show more details of a task");
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

        if (application.taskManager.DoesTaskExist(id))
        {
            bool isInProgress = application.UserData.IsTaskInProgress() && application.UserData.taskProgress.taskIDInProgress == id;
            int timeInProgress = isInProgress ? (int)(DateTime.Now - application.UserData.taskProgress.startTime).TotalMinutes : 0;
            Task task = application.taskManager.GetTask_ReadOnly(id);

            // Print Task details
            {
                // Heading
                ConsoleWriter.PrintInColor("{0, -4} {1,-15} {2}",
                    application.DesignData.HighlightColorForText,
                    "ID", "DEPT", "TITLE"
                    );

                ConsoleWriter.Print("{0, -4} {1,-15} {2}",
                    task.id,
                    (task.categories != null && task.categories.Length > 0 ? Utils.Conversions.ArrayToString(task.categories, true) : "INVALID"),
                    task.title);

                ConsoleWriter.Print();

                ConsoleWriter.Print("STATUS : {0, -15}\nTYPE : {1}\nTIME SPENT : {2,-15}\nNOTES AVAILABLE: {3,-15}",
                    (isInProgress ? "In Progress" : task.StatusString),
                    task.TypeString,
                    (isInProgress ? Utils.Time.MinutesToHoursString(timeInProgress) + " + " : "") +
                    ("(" + Utils.Time.MinutesToHoursString(application.logManager.GetTotalTimeSpentToday(task.id)) +
                    " , " + Utils.Time.MinutesToHoursString(application.logManager.GetTotalTimeSpentInMins(task.id)) + ")"),
                    (Utils.FileHandler.DoesFileExist(JConstants.PATH_TO_TASKS_NOTE + task.id) ? "YES" : "NO")
                    );

            }

            ConsoleWriter.Print();

            {
                SortedDictionary<DateTime, int> timeLogs = new SortedDictionary<DateTime, int>();

                foreach (var logEntry in application.logManager.logs.entries)
                {
                    if (logEntry.id == id)
                    {
                        if (!timeLogs.ContainsKey(logEntry.date.ZeroTime()))
                            timeLogs[logEntry.date.ZeroTime()] = 0;
                        timeLogs[logEntry.date.ZeroTime()] += logEntry.timeTakenInMinutes;
                    }
                }

                if (timeLogs.Count > 0)
                {
                    ConsoleWriter.PrintInColor("{0, -15} {1,-30}",
                        application.DesignData.HighlightColorForText,
                        "DATE", "HOURS SPEND"
                        );

                    foreach (var log in timeLogs)
                    {
                        ConsoleWriter.PrintInColor("{0, -15} {1,-30}",
                        application.DesignData.DefaultColorForText,
                        log.Key.ShortFormWithDay(), Utils.Time.MinutesToHoursString(log.Value));
                    }
                }
            }
        }
        else
            ConsoleWriter.Print("Task not found with id : " + id);

        return true;
    }
}

public class TaskSetStatusCommand : CommandHandlerBase
{
    public TaskSetStatusCommand(Task.Status status)
    {
        this.status = status;
    }

    protected override bool ShowHelp()
    {
        SharedLogic.PrintHelp("USAGE");
        SharedLogic.PrintHelp("Jarvis task complete <taskID> <time>", "time is in hours. the amount of work which was put into it. example : jarvis task complete 1 1 | this is when you completed task with ID 1 in 1 hour");
        SharedLogic.PrintHelp("Jarvis task archieve <taskID> ", "Archieve a task");
        SharedLogic.PrintHelp("Jarvis task discard <taskID> ", "Discard a task");

        return true;
    }

    protected override bool Run(Jarvis.JApplication application)
    {
        if (status == Task.Status.Archieve || status == Task.Status.Open || status == Task.Status.Discard)
        {
            if (arguments_ReadOnly.Count != 1)
            {
                ConsoleWriter.Print("Invalid arguments! \n");
                ShowHelp();
                return true;
            }
        }
        if (status == Task.Status.Complete)
        {
            if (arguments_ReadOnly.Count != 2)
            {
                ConsoleWriter.Print("Invalid arguments! Please provide ID and hours \n");
                ShowHelp();
                return true;
            }
        }

        int id = Utils.Conversions.Atoi(arguments_ReadOnly[0]);
        float timeInHours = 0;
        if (status == Task.Status.Complete)
            timeInHours = Utils.Conversions.Atof(arguments_ReadOnly[1]);

        if (application.taskManager.DoesTaskExist(id))
        {
            var task = application.taskManager.GetTask_Editable(id);
            task.SetStatus(this.status);
            ConsoleWriter.Print("Task with id : {0} marked as {1}", id, task.StatusString);

            if (status == Task.Status.Complete)
            {
                //@ todo - Clean this code up. move it somewhere else. 
                // Add record to log manager
                {
                    LogEntry le = new LogEntry();
                    le.id = id;
                    le.date = DateTime.Now;
                    le.comment = "";
                    le.timeTakenInMinutes = (int)(timeInHours * 60);

                    application.logManager.AddEntry(le);

                    ConsoleWriter.Print("Total time spent on this task {0} hours ", Utils.Time.MinutesToHoursString( application.logManager.GetTotalTimeSpentInMins(id) ));
                }

            }

        }
        else
            ConsoleWriter.Print("Task not found with id : " + id);

        return true;
    }

    private Task.Status status;
}


public class TaskRecordTimeLogCommand : CommandHandlerBase
{
    public TaskRecordTimeLogCommand()
    {
    }

    protected override bool ShowHelp()
    {
        SharedLogic.PrintHelp("USAGE");
        SharedLogic.PrintHelp("Jarvis task recordtimelog <taskID> <time in hours> <comments>", "Force record a time log ( instead of start and stop )");
        SharedLogic.PrintHelp("Jarvis task recordtimelog <taskID> <time in hours> <comments> <--when:-1>  ", "How many days before ?. -1 this timelog is of yesterday. -2 for a day before that. by default, this is 0, as in the time log is created for today.");

        return true;
    }

    protected override bool Run(Jarvis.JApplication application)
    {
        if (arguments_ReadOnly.Count < 2)
        {
            ConsoleWriter.Print("Invalid arguments! \n");
            ShowHelp();
            return true;
        }

        int id = Utils.Conversions.Atoi(arguments_ReadOnly[0]);
        int timeTakenInHours = Utils.Conversions.Atoi(arguments_ReadOnly[1]);
        string comments = arguments_ReadOnly.Count() > 2 ? arguments_ReadOnly[2] : string.Empty;

        bool syntaxErrorForWhenArgument = false;
        int deltaTime = Utils.CLI.ExtractIntFromCLIParameter(optionalArguments_ReadOnly, "--when", 0, null, null, out syntaxErrorForWhenArgument);

        if (syntaxErrorForWhenArgument)
        {
            ConsoleWriter.Print("syntax invalid for --when argument. please try again");
            return true;
        }

        if (application.taskManager.DoesTaskExist(id))
        {
            // Add record to log manager
            {
                LogEntry le = new LogEntry();
                le.id = id;
                le.date = DateTime.Now.AddDays(deltaTime);
                le.comment = comments;
                le.timeTakenInMinutes = timeTakenInHours * 60;

                application.logManager.AddEntry(le);

                ConsoleWriter.Print("New timelog added for task : {0} on date : {1}", id, le.date);
            }
        }
        else
            ConsoleWriter.Print("Task not found with id : " + id);

        return true;
    }
}

public class TaskReportCommand : CommandHandlerBase
{
    public TaskReportCommand()
    {
    }


    private IDictionary<string, int> GetReportFor(TaskManager taskManager, List<LogEntry> logs, int pastDays, out int totalMinutes)
    {
        SortedDictionary<string, int> report = new SortedDictionary<string, int>();
        totalMinutes = 0;

        foreach (var log in logs)
        {
            Task task = taskManager.GetTask_ReadOnly(log.id);

            if (task == null)
                continue;

            if (log.date.ZeroTime() >= (DateTime.Now.AddDays(-1 * pastDays).ZeroTime()))
            {
                foreach (var taskCategory in task.categories)
                {
                    if (!report.ContainsKey(taskCategory))
                        report[taskCategory] = 0;
                    report[taskCategory] += log.timeTakenInMinutes;
                    totalMinutes += log.timeTakenInMinutes;
                }
            }
        }
        return report;
    }

    protected override bool ShowHelp()
    {
        SharedLogic.PrintHelp("USAGE");
        SharedLogic.PrintHelp("Jarvis task report", "Shows the report with all the work done in the last week");
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

        if (application.taskManager.Data.entries.Count() > 0)
        {
            int totalMinutes = 0;
            IDictionary<string, int> categoryTimeMap = GetReportFor(application.taskManager, application.logManager.logs.entries, 0, out totalMinutes);

            ConsoleWriter.PrintInColor("{0,-20} {1,-7} hours",
                application.DesignData.HighlightColorForText,
                "FOR TODAY",
                Utils.Time.MinutesToHoursString(totalMinutes));

            if (categoryTimeMap.Count > 0)
            {
                foreach (var timeMap in categoryTimeMap)
                {
                    ConsoleWriter.Print("{0,-20} {1,-7} hours", timeMap.Key, Utils.Time.MinutesToHoursString(timeMap.Value));
                }
            }
            else
            {
                ConsoleWriter.Print("Found no records");
            }

            ConsoleWriter.Print();

            categoryTimeMap = GetReportFor(application.taskManager, application.logManager.logs.entries, 6, out totalMinutes);

            ConsoleWriter.PrintInColor("{0,-20} {1,-7} hours {2,-7} {3, -7} hours(avg)",
                application.DesignData.HighlightColorForText,
                "FOR LAST 7 DAYS",
                Utils.Time.MinutesToHoursString(totalMinutes), " ", Utils.Time.HoursToHoursString(Utils.Time.MinutesToHours(totalMinutes) / 7));

            if (categoryTimeMap.Count > 0)
            {
                foreach (var timeMap in categoryTimeMap)
                {
                    ConsoleWriter.Print("{0,-20} {1,-7} hours {2,-7} {3,-7} hours(avg)", timeMap.Key, Utils.Time.MinutesToHoursString(timeMap.Value), " ", Utils.Time.HoursToHoursString(Utils.Time.MinutesToHours(timeMap.Value / 7)));
                }
            }
            else
            {
                ConsoleWriter.Print("Found no records");
            }
        }
        else
            ConsoleWriter.Print("Tasks not found");

        return true;
    }
}

public class TaskCatNotesCommand : CommandHandlerBase
{
    public TaskCatNotesCommand()
    {

    }

    protected override bool ShowHelp()
    {
        SharedLogic.PrintHelp("USAGE");
        SharedLogic.PrintHelp("Jarvis task cat <taskID>", "Prints the notes of a task. You can also use printnote instead of cat");
        SharedLogic.PrintHelp("Jarvis task printnote <taskID>", "Same as cat");
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


        if (application.taskManager.DoesTaskExist(id))
        {
            if (!Utils.FileHandler.DoesFileExist(JConstants.PATH_TO_TASKS_NOTE + id))
                ConsoleWriter.Print("Notes not found for the task with id : {0}", id);
            else
            {
                ConsoleWriter.PrintInColor("NOTES :", application.DesignData.HighlightColorForText);
                ConsoleWriter.PrintText(Utils.FileHandler.Read(JConstants.PATH_TO_TASKS_NOTE + id));
            }
        }
        else
            ConsoleWriter.Print("Task not found with id : " + id);

        return true;
    }
}

public class TaskEditNoteCommand : CommandHandlerBase
{
    public TaskEditNoteCommand()
    {

    }

    protected override bool ShowHelp()
    {
        SharedLogic.PrintHelp("USAGE");
        SharedLogic.PrintHelp("Jarvis task note <taskID>", "Opens notes for a task.");
        SharedLogic.PrintHelp("Jarvis task note <taskID> --ext:<editorname>", "provide external editor name of your choice, to open the notes in. Example : code or vim");
        SharedLogic.PrintHelp("Jarvis task note <taskID> --append:<Message>", "Append a message directly to a note");
        SharedLogic.PrintHelp("Jarvis task note <taskID> --appendlog:<Message>", "Appends a message directly to a note, with a timestamp");
        SharedLogic.PrintHelp("\nADVANCED");
        SharedLogic.PrintHelp("You can change the default editor in the DesignData.json under 'defaultExternalEditor'");
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
        string externalProgram = Utils.CLI.ExtractStringFromCLIParameter(optionalArguments_ReadOnly, "--ext", string.Empty, null, null, out syntaxError);

        if (syntaxError)
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

        if (!appendLogMessage.IsEmpty())
        {
            appendMessage = "Log on " + DateTime.Now.ToShortDateString() + " " + appendLogMessage;
        }

        if (application.taskManager.DoesTaskExist(id))
        {
            if (!Utils.FileHandler.DoesFileExist(JConstants.PATH_TO_TASKS_NOTE + id))
            {
                Utils.FileHandler.Create(JConstants.PATH_TO_TASKS_NOTE + id);
                ConsoleWriter.Print("new note created");
            }

            if (!appendMessage.IsEmpty())
            {
                ConsoleWriter.Print("Message appended to the notes");
                Utils.AppendToFile(JConstants.PATH_TO_TASKS_NOTE + id, appendMessage);
            }
            else
            {
                ConsoleWriter.Print("Opening Notes");
                Utils.OpenAFileInEditor(
                    JConstants.PATH_TO_TASKS_NOTE + id,
                    externalProgram.IsEmpty() ? application.DesignData.defaultExternalEditor : externalProgram,
                    true /* wait for the program to end*/);
                ConsoleWriter.Print("Closing Notes");
            }
        }
        else
            ConsoleWriter.Print("Task not found with id : " + id);
        return true;
    }
}

public class TaskDeleteNoteCommand : CommandHandlerBase
{
    public TaskDeleteNoteCommand()
    {

    }

    protected override bool ShowHelp()
    {
        SharedLogic.PrintHelp("USAGE");
        SharedLogic.PrintHelp("Jarvis task deletenote <taskID>", "deletes the notes");
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

        if (application.taskManager.DoesTaskExist(id))
        {
            if (Utils.FileHandler.DoesFileExist(JConstants.PATH_TO_TASKS_NOTE + id))
            {
                Utils.FileHandler.DoesFileExist(JConstants.PATH_TO_TASKS_NOTE + id);
                ConsoleWriter.Print("Notes deleted");
            }
            else
                ConsoleWriter.Print("Notes doesnt exit for task with id : " + id);
        }
        else
            ConsoleWriter.Print("Task not found with id : " + id);

        return true;
    }
}