using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

public class JApplication
{
    public void Init(string[] args)
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

        ConsoleWriter.Initialize();
        app = new Jarvis.JModel();
        app.Initialize();

        commandSelector = new CommandSelector();
        commandSelector.Init(app, null);

        StartPomodoroObserverThread();
    }

    public void UpdateLoop()
    {
        string[] args = null;
        bool firstTime = true;

        do
        {
            {
                ConsoleWriter.EmptyLine();

                if (firstTime)
                {
                    ConsoleWriter.Print(
                        "Welcome! Try '--help' for more information." +
                        "\nTo exit, simply try 'exit' with out the quotes. You can also try 'save' or 'clear'. Cheers!\n");

                    firstTime = false;
                }
                string cursorText = GetCursorText();
                string userCommand = Utils.CLI.GetUserInputString(cursorText, app.DesignData.HighlightColorForText);

                if (userCommand.IsEmpty())
                {
                    continue;
                }

                //@todo - Move these somewhere else
                if (userCommand.ToLower() == "exit")
                    break;

                if (userCommand.ToLower() == "save")
                {
                    app.Save();
                    continue;
                }

                if (userCommand.ToLower() == "cls" || userCommand.ToLower() == "clear")
                {
                    ConsoleWriter.Clear();
                    continue;
                }

                args = Utils.CLI.SplitCommandLine(userCommand);
            }

            // split big command ( which could be a combination of multiple commands seperated to '+' into small ones
            // Ex: JARVIS>task list + task report 
            // is two commands in one. So we are seperating them out and handle them individually

            List<List<string>> commands = LocalUtils.SplitSingleCompositeCommandToSimpleOnes(args);
            foreach (var command in commands)
            {
                // Normally we would not have 'jarvis' as a part of the command, as the CLI strips it from the arguments 
                // but that doesnt happen if we are in the custom CLI, where we have to manually handle it. 

                LocalUtils.RemoveJarvisPrefixFromCommand(command);
                // Split a the optional arguments to a seperate list. 
                // ex: JARVIS>task list --story --cat:health
                // the optional params which start with '-' or '--' are stripped for the command into another 
                // in our example, [0] has (task, list) [1] has (--story, --cat:health)

                List<string>[] arguments = LocalUtils.SplitCommandIntoManditoryAndOptional(command);
                Utils.Assert(arguments.Length == 2);   // 0 being manditory and 1 being optional

                ConsoleWriter.EmptyLine();

                commandSelector.TryHandle(arguments[0] /*Manditory arguments*/, arguments[1] /*Optional ones*/, app);
            }
        } while (true);
    }

    public void DeInit()
    {
        KillObserverThread();

#if RELEASE_LOG
        ConsoleWriter.Print(">>> Exiting main thread!");
#endif
        app.Save();

        ConsoleWriter.EmptyLine();       // A bit of space at the end
        ConsoleWriter.DestroyAndCleanUp();
    }


    private void StartPomodoroObserverThread()
    {
        shouldStopAllThreads = false;
        pomodoroObserver = new Thread(() =>
        {
            var pomodoroObserver = new PomodoroObserver();
            pomodoroObserver.Run(app, ShouldDestroyAllThreads);
        });
        pomodoroObserver.Start();
    }
    private void KillObserverThread()
    {
        shouldStopAllThreads = true;
        pomodoroObserver.Join();
    }

    private string GetCursorText()
    {
        string lastCommand = app.UserData.GetLastCommand();
        string statusStr = GetStatusStr();

        if (lastCommand != null)
            lastCommand = lastCommand.ToUpper();

        if (lastCommand.IsEmpty())
        {
            if (statusStr.IsEmpty())
            {
                return string.Format("{0}>", "JARVIS");
            }
            else
            {
                return string.Format("{0} {1}>", "JARVIS", statusStr);
            }
        }
        else
        {
            if (statusStr.IsEmpty())
            {
                return string.Format("{0} {1}>", "JARVIS", lastCommand);
            }
            else
            {
                return string.Format("{0} {1} {2}>", "JARVIS", lastCommand, statusStr);
            }
        }
    }
    private string GetStatusStr()
    {
        if( app.UserData.IsPomodoroInProgress())
        {
            int totalMinsNeeded = app.UserData.GetPomodoroData().pomoCount * Jarvis.JConstants.POMODORO_TIME;
            int minsRemaining = totalMinsNeeded - (int)(DateTime.Now - app.UserData.GetPomodoroStartTime()).TotalMinutes;
            return minsRemaining.ToString();
        }
        return "";
    }

    private

    class LocalUtils
    {
        public static List<List<string>> SplitSingleCompositeCommandToSimpleOnes(string[] args)
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
            return new[] { valueArguments, optionalArguments };
        }
    }

    bool ShouldDestroyAllThreads() { return shouldStopAllThreads; }


    private bool shouldStopAllThreads = false;
    Jarvis.JModel app = null;
    Thread pomodoroObserver = null;
    CommandSelector commandSelector = null;
}