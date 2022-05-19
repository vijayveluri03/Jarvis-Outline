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

        List<string> argumentsForSpecializedHandler;
        CommandHandlerBase command = GetSpecializedCommandHandler(application, out argumentsForSpecializedHandler);
        if (command != null)
        {
            return command.TryHandle( argumentsForSpecializedHandler, optionalArguments, application );
        }

        bool help = optionalArguments.Contains("--help");

        if (help)
            return ShowHelp();
        else
            return Run(application);
    }

    protected virtual CommandHandlerBase GetSpecializedCommandHandler(Jarvis.JApplication application, out List<string> argumentsForSpecializedHandler )
    {
        argumentsForSpecializedHandler = null;
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
