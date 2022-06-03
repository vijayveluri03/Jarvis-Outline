using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Linq;


public static class Utils
{
    public static class Time
    {
        public static float MinutesToHours(int minutes)
        {
            return (float)(minutes / 60.0);
        }
        public static string MinutesToHoursString(int minutes)
        {
            return String.Format("{0:0.0}", MinutesToHours(minutes));
        }
        public static string HoursToHoursString(float hours)
        {
            return String.Format("{0:0.0}", hours);
        }
    }

    public static class CLI
    {
        public static string GetUserInputString(string message, string defaultInput = "")
        {
            if (!string.IsNullOrEmpty(defaultInput))
                message += "( defaults to " + defaultInput + ")";
            ConsoleWriter.PrintWithOutLineBreak(message);
            string input = Console.ReadLine();
            if (string.IsNullOrEmpty(input) && !string.IsNullOrEmpty(defaultInput))
                return defaultInput;
            return input;
        }
        public static string GetUserInputString(string message, ConsoleColor color, string defaultInput = "")
        {
            if (!string.IsNullOrEmpty(defaultInput))
                message += "( defaults to '" + defaultInput + "')";

            ConsoleWriter.PushColor(color);

            ConsoleWriter.PrintWithOutLineBreak(message);
            string input = Console.ReadLine();

            ConsoleWriter.PopColor();

            if (string.IsNullOrEmpty(input) && !string.IsNullOrEmpty(defaultInput))
                return defaultInput;
            return input;
        }
        public static int GetUserInputInt(string message, int defaultInput = -999)
        {
            if (defaultInput != -999)
                message += "( defaults to " + defaultInput + ")";
            ConsoleWriter.PrintWithOutLineBreak(message);
            string input = Console.ReadLine();

            if (string.IsNullOrEmpty(input) && defaultInput != -999)
                return defaultInput;

            int result = 0;
            return int.TryParse(input, out result) ? result : 0;
        }
        public static bool GetConfirmationFromUser(string message, bool takeNoByDefault = false)
        {
            if (!takeNoByDefault)
                ConsoleWriter.PrintWithOutLineBreak(message + " (enter yes or y to confirm, default) :");
            else
                ConsoleWriter.PrintWithOutLineBreak(message + " (enter no or n to confirm, default) :");

            string input = Console.ReadLine();
            bool isYes = input.ToLower() == "yes" || input.ToLower() == "y";

            if (string.IsNullOrEmpty(input))
            {
                if (takeNoByDefault)
                    return false;
                else
                    return true;
            }

            return isYes;
        }
        public static DateTime GetCustomDateFromUser(string message)
        {
            ConsoleWriter.PrintWithOutLineBreak(message);

            string input = Console.ReadLine();
            DateTime result;
            if (DateTime.TryParse(input, null, System.Globalization.DateTimeStyles.RoundtripKind, out result))
                return result;
            else
                return DateTime.MinValue;
        }
        public static DateTime GetCustomDateFromUser(string message, DateTime defaultValue, bool showXToReset = true)
        {
            if (defaultValue != DateTime.MinValue)
                message += "( defaults to " + defaultValue + ")";
            if (showXToReset)
                message += "(x to reset to min)";
            ConsoleWriter.PrintWithOutLineBreak(message);

            string input = Console.ReadLine();
            DateTime result;
            if (input == "x")
                return DateTime.MinValue;
            if (DateTime.TryParse(input, null, System.Globalization.DateTimeStyles.RoundtripKind, out result))
                return result;
            else
                return defaultValue;
        }
        public static int ExtractIntFromCLIParameter(List<string> arguments, string startingSubstring, int defaul, System.Action itemNotFoundCB, System.Action syntaxNotValidCB, out bool syntaxError)
        {
            syntaxError = false;
            string subString = ExtractStringFromCLIParameter(arguments, startingSubstring, string.Empty, itemNotFoundCB, syntaxNotValidCB, out syntaxError);
            if (subString == string.Empty)
                return defaul;
            return Utils.Conversions.Atoi(subString);
        }
        public static string ExtractStringFromCLIParameter(List<string> arguments, string startingSubstring, string defaul, System.Action itemNotFoundCB, System.Action syntaxNotValidCB, out bool syntaxError)
        {
            syntaxError = false;
            string listItem = arguments.FindItemWithSubstring(startingSubstring);
            if (listItem == null || listItem == string.Empty)
            {
                if (itemNotFoundCB != null)
                    itemNotFoundCB();
                return defaul;
            }
            if (!listItem.Contains(":"))
            {
                syntaxError = true;
                if (syntaxNotValidCB != null)
                    syntaxNotValidCB();
                return defaul;
            }

            string[] subStrings = listItem.Split(':');

            if (subStrings.Length <= 1)
            {
                syntaxError = true;
                if (syntaxNotValidCB != null)
                    syntaxNotValidCB();
                return defaul;
            }

            return subStrings[1];
        }
        public static void ExecuteCommandInConsole(string command, bool runSilently = true, bool printOutput = true, bool createNewWindow = true, bool waitForProgramToExit = false )
        {
            Process proc = new System.Diagnostics.Process();

#if UNIX // @todo - Need to decide based on the environment 
                proc.StartInfo.FileName = "/bin/bash";
#else
            proc.StartInfo.FileName = "cmd.exe";
#endif

            proc.StartInfo.Arguments = (runSilently ? "/C" : "/K") + " " + command;
            proc.StartInfo.UseShellExecute = createNewWindow;   // this will run in a seperate window 
            proc.StartInfo.RedirectStandardOutput = !printOutput;
            //proc.StartInfo.CreateNoWindow = !createNewWindow; // this is not working as expected 

            proc.Start();

            if (waitForProgramToExit)
                proc.WaitForExit();

            //ConsoleWriter.Print("EXITTED");

            /* todo - This hangs atm - This is supposed to print the details which are sent to Standard out */
#if false
            while ( printOutput && !proc.StandardOutput.EndOfStream)
                Console.WriteLine(proc.StandardOutput.ReadLine());
#endif
        }
        public static void ExecuteCommand(string program, string arguments, bool printOutput = true, bool waitForProgramToExit = false)
        {
            Process proc = new System.Diagnostics.Process();

#if UNIX // @todo - Need to decide based on the environment 
                proc.StartInfo.FileName = "/bin/bash";
#else
            proc.StartInfo.FileName = program;
#endif

            proc.StartInfo.Arguments = arguments;
            proc.StartInfo.UseShellExecute = true;   // this will run in a seperate window 
            proc.StartInfo.RedirectStandardOutput = !printOutput;
            //proc.StartInfo.CreateNoWindow = !createNewWindow; // this is not working as expected 

            proc.Start();

            if (waitForProgramToExit)
                proc.WaitForExit();

            /* todo - This hangs atm - This is supposed to print the details which are sent to Standard out */
#if false
            while ( printOutput && !proc.StandardOutput.EndOfStream)
                Console.WriteLine(proc.StandardOutput.ReadLine());
#endif
        }
        public static string[] SplitCommandLine(string commandLine)
        {
            bool inQuotes = false;

            var args = Split(commandLine, c =>
                                     {
                                         if (c == '\"')
                                             inQuotes = !inQuotes;

                                         return !inQuotes && c == ' ';
                                     })
                              .Select(arg => TrimMatchingQuotes(arg.Trim(), '\"'))
                              .Where(arg => !string.IsNullOrEmpty(arg));
            return args.ToArray();
        }
        private static string TrimMatchingQuotes(string input, char quote)
        {
            if ((input.Length >= 2) &&
                (input[0] == quote) && (input[input.Length - 1] == quote))
                return input.Substring(1, input.Length - 2);

            return input;
        }
        private static List<string> Split(string str,
                                                Func<char, bool> controller)
        {
            int nextPiece = 0;
            List<string> args = new List<string>();

            for (int c = 0; c < str.Length; c++)
            {
                if (controller(str[c]))
                {
                    args.Add( str.Substring(nextPiece, c - nextPiece) );
                    nextPiece = c + 1;
                }
            }

            args.Add( str.Substring(nextPiece) );
            return args;
        }
    }

