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
        string command = "hello hello2 \"hello 4\"";
        args = command.Split(' ');
#endif
        Jarvis.JApplication app = new Jarvis.JApplication();
        app.Initialize();

        //Console.Out.WriteLine("Welcome Captain!");
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

        CommandSelector commandHandler = new CommandSelector();
        commandHandler.Run(new List<string>(args), app);

        //CommandLine.Parser.Default.ParseArguments<BaseCommand>(args)
        //.WithParsed<BaseCommand>(option => option.Run(null))
        //.WithNotParsed(HandleParseError);

        //CommandLine.Parser.Default.ParseArguments<TaskAddCommand, TaskStartCommand>(args)
        //.WithParsed<ICommand>(option => RunOptions(option))
        //.WithNotParsed(HandleParseError);

        app.Save();
    }
}
