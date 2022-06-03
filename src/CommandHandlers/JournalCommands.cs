using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommandLine;
using Jarvis; //@todo 


public class JournalHandler : CommandHandlerBase
{
    public JournalHandler()
    {

    }

    protected override bool ShowHelp()
    {
        ConsoleWriter.Print("USAGE : \n" +
        "Jarvis journal add  \t\t| To add a journal\n" +
        "Jarvis journal list \t\t| to list all the journals\n" +
        "jarvis journal show \t\t| to show details of a journal\n" +
        "\n" +
        "NOTES\n" + 
        "jarvis journal editnotes" + "\t\t| open notes for a journal. If the notes doesnt exit, try addnotes first\n" +
        "jarvis journal printnotes" + "\t\t| print the notes. ( you can also use cat instead of printnotes)\n"

        );

        return true;
    }

    protected override CommandHandlerBase GetSpecializedCommandHandler(Jarvis.JApplication application, out List<string> argumentsForSpecializedHandler, bool printErrors)
    {
        string action = arguments_ReadOnly != null && arguments_ReadOnly.Count > 0 ? arguments_ReadOnly[0] : null;
        CommandHandlerBase selectedHander = null;

        switch (action)
        {
            case "add":
                selectedHander = new JournalAddCommand();
                break;
            case "list":
                selectedHander = new JournalListCommand();
                break;
            case "show":
                selectedHander = new JournalShowCommand();
                break;
            case "cat":
            case "printnotes":
                selectedHander = new JournalCatNotesCommand();
                break;
            case "editnotes":
                selectedHander = new JournalEditNotesCommand();
                break;
            default:
                if(printErrors)
                    ConsoleWriter.Print("Invalid command. Try 'jarvis journal --help' for more information");
                break;
        }

        if (selectedHander != null)
        {
            argumentsForSpecializedHandler = new List<string>(arguments_ReadOnly);
            argumentsForSpecializedHandler.RemoveAt(0);
        }
        else
            argumentsForSpecializedHandler = null;

        return selectedHander;
    }

    protected override bool Run(Jarvis.JApplication application)
    {

        if (arguments_ReadOnly.Count < 1)
        {
            ConsoleWriter.Print("Invalid arguments! \n");
            ShowHelp();

            return false;
        }

        Utils.Assert(false, "Shouldnt be here");
        return true;
    }
}

public class JournalAddCommand : CommandHandlerBase
{
    public JournalAddCommand()
    {

    }

    protected override bool ShowHelp()
    {
        ConsoleWriter.Print("USAGE : \n" +
                "jarvis journal add <title> \t\t| this will create a new journal entry. You can add to that entry using 'editnotes'" 
                );
        return true;
    }
    protected override bool Run(Jarvis.JApplication application)
    {
        if (arguments_ReadOnly.Count != 1)
        {
            ConsoleWriter.Print("Invalid arguments! \n");
            ShowHelp();
            return true;
        }

        string title = arguments_ReadOnly[0];

        var entry = SharedLogic.CreateNewJournalEntry(application.journalManager, title);
        application.journalManager.AddJournal(entry);

        Utils.FileHandler.Create(JConstants.PATH_TO_JOURNAL_NOTES + entry.id);

        ConsoleWriter.Print("New Journal added with id : {0}. You can open the notes and fill it up. ", entry.id);
        return true;
    }
}

public class JournalListCommand : CommandHandlerBase
{
    public JournalListCommand()
    {

    }
    protected override bool ShowHelp()
    {
        ConsoleWriter.Print("USAGE : \n" +
                "jarvis journal list   // lists all the journals\n" 
                

                //"jarvis journal list --detailed // show more details \n"
                );
        return true;
    }

    protected override bool Run(Jarvis.JApplication application)
    {
        if (arguments_ReadOnly.Count != 0)
        {
            ConsoleWriter.Print("Invalid arguments! \n");
            ShowHelp();
            return true;
        }

        List<JournalEntry> journals = new List<JournalEntry>(application.journalManager.Data.entries);

        int lineCount = 0;
        int titleArea = 40;


        // output Heading 
        if (journals.Count > 0)
        {
            ConsoleWriter.Print("{0, -4} {1,-" + titleArea + "} {2, -15}",
                "ID", "TITLE", "LOGGED ON"
                );

            foreach (var journal in journals)
            {

                ConsoleWriter.Print("{0, -4} {1,-" + titleArea + "} {2, -15}",
                    journal.id,
                    journal.title.TruncateWithVisualFeedback(titleArea - 7/*for the ...*/) + "+(N)",// because Notes is available for all journals 
                    journal.loggedDate.ShortForm()
                    );

                lineCount++;

                //@todo
                if (lineCount % 5 == 0)
                    ConsoleWriter.Print();
            }
        }
        else
            ConsoleWriter.Print("No journals found! Try adding a few using \"jarvis add\"");

        return true;
    }
}


