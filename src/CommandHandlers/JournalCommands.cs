using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommandLine;
using Jarvis; //@todo 


public class JournalHandler : CommandHandlerBaseWithUtility
{
    public JournalHandler()
    {

    }

    protected override bool ShowHelp()
    {
        SharedLogic.StartCachingHelpText();
        SharedLogic.PrintHelp("USAGE");
        SharedLogic.PrintHelp("  >journal add ", "To add a new journal entry");
        SharedLogic.PrintHelp("  >journal list", "To list all the journal entries");
        SharedLogic.PrintHelp("  >journal show", "Show more details of an entry");

        SharedLogic.PrintHelp("\nADVANCED"); 
        SharedLogic.PrintHelp("  >journal edittitle ", "To edit the title of an entry");

        SharedLogic.PrintHelp("\nNOTES"); 
        SharedLogic.PrintHelp("  >journal note" , "Open journal entry");
        SharedLogic.PrintHelp("  >journal printnote", "Print the journal entry");
        SharedLogic.PrintHelp("  >journal cat", "Same as printnotes");

        SharedLogic.PrintHelp("\nHELP");
        SharedLogic.PrintHelp("All the commands have their own help section. Use the argument '--help'");
        SharedLogic.PrintHelp("Example - 'journal add --help' for more examples on how to use it. Try it!");
        SharedLogic.PrintHelp("This works for every single command! Cheers!");
        SharedLogic.FlushHelpText();
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
            case "printnote":
                selectedHander = new JournalCatNotesCommand();
                break;
            case "note":
                selectedHander = new JournalEditNoteCommand();
                break;
            case "edittitle":
                selectedHander = new JournalEditTitleCommand();
                break;
            default:
                if(printErrors)
                    ConsoleWriter.Print("Invalid command. Try 'journal --help' for more information");
                break;
        }

        if (selectedHander != null)
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
        ConsoleWriter.Print("Invalid arguments! \n");
        ShowHelp();
        return true;
    }
}

public class JournalAddCommand : CommandHandlerBaseWithUtility
{
    public JournalAddCommand()
    {

    }

    protected override bool ShowHelp()
    {
        SharedLogic.StartCachingHelpText();
        SharedLogic.PrintHelp("USAGE");
        SharedLogic.PrintHelp("  >journal add <title>", "This will create a new journal entry. use 'journal note' command to open and edit it!");

        SharedLogic.PrintHelp("\nEXAMPLES");
        SharedLogic.PrintHelp("  >journal add \"Stuff that i did today!\"", "This will create a journal entry. Use jarvis journal note <id> to open and edit it" );
        SharedLogic.FlushHelpText();

        return true;
    }
    protected override bool Run()
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

        ConsoleWriter.Print("New Journal entry added with id : {0}. You can open/edit the notes using 'journal note'", entry.id);
        return true;
    }
}

public class JournalListCommand : CommandHandlerBaseWithUtility
{
    public JournalListCommand()
    {

    }
    protected override bool ShowHelp()
    {
        SharedLogic.StartCachingHelpText();
        SharedLogic.PrintHelp("USAGE");
        SharedLogic.PrintHelp("  >journal list", "lists all the entries\n" );

        SharedLogic.PrintHelp("\nEXAMPLES");
        SharedLogic.PrintHelp("  >journal list");
        SharedLogic.FlushHelpText();

        return true;
    }

    protected override bool Run()
    {
        if (arguments_ReadOnly.Count != 0)
        {
            ConsoleWriter.Print("Invalid arguments! \n");
            ShowHelp();
            return true;
        }

        List<JournalEntry> journals = new List<JournalEntry>(application.journalManager.Data.entries);

        int lineCount = 0;
        int titleArea = (optionalArguments_ReadOnly.Contains("-e") || optionalArguments_ReadOnly.Contains("--expand")) ? 120 : 40;
        const int newLineAfter = 5;

        // output Heading 
        if (journals.Count > 0)
        {
            ConsoleWriter.PrintInColor("{0, -4} {1,-" + titleArea + "} {2, -15}",
                application.DesignData.HighlightColorForText,
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
                if (lineCount % newLineAfter == 0)
                    ConsoleWriter.Print();
            }
        }
        else
            ConsoleWriter.Print("No journals found! Try adding a few using \"journal add\"");

        return true;
    }
}


public class JournalShowCommand : CommandHandlerBaseWithUtility
{
    public JournalShowCommand()
    {
    }

    protected override bool ShowHelp()
    {
        SharedLogic.StartCachingHelpText();
        SharedLogic.PrintHelp("USAGE");
        SharedLogic.PrintHelp("  >journal show <id>", "This will show all the details of a journal!");

        SharedLogic.PrintHelp("\nEXAMPLES");
        SharedLogic.PrintHelp("  >journal show 1", "Shows more details for journal with id 1");
        SharedLogic.FlushHelpText();
        return true;
    }
    protected override bool Run()
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

        // Print journal details
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

public class JournalEditTitleCommand : CommandHandlerBaseWithUtility
{
    public JournalEditTitleCommand()
    {
    }

