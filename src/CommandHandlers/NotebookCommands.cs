using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jarvis; //@todo 


public class NotebookHandler : CommandHandlerBaseWithUtility
{
    public NotebookHandler()
    {

    }

    protected override bool ShowHelp()
    {
        SharedLogic.StartCachingHelpText();
        SharedLogic.PrintHelp_Heading("USAGE");
        SharedLogic.PrintHelp_SubText(">notebook add ", "To add a new notebook entry");
        SharedLogic.PrintHelp_SubText(">notebook list", "To list all the notebook entries");
        SharedLogic.PrintHelp_SubText(">notebook show", "Show more details of an entry");

        SharedLogic.PrintHelp_Heading("ADVANCED"); 
        SharedLogic.PrintHelp_SubText(">notebook edittitle ", "To edit the title of an entry");

        SharedLogic.PrintHelp_Heading("NOTES"); 
        SharedLogic.PrintHelp_SubText(">notebook note" , "Open notebook entry");
        SharedLogic.PrintHelp_SubText(">notebook printnote", "Print the notebook entry");
        SharedLogic.PrintHelp_SubText(">notebook cat", "Same as printnotes");

        SharedLogic.PrintHelp_Heading("HELP");
        SharedLogic.PrintHelp_SubText("All the commands have their own help section. Use the argument '--help'");
        SharedLogic.PrintHelp_SubText("Example - 'notebook add --help' for more examples on how to use it. Try it!");
        SharedLogic.PrintHelp_SubText("This works for every single command! Cheers!");
        SharedLogic.FlushHelpText();
        return true;
    }

    protected override CommandHandlerBase GetSpecializedCommandHandler(Jarvis.JModel model, out List<string> argumentsForSpecializedHandler, bool printErrors)
    {
        string action = arguments_ReadOnly != null && arguments_ReadOnly.Count > 0 ? arguments_ReadOnly[0] : null;
        CommandHandlerBase selectedHander = null;

        if (AreArgumentsEmpty())
        {
            argumentsForSpecializedHandler = null;
            return null;
        }

        switch (action)
        {
            case "add":
                selectedHander = new NotebookAddCommand();
                break;
            case "list":
                selectedHander = new NotebookListCommand();
                break;
            case "show":
                selectedHander = new NotebookShowCommand();
                break;
            case "cat":
            case "printnote":
                selectedHander = new NotebookCatNotesCommand();
                break;
            case "note":
                selectedHander = new NotebookEditNoteCommand();
                break;
            case "edittitle":
                selectedHander = new NotebookEditTitleCommand();
                break;
            default:
                if(printErrors)
                    ConsoleWriter.Print("Invalid command. Try 'notebook --help' for more information");
                break;
        }

        if (selectedHander != null)
        {
            argumentsForSpecializedHandler = new List<string>(arguments_ReadOnly);
            argumentsForSpecializedHandler.RemoveAt(0);

            Utils.Assert(selectedHander is CommandHandlerBaseWithUtility);
            (selectedHander as CommandHandlerBaseWithUtility).Init(model, sharedData, notes);
        }
        else
            argumentsForSpecializedHandler = null;

        return selectedHander;
    }

    protected override bool Run()
    {
        return true;
    }
}

public class NotebookAddCommand : CommandHandlerBaseWithUtility
{
    public NotebookAddCommand()
    {

    }

    protected override bool ShowHelp()
    {
        SharedLogic.StartCachingHelpText();
        SharedLogic.PrintHelp_Heading("USAGE");
        SharedLogic.PrintHelp_SubText(">notebook add <title>", "This will create a new notebook entry. ");
        SharedLogic.PrintHelp_SubText(">notebook add <tags> <title>", "This will create a new notebook entry. ");


        SharedLogic.PrintHelp_Heading("EXAMPLES");
        SharedLogic.PrintHelp_SubText(">notebook add \"Stuff that i did today!\"", "This will create a notebook entry." );
        SharedLogic.PrintHelp_SubText(">notebook add \"dailylog\" \"Stuff that i did today!\"", "This will create a notebook entry with a tag DailyLog. ");

        SharedLogic.PrintHelp_WithHeadingAndSubText("TAGS", model.DesignData.notebook.listOfTags, "you can use these tags below, or create more in data/Design.json");
      
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
            tags = new string[] { model.DesignData.NotebookDefaultTag };
        }
        else if (arguments_ReadOnly.Count == 2)
        {
            tags = arguments_ReadOnly[0].ToLower().Split(',');
            title = arguments_ReadOnly[1];
        }
        else
            Utils.Assert(false);

