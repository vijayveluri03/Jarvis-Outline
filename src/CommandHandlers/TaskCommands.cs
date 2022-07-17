using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jarvis; //@todo 



public class TaskHandler : CommandHandlerBaseWithUtility
{
    public TaskHandler()
    {

    }

    protected override bool ShowHelp()
    {
        SharedLogic.StartCachingHelpText();
        SharedLogic.PrintHelp_Heading("USAGE");
        SharedLogic.PrintHelp_SubText(">task add", "To add a task");
        SharedLogic.PrintHelp_SubText(">task list", "to list all the tasks");
        SharedLogic.PrintHelp_SubText(">task complete", "to complete a task");

        SharedLogic.PrintHelp_SubText(">task status", "See number of tasks per status");
        SharedLogic.PrintHelp_SubText(">task setstatus", "to set a task as open/complete");

        SharedLogic.PrintHelp("\nLOG TIME - to track your time per task");
        SharedLogic.PrintHelp_SubText(">task starttimelog", "to start time logging");
        SharedLogic.PrintHelp_SubText(">task stoptimelog", "to stop time logging");
        SharedLogic.PrintHelp_SubText(">task active", "to show all the tasks with time logging on");
        SharedLogic.PrintHelp_SubText(">task addtimelog", "to directly add a timelog for a task (with out start and stop)");

        SharedLogic.PrintHelp_Heading("ADVANCED");
        SharedLogic.PrintHelp_SubText(">task moreinfo", "to show more details of a task");
        SharedLogic.PrintHelp_SubText(">task report", "to show all the work done in the last day/week");
        SharedLogic.PrintHelp_SubText(">task edittitle", "To edit the title of a task");
        SharedLogic.PrintHelp_SubText(">task clone", "To clone a task!");

        SharedLogic.PrintHelp_Heading("NOTES");
        SharedLogic.PrintHelp_SubText(">task note", "open note for a task");
        SharedLogic.PrintHelp_SubText(">task printnote", "print the notes. ( you can also use cat instead of printnote");
        SharedLogic.PrintHelp_SubText(">task deletenote", "delete note for a task");

        SharedLogic.PrintHelp_Heading("HELP");
        SharedLogic.PrintHelp_SubText("All the commands have their own help section. Use the argument '--help'");
        SharedLogic.PrintHelp_SubText("Example - 'task add --help' for more examples on how to use it. Try it!");
        SharedLogic.PrintHelp_SubText("This works for every single command! Cheers!");
        SharedLogic.FlushHelpText();
        return true;
    }