public class JournalShowCommand : CommandHandlerBase
{
    public JournalShowCommand()
    {
    }

    protected override bool ShowHelp()
    {
        ConsoleWriter.Print("USAGE : \n" +
                "jarvis journal show <id> // This will show all the details of a journal!"
                );
        return true;
    }
    protected override bool Run(Jarvis.JApplication application)
    {
        if (arguments_ReadOnly.Count != 1)
        {
            ConsoleWriter.Print("Invalid arguments! \n");
            ShowHelp();
            return true;
        }

        int id = Utils.Conversions.Atoi(arguments_ReadOnly[0]);

        JournalEntry hb = application.journalManager.GetJournal_ReadOnly(id);

        if (hb == null)
        {
            ConsoleWriter.Print("Journal with id : {0} not found!", id);
            return true;
        }

        // Print Task details
        {
            // Heading
            ConsoleWriter.PrintInColor("{0, -4} {1}",
                application.DesignData.HighlightColorForText,
                "ID", "TITLE"
                );

            ConsoleWriter.Print("{0, -4} {1}",
                hb.id,
                hb.title);

            ConsoleWriter.Print();

            ConsoleWriter.Print(
                "LOGGED ON : {1}\n", 
                hb.loggedDate.ShortForm()
                );
        }

        ConsoleWriter.Print();

        return true;
    }
}

public class JournalCatNotesCommand : CommandHandlerBase
{
    public JournalCatNotesCommand()
    {

    }

    protected override bool ShowHelp()
    {
        ConsoleWriter.Print("USAGE : \n" +
                "jarvis journal cat <journalID> \t\t| Prints the notes of a journal. You can also use printnotes instead of cat\n" +
                "jarvis journal printnotes <journalID> \t\t| Same as cat\n"
                );
        return true;
    }

    protected override bool Run(Jarvis.JApplication application)
    {
        if (arguments_ReadOnly.Count != 1)
        {
            ConsoleWriter.Print("Invalid arguments! \n");
            ShowHelp();
            return true;
        }

        int id = Utils.Conversions.Atoi(arguments_ReadOnly[0]);

        
        if (application.journalManager.DoesJournalExist(id))
        {
            if( !Utils.FileHandler.DoesFileExist(JConstants.PATH_TO_JOURNAL_NOTES + id) )
                ConsoleWriter.Print("Notes not found for the journal with id : {0}", id);
            else 
            {
                ConsoleWriter.PrintInColor("NOTES :", application.DesignData.HighlightColorForText);
                ConsoleWriter.PrintText(Utils.FileHandler.Read(JConstants.PATH_TO_JOURNAL_NOTES + id));
            }
        }
        else
            ConsoleWriter.Print("journal not found with id : " + id);

        return true;
    }
}

public class JournalEditNotesCommand : CommandHandlerBase
{
    public JournalEditNotesCommand()
    {

    }

    protected override bool ShowHelp()
    {
        ConsoleWriter.Print("USAGE : \n" +
                "jarvis journal editnotes <journalID> \t\t| Opens notes for a journal. If notes doesnt exist, you might want to try addnotes first!\n" + 
                "jarvis journal editnotes <journalID> --ext:<editorname> \t\t| provide external editor program name of your choice. Example : code or vim\n" + 
                "You can change the default editor in the DesignData.json under 'defaultExternalEditor'\n"
                );
        return true;
    }

    protected override bool Run(Jarvis.JApplication application)
    {
        if (arguments_ReadOnly.Count != 1)
        {
            ConsoleWriter.Print("Invalid arguments! \n");
            ShowHelp();
            return true;
        }

        int id = Utils.Conversions.Atoi(arguments_ReadOnly[0]);
        bool syntaxError = false;
        string externalProgram = Utils.CLI.ExtractStringFromCLIParameter(optionalArguments_ReadOnly, "--ext", string.Empty, null, null, out syntaxError );

        if ( syntaxError ) 
        {
            ConsoleWriter.Print("Invalid syntax for --ext argument."); 
            return true;
        }

        if (application.journalManager.DoesJournalExist(id))
        {
            if( !Utils.FileHandler.DoesFileExist(JConstants.PATH_TO_JOURNAL_NOTES + id) )
                ConsoleWriter.Print("Notes not found for the journal with id : {0}", id);
            else 
            {
                ConsoleWriter.Print("Opening Notes");
                Utils.OpenAFileInEditor(
                    JConstants.PATH_TO_JOURNAL_NOTES + id, 
                    externalProgram.IsEmpty() ? application.DesignData.defaultExternalEditor : externalProgram,
                    true /* wait for the program to end*/);
                ConsoleWriter.Print("Closing Notes");
            }
        }
        else
            ConsoleWriter.Print("journal not found with id : " + id);
        return true;
    }
}
