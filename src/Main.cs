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
        //string debugCommand = "task list --help";
        string debugCommand = "habit show 1";
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
        ConsoleWriter.OnAppLaunched();
        Jarvis.JApplication app = new Jarvis.JApplication();
        app.Initialize();
        
        List<List<string>> commands = SplitSingleCompositeCommandToSimpleOnes(args);

        foreach (var command in commands)
        {
            List<string>[] arguments = SplitCommandIntoManditoryAndOptional(command);
            Utils.Assert( arguments.Length == 2);   // 0 being manditory and 1 being optional
            
            ConsoleWriter.EmptyLine();

            if (!(new CommandSelector()).TryHandle(arguments[0] /*Manditory arguments*/, arguments[1] /*Optional*/, app))
            {
                ConsoleWriter.Print("Invalid arguments. Try 'jarvis --help' for more information.");
            }
        }

        app.Save();

        ConsoleWriter.EmptyLine();       // A bit of space at the end
        ConsoleWriter.OnAppKilled();
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
        return new []{valueArguments, optionalArguments };
    }

}
