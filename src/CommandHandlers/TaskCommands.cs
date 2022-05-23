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
        ConsoleWriter.Print("USAGE : \n" +
                "Jarvis task add  // To add a task\n" +
                "Jarvis task list // to list all the tasks\n" +
                "Jarvis task list --all // to list all the tasks\n" +
                "\n" +
                "Jarvis task delete // to remove a task\n" +
                "Jarvis task complete // to complete a task\n" +
                "Jarvis task discard // to discard a task\n" +
                "Jarvis task archieve // to archieve a task\n" +
                "Jarvis task open // to re-open a task\n" +
                "\n" +
                "Jarvis task start // to track the time of a task\n" +
                "Jarvis task stop // to stop time tracking\n" +
                "Jarvis task active // to show if any time record is in progress\n" +
                "\n" +
                "Jarvis task show // to show a task\n" +
                "\n" +
                "ADVANCED\n" +
                "jarvis task addsubtask // to add subtasks to a task\n" +
                "jarvis task removesubtask // to add subtasks to a task\n" +
                "jarvis task recordtimelog // to record an offline task\n" +
                "jarvis task report // to show all the work done in the last day/week\n");
        return true;
    }

    protected override CommandHandlerBase GetSpecializedCommandHandler(Jarvis.JApplication application, out List<string> argumentsForSpecializedHandler)
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
            case "addsubtask":
                selectedHander = new TaskAddSubTaskCommand();
                break;
            case "removesubtask":
                selectedHander = new TaskRemoveSubTaskCommand();
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
            return true;
        }
        
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
        ConsoleWriter.Print("USAGE : \n" +
                "jarvis task add <category> <title>\n" +
                "Category can be office,learn,chores,health. you can add more in the design data as per your need.\n\n" +
                "use --story or -s to create a story\n" +
                "use --collection or -c to create a collection\n" + 
                "\n" + 
                "use --archieve to sent it straight to archieve category\n"
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
            isStory ? Task.Type.Story : ( isCollection ? Task.Type.Collection : Task.Type.Task),
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
        ConsoleWriter.Print("USAGE : \n" +
                "jarvis task remove <taskID>  // task id is the ID of the task you are trying to remove\n"
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

        string[] ids = arguments_ReadOnly[0].Split(',');

        foreach (var idStr in ids)
        {
            int id = Utils.Atoi(idStr, -1);
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
        ConsoleWriter.Print("USAGE : \n" +
                "jarvis task start <taskID> // task id is the ID of the task you are trying to start time tracking\n"
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
        ConsoleWriter.Print("USAGE : \n" +
            "jarvis task stop\n" +
            "jarvis task stop <comments> "
                );
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
        application.UserData.StopTask();

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
        ConsoleWriter.Print("Total time recorded : {0}", Utils.MinutesToHoursString( timeTakenInMinutes ) );
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
        ConsoleWriter.Print("USAGE : \n" +
            "jarvis task active"
                );
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
        ConsoleWriter.Print("Total time recorded : {0}", Utils.MinutesToHoursString( timeTakenInMinutes ) );
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
        ConsoleWriter.Print("USAGE : \n" +
                "jarvis task list   // lists all the tasks which are open\n"
                 +
                "jarvis task list --all // Shows all the tasks including archieved, discarded and completed\n" +
                "jarvis task list --archieve // Shows all the tasks archieve\n" +
                "jarvis task list --open // Shows all the open tasks ( this is also the default setting )\n" +
                "jarvis task list --complete // Shows all the tasks complete\n" +
                "jarvis task list --discard // Shows all the tasks discard\n" +
                "jarvis task list --story // Shows only stories\n" +
                "jarvis task list --collection // Shows only stories\n" +
                "jarvis task list --task // Shows only tasks\n" +
                "jarvis task list --cat:<category> // Shows only those category\n"
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

        bool isArchieved = optionalArguments_ReadOnly.Contains("--archieve");
        bool isCompleted = optionalArguments_ReadOnly.Contains("--complete");
        bool isDiscarded = optionalArguments_ReadOnly.Contains("--discard");
        bool isOpen = optionalArguments_ReadOnly.Contains("--open");

        bool isStory = optionalArguments_ReadOnly.Contains("--story");
        bool isCollection = optionalArguments_ReadOnly.Contains("--collection");
        bool isTask = optionalArguments_ReadOnly.Contains("--task");

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
                        (task.categories != null && task.categories.Length > 0 ? Utils.ArrayToString(task.categories, true).TruncateWithVisualFeedback(categoryArea - 3) : "INVALID"),
                        task.title.TruncateWithVisualFeedback(titleArea - 6/*for the ...*/) + (task.GetSubTaskCount() > 0 ? "+(" + task.GetSubTaskCount() + ")" : ""),
                        (isInProgress ? "In Progress" : task.StatusString),
                        (isInProgress ? Utils.MinutesToHoursString(timeInProgress) + " + " : "") + ("( " + Utils.MinutesToHoursString(application.logManager.GetTotalTimeSpentToday(task.id)) + " , " + Utils.MinutesToHoursString(application.logManager.GetTotalTimeSpent(task.id)) + " )")
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
        ConsoleWriter.Print("USAGE : \n" +
                "jarvis task show <taskID> // task id is the ID of the task you are trying to see\n"
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
                    (task.categories != null && task.categories.Length > 0 ? Utils.ArrayToString(task.categories, true) : "INVALID"),
                    task.title);

                ConsoleWriter.Print();

                ConsoleWriter.Print("STATUS : {0, -15}\nTYPE : {1}\nTIME SPENT : {2,-15}",
                    (isInProgress ? "In Progress" : task.StatusString),
                    task.TypeString,
                    (isInProgress ? Utils.MinutesToHoursString(timeInProgress) + " + " : "") +
                    ("(" + Utils.MinutesToHoursString(application.logManager.GetTotalTimeSpentToday(task.id)) +
                    " , " + Utils.MinutesToHoursString(application.logManager.GetTotalTimeSpent(task.id)) + ")")
                    );

            }
            
            ConsoleWriter.Print();

            // Print sub tasks 
            {
                if (task.subTasks != null)
                {
                    if ( task.subTasks.Count > 0 )
                    {
                        ConsoleWriter.PrintInColor("{0, -15} {1,-30}",
                        application.DesignData.HighlightColorForText,
                        "SUBTASK ID", "SUBTASK TITLE"
                        );

                        foreach (var subTaskPair in task.subTasks)
                        {
                            ConsoleWriter.Print("{0, -15} {1, -30}", subTaskPair.First, subTaskPair.Second);
                        }
                    }
                    ConsoleWriter.Print();
                }
            }


            {
                SortedDictionary<DateTime, int> timeLogs = new SortedDictionary<DateTime, int>();

                foreach( var logEntry in application.logManager.logs.entries)
                {
                    if ( logEntry.id == id )
                    {
                        if( !timeLogs.ContainsKey(logEntry.date.ZeroTime()))
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
                        log.Key.ShortFormWithDay(), Utils.MinutesToHoursString(log.Value));
                    }
                }
            }
        }
        else
            ConsoleWriter.Print("Task not found with id : " + id);

        return true;
    }
}

