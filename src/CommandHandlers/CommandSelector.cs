using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using Jarvis; //@todo 


public class CommandSelector : ICommand
{
    public CommandSelector()
    {

    }

    public override bool Run(List<string> arguments, List<string> optionalArguments, Jarvis.JApplication application )
    {
        if (arguments.Count < 1)
        {
            ConsoleWriter.Print("Invalid arguments! \n");
            ConsoleWriter.Print("USAGE : \n" +
                "Jarvis task <arguments>. For more info on arguments, try \"Jarvis task\"\n" + 
                "Jarvis habit <arguments>. For more info on arguments, try \"Jarvis habit\""
                );
            return false;
        }

        string task = arguments[0];
        ICommand selectedHander = null;

        switch (task)
        {
            case "task":
                selectedHander = new TaskHandler();
                break;
            case "habit":
                selectedHander = new HabitHandler();
                break;
            default:
                ConsoleWriter.Print("unknown task");
                break;
        }

        if (selectedHander != null)
        {
            arguments.RemoveAt(0);
            selectedHander.Run(arguments, optionalArguments,  application);
        }
        return true;
    }
}
