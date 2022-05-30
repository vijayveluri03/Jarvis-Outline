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
        {   // command not provided. We are trying to see if we can use the last used command. 

            if ( printErrors )
                ConsoleWriter.Print("Invalid command. Try 'jarvis --help' for more information");
            //selectedHander = GetCommandHandler(application.UserData.GetLastCommand());
            argumentsForSpecializedHandler = new List<string>(arguments_ReadOnly);  // since the command is not provided anyway, there is nothing to strip
        }

        return selectedHander;
    }

    protected override bool ShowHelp()
    {
        ConsoleWriter.Print("USAGE : \n" +
            "Jarvis task <arguments> \t\t| For more info on arguments, try 'Jarvis task' or 'Jarvis task --help'\n" +
            "Jarvis habit <arguments> \t\t| For more info on arguments, try 'Jarvis habit' or 'Jarvis habit --help'\n" +
            "Jarvis journal <arguments> \t\t| For more info on arguments, try 'Jarvis journal' or 'Jarvis journal --help'\n" +
            "Jarvis game snake \t\t| For a fun game :) "
            );
        return true;
    }

    protected override bool Run(Jarvis.JApplication application)
    {
        return false;
    }
}
