using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using Jarvis; //@todo 


public class TaskHandler : ICommand
{
    public TaskHandler()
    {

    }

    public override bool Run(List<string> command, Jarvis.JApplication application)
    {
        if (command.Count < 1)
        {
            Console.Out.WriteLine("Invalid parameters for task. Use actions like add, list, delete, start, stop, complete, later, discard etc");
            return false;
        }

        string action = command[0];
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
            default:
                Console.Out.WriteLine("unknown action");
                break;
        }

        if (selectedHander != null)
        {
            command.RemoveAt(0);
            selectedHander.Run(command, application);
        }
        return true;
    }
}


public class TaskAddCommand : ICommand
{
    public TaskAddCommand()
    {

    }

    public override bool Run(List<string> command, Jarvis.JApplication application)
    {
        if (command.Count != 2)
        {
            Console.Out.WriteLine("Invalid parameters for task add. Append add with categories(seperated by comma) and title.");
            return true;
        }

        string[] categories = command[0].Split(',');
        string title = command[1];

        if (!application.DesignData.DoesCategoryExist(categories))
        {
            Console.Out.WriteLine("Invalid categories. if you need a new one create, add them in designdata file");
            return true;
        }

        var entry = SharedLogic.CreateNewEntry(application.taskManager, categories, title);
        application.taskManager.AddEntry(entry);

        Console.Out.WriteLine("New entry added with id : " + entry.id);
        return true;
    }
}


public class TaskRemoveCommand : ICommand
{
    public TaskRemoveCommand()
    {

    }

    public override bool Run(List<string> command, Jarvis.JApplication application)
    {
        if (command.Count != 1)
        {
            Console.Out.WriteLine("Invalid parameters for task remove. Append with Task ID which needs to be removed.");
            return true;
        }

        string[] ids = command[0].Split(',');

        foreach (var idStr in ids)
        {
            int id = Utils.Atoi(idStr, -1);
            if (application.taskManager.RemoveEntryIfExists(id))
                Console.Out.WriteLine("Entry removed with id : " + id);
            else
                Console.Out.WriteLine("Entry not found with id : " + id);
        }

        return true;
    }
}

public class TaskStartCommand : ICommand
{
    public TaskStartCommand()
    {

    }

    public override bool Run(List<string> command, Jarvis.JApplication application)
    {
        if (command.Count != 1)
        {
            Console.Out.WriteLine("Invalid parameters for task start. Append with Task ID which needs to be started.");
            return true;
        }

        int id = Utils.Atoi(command[0]);

        if (application.UserData.IsTaskInProgress())
        {
            Console.Out.WriteLine("There is already a task with ID : {0} in progress.", application.UserData.taskProgress.taskIDInProgress);
            return true;
        }

        if (application.taskManager.IsEntryAvailableWithID(id))
        {
            application.UserData.StartTask(id, DateTime.Now);
            Console.Out.WriteLine("Started progress on entry with id : " + id);
        }
        else
            Console.Out.WriteLine("Entry not found with id : " + id);

        return true;
    }
}

public class TaskStopCommand : ICommand
{
    public TaskStopCommand()
    {
    }

    public override bool Run(List<string> command, Jarvis.JApplication application)
    {
        if (command.Count != 1)
        {
            Console.Out.WriteLine("Invalid parameters for task stop. Stop does one parameter, which is the comments for the task stopped. It will auto stop a task which is in progress");
            return true;
        }

        if (!application.UserData.IsTaskInProgress())
        {
            Console.Out.WriteLine("There is no task in progress.");
            return true;
        }

        string comments = command[0];
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

        Console.Out.WriteLine("Stopped progress on entry with id : " + id);
        return true;
    }
}

public class TaskListCommand : ICommand
{
    public TaskListCommand()
    {

    }

    public override bool Run(List<string> command, Jarvis.JApplication application)
    {
        int lineCount = 0;
        foreach (var entry in application.taskManager.outlineData.entries)
        {
            int titleArea = 40;
            bool isInProgress = application.UserData.IsTaskInProgress() && application.UserData.taskProgress.taskIDInProgress == entry.id ;
            int timeInProgress = isInProgress ? (int)(DateTime.Now - application.UserData.taskProgress.startTime).TotalMinutes : 0;

            Console.Out.WriteLine("{0, -4} {1,-15} {2,-" + titleArea + "} {3, -15} {4, -15}",
                entry.id,
                (entry.categories != null && entry.categories.Length > 0 ? Utils.ArrayToString(entry.categories, true) : "INVALID"),
                entry.title.TruncateWithVisualFeedback(titleArea - 3/*for the ...*/),
                (isInProgress ? "In Progress" : entry.StatusString),
                (isInProgress ? timeInProgress + " + " : "" ) + ("("+application.logManager.GetTotalTimeSpentToday(entry.id) + "," +  application.logManager.GetTotalTimeSpent(entry.id)+")")
                );

            lineCount++;

            //@todo
            if (lineCount % 5 == 0)
                Console.Out.WriteLine();
        }

        return true;
    }
}