    protected override CommandHandlerBase GetSpecializedCommandHandler(Jarvis.JModel application, out List<string> argumentsForSpecializedHandler, bool printErrors)
    {
        string action = arguments_ReadOnly != null && arguments_ReadOnly.Count > 0 ? arguments_ReadOnly[0] : null;
        CommandHandlerBase selectedHander = null;

        switch (action)
        {
            case "add":
                selectedHander = new TaskAddCommand();
                break;
            case "clone":
                selectedHander = new TaskCloneCommand();
                break;
            case "list":
                selectedHander = new TaskListCommand();
                break;
            case "delete":
            case "remove":
                selectedHander = new TaskRemoveCommand();
                break;
            case "complete":
            case "done":
                selectedHander = new TaskCompleteCommand();
                break;
            case "starttimelog":
                selectedHander = new TaskStartCommand();
                break;
            case "stoptimelog":
                selectedHander = new TaskStopCommand();
                break;
            case "active":
                selectedHander = new TaskActiveCommand();
                break;
            case "moreinfo":
                selectedHander = new TaskShowCommand();
                break;
            case "addtimelog":
                selectedHander = new TaskRecordTimeLogCommand();
                break;
            case "status":
                selectedHander = new TaskStatusCommand();
                break;
            case "setstatus":
                selectedHander = new TaskSetStatusCommand();
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
                    ConsoleWriter.Print("Invalid command. Try 'task --help' for more information.");
                break;
        }

        if (selectedHander != null)
        {
            argumentsForSpecializedHandler = new List<string>(arguments_ReadOnly);
            argumentsForSpecializedHandler.RemoveAt(0);

            Utils.Assert(selectedHander is CommandHandlerBaseWithUtility);
            (selectedHander as CommandHandlerBaseWithUtility).Init(application, notes);
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


public class TaskAddCommand : CommandHandlerBaseWithUtility
{
    public TaskAddCommand()
    {

    }

    protected override bool ShowHelp()
    {
        SharedLogic.StartCachingHelpText();
        SharedLogic.PrintHelp_Heading("USAGE");
        SharedLogic.PrintHelp_SubText(">task add <category> <title>");
        SharedLogic.PrintHelp_SubText(">task add <category> <title> --story", "A story is collection of tasks" );
        SharedLogic.PrintHelp_SubText(">task add <category> <title> --collection", "Creates a collection instead of a task). Collections are to club many simple tasks. Like Grocery list. Easy to keep them in one place");
        SharedLogic.PrintHelp_SubText(">task add <category> <title> --status:<status>", "To directly assign a status after creation. (shortform -s)");
        
        SharedLogic.PrintHelp_Heading("EXAMPLES");
        SharedLogic.PrintHelp_SubText(">task add chores \"Buy a T-Shirt\"");
        SharedLogic.PrintHelp_SubText(">task add chores \"Shopping list\" --collection", "Creates a collection");
        SharedLogic.PrintHelp_SubText(">task add chores \"Go to Goa!\" --story", "Creates a story");
        SharedLogic.PrintHelp_SubText(">task add chores \"Go to Goa!\" --story --status:complete", "Creates a story, and status is set as complete.");

        SharedLogic.PrintHelp_WithHeadingAndSubText("Whats category", application.DesignData.categories.listOfCategories,  "Category can be one of these following. you can add more in the Data/Design.json as per your need.");

        SharedLogic.PrintHelp_Heading("MORE INFO");
        SharedLogic.PrintHelp_SubText("use --story or -s to create a story (instead of a task)");
        SharedLogic.PrintHelp_SubText("use --collection or -c to create a collection (instead of a task)");
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

        string[] categories = arguments_ReadOnly[0].Split(',');
        string title = arguments_ReadOnly[1];
        bool isStory = optionalArguments_ReadOnly.Contains("--story") || optionalArguments_ReadOnly.Contains("-s");
        bool isCollection = optionalArguments_ReadOnly.Contains("--collection") || optionalArguments_ReadOnly.Contains("-c");

        if (!application.DesignData.DoesCategoryExist(categories))
        {
            ConsoleWriter.Print("Invalid categories.\n");
            SharedLogic.PrintHelp_WithHeadingAndSubText("Whats category", application.DesignData.categories.listOfCategories, "Category can be one of these following. you can add more in the Data/Design.json as per your need.");
            return true;
        }


        bool syntaxError = false;
        string status = Utils.CLI.ExtractStringFromCLIParameter(optionalArguments_ReadOnly, new List<string> { "--status", "-s" }, "", null, null, out syntaxError);

        if (syntaxError)
        {
            ConsoleWriter.Print("syntax invalid for --status argument. please try again");
            return true;
        }

        if (status.IsEmpty())
            status = application.DesignData.TaskDefaultStatus;

        if (!application.DesignData.DoesTaskStatusExist(status))
        {
            ConsoleWriter.Print("Invalid status");
            SharedLogic.PrintHelp_WithHeadingAndSubText("Whats Status", application.DesignData.tasks.statusList, "Status can be one of these following. you can add more in the Data/Design.json as per your need.");
            return true;
        }

        var entry = SharedLogic.CreateNewTask(
            application.taskManager,
            categories,
            title,
            isStory ? Task.Type.Story : (isCollection ? Task.Type.Collection : Task.Type.Task),
            status);

        application.taskManager.AddTask(entry);

        ConsoleWriter.Print("New task added with id : " + entry.id);
        return true;
    }
}

public class TaskCloneCommand : CommandHandlerBaseWithUtility
{
    public TaskCloneCommand()
    {

    }

    protected override bool ShowHelp()
    {
        SharedLogic.StartCachingHelpText();
        SharedLogic.PrintHelp_Heading("USAGE");
        SharedLogic.PrintHelp_SubText(">task clone id");
        SharedLogic.PrintHelp_SubText(">task clone id --status:<status>", "To directly assign a status after cloning. (shortform -s)");
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

        int originalTaskID = Utils.Conversions.Atoi(arguments_ReadOnly[0]);

        if (!application.taskManager.DoesTaskExist(originalTaskID))
        {
            Console.WriteLine("Task with id:" + originalTaskID + " not found!");
            return true;
        }

        Task originalTask = application.taskManager.GetTask_ReadOnly(originalTaskID);
        bool syntaxError = false;
        string status = Utils.CLI.ExtractStringFromCLIParameter(optionalArguments_ReadOnly, new List<string> { "--status", "-s" }, "", null, null, out syntaxError);

        if (syntaxError)
        {
            ConsoleWriter.Print("syntax invalid for --status argument. please try again");
            return true;
        }

        if (status.IsEmpty())
            status = originalTask.status;

        var clonedTask = SharedLogic.CreateNewTask(
            application.taskManager,
            originalTask.categories,
            originalTask.title,
            originalTask.type,
            status);

        application.taskManager.AddTask(clonedTask);

        ConsoleWriter.Print("New task added with id : " + clonedTask.id);

        if (notes.DoesNoteExist(originalTask.id))
        {
            notes.CreateNoteIfUnavailable(clonedTask.id);
            notes.AppendToNote(clonedTask.id, notes.GetNoteContent(originalTask.id));
            ConsoleWriter.Print("Notes is cloned!");
        }

        return true;
    }
}

public class TaskCompleteCommand : CommandHandlerBaseWithUtility
{
    public TaskCompleteCommand()
    {

    }

    protected override bool ShowHelp()
    {
        SharedLogic.StartCachingHelpText();
        SharedLogic.PrintHelp_Heading("USAGE");
        SharedLogic.PrintHelp_SubText(">task complete <taskID> ", "Task id is the ID of the task which is done");

        SharedLogic.PrintHelp_Heading("EXAMPLES");
        SharedLogic.PrintHelp_SubText(">task complete 1", "If you want to complete a task with ID : 1");
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

        int[] ids = Utils.Conversions.SplitAndAtoi(arguments_ReadOnly[0]);

        foreach (var id in ids)
        {

            if (application.taskManager.DoesTaskExist(id))
            {
                var task = application.taskManager.GetTask_Editable(id);
                task.SetStatus(application.DesignData.TaskCompletedStatus);
                ConsoleWriter.Print("Task with id : {0} marked as {1}", id, task.StatusString);
            }
            else
                ConsoleWriter.Print("Task not found with id : " + id);
        }

        return true;
    }
}

public class TaskRemoveCommand : CommandHandlerBaseWithUtility
{
    public TaskRemoveCommand()
    {

    }

    protected override bool ShowHelp()
    {
        SharedLogic.StartCachingHelpText();
        SharedLogic.PrintHelp_Heading("USAGE");
        SharedLogic.PrintHelp_SubText(">task remove <taskID> ", "Task id is the ID of the task you are trying to remove");

        SharedLogic.PrintHelp_Heading("EXAMPLES");
        SharedLogic.PrintHelp_SubText(">task remove 1", "If you want to remove a task with ID : 1");
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

        int[] ids = Utils.Conversions.SplitAndAtoi(arguments_ReadOnly[0]);

        foreach (var id in ids)
        {
            if (application.taskManager.RemoveTaskIfExists(id))
            {
                application.logManager.RemoveAllEntries(id);
                ConsoleWriter.Print("Task removed with id : " + id);

                if (notes.DoesNoteExist(id))
                {
                    notes.RemoveNote(id);
                    ConsoleWriter.Print("Notes removed");
                }
            }
            else
                ConsoleWriter.Print("Task not found with id : " + id);
        }

        return true;
    }
}

public class TaskStartCommand : CommandHandlerBaseWithUtility
{
    public TaskStartCommand()
    {

    }

    protected override bool ShowHelp()
    {
        SharedLogic.StartCachingHelpText();
        SharedLogic.PrintHelp_Heading("USAGE");
        SharedLogic.PrintHelp_SubText(">task starttimelog <taskID>", "Start time logging for a task");

        SharedLogic.PrintHelp_Heading("EXAMPLES");
        SharedLogic.PrintHelp_SubText(">task starttimelog 1", "Start logging time for task 1");
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

        int id = Utils.Conversions.Atoi(arguments_ReadOnly[0]);

        if (application.UserData.IsTaskInProgress())
        {
            ConsoleWriter.Print("There is already a task with ID : {0} in progress.", application.UserData.taskProgress.taskIDInProgress);
            return true;
        }

        if (application.taskManager.DoesTaskExist(id))
        {
            application.UserData.StartTask(id, DateTime.Now);
            ConsoleWriter.Print("Started logging time for Task with id : {0} -> {1}", id, application.taskManager.GetTask_ReadOnly(id).title);
        }
        else
            ConsoleWriter.Print("Task not found with id : " + id);

        return true;
    }
}

public class TaskStopCommand : CommandHandlerBaseWithUtility
{
    public TaskStopCommand()
    {
    }

    protected override bool ShowHelp()
    {
        SharedLogic.StartCachingHelpText();
        SharedLogic.PrintHelp_Heading("USAGE");
        SharedLogic.PrintHelp_SubText(">task stoptimelog" , "Stops time tracking/logging for a task which is active. If no task is active, no action will be performed.");
        SharedLogic.PrintHelp_SubText(">task stoptimelog <comments>");
        SharedLogic.PrintHelp_SubText(">task stoptimelog --discard", "Stop tracking, but do not save the log. Discard it!");

        SharedLogic.PrintHelp_Heading("EXAMPLES");
        SharedLogic.PrintHelp_SubText(">task stoptimelog", "Stop logging time for a task which is active.");
        SharedLogic.FlushHelpText();
        return true;
    }

    protected override bool Run()
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
            ConsoleWriter.Print("Stopped and discarded progress for Task with id : {0} -> {1} ", id, application.taskManager.GetTask_ReadOnly(id).title);
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
        ConsoleWriter.Print("Total time logged : {0}", Utils.Time.MinutesToHoursString(timeTakenInMinutes));

        return true;
    }
}

public class TaskEditTitleCommand : CommandHandlerBaseWithUtility
{
    public TaskEditTitleCommand()
    {

    }

    protected override bool ShowHelp()
    {
        SharedLogic.StartCachingHelpText();
        SharedLogic.PrintHelp_Heading("USAGE");
        SharedLogic.PrintHelp_SubText(">task edittitle <taskID> <title>", "rename the title");

        SharedLogic.PrintHelp_Heading("EXAMPLES");
        SharedLogic.PrintHelp_SubText(">task edittitle 1 \"Buy Blue T-shirt\"", "rename the title of task : 1 to 'Buy blue T-shirt'");
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

public class TaskActiveCommand : CommandHandlerBaseWithUtility
{
    public TaskActiveCommand()
    {
    }

    protected override bool ShowHelp()
    {
        SharedLogic.StartCachingHelpText();
        SharedLogic.PrintHelp_Heading("USAGE");
        SharedLogic.PrintHelp_SubText(">task active", "Shows if time tracking is active for any task");

        SharedLogic.PrintHelp_Heading("EXAMPLES");
        SharedLogic.PrintHelp_SubText(">task active");
        SharedLogic.FlushHelpText();
        return true;
    }

    protected override bool Run()
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
        ConsoleWriter.Print("Total time logged : {0}", Utils.Time.MinutesToHoursString(timeTakenInMinutes));
        return true;
    }
}

public class TaskListCommand : CommandHandlerBaseWithUtility
{
    public TaskListCommand()
    {

    }
    protected override bool ShowHelp()
    {
        SharedLogic.StartCachingHelpText();
        SharedLogic.PrintHelp_Heading("USAGE");
        SharedLogic.PrintHelp_SubText(">task list", "lists all the tasks which are open");
        SharedLogic.PrintHelp_SubText(">task list --allstatus", "Shows everything across all statuses!(Short form : -a)");
        SharedLogic.PrintHelp_SubText(">task list --status:<status>", "Shows tasks within a specific status.See examples for how to use it.(Shortform:-s)");
        SharedLogic.PrintHelp_SubText(">task list --story", "Shows only stories. Can be used with --allstatus");
        SharedLogic.PrintHelp_SubText(">task list --collection", "Shows only collections. Can be used with --allstats");
        SharedLogic.PrintHelp_SubText(">task list --task", "Shows only tasks. Can be used with --allstatus");
        SharedLogic.PrintHelp_SubText(">task list --cat:<category>", "Shows only those category. Can be used with --allstatus");
        SharedLogic.PrintHelp_SubText(">task list --search:<text>", "Searches for stuff with the current filter. Use it with --allstatus for overall search");

        SharedLogic.PrintHelp_Heading("EXAMPLES");
        SharedLogic.PrintHelp_SubText(">task list");
        SharedLogic.PrintHelp_SubText(">task list --allstatus", "Shows all the tasks");
        SharedLogic.PrintHelp_SubText(">task list --story");
        SharedLogic.PrintHelp_SubText(">task list --status:archieve", "Shows all the tasks archieved");
        SharedLogic.PrintHelp_SubText(">task list --status:open", "Shows only the open tasks ( this is also the default setting )");
        SharedLogic.PrintHelp_SubText(">task list --status:complete", "Shows only the tasks completed");
        SharedLogic.PrintHelp_SubText(">task list --status:discard", "Shows only the tasks discard");
        SharedLogic.PrintHelp_SubText(">task list --cat:office", "filter by category");
        SharedLogic.PrintHelp_SubText(">task list --search:\"Buy shoes\" --allstatus", "Search in all the statuses");
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

        int lineCount = 0;
        int titleArea = (optionalArguments_ReadOnly.Contains("-e") || optionalArguments_ReadOnly.Contains("--expand")) ? 120 : 40;
        int categoryArea = 15;
        int newLineAfter = 5;

        #region STATUS
        string status;
        {
            bool syntaxError = false;
            status = Utils.CLI.ExtractStringFromCLIParameter(optionalArguments_ReadOnly, new List<string> { "--status", "-s" }, "", null, null, out syntaxError);

            if (syntaxError)
            {
                ConsoleWriter.Print("syntax invalid for --status argument. please try again");
                return true;
            }

            if ( !status.IsEmpty() && !application.DesignData.DoesTaskStatusExist(status))
            {
                ConsoleWriter.Print("Invalid status");
                SharedLogic.PrintHelp_WithHeadingAndSubText("Whats Status", application.DesignData.tasks.statusList, "Status can be one of these following. you can add more in the Data/Design.json as per your need.");
                return true;
            }
        }
        #endregion


        bool isStoryOnly = optionalArguments_ReadOnly.Contains("--story");
        bool isCollectionOnly = optionalArguments_ReadOnly.Contains("--collection");
        bool isTaskOnly = optionalArguments_ReadOnly.Contains("--task");

        bool isAll = optionalArguments_ReadOnly.Contains("--allstatus") || optionalArguments_ReadOnly.Contains("-a");

        #region CATEGORY
        string categoryFilter;
        {
            bool syntaxErrorInCategoryFilter = false;
            categoryFilter = Utils.CLI.ExtractStringFromCLIParameter(optionalArguments_ReadOnly, "--cat", string.Empty, null, null, out syntaxErrorInCategoryFilter);
            if (syntaxErrorInCategoryFilter)
            {
                ConsoleWriter.Print("Invalid syntax for --cat argument.");
                categoryFilter = string.Empty;
                return true;
            }
            else if (categoryFilter != string.Empty && !application.DesignData.DoesCategoryExist(categoryFilter))
            {
                ConsoleWriter.Print("Invalid category");
                SharedLogic.PrintHelp_WithHeadingAndSubText("Whats category", application.DesignData.categories.listOfCategories, "Category can be one of these following. you can add more in the Data/Design.json as per your need.");
                categoryFilter = string.Empty;
                return true;
            }
        }
        #endregion
        string searchFilter = "";
        {
            bool syntaxError = false;
            searchFilter = Utils.CLI.ExtractStringFromCLIParameter(optionalArguments_ReadOnly, "--search", string.Empty, null, null, out syntaxError);
            if (syntaxError)
            {
                ConsoleWriter.Print("Invalid syntax for --search argument.");
                searchFilter = string.Empty;
                return true;
            }
            searchFilter = searchFilter.ToLower();
        }

        // by default only open ones are shown, unless specified in which case open ones are not shown. 
        if (status.IsEmpty())
            status = application.DesignData.TaskDefaultStatus;

        Dictionary<string, List<Task>> filteredTasks = new Dictionary<string, List<Task>>();

        foreach (var task in application.taskManager.Data.entries)
        {
            if (isAll)
            {
                // no filterning
            }
            else
            {
                if (task.status.ToLower() != (status.ToLower()))
                    continue;
            }

            if (isTaskOnly && !task.IsTask)
                continue;

            if (isStoryOnly && !task.IsStory)
                continue;

            if (isCollectionOnly && !task.IsCollection)
                continue;

            if (!categoryFilter.IsEmpty() && !task.categories.Contains(categoryFilter))
                continue;

            if( !searchFilter.IsEmpty() && !task.title.ToLower().Contains(searchFilter))
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
            ConsoleWriter.PrintInColor("{0, -4} {1,-" + categoryArea + "} {2,-" + titleArea + "} {3, -15} {4, -15}",
                application.DesignData.HighlightColorForText,
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

                    ConsoleWriter.PrintInColor("{0, -4} {1,-" + categoryArea + "} {2,-" + titleArea + "} {3, -15} {4, -15} {5}",
                        textColor,
                        task.id,
                        (task.categories != null && task.categories.Length > 0 ? Utils.Conversions.ArrayToString(task.categories, true).TruncateWithVisualFeedback(categoryArea - 3) : "INVALID"),
                        task.title.TruncateWithVisualFeedback(titleArea - 7/*for the ...*/)
                            + (notes.DoesNoteExist(task.id) ? "+(N)" : ""),
                        task.StatusString,
                        (isInProgress ? Utils.Time.MinutesToHoursString(timeInProgress) + " + " : "") + ("(" + Utils.Time.MinutesToHoursString(application.logManager.GetTotalTimeSpentInMins(task.id)) + "h)"),
                        (isInProgress ? "LOGGING TIME" : "")
                        ); ;

                    lineCount++;

                    //@todo
                    if (lineCount % newLineAfter == 0)
                        ConsoleWriter.Print();
                }
            }
        }
        else
            ConsoleWriter.Print("No tasks found!");

        return true;
    }
}


public class TaskShowCommand : CommandHandlerBaseWithUtility
{
    public TaskShowCommand()
    {

    }

    protected override bool ShowHelp()
    {
        SharedLogic.StartCachingHelpText();
        SharedLogic.PrintHelp_Heading("USAGE");
        SharedLogic.PrintHelp_SubText(">task moreinfo <taskID>", "Show more details of a task");

        SharedLogic.PrintHelp_Heading("EXAMPLES");
        SharedLogic.PrintHelp_SubText(">task moreinfo 1", "Show more details for task : 1" );
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

                ConsoleWriter.Print("STATUS : {0, -15}\nTYPE : {1}\nTIME SPENT : {2,-15}\nNOTES AVAILABLE: {3,-15}\nLOGGING TIME : {4}",
                    task.StatusString,
                    task.TypeString,
                    (isInProgress ? Utils.Time.MinutesToHoursString(timeInProgress) + " + " : "") +
                    ("(" + Utils.Time.MinutesToHoursString(application.logManager.GetTotalTimeSpentInMins(task.id)) + "h)"),
                    (notes.DoesNoteExist(task.id) ? "YES" : "NO"),
                    (isInProgress ? "YES" : "NO")
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

public class TaskSetStatusCommand : CommandHandlerBaseWithUtility
{
    public TaskSetStatusCommand()
    {
    }

    protected override bool ShowHelp()
    {
        SharedLogic.StartCachingHelpText();
        SharedLogic.PrintHelp_Heading("USAGE");
        SharedLogic.PrintHelp_SubText(">task setstatus <taskID> <status>", "This will set the status to open/complete/discard");
        SharedLogic.PrintHelp_SubText(">task setstatus <taskID> <status> --time:<time>", "If you want to also log unlogged time spent on this task");

        SharedLogic.PrintHelp_WithHeadingAndSubText("WHAT'S STATUS", application.DesignData.tasks.statusList, "You can set these following statuses");
        SharedLogic.PrintHelp_SubText("You can add new statuses or change any of the existing statuses in the data/Design.Json. Feel free to add more as you wish!");

        SharedLogic.PrintHelp_Heading("EXAMPLES");
        SharedLogic.PrintHelp_SubText(">task setstatus 1 complete", "Task with ID 1 is now completed");
        SharedLogic.PrintHelp_SubText(">task setstatus 1 complete --time:2", "Task with ID 1 is now completed. 2 hours is logged!");
        SharedLogic.PrintHelp_SubText(">task setstatus 1 open", "Task with ID 1 is open");
        SharedLogic.PrintHelp_SubText(">task setstatus 1 discard", "Task with ID 1 is discarded");
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
        
        int[] ids = Utils.Conversions.SplitAndAtoi(arguments_ReadOnly[0]);
        string status = arguments_ReadOnly[1];

        bool syntaxError = false;
        int timeInHours = Utils.CLI.ExtractIntFromCLIParameter(optionalArguments_ReadOnly, "--time", 0, null, null, out syntaxError);

        if (syntaxError)
        {
            ConsoleWriter.Print("syntax invalid for --time argument. please try again");
            return true;
        }

        if ( !application.DesignData.DoesTaskStatusExist(status))
        {
            ConsoleWriter.Print("Invalid status");
            SharedLogic.PrintHelp_WithHeadingAndSubText("Whats Status", application.DesignData.tasks.statusList, "Status can be one of these following. you can add more in the Data/Design.json as per your need.");

            return true;
        }

        foreach (var id in ids)
        {
            if (application.taskManager.DoesTaskExist(id))
            {
                var task = application.taskManager.GetTask_Editable(id);
                task.SetStatus(status);
                ConsoleWriter.Print("Task with id : {0} marked as {1}", id, task.StatusString);

                if (timeInHours > 0)
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

                        ConsoleWriter.Print("Total time spent on this task {0} hours ", Utils.Time.MinutesToHoursString(application.logManager.GetTotalTimeSpentInMins(id)));
                    }
                }
            }
            else
                ConsoleWriter.Print("Task not found with id : " + id);
        }

        return true;
    }

}


public class TaskRecordTimeLogCommand : CommandHandlerBaseWithUtility
{
    public TaskRecordTimeLogCommand()
    {
    }

    protected override bool ShowHelp()
    {
        SharedLogic.StartCachingHelpText();
        SharedLogic.PrintHelp_Heading("USAGE");
        SharedLogic.PrintHelp_SubText(">task addtimelog <taskID> <time in hours> <comments>", "Force add a time log ( instead of start and stop )");
        SharedLogic.PrintHelp_SubText(">task addtimelog <taskID> <time in hours> <comments> <--when:-1>  ", "How many days before ?. -1 if this timelog is for yesterday. -2 for a day before that. by default, this is 0, as in the time log is created for today.");

        SharedLogic.PrintHelp_Heading("EXAMPLES");
        SharedLogic.PrintHelp_SubText(">task addtimelog 1 2", "A time of 2 hours is logged for a Task with ID 1");
        SharedLogic.FlushHelpText();

        return true;
    }

    protected override bool Run()
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

        bool syntaxError = false;
        int deltaTime = Utils.CLI.ExtractIntFromCLIParameter(optionalArguments_ReadOnly, "--when", 0, null, null, out syntaxError);

        if (syntaxError)
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

public class TaskStatusCommand : CommandHandlerBaseWithUtility
{
    public TaskStatusCommand()
    {
    }


    protected override bool ShowHelp()
    {
        SharedLogic.StartCachingHelpText();
        SharedLogic.PrintHelp_Heading("USAGE");
        SharedLogic.PrintHelp_SubText(">task status", "Shows count of tasks for each status");
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

        Dictionary<string, int> countByStatus = new Dictionary<string, int>();
        foreach (var task in application.taskManager.Data.entries)
        {
            if(!countByStatus.ContainsKey(task.status))
                countByStatus[task.status] = 0;

            countByStatus[task.status]++;
        }


        // output Heading 
        if (countByStatus.Count() > 0)
        {
            ConsoleWriter.PrintInColor("{0, -30} {1, -15}",
                application.DesignData.HighlightColorForText,
                "STATUS", "TASK COUNT"
                );

            int lineCount = 0;
            foreach (var status in countByStatus)
            {
                ConsoleWriter.Print("{0, -30} {1, -15}", status.Key, status.Value);

                lineCount++;
                if (lineCount % 5 == 0)
                    ConsoleWriter.EmptyLine();
            }
        }
        else
            ConsoleWriter.Print("No tasks found!");
        return true;
    }
}

public class TaskReportCommand : CommandHandlerBaseWithUtility
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
        SharedLogic.StartCachingHelpText();
        SharedLogic.PrintHelp_Heading("USAGE");
        SharedLogic.PrintHelp_SubText(">task report", "Shows the report with all the work done in the last week");
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

public class TaskCatNotesCommand : CommandHandlerBaseWithUtility
{
    public TaskCatNotesCommand()
    {

    }

    protected override bool ShowHelp()
    {
        SharedLogic.StartCachingHelpText();
        SharedLogic.PrintHelp_Heading("USAGE");
        SharedLogic.PrintHelp_SubText(">task cat <taskID>", "Prints the notes of a task. You can also use printnote instead of cat");
        SharedLogic.PrintHelp_SubText(">task printnote <taskID>", "Same as cat");

        SharedLogic.PrintHelp_Heading("EXAMPLES");
        SharedLogic.PrintHelp_SubText(">task cat 1", "show the notes for task 1");
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

        int id = Utils.Conversions.Atoi(arguments_ReadOnly[0]);


        if (application.taskManager.DoesTaskExist(id))
        {
            if (!notes.DoesNoteExist(id))
                ConsoleWriter.Print("Notes not found for the task with id : {0}", id);
            else
            {
                ConsoleWriter.PrintInColor("NOTES :", application.DesignData.HighlightColorForText);
                ConsoleWriter.PrintText(notes.GetNoteContent(id));
            }
        }
        else
            ConsoleWriter.Print("Task not found with id : " + id);

        return true;
    }
}

public class TaskEditNoteCommand : CommandHandlerBaseWithUtility
{
    public TaskEditNoteCommand()
    {

    }

    protected override bool ShowHelp()
    {
        SharedLogic.StartCachingHelpText();
        SharedLogic.PrintHelp_Heading("USAGE");
        SharedLogic.PrintHelp_SubText(">task note <taskID>", "Opens notes for a task.");
        SharedLogic.PrintHelp_SubText(">task note <taskID> --ext:<editorname>", "provide external editor name of your choice, to open the notes in. Example : code or vim");
        SharedLogic.PrintHelp_SubText(">task note <taskID> --append:<Message>", "Append a message directly to a note");
        SharedLogic.PrintHelp_SubText(">task note <taskID> --appendlog:<Message>", "Appends a message directly to a note, with a timestamp");
        SharedLogic.PrintHelp_SubText(">task note <taskID> --appendtask:<taskID>", "Appends the title of a task directly to a note with a prefix task:");

        SharedLogic.PrintHelp_Heading("ADVANCED");
        SharedLogic.PrintHelp_SubText("You can change the default editor in the Data/Design.json under 'defaultExternalEditor'");
        SharedLogic.PrintHelp_SubText("you can use '--nowait' to have jarvis not wait for the notes to be closed.");

        SharedLogic.PrintHelp_Heading("EXAMPLES");
        SharedLogic.PrintHelp_SubText(">task note 1", "Edit the notes for task : 1");
        SharedLogic.PrintHelp_SubText(">task note 1 --ext:code", "Edit the notes for task : 1, within the visual studio code");
        SharedLogic.PrintHelp_SubText(">task note 1 --append:\"Buy milk\"", "Add 'buy milk' to the notes!");
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

        int id = Utils.Conversions.Atoi(arguments_ReadOnly[0]);
        bool waitForTheProgramToEnd = !optionalArguments_ReadOnly.Contains("--nowait");

        bool syntaxError = false;
        string externalProgram = Utils.CLI.ExtractStringFromCLIParameter(optionalArguments_ReadOnly, "--ext", string.Empty, null, null, out syntaxError);

        if (syntaxError)
        {
            ConsoleWriter.Print("Invalid syntax for --ext argument.");
            return true;
        }

        syntaxError = false;
        string appendMessage = Utils.CLI.ExtractStringFromCLIParameter(optionalArguments_ReadOnly, "--append:", string.Empty, null, null, out syntaxError);

        if (syntaxError)
        {
            ConsoleWriter.Print("Invalid syntax for --append argument.");
            return true;
        }

        if (appendMessage.IsEmpty())
        {
            string appendLogMessage = Utils.CLI.ExtractStringFromCLIParameter(optionalArguments_ReadOnly, "--appendlog:", string.Empty, null, null, out syntaxError);

            if (syntaxError)
            {
                ConsoleWriter.Print("Invalid syntax for --appendlog argument.");
                return true;
            }
            if (!appendLogMessage.IsEmpty())
            {
                appendMessage = "Log on " + DateTime.Now.ToShortDateString() + " " + appendLogMessage;
            }
        }

        if (appendMessage.IsEmpty())
        {
            int taskID = Utils.CLI.ExtractIntFromCLIParameter(optionalArguments_ReadOnly, "--appendtask:", -1, null, null, out syntaxError);

            if (syntaxError)
            {
                ConsoleWriter.Print("Invalid syntax for --appendlog argument.");
                return true;
            }
            if (taskID != -1)
            {
                Task task = application.taskManager.GetTask_ReadOnly(taskID);
                if (task == null)
                {
                    ConsoleWriter.Print("Task not found with id:" + taskID);
                    return true;
                }
                appendMessage = "Task:" + task.title;
            }
        }

        if (application.taskManager.DoesTaskExist(id))
        {
            notes.CreateNoteIfUnavailable(id);

            if (!appendMessage.IsEmpty())
            {
                ConsoleWriter.Print("Message appended to the notes");
                notes.AppendToNote(id, appendMessage);
            }
            else
            {
                notes.OpenNote(application, id, externalProgram, waitForTheProgramToEnd, true);
            }
        }
        else
            ConsoleWriter.Print("Task not found with id : " + id);
        return true;
    }
}

public class TaskDeleteNoteCommand : CommandHandlerBaseWithUtility
{
    public TaskDeleteNoteCommand()
    {

    }

    protected override bool ShowHelp()
    {
        SharedLogic.StartCachingHelpText();
        SharedLogic.PrintHelp_Heading("USAGE");
        SharedLogic.PrintHelp_SubText(">task deletenote <taskID>", "deletes the notes");
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

        int id = Utils.Conversions.Atoi(arguments_ReadOnly[0]);

        if (application.taskManager.DoesTaskExist(id))
        {
            if (notes.DoesNoteExist(id))
            {
                notes.RemoveNote(id);
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