    public static class Conversions
    {
        public static string ArrayToString(List<string> array, bool useDelimitter, char delimitter = ',')
        {
            StringBuilder sb = new StringBuilder();
            foreach (var str in array)
            {
                sb.Append(str);
                if (useDelimitter)
                    sb.Append(delimitter);
            }
            return sb.ToString();
        }
        // @todo - I guess we can simply use ienumerable, instead of two seperate methods. 
        public static string ArrayToString(string[] array, bool useDelimitter, char delimitter = ',')
        {
            StringBuilder sb = new StringBuilder();
            foreach (var str in array)
            {
                sb.Append(str);
                if (useDelimitter)
                    sb.Append(delimitter);
            }
            // Just to remove the last bit of dilimitter. @todo - find a better way 
            if (useDelimitter)
                sb.Remove(sb.Length - 1, 1);

            return sb.ToString();
        }
        public static int Atoi(string txt, int fallback = -1)
        {
            int num = fallback;
            if (int.TryParse(txt, out num))
                return num;
            return fallback;
        }
    }

    public static class Math
    {
        public static int Clamp(int value, int min, int max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }
        public static int ReMap(int min, int max, int newMin, int newMax, int value)
        {
            float ratio = (value - min) / (float)(max - min);
            return (int)(newMin + (newMax - newMin) * ratio);
        }
        public static float ReMap(float min, float max, float newMin, float newMax, float value)
        {
            float ratio = (value - min) / (float)(max - min);
            return (float)(newMin + (newMax - newMin) * ratio);
        }
        public static int Lerp(int min, int max, int ratio)
        {
            return (int)(min + (max - min) * ratio);
        }
    }

