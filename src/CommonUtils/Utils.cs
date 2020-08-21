﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;


public static class Utils
{
    // Fetch user inputs from console 
    #region GET USER INPUT FROM CONSOLE

    public static string GetUserInputString(string message, string defaultInput = "") {
        if (!string.IsNullOrEmpty(defaultInput))
            message += "( defaults to " + defaultInput + ")";
        ConsoleWriter.PrintWithOutLineBreak(message);
        string input = Console.ReadLine();
        if (string.IsNullOrEmpty(input) && !string.IsNullOrEmpty(defaultInput))
            return defaultInput;
        return input;
    }
    public static string GetUserInputString(string message, ConsoleColor color, string defaultInput = "") {
        if (!string.IsNullOrEmpty(defaultInput))
            message += "( defaults to " + defaultInput + ")";

        ConsoleWriter.PushColor(color);

        ConsoleWriter.PrintWithOutLineBreak(message, false);
        string input = Console.ReadLine();

        ConsoleWriter.PopColor();

        if (string.IsNullOrEmpty(input) && !string.IsNullOrEmpty(defaultInput))
            return defaultInput;
        return input;
    }
    public static string[] GetUserInputStringArray(string message) {
        ConsoleWriter.PrintWithOutLineBreak(message + "(Double Enter to exit):");
        List<string> input = new List<string>();
        while (true) {
            string line = Console.ReadLine();

            if (string.IsNullOrEmpty(line))
                break;
            else
                input.Add(line);
        }

        return input.ToArray();
    }
    public static int GetUserInputInt(string message, int defaultInput = -999) {
        if (defaultInput != -999)
            message += "( defaults to " + defaultInput + ")";
        ConsoleWriter.PrintWithOutLineBreak(message);
        string input = Console.ReadLine();

        if (string.IsNullOrEmpty(input) && defaultInput != -999)
            return defaultInput;

        int result = 0;
        return int.TryParse(input, out result) ? result : 0;
    }
    public static int[] GetUserInputIntArray(string message) {
        ConsoleWriter.PrintWithOutLineBreak(message + "(use comma to seperate");
        string input = Console.ReadLine();

        if (string.IsNullOrEmpty(input))
            return null;

        if (input.Contains(",")) {
            string[] splitStr = input.Split(',');
            int[] result = new int[splitStr.Length];

            for (int i = 0; i < result.Length; i++) {
                int temp = 0;
                temp = int.TryParse(splitStr[i], out temp) ? temp : 0;
                result[i] = temp;
            }
            return result;
        }
        else {
            int result = 0;
            result = int.TryParse(input, out result) ? result : 0;
            return new int[] { result };
        }
    }
    public static bool GetConfirmationFromUser(string message, bool takeNoByDefault = false) {
        if (!takeNoByDefault)
            ConsoleWriter.PrintWithOutLineBreak(message + " (enter yes or y to confirm, default) :");
        else
            ConsoleWriter.PrintWithOutLineBreak(message + " (enter no or n to confirm, default) :");

        string input = Console.ReadLine();
        bool isYes = input.ToLower() == "yes" || input.ToLower() == "y";

        if (string.IsNullOrEmpty(input)) {
            if (takeNoByDefault)
                return false;
            else
                return true;
        }

        return isYes;
    }
    public static DateTime GetDateFromUser(string message) {
        return GetDateFromUser(message, DateTime.MinValue);
    }
    public static DateTime GetDateFromUser(string message, DateTime defaultDate) {
        message += "( default or x to " + defaultDate.ShortForm() + ")";

        DateTime date = DateTime.MinValue;
        Utils.DoAction(message, ":", "",
            new Utils.ActionParams(true, "", ". default " + defaultDate.ShortForm(), delegate (string fullmessage) {
                date = defaultDate;
            }),
            new Utils.ActionParams(true, "x", "x. default " + defaultDate.ShortForm(), delegate (string fullmessage) {
                date = defaultDate;
            }),
            new Utils.ActionParams(true, "r", "r. reset " + DateTime.MinValue.ShortForm(), delegate (string fullmessage) {
                date = DateTime.MinValue;
            }),
            new Utils.ActionParams(true, "0", "0. today " + Utils.Now.ShortForm(), delegate (string fullmessage) {
                date = Utils.Now;
            }),
            new Utils.ActionParams(true, "-1", "-1. yest " + Utils.Now.AddDays(-1).ShortForm(), delegate (string fullmessage) {
                date = Utils.Now.AddDays(-1);
            }),
            new Utils.ActionParams(true, "-2", "-2. " + Utils.Now.AddDays(-2).ShortForm(), delegate (string fullmessage) {
                date = Utils.Now.AddDays(-2);
            }),
            new Utils.ActionParams(true, "-3", "-3. " + Utils.Now.AddDays(-3).ShortForm(), delegate (string fullmessage) {
                date = Utils.Now.AddDays(-3);
            }),
            new Utils.ActionParams(true, "1", "1. tomo " + Utils.Now.AddDays(1).ShortForm(), delegate (string fullmessage) {
                date = Utils.Now.AddDays(1);
            }),
            new Utils.ActionParams(true, "2", "2. " + Utils.Now.AddDays(2).ShortForm(), delegate (string fullmessage) {
                date = Utils.Now.AddDays(2);
            }),
            new Utils.ActionParams(true, "3", "3. " + Utils.Now.AddDays(3).ShortForm(), delegate (string fullmessage) {
                date = Utils.Now.AddDays(3);
            }),
            new Utils.ActionParams(true, "4", "4. " + Utils.Now.AddDays(4).ShortForm(), delegate (string fullmessage) {
                date = Utils.Now.AddDays(4);
            }),
            new Utils.ActionParams(true, "7", "7. " + Utils.Now.AddDays(7).ShortForm(), delegate (string fullmessage) {
                date = Utils.Now.AddDays(7);
            }),
            new Utils.ActionParams(true, "c", "c. custom ", delegate (string fullmessage) {
                date = GetCustomDateFromUser("Enter Date (mm/dd):");
            })
        );
        return date;
    }
    public static DateTime GetCustomDateFromUser(string message) {
        ConsoleWriter.PrintWithOutLineBreak(message);

        string input = Console.ReadLine();
        DateTime result;
        if (DateTime.TryParse(input, null, System.Globalization.DateTimeStyles.RoundtripKind, out result))
            return result;
        else
            return DateTime.MinValue;
    }
    public static DateTime GetCustomDateFromUser(string message, DateTime defaultValue, bool showXToReset = true) {
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



    // Select User Action from the possible action list

    #region SELECT USER ACTION

    public class ActionParams {
        public ActionParams(bool show, string userAction, string heading, System.Action<System.String> actionToPerform) {
            this.userAction = userAction;
            this.heading = heading;
            this.actionToPerform = actionToPerform;
            this.show = show;
        }
        public string userAction;
        public string heading;
        public System.Action<System.String> actionToPerform;
        public bool show;
    }
    public static void SetMaxColumnsForOptions(int max) {
        maxColumnsInALine = max;
    }
    public static void DoAction(string heading, string question, string defaultAction, params ActionParams[] prms) {
        bool isDefaultActionAvailable = !string.IsNullOrEmpty(defaultAction);
        ActionParams selectedAction = null;

        string userInput = "";
        while (true) {
            int maxCharLength = 0;
            foreach (ActionParams prm in prms) {
                if (prm.heading.Length > maxCharLength)
                    maxCharLength = prm.heading.Length;
            }
            maxCharLength += 2;

            System.Text.StringBuilder consoleMsg = new System.Text.StringBuilder();
            consoleMsg.Append("" + heading + "\n");
            int shown = 0;
            for (int i = 0; i < prms.Length; i++) {
                if (!prms[i].show)
                    continue;
                consoleMsg.Append(string.Format("{0,-" + maxCharLength + "}", prms[i].heading));
                if ((shown + 1) % maxColumnsInALine == 0 && (shown + 1) < prms.Length)
                    consoleMsg.Append("\n");
                shown++;
            }

            consoleMsg.Append("\n" + question);
            // if ( isDefaultActionAvailable )
            //     consoleMesg.Append( "(default is " + defaultAction + ")");

            userInput = GetUserInputString(consoleMsg.ToString(), ConsoleColor.Blue, isDefaultActionAvailable ? defaultAction : "");
            //ConsoleWriter.ResetConsoleColor();

            foreach (ActionParams prm in prms) {
                if (prm.userAction == userInput) {
                    selectedAction = prm;
                    break;
                }
            }
            // trying to see if its a big command with context
            if (selectedAction == null && (userInput.Contains("|"))) {
                string command = userInput.Split("|")[0];
                foreach (ActionParams prm in prms) {
                    if (prm.userAction.Contains(command)) {
                        selectedAction = prm;
                        break;
                    }
                }
            }
            if (selectedAction != null)
                break;
            else
                ConsoleWriter.Print("Action was invalid. Please try again...");
        }

        selectedAction.actionToPerform(userInput);
    }
    public static string SelectFrom(string heading, string question, string defaultValue, params string[] prms) {
        while (true) {
            int maxCharLength = 0;
            foreach (string prm in prms) {
                if (prm.Length > maxCharLength)
                    maxCharLength = prm.Length;
            }
            maxCharLength += 4;
            int defaultValueInt = -1;

            System.Text.StringBuilder consoleMesg = new System.Text.StringBuilder();
            consoleMesg.Append("" + heading + "\n");
            for (int i = 0; i < prms.Length; i++) {
                consoleMesg.Append(string.Format("{0,-" + maxCharLength + "}", (i + 1) + ". " + prms[i]));
                if ((i + 1) % maxColumnsInALine == 0 && (i + 1) < prms.Length)
                    consoleMesg.Append("\n");
                if (defaultValue == prms[i])
                    defaultValueInt = i;
            }

            consoleMesg.Append("\n" + question);
            int userInput = GetUserInputInt(consoleMesg.ToString(), defaultValueInt + 1);

            userInput--;
            if (userInput < prms.Length && userInput >= 0)
                return prms[userInput];

            ConsoleWriter.Print("Action was invalid. Please try again...");
        }
    }
    private static int maxColumnsInALine = 5;
    #endregion

    // Utils

    public static int Clamp(int value, int min, int max) {
        return (value < min) ? min : (value > max) ? max : value;
    }
    public static T ParseEnum<T>(string value) {
        return (T)Enum.Parse(typeof(T), value, true);
    }
    public static void Assert(bool condition, string message = null) {
        if (!condition)
            ConsoleWriter.Print("==== ASSERT here =======");
        Debug.Assert(condition, message);
    }
    public static bool IsThisToday(DateTime date) {
        if (date.Day == Utils.Now.Day &&
            date.Month == Utils.Now.Month &&
            date.Year == Utils.Now.Year)
            return true;
        return false;
    }
    public static int ConvertRange(int min, int max, int newMin, int newMax, int value) {
        float ratio = (value - min) / (float)(max - min);
        return (int)(newMin + (newMax - newMin) * ratio);
    }
    public static float ConvertRange(float min, float max, float newMin, float newMax, float value) {
        float ratio = (value - min) / (float)(max - min);
        return (float)(newMin + (newMax - newMin) * ratio);
    }
    public static int Lerp(int min, int max, int ratio) {
        return (int)(min + (max - min) * ratio);
    }
    public static DateTime Now {
        get {
            return DateTime.Now.ZeroTime();
        }
    }

    public static bool CreateFileIfNotExit ( string path, string templateFile ) {
        if ( !File.Exists ( path )) {
            Assert(File.Exists(templateFile), "Template file doesnt exist.");
            File.Copy(templateFile, path);
            return true;
        }
        return false;
    }
}

// Date Utils

public static class DateExt {
    public static string ShortForm ( this DateTime date ) {
        return date.Month + "/" + date.Date;
    }
}

// utilities

public static class StringExt {
    public static string Truncate(this string value, int maxLength) {
        if (string.IsNullOrEmpty(value)) return value;
        return value.Length <= maxLength ? value : value.Substring(0, maxLength);
    }
    public static DateTime ZeroTime(this DateTime date) {
        return new DateTime(date.Year, date.Month, date.Day, 0, 0, 0);
    }
    public static bool IsSameAs(this DateTime date, DateTime other) {
        if (date.Day == other.Day &&
            date.Month == other.Month &&
            date.Year == other.Year)
            return true;
        return false;
    }
    public static bool IsToday(this DateTime date, int offset = 0) {
        return IsSameAs(date, Utils.Now.AddDays(offset));
    }
    public static bool IsThisMinDate(this DateTime date) {
        return date == DateTime.MinValue;
    }
}

public class Pair<T, U> {
    public Pair() {
    }
    public Pair(T first, U second) {
        this.First = first;
        this.Second = second;
    }
    public T First { get; set; }
    public U Second { get; set; }
};

public class Triple<T, U, V> {
    public Triple() {
    }
    public Triple(T first, U second, V third) {
        this.First = first;
        this.Second = second;
        this.Third = third;
    }
    public T First { get; set; }
    public U Second { get; set; }
    public V Third { get; set; }

    public Triple<T, U, V> Clone() {
        Triple<T, U, V> newone = new Triple<T, U, V>();
        newone.First = First;
        newone.Second = Second;
        newone.Third = Third;
        return newone;
    }
};

public class Quadrupel<T, U, V, W> {
    public Quadrupel() {
    }
    public Quadrupel(T first, U second, V third, W forth) {
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

public static class ListExtension {
    public static bool Contains<T>(this T[] arr, T arrObj) {
        if (arr == null || arr.Length == 0)
            return false;
        for (int i = 0; i < arr.Length; i++) {
            if (arr[i].Equals(arrObj))
                return true;
        }
        return false;
    }
}
