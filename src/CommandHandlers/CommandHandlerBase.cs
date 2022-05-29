using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

public abstract class CommandHandlerBase
{
    public bool TryHandle(List<string> arguments, List<string> optionalArguments, Jarvis.JApplication application)
    {
        this.arguments_ReadOnly = arguments;
        this.optionalArguments_ReadOnly = optionalArguments;
        bool help = optionalArguments.Contains("--help");

        List<string> argumentsForSpecializedHandler;
        CommandHandlerBase command = GetSpecializedCommandHandler(application, out argumentsForSpecializedHandler, !help );
        
        if (command != null)
        {
            return command.TryHandle( argumentsForSpecializedHandler, optionalArguments, application );
        }

        if (help)
            return ShowHelp();
        else
            return Run(application);
    }

    protected virtual CommandHandlerBase GetSpecializedCommandHandler(Jarvis.JApplication application, out List<string> argumentsForSpecializedHandler, bool printErrors )
    {
        argumentsForSpecializedHandler = null;
        return null;
    }

    // Returns true if a request is handled. else move on to the next element in the chain of responsibility
    protected abstract bool ShowHelp();

    // Returns true if a request is handled. else move on to the next element in the chain of responsibility
    protected abstract bool Run(Jarvis.JApplication application);

    protected List<string> arguments_ReadOnly;
    protected List<string> optionalArguments_ReadOnly;
}
