using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jarvis; //@todo 


public class CommandSelector : CommandHandlerBaseWithUtility
{
    public CommandSelector()
    {
    }

    private CommandHandlerBase GetCommandHandler(string command)
    {
        switch (command)
        {
            case "task":
                return new TaskHandler().Init(model, new NotesUtility(JConstants.PATH_TO_TASKS_NOTE));
            case "habit":
                return new HabitHandler().Init(model, new NotesUtility(JConstants.PATH_TO_HABITS_NOTE));
            case "notebook":
                return new NotebookHandler().Init(model, new NotesUtility(JConstants.PATH_TO_JOURNAL_NOTE));
            case "game":
                return new GameHandler().Init(model, null);
            case "pomo":
            case "pomodoro":
                return new PomodoroHandler().Init(model, null);
            default:
                break;
        }
        return null;
    }
    protected override CommandHandlerBase GetSpecializedCommandHandler(Jarvis.JModel model, out List<string> argumentsForSpecializedHandler, bool printErrors)
    {
        // @todo printErrors is not being used 
        string command = arguments_ReadOnly != null && arguments_ReadOnly.Count > 0 ? arguments_ReadOnly[0] : null;
        CommandHandlerBase selectedHander = GetCommandHandler(command);
        //bool help = optionalArguments.Contains("--help");


        if (selectedHander != null) // If the command is provided 
        {
            model.UserData.SetCommandUsed(command);
            argumentsForSpecializedHandler = new List<string>(arguments_ReadOnly);
            argumentsForSpecializedHandler.RemoveAt(0);     // stripping the command from the arguments before sending it along
        }
        else
        {   
            selectedHander = GetCommandHandler(model.UserData.GetLastCommand());
            argumentsForSpecializedHandler = new List<string>(arguments_ReadOnly);  // since the command is not provided anyway, there is nothing to strip
        }

        return selectedHander;
    }

    protected override bool ShowHelp()
    {
        SharedLogic.StartCachingHelpText();
        SharedLogic.PrintHelp_Heading("USAGE");
        SharedLogic.PrintHelp_SubText(">task <arguments>", "To manage your tasks. try 'task --help' for more information");
        SharedLogic.PrintHelp_SubText(">habit <arguments>", "To manage your habits. try 'habit --help' for more information");
        SharedLogic.PrintHelp_SubText(">notebook <arguments>", "To manage your notebook. 'notebook --help' for more information");
        SharedLogic.PrintHelp_SubText(">pomo <arguments>", "To manage your pomodoro timers. 'pomo --help' for more information");
        SharedLogic.PrintHelp_SubText(">game snake", "For a fun game :) ");

        SharedLogic.PrintHelp_Heading("TIPS");
        SharedLogic.PrintHelp_SubText("If you add Jarvis to your system path, you can access jarvis from anywhere in the command prompt ( or terminal )");

        SharedLogic.PrintHelp_Heading("Designed by Vijay Veluri!");

        SharedLogic.FlushHelpText();

        return true;
    }

    protected override bool Run()
    {
        ShowHelp();
        return false;
    }
}
