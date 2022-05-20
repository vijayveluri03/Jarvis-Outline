﻿using System;
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
            default:
                break;
        }
        return null;
    }
    protected override CommandHandlerBase GetSpecializedCommandHandler(Jarvis.JApplication application, out List<string> argumentsForSpecializedHandler)
    {
        string command = arguments_ReadOnly != null && arguments_ReadOnly.Count > 0 ? arguments_ReadOnly[0] : null;
        CommandHandlerBase selectedHander = GetCommandHandler(command);

        if (selectedHander != null) // If the command is provided 
        {
            application.UserData.SetCommandUsed(command);
            argumentsForSpecializedHandler = new List<string>(arguments_ReadOnly);
            argumentsForSpecializedHandler.RemoveAt(0);     // stripping the command from the arguments before sending it along
        }
        else
        {   // command not provided. We are trying to see if we can use the last used command. 
            selectedHander = GetCommandHandler(application.UserData.GetLastCommand());
            argumentsForSpecializedHandler = new List<string>(arguments_ReadOnly);  // since the command is not provided anyway, there is nothing to strip
        }

        return selectedHander;
    }

    protected override bool ShowHelp()
    {
        ConsoleWriter.Print("USAGE : \n" +
            "Jarvis task <arguments>. For more info on arguments, try 'Jarvis task' or 'Jarvis task --help'\n" +
            "Jarvis habit <arguments>. For more info on arguments, try 'Jarvis habit' or 'Jarvis habit --help'"
            );
        return true;
    }

    protected override bool Run(Jarvis.JApplication application)
    {
        return false;
    }
}
