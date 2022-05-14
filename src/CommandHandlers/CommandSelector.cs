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
            Console.Out.WriteLine("Invalid arguments! \n");
            Console.Out.WriteLine("USAGE : \n" +
                "Jarvis task <arguments>. For more info on arguments, try \"Jarvis task\"");
            return false;
        }

        string task = arguments[0];
        ICommand selectedHander = null;

        switch (task)
        {
            case "task":
                selectedHander = (new TaskHandler());
                break;
            case "habit":
                Console.Out.WriteLine("NYI");
                break;
            default:
                Console.Out.WriteLine("unknown task");
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
