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

    public override bool Run(List<string> command, Jarvis.JApplication application)
    {
        if (command.Count < 1)
        {
            Console.Out.WriteLine("Invalid parameters! Append the Jarvis command with sub-commands like task, habit etc");
            return false;
        }

        string task = command[0];
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
            command.RemoveAt(0);
            selectedHander.Run(command, application);
        }
        return true;
    }
}
