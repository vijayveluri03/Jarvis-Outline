using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommandLine;
using Jarvis; //@todo 


public class HabitHandler : ICommand
{
    public HabitHandler()
    {

    }

    public override bool Run(List<string> arguments, List<string> optionalArguments, Jarvis.JApplication application)
    {

        if (arguments.Count < 1)
        {
            ConsoleWriter.Print("Invalid arguments! \n");
            ConsoleWriter.Print("USAGE : \n" +
                "Jarvis habit add  // To add a task\n" +
                "Jarvis habit done // to list all the tasks\n" +
                "Jarvis habit reset // to list all the tasks\n" +
                "Jarvis habit list // to list all the tasks\n");

            return false;
        }

        string action = arguments[0];
        ICommand selectedHander = null;

        switch (action)
        {
            case "add":
                selectedHander = null;
                ConsoleWriter.Print("NYI"); // @todo
                break;
            case "done":
                selectedHander = null;
                ConsoleWriter.Print("NYI"); // @todo
                break;
            case "list":
                selectedHander = null;
                ConsoleWriter.Print("NYI"); // @todo
                break;
            case "reset":
                selectedHander = null;
                ConsoleWriter.Print("NYI"); // @todo
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