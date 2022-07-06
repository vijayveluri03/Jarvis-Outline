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
        SharedLogic.PrintHelp_Heading("USAGE");
        SharedLogic.PrintHelp_SubText(">journal add ", "To add a new journal entry");
        SharedLogic.PrintHelp_SubText(">journal list", "To list all the journal entries");
        SharedLogic.PrintHelp_SubText(">journal show", "Show more details of an entry");

        SharedLogic.PrintHelp_Heading("ADVANCED"); 
        SharedLogic.PrintHelp_SubText(">journal edittitle ", "To edit the title of an entry");

        SharedLogic.PrintHelp_Heading("NOTES"); 
        SharedLogic.PrintHelp_SubText(">journal note" , "Open journal entry");
        SharedLogic.PrintHelp_SubText(">journal printnote", "Print the journal entry");
        SharedLogic.PrintHelp_SubText(">journal cat", "Same as printnotes");

        SharedLogic.PrintHelp_Heading("HELP");
        SharedLogic.PrintHelp_SubText("All the commands have their own help section. Use the argument '--help'");
        SharedLogic.PrintHelp_SubText("Example - 'journal add --help' for more examples on how to use it. Try it!");
        SharedLogic.PrintHelp_SubText("This works for every single command! Cheers!");
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
        SharedLogic.PrintHelp_Heading("USAGE");
        SharedLogic.PrintHelp_SubText(">journal add <title>", "This will create a new journal entry. ");
        SharedLogic.PrintHelp_SubText(">journal add <tags> <title>", "This will create a new journal entry. ");


        SharedLogic.PrintHelp_Heading("EXAMPLES");
        SharedLogic.PrintHelp_SubText(">journal add \"Stuff that i did today!\"", "This will create a journal entry." );
        SharedLogic.PrintHelp_SubText(">journal add \"dailylog\" \"Stuff that i did today!\"", "This will create a journal entry with a tag DailyLog. ");

        SharedLogic.PrintHelp_WithHeadingAndSubText("TAGS", application.DesignData.journal.listOfTags, "you can use these tags below, or create more in data/Design.json");
      
        SharedLogic.FlushHelpText();

        return true;
    }
    protected override bool Run()
    {
        if (arguments_ReadOnly.Count != 2 && arguments_ReadOnly.Count != 1 )
        {
            ConsoleWriter.Print("Invalid arguments! \n");
            ShowHelp();
            return true;
        }

        string title = null;
        string[] tags = null;
        if (arguments_ReadOnly.Count == 1)
        {
            title = arguments_ReadOnly[0];
            tags = new string[] { application.DesignData.JournalDefaultTag };
        }
        else if (arguments_ReadOnly.Count == 2)
        {
            tags = arguments_ReadOnly[0].ToLower().Split(',');
            title = arguments_ReadOnly[1];
        }
        else
            Utils.Assert(false);

        if (!application.DesignData.DoesJournalTagExist(tags))
        {
            ConsoleWriter.Print("Invalid Tag");
            SharedLogic.PrintHelp_WithHeadingAndSubText("TAGS", application.DesignData.journal.listOfTags, "you can use these tags below, or create more in data/Design.json");
            return true;
        }

        var entry = SharedLogic.CreateNewJournalEntry(application.journalManager, tags, title);
        application.journalManager.AddJournal(entry);

        ConsoleWriter.Print("New Journal entry added with id : {0}. You can also edit the notes using 'journal note'", entry.id);

        notes.CreateNoteIfUnavailable(entry.id);
        notes.OpenNote(application, entry.id, null, true, true);

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
        SharedLogic.PrintHelp_Heading("USAGE");
        SharedLogic.PrintHelp_SubText(">journal list", "lists all the entries\n" );

        SharedLogic.PrintHelp_Heading("EXAMPLES");
        SharedLogic.PrintHelp_SubText(">journal list");
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
        int categoryArea = 30;
        int titleArea = (optionalArguments_ReadOnly.Contains("-e") || optionalArguments_ReadOnly.Contains("--expand")) ? 120 : 40;
        const int newLineAfter = 5;

        // output Heading 
        if (journals.Count > 0)
        {
            ConsoleWriter.PrintInColor("{0, -4} {1,-" + categoryArea + "} {2,-" + titleArea + "} {3, -15}",
                application.DesignData.HighlightColorForText,
                "ID", "TAGS", "TITLE", "LOGGED ON"
                );

            foreach (var journal in journals)
            {

                ConsoleWriter.Print("{0, -4} {1,-" + categoryArea + "} {2,-" + titleArea + "} {3, -15}",
                    journal.id,
                    (journal.tags != null && journal.tags.Length > 0 ? Utils.Conversions.ArrayToString(journal.tags, true).TruncateWithVisualFeedback(categoryArea - 3) : "ERROR"),
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
        SharedLogic.PrintHelp_Heading("USAGE");
        SharedLogic.PrintHelp_SubText(">journal show <id>", "This will show all the details of a journal!");

        SharedLogic.PrintHelp_Heading("EXAMPLES");
        SharedLogic.PrintHelp_SubText(">journal show 1", "Shows more details for journal with id 1");
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

        int categoryArea = 45;

        JournalEntry journal = application.journalManager.GetJournal_ReadOnly(id);

        if (journal == null)
        {
            ConsoleWriter.Print("Journal with id : {0} not found!", id);
            return true;
        }

        // Print journal details
        {
            // Heading
            ConsoleWriter.PrintInColor("{0, -4} {1, -" + categoryArea + "} {2}",
                application.DesignData.HighlightColorForText,
                "ID", "TAGS", "TITLE"
                );

            ConsoleWriter.Print("{0, -4} {1, -" + categoryArea + "} {2}",
                journal.id,
                (journal.tags != null && journal.tags.Length > 0 ? Utils.Conversions.ArrayToString(journal.tags, true).TruncateWithVisualFeedback(categoryArea - 3) : "ERROR"),
                journal.title);

            ConsoleWriter.Print();

            ConsoleWriter.Print(
                "LOGGED ON : {0}\n", 
                journal.loggedDate.ShortForm()
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
        SharedLogic.PrintHelp_Heading("USAGE");
        SharedLogic.PrintHelp_SubText(">journal edittitle <id> <new title>", "Renames the entry title");

        SharedLogic.PrintHelp_Heading("EXAMPLES");
        SharedLogic.PrintHelp_SubText(">journal edittitle 1 \"Wake up at 7 AM\"", "rename the title of journal : 1 to 'Wake up at 7 AM'");
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
        SharedLogic.PrintHelp_Heading("USAGE");
        SharedLogic.PrintHelp_SubText(">journal cat <journalID>", "Prints the notes of a journal entry. You can also use printnote instead of cat");
        SharedLogic.PrintHelp_SubText(">journal printnote <journalID>", "Same as cat");

        SharedLogic.PrintHelp_Heading("EXAMPLES");
        SharedLogic.PrintHelp_SubText(">journal cat 1", "Prints the notes for journal with id 1");
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
        SharedLogic.PrintHelp_Heading("USAGE");
        SharedLogic.PrintHelp_SubText(">journal note <journalID>", "Opens notes for a journal");
        SharedLogic.PrintHelp_SubText(">journal note <journalID> --ext:<editorname>", "Provide external editor name of your choice. Example : code or vim");
        SharedLogic.PrintHelp_SubText(">journal note <journalID> --append:<Message>", "Append the message directly to the note");
        SharedLogic.PrintHelp_SubText(">journal note <journalID> --appendlog:<Message>", "Append the message directly to the note, with a timestamp!");

        SharedLogic.PrintHelp_Heading("ADVANCED");
        SharedLogic.PrintHelp_SubText("You can change the default editor (to open the notes) in the Data/Design.json under 'defaultExternalEditor'");
        SharedLogic.PrintHelp_SubText("you can use '--nowait' to have jarvis not wait for the notes to be closed.");

        SharedLogic.PrintHelp_Heading("EXAMPLES");
        SharedLogic.PrintHelp_SubText(">journal note 1", "Edit the notes for journal : 1");
        SharedLogic.PrintHelp_SubText(">journal note 1 --ext:code", "Edit the notes for journal : 1, within the visual studio code");
        SharedLogic.PrintHelp_SubText(">journal note 1 --append:\"Buy milk\"", "Add 'buy milk' to the notes!");
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