        if (!model.DesignData.DoesNotebookTagExist(tags))
        {
            ConsoleWriter.Print("Invalid Tag");
            SharedLogic.PrintHelp_WithHeadingAndSubText("TAGS", model.DesignData.notebook.listOfTags, "you can use these tags below, or create more in data/Design.json");
            return true;
        }

        var entry = SharedLogic.CreateNewNotebookEntry(model.notebookManager, tags, title);
        model.notebookManager.AddNotebook(entry);

        ConsoleWriter.Print("New Notebook entry added with id : {0}. You can also edit the notes using 'notebook note'", entry.id);

        notes.CreateNoteIfUnavailable(entry.id);
        notes.OpenNote(model, entry.id, null, true, true);

        return true;
    }
}

public class NotebookListCommand : CommandHandlerBaseWithUtility
{
    public NotebookListCommand()
    {

    }
    protected override bool ShowHelp()
    {
        SharedLogic.StartCachingHelpText();
        SharedLogic.PrintHelp_Heading("USAGE");
        SharedLogic.PrintHelp_SubText(">notebook list", "lists all the entries\n" );

        SharedLogic.PrintHelp_Heading("EXAMPLES");
        SharedLogic.PrintHelp_SubText(">notebook list");
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

        List<NotebookEntry> notebooks = new List<NotebookEntry>(model.notebookManager.Data.entries);

        int lineCount = 0;
        int categoryArea = 30;
        int titleArea = (optionalArguments_ReadOnly.Contains("-e") || optionalArguments_ReadOnly.Contains("--expand")) ? 120 : 40;
        const int newLineAfter = 5;

        // output Heading 
        if (notebooks.Count > 0)
        {
            ConsoleWriter.PrintInColor("{0, -4} {1,-" + categoryArea + "} {2,-" + titleArea + "} {3, -15}",
                model.DesignData.HighlightColorForText,
                "ID", "TAGS", "TITLE", "LOGGED ON"
                );

            foreach (var notebook in notebooks)
            {

                ConsoleWriter.Print("{0, -4} {1,-" + categoryArea + "} {2,-" + titleArea + "} {3, -15}",
                    notebook.id,
                    (notebook.tags != null && notebook.tags.Length > 0 ? Utils.Conversions.ArrayToString(notebook.tags, true).TruncateWithVisualFeedback(categoryArea - 3) : "ERROR"),
                    notebook.title.TruncateWithVisualFeedback(titleArea - 7/*for the ...*/) + "+(N)",// because Notes is available for all notebooks 
                    notebook.loggedDate.ShortForm()
                    );

                lineCount++;

                //@todo
                if (lineCount % newLineAfter == 0)
                    ConsoleWriter.Print();
            }
        }
        else
            ConsoleWriter.Print("No notebooks found! Try adding a few using \"notebook add\"");

        return true;
    }
}


public class NotebookShowCommand : CommandHandlerBaseWithUtility
{
    public NotebookShowCommand()
    {
    }

    protected override bool ShowHelp()
    {
        SharedLogic.StartCachingHelpText();
        SharedLogic.PrintHelp_Heading("USAGE");
        SharedLogic.PrintHelp_SubText(">notebook show <id>", "This will show all the details of a notebook!");

        SharedLogic.PrintHelp_Heading("EXAMPLES");
        SharedLogic.PrintHelp_SubText(">notebook show 1", "Shows more details for notebook with id 1");
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

        NotebookEntry notebook = model.notebookManager.GetNotebook_ReadOnly(id);

        if (notebook == null)
        {
            ConsoleWriter.Print("Notebook with id : {0} not found!", id);
            return true;
        }

        // Print notebook details
        {
            // Heading
            ConsoleWriter.PrintInColor("{0, -4} {1, -" + categoryArea + "} {2}",
                model.DesignData.HighlightColorForText,
                "ID", "TAGS", "TITLE"
                );

            ConsoleWriter.Print("{0, -4} {1, -" + categoryArea + "} {2}",
                notebook.id,
                (notebook.tags != null && notebook.tags.Length > 0 ? Utils.Conversions.ArrayToString(notebook.tags, true).TruncateWithVisualFeedback(categoryArea - 3) : "ERROR"),
                notebook.title);

            ConsoleWriter.Print();

            ConsoleWriter.Print(
                "LOGGED ON : {0}\n", 
                notebook.loggedDate.ShortForm()
                );
        }

        ConsoleWriter.Print();

        return true;
    }
}