    public static class FileHandler
    {
        public static bool DoesFileExist(string path)
        {
            return File.Exists(path);
        }

        public static bool Create(string path, bool overwriteIfExists = false)
        {
            if (DoesFileExist(path) && !overwriteIfExists)
                return false;

            StreamWriter stream = File.CreateText(path);
            stream.Close();
            return true;
        }

        public static string Read(string path)
        {
            if (!DoesFileExist(path))
                return string.Empty;

            return File.ReadAllText(path);
        }

        public static void Append(string path, string txt)
        {
            Assert(DoesFileExist(path));
            File.AppendAllText( path, txt);
        }

        public static void Clean (string path)
        {
            Assert(DoesFileExist(path));
            File.WriteAllText(path, string.Empty);
        }

        public static void Remove (string path)
        {
            Assert(DoesFileExist(path));
            File.Delete(path);
        }

    }

    public static T ParseEnum<T>(string value)
    {
        return (T)Enum.Parse(typeof(T), value, true);
    }
    public static void Assert(bool condition, string message = null)
    {
        if (!condition)
            ConsoleWriter.Print("==== ASSERT here =======");
        Debug.Assert(condition, message);
    }
    public static bool CreateFileIfNotExit(string path, string copyFrom)
    {
        if (!File.Exists(path))
        {
            Assert(File.Exists(copyFrom), "Source file doesnt exist : " + copyFrom);
            File.Copy(copyFrom, path);
            return true;
        }
        return false;
    }

    public static void AppendToFile( string filePath, string text, bool newline = true )
    {
        FileHandler.Append(filePath, newline ? "\n" + text : text );
    }

    public static void OpenAFileInEditor(string filePath, string editor = "vim", bool waitForTheProgramToEnd = false)
    {
        //CLI.ExecuteCommandInConsole(editor + " " + filePath, false, true, true, waitForTheProgramToEnd);
        CLI.ExecuteCommand(editor, filePath, true, waitForTheProgramToEnd);
    }

