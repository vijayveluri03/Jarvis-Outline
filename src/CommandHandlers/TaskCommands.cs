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
                "\n" +
                "Jarvis task delete // to remove a task\n" +
                "Jarvis task complete // to complete a task\n" +
                "Jarvis task discard // to discard a task\n" +
                "Jarvis task archieve // to archieve a task\n" +
                "\n" +
                "Jarvis task start // to track the time of a task\n" +
                "Jarvis task stop // to stop time tracking\n" +
                "\n" +
                "Jarvis task show // to show a task\n" +
                "\n" +
                "ADVANCED\n" +
                "jarvis task addsubtask // to add subtasks to a task\n" +
                "jarvis task recordtimelog // to record an offline task\n");

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
            case "recordtimelog":
                selectedHander = new TaskRecordTimeLogCommand();
                break;
            case "complete":
                selectedHander = new TaskSetStatusCommand(Task.Status.Complete);
                break;
            case "discard":
                selectedHander = new TaskSetStatusCommand(Task.Status.Discard);
                break;

            case "archieve":
                selectedHander = new TaskSetStatusCommand(Task.Status.Archieve);
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
        application.taskManager.AddTask(entry);

        Console.Out.WriteLine("New task added with id : " + entry.id);
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
            if (application.taskManager.RemoveTaskIfExists(id))
                Console.Out.WriteLine("Task removed with id : " + id);
            else
                Console.Out.WriteLine("Task not found with id : " + id);
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

        if (application.taskManager.DoesTaskExist(id))
        {
            application.UserData.StartTask(id, DateTime.Now);
            Console.Out.WriteLine("Started progress on Task with id : " + id);
        }
        else
            Console.Out.WriteLine("Task not found with id : " + id);

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

        Console.Out.WriteLine("Stopped progress on Task with id : " + id);
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
        if (application.taskManager.outlineData.entries.Count() > 0)
        {
            Console.Out.WriteLine("{0, -4} {1,-15} {2,-" + titleArea + "} {3, -15} {4, -15}",
                "ID", "DEPT", "TITLE", "STATUS", "TIME SPENT"
                );


            foreach (var entry in application.taskManager.outlineData.entries)
            {
                bool isInProgress = application.UserData.IsTaskInProgress() && application.UserData.taskProgress.taskIDInProgress == entry.id;
                int timeInProgress = isInProgress ? (int)(DateTime.Now - application.UserData.taskProgress.startTime).TotalMinutes : 0;

                Console.Out.WriteLine("{0, -4} {1,-15} {2,-" + titleArea + "} {3, -15} {4, -15}",
                    entry.id,
                    (entry.categories != null && entry.categories.Length > 0 ? Utils.ArrayToString(entry.categories, true) : "INVALID"),
                    entry.title.TruncateWithVisualFeedback(titleArea - 6/*for the ...*/) + (entry.subTasks != null && entry.subTasks.Length > 0 ? "+(" + entry.subTasks.Length + ")" : ""),
                    (isInProgress ? "In Progress" : entry.StatusString),
                    (isInProgress ? timeInProgress + " + " : "") + ("(" + application.logManager.GetTotalTimeSpentToday(entry.id) + "," + application.logManager.GetTotalTimeSpent(entry.id) + ")")
                    );

                lineCount++;

                //@todo
                if (lineCount % 5 == 0)
                    Console.Out.WriteLine();
            }
        }
        else
            Console.Out.WriteLine("No tasks found! Try adding a few using \"jarvis add\"");

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

        if (application.taskManager.DoesTaskExist(id))
        {
            bool isInProgress = application.UserData.IsTaskInProgress() && application.UserData.taskProgress.taskIDInProgress == id;
            int timeInProgress = isInProgress ? (int)(DateTime.Now - application.UserData.taskProgress.startTime).TotalMinutes : 0;
            Task entry = application.taskManager.GetTask(id);

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

            foreach (string subTask in entry.subTasks)
            {
                Console.Out.WriteLine(subTask);
            }
        }
        else
            Console.Out.WriteLine("Task not found with id : " + id);

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


        if (application.taskManager.DoesTaskExist(id))
        {
            application.taskManager.GetTask(id).AddSubTask(title);
            Console.Out.WriteLine("Subtask added to Task with id : " + id);
        }
        else
            Console.Out.WriteLine("Task not found with id : " + id);

        return true;
    }
}

public class TaskSetStatusCommand : ICommand
{
    public TaskSetStatusCommand(Task.Status status)
    {
        this.status = status;
    }

    public override bool Run(List<string> command, Jarvis.JApplication application)
    {
        if (command.Count != 1)
        {
            Console.Out.WriteLine("Invalid arguments! \n");
            Console.Out.WriteLine("USAGE : \n" +
                "jarvis task complete <taskID>  // task id is the ID of the task under which the subtask would be created\n" +
                "jarvis task archieve <taskID>  // task id is the ID of the task under which the subtask would be created\n" +
                "jarvis task discard <taskID>  // task id is the ID of the task under which the subtask would be created\n"
                );
            return true;
        }

        int id = Utils.Atoi(command[0]);

        if (application.taskManager.DoesTaskExist(id))
        {
            application.taskManager.GetTask(id).SetStatus(this.status);
            Console.Out.WriteLine("Task with id : {0} marked as {1}", id, application.taskManager.GetTask(id).StatusString);
        }
        else
            Console.Out.WriteLine("Task not found with id : " + id);

        return true;
    }

    private Task.Status status;
}


public class TaskRecordTimeLogCommand : ICommand
{
    public TaskRecordTimeLogCommand()
    {
    }

    public override bool Run(List<string> command, Jarvis.JApplication application)
    {
        if (command.Count < 2)
        {
            Console.Out.WriteLine("Invalid arguments! \n");
            Console.Out.WriteLine("USAGE : \n" +
                "jarvis task recordtimelog <taskID> <time in mins> <comments>"
                );
            return true;
        }

        int id = Utils.Atoi(command[0]);
        int timeTakenInMinutes = Utils.Atoi(command[1]);
        string comments = command.Count() > 2 ? command[2] : "";

        if (application.taskManager.DoesTaskExist(id))
        {
            // Add record to log manager
            {
                LogEntry le = new LogEntry();
                le.id = id;
                le.date = DateTime.Now;
                le.comment = comments;
                le.timeTakenInMinutes = timeTakenInMinutes;

                application.logManager.AddEntry(le);
            }

            Console.Out.WriteLine("New timelog added for task : " + id);
        }
        else
            Console.Out.WriteLine("Task not found with id : " + id);

        return true;
    }
}