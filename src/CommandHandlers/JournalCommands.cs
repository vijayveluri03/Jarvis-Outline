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
        ConsoleWriter.Print("USAGE : \n" +
        "Jarvis journal add  \t\t| To add a journal\n" +
        "Jarvis journal list \t\t| to list all the journals\n" +
        "jarvis journal show \t\t| to show details of a journal\n" +
        "Jarvis journal edittitle  \t\t| to edit the title of journal\n" +
        "\n" +
        "NOTES\n" + 
        "jarvis journal editnote" + "\t\t| open notes for a journal. If the notes doesnt exit, try createnote first\n" +
        "jarvis journal printnote" + "\t\t| print the notes. ( you can also use cat instead of printnote)\n"

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
            case "printnote":
                selectedHander = new JournalCatNotesCommand();
                break;
            case "editnote":
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
                "jarvis journal add <title> \t\t| this will create a new journal entry. You can add to that entry using 'editnote'" 
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

        Utils.FileHandler.Create(JConstants.PATH_TO_JOURNAL_NOTE + entry.id);

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

public class JournalEditTitleCommand : CommandHandlerBase
{
    public JournalEditTitleCommand()
    {
    }

    protected override bool ShowHelp()
    {
        ConsoleWriter.Print("USAGE : \n" +
                "jarvis journal edittitle <id> <new title>"
                );
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
        ConsoleWriter.Print("USAGE : \n" +
                "jarvis journal cat <journalID> \t\t| Prints the notes of a journal. You can also use printnote instead of cat\n" +
                "jarvis journal printnote <journalID> \t\t| Same as cat\n"
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
        ConsoleWriter.Print("USAGE : \n" +
                "jarvis journal editnote <journalID> \t\t| Opens notes for a journal. If notes doesnt exist, you might want to try createnote first!\n" + 
                "jarvis journal editnote <journalID> --ext:<editorname> \t\t| provide external editor program name of your choice. Example : code or vim\n" + 
                "jarvis journal editnote <journalID> --append:<Message> \t\t| Added the message directly to the note\n" + 
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

        syntaxError = false;
        string appendMessage = Utils.CLI.ExtractStringFromCLIParameter(optionalArguments_ReadOnly, "--append", string.Empty, null, null, out syntaxError);

        if (syntaxError)
        {
            ConsoleWriter.Print("Invalid syntax for --append argument.");
            return true;
        }

        if (application.journalManager.DoesJournalExist(id))
        {
            if( !Utils.FileHandler.DoesFileExist(JConstants.PATH_TO_JOURNAL_NOTE + id) )
                ConsoleWriter.Print("Notes not found for the journal with id : {0}", id);
            else if ( !appendMessage.IsEmpty() )
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