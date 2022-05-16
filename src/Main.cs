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
        //string command = "task report";
        string debugCommand = "task list --cat:asdf:fefe";
        args = debugCommand.Split(' ');
#endif
        ConsoleWriter.OnAppLaunched();
        Jarvis.JApplication app = new Jarvis.JApplication();
        app.Initialize();

        // A bit of space at the head
        ConsoleWriter.Print(" ");
        //ConsoleWriter.Print("Number of arguments" + args.Length);

#if DEBUG
        ConsoleWriter.Print("****** DEBUG ******" );
        ConsoleWriter.Print("List of parameters:");
        foreach(string arg in args)
        {
            ConsoleWriter.Print(arg);
        }
        ConsoleWriter.Print("****** DEBUG ******" );
#endif

        #region Splitting multiple commands
        List<List<string>> commands = new List<List<string>>();
        commands.Add(new List<string>());

        int commandIndex = 0; 
        foreach( var arg in args )
        {
            if(arg =="&" || arg =="+")
            {
                commandIndex++;
                commands.Add(new List<string>());
                continue;
            }

            commands[commandIndex].Add(arg);
        }
        #endregion

        for( commandIndex = 0; commandIndex < commands.Count; commandIndex++ )
        {
            var command = commands[commandIndex];

            List<string> valueArguments = new List<string>(command);
            List<string> optionalArguments = new List<string>();

            // Seperating arguments into Value and Optional. 
            // optional are the ones with -- or - before an argument
            {
            for( int i = 0; i < valueArguments.Count; i++ )
            {
                if(valueArguments[i].StartsWith("-"))
                {
                    optionalArguments.Add(valueArguments[i]);
                    valueArguments.RemoveAt(i);
                    i--;
                }
            }
            }

            CommandSelector commandHandler = new CommandSelector();
            commandHandler.Run(valueArguments, optionalArguments, app);

            
            // A bit of space in between commands 
            if( commandIndex < commands.Count - 1 )
                ConsoleWriter.Print(" ");

        }

        //CommandLine.Parser.Default.ParseArguments<BaseCommand>(args)
        //.WithParsed<BaseCommand>(option => option.Run(null))
        //.WithNotParsed(HandleParseError);

        //CommandLine.Parser.Default.ParseArguments<TaskAddCommand, TaskStartCommand>(args)
        //.WithParsed<ICommand>(option => RunOptions(option))
        //.WithNotParsed(HandleParseError);

        app.Save();

        // A bit of space at the tail
        ConsoleWriter.Print(" ");
        ConsoleWriter.OnAppKilled();
    }
}
