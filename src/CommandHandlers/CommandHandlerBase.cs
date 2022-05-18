using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

public class CommandHandlerBase
{
    public bool TryHandle(List<string> arguments, List<string> optionalArguments, Jarvis.JApplication application)
    {
        this.arguments_ReadOnly = arguments;
        this.optionalArguments_ReadOnly = optionalArguments;

        CommandHandlerBase command = GetSpecializedCommandHandler();
        if (command != null)
        {
            List<string> argumentsNew = new List<string>(arguments);
            argumentsNew.RemoveAt(0);
            return command.TryHandle( argumentsNew, optionalArguments, application );
        }

        bool help = optionalArguments.Contains("--help");

        if (help)
            return ShowHelp();
        else
            return Run(application);
    }

    protected virtual CommandHandlerBase GetSpecializedCommandHandler()
    {
        return null;
    }


    // Returns true if a request is handled. else move on to the next element in the chain of responsibility
    protected virtual bool ShowHelp()
    {
        return false;
    }


    // Returns true if a request is handled. else move on to the next element in the chain of responsibility
    protected virtual bool Run(Jarvis.JApplication application)
    {
        return false;
    }

    protected List<string> arguments_ReadOnly;
    protected List<string> optionalArguments_ReadOnly;
}
