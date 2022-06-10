﻿using System;
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
        SharedLogic.PrintHelp("USAGE");
        SharedLogic.PrintHelp("Jarvis journal add ", "To add a new journal entry");
        SharedLogic.PrintHelp("Jarvis journal list", "To list all the journal entries");
        SharedLogic.PrintHelp("Jarvis journal show", "Show more details of an entry");

        SharedLogic.PrintHelp("\nADVANCED"); 
        SharedLogic.PrintHelp("Jarvis journal edittitle ", "To edit the title of an entry");

        SharedLogic.PrintHelp("\nNOTES"); 
        SharedLogic.PrintHelp("Jarvis journal note" , "Open journal entry");
        SharedLogic.PrintHelp("Jarvis journal printnote", "Print the journal entry");
        SharedLogic.PrintHelp("Jarvis journal cat", "Same as printnotes");

        SharedLogic.PrintHelp("\nHELP");
        SharedLogic.PrintHelp("All the commands have their own help section. Use the argument '--help'");
        SharedLogic.PrintHelp("Example - 'jarvis journal add --help' for more examples on how to use it. Try it!");
        SharedLogic.PrintHelp("This works for every single command! Cheers!");

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
        ConsoleWriter.Print("Invalid arguments! \n");
        ShowHelp();
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
        SharedLogic.PrintHelp("USAGE");
        SharedLogic.PrintHelp("Jarvis journal add <title>", "This will create a new journal entry. use 'Jarvis journal note' command to open and edit it!");

        SharedLogic.PrintHelp("\nEXAMPLES");
        SharedLogic.PrintHelp("jarvis journal add \"Stuff that i did today!\"", "This will create a journal entry. Use jarvis journal note <id> to open and edit it" );

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

        ConsoleWriter.Print("New Journal entry added with id : {0}. You can open/edit the notes using 'jarvis journal note'", entry.id);
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
        SharedLogic.PrintHelp("USAGE");
        SharedLogic.PrintHelp("Jarvis journal list", "lists all the entries\n" );

        SharedLogic.PrintHelp("\nEXAMPLES");
        SharedLogic.PrintHelp("jarvis journal list");

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
            ConsoleWriter.Print("No journals found! Try adding a few using \"jarvis journal add\"");

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
        SharedLogic.PrintHelp("USAGE");
        SharedLogic.PrintHelp("Jarvis journal show <id>", "This will show all the details of a journal!");

        SharedLogic.PrintHelp("\nEXAMPLES");
        SharedLogic.PrintHelp("jarvis journal show 1", "Shows more details for journal with id 1");
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

public class JournalEditTitleCommand : CommandHandlerBase
{
    public JournalEditTitleCommand()
    {
    }

    protected override bool ShowHelp()
    {
        SharedLogic.PrintHelp("USAGE");
        SharedLogic.PrintHelp("Jarvis journal edittitle <id> <new title>", "Renames the entry title");

        SharedLogic.PrintHelp("\nEXAMPLES");
        SharedLogic.PrintHelp("jarvis journal edittitle 1 \"Wake up at 7 AM\"", "rename the title of journal : 1 to 'Wake up at 7 AM'");
        return true;
    }
    protected override bool Run(Jarvis.JApplication application)
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

public class JournalCatNotesCommand : CommandHandlerBase
{
    public JournalCatNotesCommand()
    {

    }

    protected override bool ShowHelp()
    {
        SharedLogic.PrintHelp("USAGE");
        SharedLogic.PrintHelp("Jarvis journal cat <journalID>", "Prints the notes of a journal entry. You can also use printnote instead of cat");
        SharedLogic.PrintHelp("Jarvis journal printnote <journalID>", "Same as cat");

        SharedLogic.PrintHelp("\nEXAMPLES");
        SharedLogic.PrintHelp("jarvis journal cat 1", "Prints the notes for journal with id 1");
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
            if( !Utils.FileHandler.DoesFileExist(JConstants.PATH_TO_JOURNAL_NOTE + id) )
                ConsoleWriter.Print("Notes not found for the journal with id : {0}", id);
            else 
            {
                ConsoleWriter.PrintInColor("NOTES :", application.DesignData.HighlightColorForText);
                ConsoleWriter.PrintText(Utils.FileHandler.Read(JConstants.PATH_TO_JOURNAL_NOTE + id));
            }
        }
        else
            ConsoleWriter.Print("journal not found with id : " + id);

        return true;
    }
}

public class JournalEditNoteCommand : CommandHandlerBase
{
    public JournalEditNoteCommand()
    {

    }

    protected override bool ShowHelp()
    {
        SharedLogic.PrintHelp("USAGE");
        SharedLogic.PrintHelp("Jarvis journal note <journalID>", "Opens notes for a journal");
        SharedLogic.PrintHelp("Jarvis journal note <journalID> --ext:<editorname>", "Provide external editor name of your choice. Example : code or vim");
        SharedLogic.PrintHelp("Jarvis journal note <journalID> --append:<Message>", "Append the message directly to the note");
        SharedLogic.PrintHelp("Jarvis journal note <journalID> --appendlog:<Message>", "Append the message directly to the note, with a timestamp!");

        SharedLogic.PrintHelp("\nMORE INFO :");
        SharedLogic.PrintHelp("You can change the default editor (to open the notes) in the DesignData.json under 'defaultExternalEditor'");

        SharedLogic.PrintHelp("\nEXAMPLES");
        SharedLogic.PrintHelp("jarvis journal note 1", "Edit the notes for journal : 1");
        SharedLogic.PrintHelp("jarvis journal note 1 --ext:code", "Edit the notes for journal : 1, within the visual studio code");
        SharedLogic.PrintHelp("jarvis journal note 1 --append:\"Buy milk\"", "Add 'buy milk' to the notes!");
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
            if( !Utils.FileHandler.DoesFileExist(JConstants.PATH_TO_JOURNAL_NOTE + id) )
            {
                Utils.FileHandler.Create(JConstants.PATH_TO_JOURNAL_NOTE + id);
                ConsoleWriter.Print("New note created");
            }

            if ( !appendMessage.IsEmpty() )
            {
                ConsoleWriter.Print("Message appended to the notes");
                Utils.AppendToFile(JConstants.PATH_TO_JOURNAL_NOTE + id, appendMessage );
            }
            else 
            {
                ConsoleWriter.Print("Opening Notes");
                Utils.OpenAFileInEditor(
                    JConstants.PATH_TO_JOURNAL_NOTE + id, 
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