    protected override bool ShowHelp()
    {
        SharedLogic.StartCachingHelpText();
        SharedLogic.PrintHelp("USAGE");
        SharedLogic.PrintHelp("  >journal edittitle <id> <new title>", "Renames the entry title");

        SharedLogic.PrintHelp("\nEXAMPLES");
        SharedLogic.PrintHelp("  >journal edittitle 1 \"Wake up at 7 AM\"", "rename the title of journal : 1 to 'Wake up at 7 AM'");
        SharedLogic.FlushHelpText();
        return true;
    }
    protected override bool Run()
    {
        if (arguments_ReadOnly.Count != 2)
        {
            ConsoleWriter.Print("Invalid arguments! \n");
            ShowHelp();
            return true;
        }

        int id = Utils.Conversions.Atoi(arguments_ReadOnly[0]);
        string title = arguments_ReadOnly[1];

        JournalEntry hb = application.journalManager.GetJournal_ReadOnly(id);

        if (hb == null)
        {
            ConsoleWriter.Print("Journal with id : {0} not found!", id);
            return true;
        }

        application.journalManager.GetJournal_Editable(id).title = title;
        ConsoleWriter.Print("Journal with id : {0} renamed to - {1}", id, title);

        return true;
    }
}

public class JournalCatNotesCommand : CommandHandlerBaseWithUtility
{
    public JournalCatNotesCommand()
    {

    }

    protected override bool ShowHelp()
    {
        SharedLogic.StartCachingHelpText();
        SharedLogic.PrintHelp("USAGE");
        SharedLogic.PrintHelp("  >journal cat <journalID>", "Prints the notes of a journal entry. You can also use printnote instead of cat");
        SharedLogic.PrintHelp("  >journal printnote <journalID>", "Same as cat");

        SharedLogic.PrintHelp("\nEXAMPLES");
        SharedLogic.PrintHelp("  >journal cat 1", "Prints the notes for journal with id 1");
        SharedLogic.FlushHelpText();
        return true;
    }

    protected override bool Run()
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
            if( !notes.DoesNoteExist(id))
                ConsoleWriter.Print("Notes not found for the journal with id : {0}", id);
            else 
            {
                ConsoleWriter.PrintInColor("NOTES :", application.DesignData.HighlightColorForText);
                ConsoleWriter.PrintText(notes.GetNoteContent(id));
            }
        }
        else
            ConsoleWriter.Print("journal not found with id : " + id);

        return true;
    }
}

public class JournalEditNoteCommand : CommandHandlerBaseWithUtility
{
    public JournalEditNoteCommand()
    {

    }

    protected override bool ShowHelp()
    {
        SharedLogic.StartCachingHelpText();
        SharedLogic.PrintHelp("USAGE");
        SharedLogic.PrintHelp("  >journal note <journalID>", "Opens notes for a journal");
        SharedLogic.PrintHelp("  >journal note <journalID> --ext:<editorname>", "Provide external editor name of your choice. Example : code or vim");
        SharedLogic.PrintHelp("  >journal note <journalID> --append:<Message>", "Append the message directly to the note");
        SharedLogic.PrintHelp("  >journal note <journalID> --appendlog:<Message>", "Append the message directly to the note, with a timestamp!");

        SharedLogic.PrintHelp("\nADVANCED :");
        SharedLogic.PrintHelp("You can change the default editor (to open the notes) in the Data/Design.json under 'defaultExternalEditor'");
        SharedLogic.PrintHelp("you can use '--nowait' to have jarvis not wait for the notes to be closed.");

        SharedLogic.PrintHelp("\nEXAMPLES");
        SharedLogic.PrintHelp("  >journal note 1", "Edit the notes for journal : 1");
        SharedLogic.PrintHelp("  >journal note 1 --ext:code", "Edit the notes for journal : 1, within the visual studio code");
        SharedLogic.PrintHelp("  >journal note 1 --append:\"Buy milk\"", "Add 'buy milk' to the notes!");
        SharedLogic.FlushHelpText();
        return true;
    }

    protected override bool Run()
    {
        if (arguments_ReadOnly.Count != 1)
        {
            ConsoleWriter.Print("Invalid arguments! \n");
            ShowHelp();
            return true;
        }

        int id = Utils.Conversions.Atoi(arguments_ReadOnly[0]);
        bool syntaxError = false;
        bool waitForTheProgramToEnd = !optionalArguments_ReadOnly.Contains("--nowait");


        string externalProgram = Utils.CLI.ExtractStringFromCLIParameter(optionalArguments_ReadOnly, "--ext", string.Empty, null, null, out syntaxError );

        if ( syntaxError ) 
        {
            ConsoleWriter.Print("Invalid syntax for --ext argument."); 
            return true;
        }

        syntaxError = false;
        string appendMessage = Utils.CLI.ExtractStringFromCLIParameter(optionalArguments_ReadOnly, "--append", string.Empty, null, null, out syntaxError);

        if (syntaxError)
        {
            ConsoleWriter.Print("Invalid syntax for --append argument.");
            return true;
        }

        string appendLogMessage = Utils.CLI.ExtractStringFromCLIParameter(optionalArguments_ReadOnly, "--appendlog", string.Empty, null, null, out syntaxError);

        if (syntaxError)
        {
            ConsoleWriter.Print("Invalid syntax for --appendlog argument.");
            return true;
        }

        if ( !appendLogMessage.IsEmpty() )
        {
            appendMessage = "Log on " +  DateTime.Now.ToShortDateString() + " " + appendLogMessage;
        }

        if (application.journalManager.DoesJournalExist(id))
        {
            notes.CreateNoteIfUnavailable(id);

            if (!appendMessage.IsEmpty())
            {
                ConsoleWriter.Print("Message appended to the notes");
                notes.AppendToNote(id, appendMessage);
            }
            else
            {
                notes.OpenNote(application, id, externalProgram, waitForTheProgramToEnd, true);
            }
        }
        else
            ConsoleWriter.Print("journal not found with id : " + id);
        return true;
    }
}