    public static void OpenAProgram(string filePath, string arguments = default(string), bool waitForTheProgramToEnd = false)
    {
        //CLI.ExecuteCommandInConsole(editor + " " + filePath, false, true, true, waitForTheProgramToEnd);
        CLI.ExecuteCommand(filePath, arguments, true, waitForTheProgramToEnd);
    }

}

// Date Utils

public static class DateExt
{
    public static string ShortForm(this DateTime date)
    {
        return date.Month + "/" + date.Day;
    }
    public static string ShortFormWithDay(this DateTime date)
    {
        return date.Month + "/" + date.Day + " - " + date.DayOfWeek.ToString().Truncate(2);
    }
    public static DateTime ZeroTime(this DateTime date)
    {
        return new DateTime(date.Year, date.Month, date.Day, 0, 0, 0);
    }
    public static DateTime MaxTime(this DateTime date)
    {
        return new DateTime(date.Year, date.Month, date.Day, 23, 59, 59);
    }
    public static bool IsSameAs(this DateTime date, DateTime other)
    {
        if (date.Day == other.Day &&
            date.Month == other.Month &&
            date.Year == other.Year)
            return true;
        return false;
    }
    public static bool IsToday(this DateTime date, int offset = 0)
    {
        return IsSameAs(date, DateTime.Now.AddDays(offset));
    }
    public static bool IsThisMinDate(this DateTime date)
    {
        return date == DateTime.MinValue;
    }
}

// utilities

public static class StringExt
{
    public static bool IsEmpty(this string value )
    {
        return string.IsNullOrEmpty(value);
    }
    public static string Truncate(this string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) return value;
        return value.Length <= maxLength ? value : value.Substring(0, maxLength);
    }
    public static string TruncateWithVisualFeedback(this string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) return value;
        return value.Length <= maxLength ? value : value.Substring(0, maxLength) + "...";
    }
    public static string SplitAndGetString(this string value, int index, char delimiter = ':')
    {
        string[] subStrings = value.Split(delimiter);
        Utils.Assert(subStrings.Length > index);
        return subStrings[index];
    }
}

public static class List
{
    public static string FindItemWithSubstring(this List<string> list, string substring)
    {
        foreach (var value in list)
        {
            if (value.Contains(substring))
            {
                return value;
            }
        }
        return string.Empty;
    }
}

public class Pair<T, U>
{
    public Pair()
    {
    }
    public Pair(T first, U second)
    {
        this.First = first;
        this.Second = second;
    }
    public T First { get; set; }
    public U Second { get; set; }
};

public class Triple<T, U, V>
{
    public Triple()
    {
    }
    public Triple(T first, U second, V third)
    {
        this.First = first;
        this.Second = second;
        this.Third = third;
    }
    public T First { get; set; }
    public U Second { get; set; }
    public V Third { get; set; }

    public Triple<T, U, V> Clone()
    {
        Triple<T, U, V> newone = new Triple<T, U, V>();
        newone.First = First;
        newone.Second = Second;
        newone.Third = Third;
        return newone;
    }
};

public class Quadrupel<T, U, V, W>
{
    public Quadrupel()
    {
    }
    public Quadrupel(T first, U second, V third, W forth)
    {
        this.First = first;
        this.Second = second;
        this.Third = third;
        this.Forth = forth;
    }
    public T First { get; set; }
    public U Second { get; set; }
    public V Third { get; set; }
    public W Forth { get; set; }
};

public static class ListExtension
{
    public static bool Contains<T>(this T[] arr, T arrObj)
    {
        if (arr == null || arr.Length == 0)
            return false;
        for (int i = 0; i < arr.Length; i++)
        {
            if (arr[i].Equals(arrObj))
                return true;
        }
        return false;
    }
}
