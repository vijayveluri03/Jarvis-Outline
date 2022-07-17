using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jarvis; //@todo 


public abstract class CommandHandlerBase
{
    public bool TryHandle(List<string> arguments, List<string> optionalArguments, Jarvis.JModel application)
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
            return Run();
    }

    protected virtual CommandHandlerBase GetSpecializedCommandHandler(Jarvis.JModel application, out List<string> argumentsForSpecializedHandler, bool printErrors )
    {
        argumentsForSpecializedHandler = null;
        return null;
    }

    // Returns true if a request is handled. else move on to the next element in the chain of responsibility
    protected abstract bool ShowHelp();

    // Returns true if a request is handled. else move on to the next element in the chain of responsibility
    protected abstract bool Run();

    protected List<string> arguments_ReadOnly;
    protected List<string> optionalArguments_ReadOnly;
}

public abstract class CommandHandlerBaseWithUtility : CommandHandlerBase
{
    public CommandHandlerBase Init ( JModel application, NotesUtility notes )
    {
        this.application = application;
        this.notes = notes;
        return this;
    }

    protected NotesUtility notes;
    protected JModel application; // @todo, unnecessary interlinking
}
