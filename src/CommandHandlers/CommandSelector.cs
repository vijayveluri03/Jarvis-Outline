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

    protected override CommandHandlerBase GetSpecializedCommandHandler()
    {
        string task = arguments_ReadOnly != null && arguments_ReadOnly.Count > 0 ? arguments_ReadOnly[0] : null;
        CommandHandlerBase selectedHander = null;

        switch (task)
        {
            case "task":
                selectedHander = new TaskHandler();
                break;
            case "habit":
                selectedHander = new HabitHandler();
                break;
            default:
                break;
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
        if (arguments_ReadOnly.Count < 1)
        {
            ConsoleWriter.Print("Invalid arguments! \n");
            ShowHelp();
            return true;
        }
        return true;
    }
}