public class NotebookEditTitleCommand : CommandHandlerBaseWithUtility
{
    public NotebookEditTitleCommand()
    {
    }

    protected override bool ShowHelp()
    {
        SharedLogic.StartCachingHelpText();
        SharedLogic.PrintHelp_Heading("USAGE");
        SharedLogic.PrintHelp_SubText(">notebook edittitle <id> <new title>", "Renames the entry title");

        SharedLogic.PrintHelp_Heading("EXAMPLES");
        SharedLogic.PrintHelp_SubText(">notebook edittitle 1 \"Wake up at 7 AM\"", "rename the title of notebook : 1 to 'Wake up at 7 AM'");
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

        NotebookEntry hb = model.notebookManager.GetNotebook_ReadOnly(id);

        if (hb == null)
        {
            ConsoleWriter.Print("Notebook with id : {0} not found!", id);
            return true;
        }

        model.notebookManager.GetNotebook_Editable(id).title = title;
        ConsoleWriter.Print("Notebook with id : {0} renamed to - {1}", id, title);

        return true;
    }
}

public class NotebookCatNotesCommand : CommandHandlerBaseWithUtility
{
    public NotebookCatNotesCommand()
    {

    }

    protected override bool ShowHelp()
    {
        SharedLogic.StartCachingHelpText();
        SharedLogic.PrintHelp_Heading("USAGE");
        SharedLogic.PrintHelp_SubText(">notebook cat <notebookID>", "Prints the notes of a notebook entry. You can also use printnote instead of cat");
        SharedLogic.PrintHelp_SubText(">notebook printnote <notebookID>", "Same as cat");

        SharedLogic.PrintHelp_Heading("EXAMPLES");
        SharedLogic.PrintHelp_SubText(">notebook cat 1", "Prints the notes for notebook with id 1");
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

        
        if (model.notebookManager.DoesNotebookExist(id))
        {
            if( !notes.DoesNoteExist(id))
                ConsoleWriter.Print("Notes not found for the notebook with id : {0}", id);
            else 
            {
                ConsoleWriter.PrintInColor("NOTES :", model.DesignData.HighlightColorForText);
                ConsoleWriter.PrintText(notes.GetNoteContent(id));
            }
        }
        else
            ConsoleWriter.Print("notebook not found with id : " + id);

        return true;
    }
}

public class NotebookEditNoteCommand : CommandHandlerBaseWithUtility
{
    public NotebookEditNoteCommand()
    {

    }

    protected override bool ShowHelp()
    {
        SharedLogic.StartCachingHelpText();
        SharedLogic.PrintHelp_Heading("USAGE");
        SharedLogic.PrintHelp_SubText(">notebook note <notebookID>", "Opens notes for a notebook");
        SharedLogic.PrintHelp_SubText(">notebook note <notebookID> --ext:<editorname>", "Provide external editor name of your choice. Example : code or vim");
        SharedLogic.PrintHelp_SubText(">notebook note <notebookID> --append:<Message>", "Append the message directly to the note");
        SharedLogic.PrintHelp_SubText(">notebook note <notebookID> --appendlog:<Message>", "Append the message directly to the note, with a timestamp!");

        SharedLogic.PrintHelp_Heading("ADVANCED");
        SharedLogic.PrintHelp_SubText("You can change the default editor (to open the notes) in the Data/Design.json under 'defaultExternalEditor'");
        SharedLogic.PrintHelp_SubText("you can use '--nowait' to have jarvis not wait for the notes to be closed.");

        SharedLogic.PrintHelp_Heading("EXAMPLES");
        SharedLogic.PrintHelp_SubText(">notebook note 1", "Edit the notes for notebook : 1");
        SharedLogic.PrintHelp_SubText(">notebook note 1 --ext:code", "Edit the notes for notebook : 1, within the visual studio code");
        SharedLogic.PrintHelp_SubText(">notebook note 1 --append:\"Buy milk\"", "Add 'buy milk' to the notes!");
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

        if (model.notebookManager.DoesNotebookExist(id))
        {
            notes.CreateNoteIfUnavailable(id);

            if (!appendMessage.IsEmpty())
            {
                ConsoleWriter.Print("Message appended to the notes");
                notes.AppendToNote(id, appendMessage);
            }
            else
            {
                notes.OpenNote(model, id, externalProgram, waitForTheProgramToEnd, true);
            }
        }
        else
            ConsoleWriter.Print("notebook not found with id : " + id);
        return true;
    }
}