public class TaskAddSubTaskCommand : CommandHandlerBase
{
    public TaskAddSubTaskCommand()
    {
    }

    protected override bool ShowHelp()
    {
        ConsoleWriter.Print("USAGE : \n" +
                "jarvis task addsubtask <taskID> <subtasktitle>  // task id is the ID of the task under which the subtask would be created\n"
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

        int id = Utils.Atoi(arguments_ReadOnly[0]);
        string title = arguments_ReadOnly[1];


        if (application.taskManager.DoesTaskExist(id))
        {
            application.taskManager.GetTask_Editable(id).AddSubTask(title);
            ConsoleWriter.Print("Subtask added to Task with id : " + id);
        }
        else
            ConsoleWriter.Print("Task not found with id : " + id);

        return true;
    }
}

public class TaskRemoveSubTaskCommand : CommandHandlerBase
{
    public TaskRemoveSubTaskCommand()
    {
    }

    protected override bool ShowHelp()
    {
        ConsoleWriter.Print("USAGE : \n" +
                "jarvis task removesubtask <taskID> <subtaskID>  // task id is the ID of the task. SubtaskId is the one which is assigned to each of the subtasks\n"
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

        int id = Utils.Atoi(arguments_ReadOnly[0]);
        int subTaskID = Utils.Atoi( arguments_ReadOnly[1]); 


        if (application.taskManager.DoesTaskExist(id))
        {
            var subTaskPair = application.taskManager.GetTask_ReadOnly(id).GetSubtask(subTaskID);
            if (subTaskPair != null)
            {
                application.taskManager.GetTask_Editable(id).RemoveSubTask(subTaskID);
                ConsoleWriter.Print("Subtask removed from Task with id : " + id);
            }
            else
                ConsoleWriter.Print("SubTask not found! Make sure you verify the subtask ID using 'Jarvis task show'");
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
        ConsoleWriter.Print("USAGE : \n" +
                "jarvis task complete <taskID>  // task id is the ID of the task under which the subtask would be created\n" +
                "jarvis task archieve <taskID>  // task id is the ID of the task under which the subtask would be created\n" +
                "jarvis task discard <taskID>  // task id is the ID of the task under which the subtask would be created\n"
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

        if (application.taskManager.DoesTaskExist(id))
        {
            application.taskManager.GetTask_Editable(id).SetStatus(this.status);
            ConsoleWriter.Print("Task with id : {0} marked as {1}", id, application.taskManager.GetTask_Editable(id).StatusString);
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
        ConsoleWriter.Print("USAGE : \n" +
                "jarvis task recordtimelog <taskID> <time in mins> <comments>\n" +
                "jarvis task recordtimelog <taskID> <time in mins> <comments> <---when:-1>   // delta day count. -1 as in this timelog is created for yesterday. -2 as in day before yesterday. by default, this is 0, as in the time log is created for today."
                );
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

        int id = Utils.Atoi(arguments_ReadOnly[0]);
        int timeTakenInMinutes = Utils.Atoi(arguments_ReadOnly[1]);
        string comments = arguments_ReadOnly.Count() > 2 ? arguments_ReadOnly[2] : string.Empty;

        bool syntaxErrorForWhenArgument = false;
        int deltaTime = Utils.ExtractIntFromArgument(optionalArguments_ReadOnly, "--when", 0, null, null, out syntaxErrorForWhenArgument);

        if( syntaxErrorForWhenArgument)
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
                le.timeTakenInMinutes = timeTakenInMinutes;

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
        ConsoleWriter.Print("USAGE : \n" +
                "jarvis task report"
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

        if (application.taskManager.Data.entries.Count() > 0)
        {
            int totalMinutes = 0;
            IDictionary<string, int> categoryTimeMap = GetReportFor(application.taskManager, application.logManager.logs.entries, 0, out totalMinutes);

            ConsoleWriter.PrintInColor("{0,-20} {1,-7} hours", 
                application.DesignData.HighlightColorForText, 
                "FOR TODAY", 
                Utils.MinutesToHoursString(totalMinutes));

            if (categoryTimeMap.Count > 0)
            {
                foreach (var timeMap in categoryTimeMap)
                {
                    ConsoleWriter.Print("{0,-20} {1,-7} hours", timeMap.Key, Utils.MinutesToHoursString(timeMap.Value));
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
                Utils.MinutesToHoursString(totalMinutes), " ", Utils.HoursToHoursString(Utils.MinutesToHours(totalMinutes) / 7));

            if (categoryTimeMap.Count > 0)
            {
                foreach (var timeMap in categoryTimeMap)
                {
                    ConsoleWriter.Print("{0,-20} {1,-7} hours {2,-7} {3,-7} hours(avg)", timeMap.Key, Utils.MinutesToHoursString(timeMap.Value), " ", Utils.HoursToHoursString(Utils.MinutesToHours(timeMap.Value / 7)));
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