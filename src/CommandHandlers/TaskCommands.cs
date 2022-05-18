using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommandLine;
using Jarvis; //@todo 


public class TaskHandler : ICommand
{
    public TaskHandler()
    {

    }

    public override bool Run(List<string> arguments, List<string> optionalArguments, Jarvis.JApplication application)
    {

        if (arguments.Count < 1)
        {
            ConsoleWriter.Print("Invalid arguments! \n");
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
                "\n" +
                "Jarvis task show // to show a task\n" +
                "\n" +
                "ADVANCED\n" +
                "jarvis task addsubtask // to add subtasks to a task\n" +
                "jarvis task removesubtask // to add subtasks to a task\n" +
                "jarvis task recordtimelog // to record an offline task\n" +
                "jarvis task report // to show all the work done in the last day/week\n");

            return false;
        }

        string action = arguments[0];
        ICommand selectedHander = null;

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
                ConsoleWriter.Print("unknown action");
                break;
        }

        if (selectedHander != null)
        {
            arguments.RemoveAt(0);
            selectedHander.Run(arguments, optionalArguments, application);
        }
        return true;
    }
}


public class TaskAddCommand : ICommand
{
    public TaskAddCommand()
    {

    }

    public override bool Run(List<string> arguments, List<string> optionalArguments, Jarvis.JApplication application)
    {
        if (arguments.Count != 2)
        {
            ConsoleWriter.Print("Invalid arguments! \n");
            ConsoleWriter.Print("USAGE : \n" +
                "jarvis task add <category> <title>\n" +
                "Category can be office,learn,chores,health. you can add more in the design data as per your need.\n\n" +
                "use --story or -s to create a story"
                );
            return true;
        }

        string[] categories = arguments[0].Split(',');
        string title = arguments[1];
        bool isStory = optionalArguments.Contains("--story") || optionalArguments.Contains("-s");

        if (!application.DesignData.DoesCategoryExist(categories))
        {
            ConsoleWriter.Print("Invalid categories.\n" +
                "Category can be office,learn,chores,health. you can add more in the design data as per your need.");
            return true;
        }

        var entry = SharedLogic.CreateNewTask(application.taskManager, categories, title, isStory ? Task.Type.Story : Task.Type.Task);
        application.taskManager.AddTask(entry);

        ConsoleWriter.Print("New task added with id : " + entry.id);
        return true;
    }
}


public class TaskRemoveCommand : ICommand
{
    public TaskRemoveCommand()
    {

    }

    public override bool Run(List<string> arguments, List<string> optionalArguments, Jarvis.JApplication application)
    {
        if (arguments.Count != 1)
        {
            ConsoleWriter.Print("Invalid arguments! \n");
            ConsoleWriter.Print("USAGE : \n" +
                "jarvis task remove <taskID>  // task id is the ID of the task you are trying to remove\n"
                );
            return true;
        }

        string[] ids = arguments[0].Split(',');

        foreach (var idStr in ids)
        {
            int id = Utils.Atoi(idStr, -1);
            if (application.taskManager.RemoveTaskIfExists(id))
                ConsoleWriter.Print("Task removed with id : " + id);
            else
                ConsoleWriter.Print("Task not found with id : " + id);
        }

        return true;
    }
}

public class TaskStartCommand : ICommand
{
    public TaskStartCommand()
    {

    }

