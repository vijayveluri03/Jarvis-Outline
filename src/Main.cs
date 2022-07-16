using System;
using System.Collections.Generic;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;
using CommandLine;

class Program
{

    static void Main(string[] args)
    {
#if DEBUG
        //string debugCommand = "task list & task report";
        //string debugCommand = "game snake";
        string debugCommand = "--enter";
        //string debugCommand = "";
        //string debugCommand = "habit show 1";
        //string debugCommand = "task list --cat:asdf:fefe";
        args = debugCommand.Split(' ');

        ConsoleWriter.Print("****** DEBUG ******");
        ConsoleWriter.Print("List of parameters:");
        foreach (string arg in args)
        {
            ConsoleWriter.Print(arg);
        }
        ConsoleWriter.Print("****** DEBUG ******");
#endif

        // Custom CLI is a seperate Command line interface which goes beyond single commands
        // your console would look like 
        // JARVIS>
        // here you can go about providing the jarvis commands with out the prefer 'jarvis'
        // Example : 
        // JARVIS>tast list 
        // Why do we need custom interface ?
        // 1. to not have to type jarvis all the time. 2. loads and saves the data only once for an entire session.

        bool customCLI = false;
        customCLI = args.Contains<string>("--enter") || args.Contains<string>("--doyourthing");
#if FORCE_JARVIS_CLI
        customCLI = true;
#endif

        bool firstTime = true;

        ConsoleWriter.Initialize();
        Jarvis.JApplication app = new Jarvis.JApplication();
        app.Initialize();

        var commandSelector = new CommandSelector();
        commandSelector.Init(app, null);


        do
        {

            // If this is custom CLI, then we are reaching out for inputs from user, 
            // unless its 'exit', we are going to stay in this mode forever
            // on 'exit' we save the changes and quit. 

            if (customCLI)
            {
                ConsoleWriter.EmptyLine();

                if ( firstTime )
                {
                    ConsoleWriter.Print(
#if !FORCE_JARVIS_CLI
                        "Entering JARVIS mode. You can enter any jarvis command with out the prefix 'jarvis' here." +
                        "\nExample - JARVIS>task list (or) JARVIS>habit list" +
#else
                        "Welcome! This is jarvis mode, where only jarvis commands would work. Try '--help' for more information." +
#endif
                        "\nTo exit, simply try 'exit' with out the quotes. You can also try 'save' or 'clear'. Cheers!\n");
                        
                    firstTime = false;
                }
                string lastCommand = app.UserData.GetLastCommand();
                string cursorText = lastCommand.IsEmpty() ? "JARVIS>" : "JARVIS " + lastCommand.ToUpper() + ">";
                string customJarvisCommand = Utils.CLI.GetUserInputString(cursorText, app.DesignData.HighlightColorForText);

                if (customJarvisCommand.IsEmpty())
                {
                    continue;
                }

                //@todo - Move these somewhere else
                if (customJarvisCommand.ToLower() == "exit")
                    break;

                if (customJarvisCommand.ToLower() == "save")
                {
                    app.Save();
                    continue;
                }

                if (customJarvisCommand.ToLower() == "cls" || customJarvisCommand.ToLower() == "clear")
                {
                    ConsoleWriter.Clear();
                    continue;
                }

                args = Utils.CLI.SplitCommandLine(customJarvisCommand);
            }

            // split big command ( which could be a combination of multiple commands seperated to '+' into small ones
            // Ex: JARVIS>task list + task report 
            // is two commands in one. So we are seperating them out and handle them individually

            List<List<string>> commands = SplitSingleCompositeCommandToSimpleOnes(args);
            foreach (var command in commands)
            {
                // Normally we would not have 'jarvis' as a part of the command, as the CLI strips it from the arguments 
                // but that doesnt happen if we are in the custom CLI, where we have to manually handle it. 

                RemoveJarvisPrefixFromCommand(command);
                // Split a the optional arguments to a seperate list. 
                // ex: JARVIS>task list --story --cat:health
                // the optional params which start with '-' or '--' are stripped for the command into another 
                // in our example, [0] has (task, list) [1] has (--story, --cat:health)

                List<string>[] arguments = SplitCommandIntoManditoryAndOptional(command);
                Utils.Assert(arguments.Length == 2);   // 0 being manditory and 1 being optional

                ConsoleWriter.EmptyLine();

                commandSelector.TryHandle(arguments[0] /*Manditory arguments*/, arguments[1] /*Optional*/, app);
            }
        } while( customCLI);

        app.Save();

        ConsoleWriter.EmptyLine();       // A bit of space at the end
        ConsoleWriter.DestroyAndCleanUp();
    }

    public static List<List<string>> SplitSingleCompositeCommandToSimpleOnes( string[] args )
    {
        List<List<string>> commands = new List<List<string>>();
        commands.Add(new List<string>());

        int commandIndex = 0;
        foreach (var arg in args)
        {
            if (arg == "&" || arg == "+")
            {
                commandIndex++;
                commands.Add(new List<string>());
                continue;
            }

            commands[commandIndex].Add(arg);
        }
        return commands;
    }

    public static void RemoveJarvisPrefixFromCommand(List<string> command)
    {
        if (command != null && command.Count > 0)
            if (command[0] == "jarvis")
                command.RemoveAt(0);
    }
    public static List<string>[] SplitCommandIntoManditoryAndOptional(List<string> arguments)
    {
        List<string> valueArguments = new List<string>(arguments);
        List<string> optionalArguments = new List<string>();

        // Seperating arguments into Value and Optional. 
        // optional are the ones with -- or - before an argument
        {
            for (int i = 0; i < valueArguments.Count; i++)
            {
                if (valueArguments[i].StartsWith("-"))
                {
                    optionalArguments.Add(valueArguments[i]);
                    valueArguments.RemoveAt(i);
                    i--;
                }
            }
        }
        return new []{ valueArguments, optionalArguments };
    }


}
