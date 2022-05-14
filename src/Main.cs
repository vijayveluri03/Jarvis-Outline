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
        //string command = "task list";
        string command = "task recordtimelog 8 90 \"\"";
        args = command.Split(' ');
#endif
        Jarvis.JApplication app = new Jarvis.JApplication();
        app.Initialize();

        // A bit of space at the head
        Console.Out.WriteLine(" ");
        //Console.Out.WriteLine("Number of arguments" + args.Length);

#if DEBUG
        Console.Out.WriteLine("****** DEBUG ******" );
        Console.Out.WriteLine("List of parameters:");
        foreach(string arg in args)
        {
            Console.Out.WriteLine(arg);
        }
        Console.Out.WriteLine("****** DEBUG ******" );
#endif

        List<string> valueArguments = new List<string>(args);
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

        //CommandLine.Parser.Default.ParseArguments<BaseCommand>(args)
        //.WithParsed<BaseCommand>(option => option.Run(null))
        //.WithNotParsed(HandleParseError);

        //CommandLine.Parser.Default.ParseArguments<TaskAddCommand, TaskStartCommand>(args)
        //.WithParsed<ICommand>(option => RunOptions(option))
        //.WithNotParsed(HandleParseError);

        app.Save();

        // A bit of space at the tail
        Console.Out.WriteLine(" ");
    }
}
