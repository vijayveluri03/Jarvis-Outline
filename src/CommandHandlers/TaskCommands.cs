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

    public override bool Run(List<string> command, Jarvis.JApplication application)
    {
        if (command.Count < 1)
        {
            Console.Out.WriteLine("Invalid arguments! \n");
            Console.Out.WriteLine("USAGE : \n" +
                "Jarvis task add  // To add a task\n" +
                "Jarvis task list // to list all the tasks\n" +
                "Jarvis task delete // to remove a task\n" +
                "Jarvis task start // to track the time of a task\n" +
                "Jarvis task stop // to stop time tracking\n" +
                "Jarvis task show // to show a task\n");

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
            case "show":
                selectedHander = new TaskShowCommand();
                break;
            case "addsubtask":
                selectedHander = new TaskAddSubTaskCommand();
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
            Console.Out.WriteLine("Invalid arguments! \n");
            Console.Out.WriteLine("USAGE : \n" +
                "jarvis task add <category> <title>\n" +
                "Category can be office,learn,chores,health. you can add more in the design data as per your need."
                );
            return true;
        }

        string[] categories = command[0].Split(',');
        string title = command[1];

        if (!application.DesignData.DoesCategoryExist(categories))
        {
            Console.Out.WriteLine("Invalid categories.\n" + 
                "Category can be office,learn,chores,health. you can add more in the design data as per your need.");
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
            Console.Out.WriteLine("Invalid arguments! \n");
            Console.Out.WriteLine("USAGE : \n" +
                "jarvis task remove <taskID>  // task id is the ID of the task you are trying to remove\n"                 
                );
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
            Console.Out.WriteLine("Invalid arguments! \n");
            Console.Out.WriteLine("USAGE : \n" +
                "jarvis task start <taskID> // task id is the ID of the task you are trying to start time tracking\n"                 
                );
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
            Console.Out.WriteLine("Invalid arguments! \n");
            Console.Out.WriteLine("USAGE : \n" +
                "jarvis task stop <comments> "
                );
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
        if (command.Count != 0)
        {
            Console.Out.WriteLine("Invalid arguments! \n");
            Console.Out.WriteLine("USAGE : \n" +
                "jarvis task list   // lists all the tasks"
                );
            return true;
        }

        int lineCount = 0;
        int titleArea = 40;

        // output Heading 
        if ( application.taskManager.outlineData.entries.Count() > 0 )
        {
            Console.Out.WriteLine("{0, -4} {1,-15} {2,-" + titleArea + "} {3, -15} {4, -15}",
                "ID", "DEPT", "TITLE", "STATUS", "TIME SPENT"
                );
        }

        foreach (var entry in application.taskManager.outlineData.entries)
        {
            bool isInProgress = application.UserData.IsTaskInProgress() && application.UserData.taskProgress.taskIDInProgress == entry.id;
            int timeInProgress = isInProgress ? (int)(DateTime.Now - application.UserData.taskProgress.startTime).TotalMinutes : 0;

            Console.Out.WriteLine("{0, -4} {1,-15} {2,-" + titleArea + "} {3, -15} {4, -15}",
                entry.id,
                (entry.categories != null && entry.categories.Length > 0 ? Utils.ArrayToString(entry.categories, true) : "INVALID"),
                entry.title.TruncateWithVisualFeedback(titleArea - 6/*for the ...*/) + ( entry.subTasks != null && entry.subTasks.Length > 0 ? "+(" + entry.subTasks.Length + ")" : "" ),
                (isInProgress ? "In Progress" : entry.StatusString),
                (isInProgress ? timeInProgress + " + " : "") + ("(" + application.logManager.GetTotalTimeSpentToday(entry.id) + "," + application.logManager.GetTotalTimeSpent(entry.id) + ")")
                );

            lineCount++;

            //@todo
            if (lineCount % 5 == 0)
                Console.Out.WriteLine();
        }

        return true;
    }
}


public class TaskShowCommand : ICommand
{
    public TaskShowCommand()
    {

    }

    public override bool Run(List<string> command, Jarvis.JApplication application)
    {
        if (command.Count != 1)
        {
            Console.Out.WriteLine("Invalid arguments! \n");
            Console.Out.WriteLine("USAGE : \n" +
                "jarvis task show <taskID> // task id is the ID of the task you are trying to see\n"                 
                );
            return true;
        }

        int id = Utils.Atoi(command[0]);

        if (application.taskManager.IsEntryAvailableWithID(id))
        {
            bool isInProgress = application.UserData.IsTaskInProgress() && application.UserData.taskProgress.taskIDInProgress == id;
            int timeInProgress = isInProgress ? (int)(DateTime.Now - application.UserData.taskProgress.startTime).TotalMinutes : 0;
            Task entry = application.taskManager.GetEntry(id);

            // Heading
            Console.Out.WriteLine("{0, -4} {1,-15} {2}",
                "ID", "DEPT", "TITLE"
                );

            Console.Out.WriteLine("{0, -4} {1,-15} {2}",
                entry.id,
                (entry.categories != null && entry.categories.Length > 0 ? Utils.ArrayToString(entry.categories, true) : "INVALID"),
                entry.title);

            Console.Out.WriteLine();

            Console.Out.WriteLine("STATUS : {0, -15}\nTIME SPENT : {1,-15}",
                (isInProgress ? "In Progress" : entry.StatusString),
                (isInProgress ? timeInProgress + " + " : "") + ("(" + application.logManager.GetTotalTimeSpentToday(entry.id) + "," + application.logManager.GetTotalTimeSpent(entry.id) + ")")
                );

            Console.Out.WriteLine();

            foreach( string subTask in entry.subTasks)
            {
                Console.Out.WriteLine(subTask);
            }
        }
        else
            Console.Out.WriteLine("Entry not found with id : " + id);

        return true;
    }
}

public class TaskAddSubTaskCommand : ICommand
{
    public TaskAddSubTaskCommand()
    {
    }

    public override bool Run(List<string> command, Jarvis.JApplication application)
    {
        if (command.Count != 2)
        {
            Console.Out.WriteLine("Invalid arguments! \n");
            Console.Out.WriteLine("USAGE : \n" +
                "jarvis task addsubtask <taskID> <subtasktitle>  // task id is the ID of the task under which the subtask would be created\n"                 
                );
            return true;
        }

        int id = Utils.Atoi(command[0]);
        string title = command[1];

        
        if (application.taskManager.IsEntryAvailableWithID(id))
        {
            application.taskManager.GetEntry(id).AddSubTask(title);
            Console.Out.WriteLine("Subtask added to entry with id : " + id);
        }
        else
            Console.Out.WriteLine("Entry not found with id : " + id);

        return true;
    }
}