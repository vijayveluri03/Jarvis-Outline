using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using Jarvis; //@todo 


public class CommandSelector : CommandHandlerBase
{
    public CommandSelector()
    {
    }

    private CommandHandlerBase GetCommandHandler(string command)
    {
        switch (command)
        {
            case "task":
                return new TaskHandler();
            case "habit":
                return new HabitHandler();
            case "journal":
                return new JournalHandler();
            case "game":
                return new GameHandler();
            default:
                break;
        }
        return null;
    }
    protected override CommandHandlerBase GetSpecializedCommandHandler(Jarvis.JApplication application, out List<string> argumentsForSpecializedHandler, bool printErrors)
    {
        // @todo printErrors is not being used 
        string command = arguments_ReadOnly != null && arguments_ReadOnly.Count > 0 ? arguments_ReadOnly[0] : null;
        CommandHandlerBase selectedHander = GetCommandHandler(command);
        //bool help = optionalArguments.Contains("--help");


        if (selectedHander != null) // If the command is provided 
        {
            application.UserData.SetCommandUsed(command);
            argumentsForSpecializedHandler = new List<string>(arguments_ReadOnly);
            argumentsForSpecializedHandler.RemoveAt(0);     // stripping the command from the arguments before sending it along
        }
        else
        {   
            selectedHander = GetCommandHandler(application.UserData.GetLastCommand());
            argumentsForSpecializedHandler = new List<string>(arguments_ReadOnly);  // since the command is not provided anyway, there is nothing to strip
        }

        return selectedHander;
    }

    protected override bool ShowHelp()
    {
        SharedLogic.PrintHelp("USAGE");
        SharedLogic.PrintHelp("Jarvis task <arguments>", "To manage your tasks. try 'Jarvis task' or 'Jarvis task --help' for more information");
        SharedLogic.PrintHelp("Jarvis habit <arguments>", "To manage your habits. try 'Jarvis habit' or 'Jarvis habit --help' for more information");
        SharedLogic.PrintHelp("Jarvis journal <arguments>", "To manage your journal. try 'Jarvis journal' or 'Jarvis journal --help' for more information");
        SharedLogic.PrintHelp("Jarvis game snake", "For a fun game :) ");

        SharedLogic.PrintHelp("\nTIPS");
        SharedLogic.PrintHelp("Try jarvis --enter", "This is a CLI interface built to for jarvis!");
        SharedLogic.PrintHelp("If you add Jarvis to your system path, you can access jarvis from anywhere in the command prompt ( or terminal )");

        SharedLogic.PrintHelp("\nDesigned by Vijay Veluri!");

        return true;
    }

    protected override bool Run(Jarvis.JApplication application)
    {
        ShowHelp();
        return false;
    }
}