    public override bool Run(List<string> arguments, List<string> optionalArguments, Jarvis.JApplication application)
    {
        if (arguments.Count != 1)
        {
            ConsoleWriter.Print("Invalid arguments! \n");
            ConsoleWriter.Print("USAGE : \n" +
                "jarvis task start <taskID> // task id is the ID of the task you are trying to start time tracking\n"
                );
            return true;
        }

        int id = Utils.Atoi(arguments[0]);

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

public class TaskStopCommand : ICommand
{
    public TaskStopCommand()
    {
    }

    public override bool Run(List<string> arguments, List<string> optionalArguments, Jarvis.JApplication application)
    {
        if (arguments.Count > 1)
        {
            ConsoleWriter.Print("Invalid arguments! \n");
            ConsoleWriter.Print("USAGE : \n" +
                "jarvis task stop <comments> "
                );
            return true;
        }

        if (!application.UserData.IsTaskInProgress())
        {
            ConsoleWriter.Print("There is no task in progress.");
            return true;
        }

        string comments = arguments != null && arguments.Count > 0 ? arguments[0] : string.Empty; 
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
        ConsoleWriter.Print("Total time record : {0}", Utils.MinutesToHoursString( timeTakenInMinutes ) );
        return true;
    }
}

public class TaskListCommand : ICommand
{
    public TaskListCommand()
    {

    }

    public override bool Run(List<string> arguments, List<string> optionalArguments, Jarvis.JApplication application)
    {
        if (arguments.Count != 0)
        {
            ConsoleWriter.Print("Invalid arguments! \n");
            ConsoleWriter.Print("USAGE : \n" +
                "jarvis task list   // lists all the tasks which are open\n"
                 +
                "jarvis task list --all // Shows all the tasks including archieved, discarded and completed\n" +
                "jarvis task list --archieve // Shows all the tasks archieved\n" +
                "jarvis task list --completed // Shows all the tasks completed\n" +
                "jarvis task list --discarded // Shows all the tasks discarded\n" +
                "jarvis task list --story // Shows only stories\n" +
                "jarvis task list --task // Shows only tasks\n" +
                "jarvis task list --cat:<category> // Shows only those category\n"
                );
            return true;
        }

        int lineCount = 0;
        int titleArea = 40;
        int categoryArea = 15;

        bool archieved = optionalArguments.Contains("--archieved");
        bool completed = optionalArguments.Contains("--completed");
        bool discarded = optionalArguments.Contains("--discarded");

        bool story = optionalArguments.Contains("--story");
        bool task = optionalArguments.Contains("--task");

        bool syntaxErrorInCategoryFilter = false;
        string categoryFilter = Utils.ExtractStringFromArgument(optionalArguments, "--cat", string.Empty, null, null, out syntaxErrorInCategoryFilter );
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
        if (!story && !task)
            story = task = true;

        // by default only open ones are shown, unless specified in which case open ones are not shown. 
        bool open = true;

        if (archieved || completed || discarded)
            open = false;

        if (optionalArguments.Contains("--all") || optionalArguments.Contains("-a"))
        {
            archieved = completed = discarded = open = true;
        }

        // output Heading 
        if (application.taskManager.Data.entries.Count() > 0)
        {
            ConsoleWriter.Print("{0, -4} {1,-" + categoryArea + "} {2,-" + titleArea + "} {3, -15} {4, -15}",
                "ID", "DEPT", "TITLE", "STATUS", "TIME SPENT"
                );


            foreach (var entry in application.taskManager.Data.entries)
            {
                if (entry.IsOpen && !open)
                    continue;

                if (entry.IsComplete && !completed)
                    continue;

                if (entry.IsDiscarded && !discarded)
                    continue;

                if (entry.IsArchieved && !archieved)
                    continue;

                if (entry.IsTask && !task)
                    continue;

                if (entry.IsStory && !story)
                    continue;

                if (categoryFilter != string.Empty && !entry.categories.Contains(categoryFilter))
                    continue;

                bool isInProgress = application.UserData.IsTaskInProgress() && application.UserData.taskProgress.taskIDInProgress == entry.id;
                int timeInProgress = isInProgress ? (int)(DateTime.Now - application.UserData.taskProgress.startTime).TotalMinutes : 0;
                
                ConsoleWriter.PrintInColor("{0, -4} {1,-" + categoryArea + "} {2,-" + titleArea + "} {3, -15} {4, -15}",
                    entry.IsStory ? application.DesignData.HighlightColorForText_2: application.DesignData.DefaultColorForText,
                    entry.id,
                    (entry.categories != null && entry.categories.Length > 0 ? Utils.ArrayToString(entry.categories, true).TruncateWithVisualFeedback(categoryArea - 3) : "INVALID"),
                    entry.title.TruncateWithVisualFeedback(titleArea - 6/*for the ...*/) + (entry.GetSubTaskCount() > 0 ? "+(" + entry.GetSubTaskCount() + ")" : ""),
                    (isInProgress ? "In Progress" : entry.StatusString),
                    (isInProgress ? Utils.MinutesToHoursString(timeInProgress) + " + " : "") + ("( " + Utils.MinutesToHoursString(application.logManager.GetTotalTimeSpentToday(entry.id)) + " , " + Utils.MinutesToHoursString(application.logManager.GetTotalTimeSpent(entry.id)) + " )")
                    );

                lineCount++;

                //@todo
                if (lineCount % 5 == 0)
                    ConsoleWriter.Print();
            }
        }
        else
            ConsoleWriter.Print("No tasks found! Try adding a few using \"jarvis add\"");

        return true;
    }
}


public class TaskShowCommand : ICommand
{
    public TaskShowCommand()
    {

    }

    public override bool Run(List<string> arguments, List<string> optionalArguments, Jarvis.JApplication application)
    {
        if (arguments.Count != 1)
        {
            ConsoleWriter.Print("Invalid arguments! \n");
            ConsoleWriter.Print("USAGE : \n" +
                "jarvis task show <taskID> // task id is the ID of the task you are trying to see\n"
                );
            return true;
        }

        int id = Utils.Atoi(arguments[0]);

        if (application.taskManager.DoesTaskExist(id))
        {
            bool isInProgress = application.UserData.IsTaskInProgress() && application.UserData.taskProgress.taskIDInProgress == id;
            int timeInProgress = isInProgress ? (int)(DateTime.Now - application.UserData.taskProgress.startTime).TotalMinutes : 0;
            Task entry = application.taskManager.GetTask_ReadOnly(id);

            // Heading
            ConsoleWriter.PrintInColor("{0, -4} {1,-15} {2}",
                application.DesignData.HighlightColorForText,
                "ID", "DEPT", "TITLE"
                );

            ConsoleWriter.Print("{0, -4} {1,-15} {2}",
                entry.id,
                (entry.categories != null && entry.categories.Length > 0 ? Utils.ArrayToString(entry.categories, true) : "INVALID"),
                entry.title);

            ConsoleWriter.Print();

            ConsoleWriter.Print("STATUS : {0, -15}\nTIME SPENT : {1,-15}",
                (isInProgress ? "In Progress" : entry.StatusString),
                (isInProgress ? Utils.MinutesToHoursString(timeInProgress) + " + " : "") + 
                ("(" + Utils.MinutesToHoursString(application.logManager.GetTotalTimeSpentToday(entry.id)) + 
                " , " + Utils.MinutesToHoursString(application.logManager.GetTotalTimeSpent(entry.id)) + ")")
                );

            ConsoleWriter.Print();

            if (entry.subTasks != null)
            {
                ConsoleWriter.PrintInColor("{0, -15} {1,-30}",
                application.DesignData.HighlightColorForText,
                "SUBTASK ID", "SUBTASK TITLE"
                );

                foreach (var subTaskPair in entry.subTasks)
                {
                    ConsoleWriter.Print( "{0, -15} {1, -30}\n", subTaskPair.First, subTaskPair.Second);
                }
            }
        }
        else
            ConsoleWriter.Print("Task not found with id : " + id);

        return true;
    }
}

public class TaskAddSubTaskCommand : ICommand
{
    public TaskAddSubTaskCommand()
    {
    }

    public override bool Run(List<string> arguments, List<string> optionalArguments, Jarvis.JApplication application)
    {
        if (arguments.Count != 2)
        {
            ConsoleWriter.Print("Invalid arguments! \n");
            ConsoleWriter.Print("USAGE : \n" +
                "jarvis task addsubtask <taskID> <subtasktitle>  // task id is the ID of the task under which the subtask would be created\n"
                );
            return true;
        }

        int id = Utils.Atoi(arguments[0]);
        string title = arguments[1];


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

public class TaskRemoveSubTaskCommand : ICommand
{
    public TaskRemoveSubTaskCommand()
    {
    }

    public override bool Run(List<string> arguments, List<string> optionalArguments, Jarvis.JApplication application)
    {
        if (arguments.Count != 2)
        {
            ConsoleWriter.Print("Invalid arguments! \n");
            ConsoleWriter.Print("USAGE : \n" +
                "jarvis task removesubtask <taskID> <subtaskID>  // task id is the ID of the task. SubtaskId is the one which is assigned to each of the subtasks\n"
                );
            return true;
        }

        int id = Utils.Atoi(arguments[0]);
        int subTaskID = Utils.Atoi( arguments[1]); 


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

public class TaskSetStatusCommand : ICommand
{
    public TaskSetStatusCommand(Task.Status status)
    {
        this.status = status;
    }

    public override bool Run(List<string> arguments, List<string> optionalArguments, Jarvis.JApplication application)
    {
        if (arguments.Count != 1)
        {
            ConsoleWriter.Print("Invalid arguments! \n");
            ConsoleWriter.Print("USAGE : \n" +
                "jarvis task complete <taskID>  // task id is the ID of the task under which the subtask would be created\n" +
                "jarvis task archieve <taskID>  // task id is the ID of the task under which the subtask would be created\n" +
                "jarvis task discard <taskID>  // task id is the ID of the task under which the subtask would be created\n"
                );
            return true;
        }

        int id = Utils.Atoi(arguments[0]);

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


public class TaskRecordTimeLogCommand : ICommand
{
    public TaskRecordTimeLogCommand()
    {
    }

    public override bool Run(List<string> arguments, List<string> optionalArguments, Jarvis.JApplication application)
    {
        if (arguments.Count < 2)
        {
            ConsoleWriter.Print("Invalid arguments! \n");
            ConsoleWriter.Print("USAGE : \n" +
                "jarvis task recordtimelog <taskID> <time in mins> <comments>\n" +
                "jarvis task recordtimelog <taskID> <time in mins> <comments> <---when:-1>   // delta day count. -1 as in this timelog is created for yesterday. -2 as in day before yesterday. by default, this is 0, as in the time log is created for today."
                );
            return true;
        }

        int id = Utils.Atoi(arguments[0]);
        int timeTakenInMinutes = Utils.Atoi(arguments[1]);
        string comments = arguments.Count() > 2 ? arguments[2] : string.Empty;

        bool syntaxErrorForWhenArgument = false;
        int deltaTime = Utils.ExtractIntFromArgument(optionalArguments, "--when", 0, null, null, out syntaxErrorForWhenArgument);

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

public class TaskReportCommand : ICommand
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

    public override bool Run(List<string> arguments, List<string> optionalArguments, Jarvis.JApplication application)
    {
        if (arguments.Count != 0)
        {
            ConsoleWriter.Print("Invalid arguments! \n");
            ConsoleWriter.Print("USAGE : \n" +
                "jarvis task report"
                );
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