using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;


public static class Utils
{
    // Fetch user inputs from console 
    #region GET USER INPUT FROM CONSOLE

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

        ConsoleWriter.PrintWithOutLineBreak(message, false);
        string input = Console.ReadLine();

        ConsoleWriter.PopColor();

        if (string.IsNullOrEmpty(input) && !string.IsNullOrEmpty(defaultInput))
            return defaultInput;
        return input;
    }
    public static string[] GetUserInputStringArray(string message)
    {
        ConsoleWriter.PrintWithOutLineBreak(message + "(Double Enter to exit):");
        List<string> input = new List<string>();
        while (true)
        {
            string line = Console.ReadLine();

            if (string.IsNullOrEmpty(line))
                break;
            else
                input.Add(line);
        }

        return input.ToArray();
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
    public static int[] GetUserInputIntArray(string message)
    {
        ConsoleWriter.PrintWithOutLineBreak(message + "(use comma to seperate");
        string input = Console.ReadLine();

        if (string.IsNullOrEmpty(input))
            return null;

        if (input.Contains(","))
        {
            string[] splitStr = input.Split(',');
            int[] result = new int[splitStr.Length];

            for (int i = 0; i < result.Length; i++)
            {
                int temp = 0;
                temp = int.TryParse(splitStr[i], out temp) ? temp : 0;
                result[i] = temp;
            }
            return result;
        }
        else
        {
            int result = 0;
            result = int.TryParse(input, out result) ? result : 0;
            return new int[] { result };
        }
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
    #endregion


    public static float MinutesToHours (int minutes)
    {
        return (float)(minutes/60.0);
    }

    public static string MinutesToHoursString (int minutes)
    {
        return String.Format("{0:0.0}", MinutesToHours(minutes));
    }

    public static string HoursToHoursString (float hours)
    {
        return String.Format("{0:0.0}", hours);
    }

    public static int ExtractIntFromArgument(List<string> arguments, string startingSubstring, int defaul, System.Action itemNotFoundCB, System.Action syntaxNotValidCB, out bool syntaxError)
    {
        syntaxError = false;
        string subString = ExtractStringFromArgument(arguments, startingSubstring, string.Empty, itemNotFoundCB, syntaxNotValidCB, out syntaxError);
        if ( subString == string.Empty)
            return defaul;
        return Atoi(subString);
    }

    public static string ExtractStringFromArgument( List<string> arguments, string startingSubstring, string defaul, System.Action itemNotFoundCB, System.Action syntaxNotValidCB, out bool syntaxError)
    {
        syntaxError = false;
        string listItem = arguments.FindItemWithSubstring(startingSubstring);
        if ( listItem == null || listItem == string.Empty)
        {
            if( itemNotFoundCB != null)
                itemNotFoundCB();
            return defaul;
        }
        if(!listItem.Contains(":"))
        {
            syntaxError = true;
            if(syntaxNotValidCB != null)
                syntaxNotValidCB();
            return defaul;
        }

        string[] subStrings = listItem.Split(':');

        if(subStrings.Length <= 1)
        {
            syntaxError = true;
            if(syntaxNotValidCB != null)
                syntaxNotValidCB();
            return defaul;
        }

        return subStrings[1];
    }

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
        if(useDelimitter)
            sb.Remove(sb.Length -1, 1);

        return sb.ToString();
    }

    // Utils

    public static int Clamp(int value, int min, int max)
    {
        return (value < min) ? min : (value > max) ? max : value;
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
    public static int ConvertRange(int min, int max, int newMin, int newMax, int value)
    {
        float ratio = (value - min) / (float)(max - min);
        return (int)(newMin + (newMax - newMin) * ratio);
    }
    public static float ConvertRange(float min, float max, float newMin, float newMax, float value)
    {
        float ratio = (value - min) / (float)(max - min);
        return (float)(newMin + (newMax - newMin) * ratio);
    }
    public static int Lerp(int min, int max, int ratio)
    {
        return (int)(min + (max - min) * ratio);
    }
    public static DateTime Now
    {
        get
        {
            return DateTime.Now.ZeroTime();
        }
    }

    public static int[] ConvertCommaAndHyphenSeperateStringToIDs(string uberText)
    {
        string[] IDs = uberText.Split(',');
        List<int> convertedIDs = new List<int>();
        foreach (string idStr in IDs)
        {

            if (idStr.Contains('-'))
            {
                string[] range = idStr.Split('-');
                Assert(range != null && range.Length == 2);
                int num1 = Atoi(range[0], -1);
                int num2 = Atoi(range[1], -1);
                if (num1 != -1 && num2 != -1 && num1 <= num2)
                {
                    while (num1 <= num2)
                    {
                        convertedIDs.Add(num1);
                        num1++;
                    }
                }
            }
            else
            {
                int id = Atoi(idStr, -1);
                if (id != -1)
                    convertedIDs.Add(id);
            }
        }
        return convertedIDs.ToArray();
    }

    public static int Atoi(string txt, int fallback = -1)
    {
        int num = fallback;
        if (int.TryParse(txt, out num))
            return num;
        return fallback;
    }

    public static bool CreateFileIfNotExit(string path, string templateFile)
    {
        if (!File.Exists(path))
        {
            Assert(File.Exists(templateFile), "Template file doesnt exist.");
            File.Copy(templateFile, path);
            return true;
        }
        return false;
    }
    //Execute a command in console. 
    public static void ExecuteCommandInConsole(string command)
    {
        Process proc = new System.Diagnostics.Process();
        proc.StartInfo.FileName = "/bin/bash";
        proc.StartInfo.Arguments = "-c \" " + command + " \"";
        //ConsoleWriter.PrintInRed(proc.StartInfo.Arguments);
        proc.StartInfo.UseShellExecute = false;
        proc.StartInfo.RedirectStandardOutput = true;
        proc.Start();

        while (!proc.StandardOutput.EndOfStream)
        {
            Console.WriteLine(proc.StandardOutput.ReadLine());
        }
    }
}

// Date Utils

public static class DateExt
{
    public static string ShortForm(this DateTime date)
    {
        return date.Month + "/" + date.Day;
    }
    public static DateTime ZeroTime(this DateTime date)
    {
        return new DateTime(date.Year, date.Month, date.Day, 0, 0, 0);
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
        return IsSameAs(date, Utils.Now.AddDays(offset));
    }
    public static bool IsThisMinDate(this DateTime date)
    {
        return date == DateTime.MinValue;
    }
}

// utilities

public static class StringExt
{
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
        foreach( var value in list)
        {
            if ( value.Contains(substring))
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
