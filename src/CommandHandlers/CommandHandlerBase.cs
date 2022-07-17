using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jarvis; //@todo 


public abstract class CommandHandlerBase
{
    public bool TryHandle(List<string> arguments, List<string> optionalArguments, Jarvis.JModel model)
    {
        this.arguments_ReadOnly = arguments;
        this.optionalArguments_ReadOnly = optionalArguments;
        bool help = optionalArguments.Contains("--help");

        List<string> argumentsForSpecializedHandler;
        CommandHandlerBase command = GetSpecializedCommandHandler(model, out argumentsForSpecializedHandler, !help );
        
        if (command != null)
        {
            return command.TryHandle( argumentsForSpecializedHandler, optionalArguments, model );
        }

        if (help)
            return ShowHelp();
        else
            return Run();
    }

    protected virtual CommandHandlerBase GetSpecializedCommandHandler(Jarvis.JModel model, out List<string> argumentsForSpecializedHandler, bool printErrors )
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
    public CommandHandlerBase Init ( JModel model, NotesUtility notes )
    {
        this.model = model;
        this.notes = notes;
        return this;
    }

    protected NotesUtility notes;
    protected JModel model; // @todo, unnecessary interlinking
}
