using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommandLine;
using Jarvis; //@todo 


public class GameHandler : CommandHandlerBaseWithUtility
{
    public GameHandler()
    {

    }

    protected override bool ShowHelp()
    {
        SharedLogic.StartCachingHelpText();
        SharedLogic.PrintHelp_Heading("USAGE");
                SharedLogic.PrintHelp_SubText(">game snake ", "To play a simple game\n");
        SharedLogic.FlushHelpText();
        return true;
    }

    protected override CommandHandlerBase GetSpecializedCommandHandler(Jarvis.JApplication application, out List<string> argumentsForSpecializedHandler, bool printErrors)
    {
        string action = arguments_ReadOnly != null && arguments_ReadOnly.Count > 0 ? arguments_ReadOnly[0] : null;
        CommandHandlerBase selectedHander = null;

        switch (action)
        {
            case "snake":
                selectedHander = new HungryEkansCommand();
                break;
            default:
                if(printErrors)
                    ConsoleWriter.Print("Invalid command. Try 'game --help' for more information");
                break;
        }

        if ( selectedHander != null )
        {
            argumentsForSpecializedHandler = new List<string>(arguments_ReadOnly);
            argumentsForSpecializedHandler.RemoveAt(0);

            Utils.Assert(selectedHander is CommandHandlerBaseWithUtility);
            (selectedHander as CommandHandlerBaseWithUtility).Init(application, notes);
        }
        else 
            argumentsForSpecializedHandler = null;

        return selectedHander;
    }

    protected override bool Run()
    {

        if (arguments_ReadOnly.Count < 1)
        {
            ConsoleWriter.Print("Invalid arguments! \n");
            ShowHelp();
            return true;
        }
        
        Utils.Assert(false, "Shouldnt be here");
        return true;
    }
}


public class HungryEkansCommand : CommandHandlerBaseWithUtility
{
    public HungryEkansCommand()
    {

    }

    protected override bool ShowHelp()
    {
        SharedLogic.StartCachingHelpText();
        SharedLogic.PrintHelp_Heading("USAGE");
        SharedLogic.PrintHelp_SubText(">game snake");
        SharedLogic.FlushHelpText();
        return true;
    }

    protected override bool Run()
    {
        ConsoleWriter.Print("Launching Game!");

        Utils.OpenAProgram("Hungry Ekans.exe", string.Empty, true);
        return true;
    }
}

