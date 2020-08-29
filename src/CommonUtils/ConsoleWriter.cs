using System;
using System.Collections.Generic;
using System.Diagnostics;

public static class ConsoleWriter {
    public const int SLEEP_TIME_IN_MS = 30;

    // Foreground Colors for the text

    public static void PushColor(ConsoleColor color) {
        foregroundTextColorStack.Push(color);
        Console.ForegroundColor = color;
    }
    public static void PopColor() {
        Utils.Assert(foregroundTextColorStack.Count > 0);
        foregroundTextColorStack.Pop();
    }


    // Print message in console

    public static void PrintURL(string message) {
        Console.ForegroundColor = foregroundTextColorStack.Peek();

        Console.Write(message + "\n");
        System.Threading.Thread.Sleep(SLEEP_TIME_IN_MS);
    }
    public static void Print(string message, params object[] parms) {
        Console.ForegroundColor = foregroundTextColorStack.Peek();

        Console.Write(message + "\n", parms);
        System.Threading.Thread.Sleep(SLEEP_TIME_IN_MS);
    }
    public static void PrintInColor(string message, ConsoleColor foregroundColor, params object[] parms) {
        PushColor(foregroundColor);
        Print(message, parms);
        PopColor();
    }
    public static void PrintInRed(string message, params object[] parms) {
        PushColor(ConsoleColor.Red);
        Print(message, parms);
        PopColor();
    }
    public static void PrintWithOutLineBreak(string message, bool enableDelay = true, params object[] parms) {
        Console.ForegroundColor = foregroundTextColorStack.Peek();

        Console.Write(message, parms);
        if (enableDelay)
            System.Threading.Thread.Sleep(SLEEP_TIME_IN_MS);
    }
    public static void PrintWithExtraLineBreaks(string message, int linebreakCountBefore, int linebreakCountAfter, params object[] parms) {
        Console.ForegroundColor = foregroundTextColorStack.Peek();

        while (linebreakCountBefore > 0) {
            Print("");
            linebreakCountBefore--;
        }
        Console.Write(message, parms);
        while (linebreakCountAfter > 0) {
            Print("");
            linebreakCountAfter--;
        }
        System.Threading.Thread.Sleep(SLEEP_TIME_IN_MS);
    }
    public static void PrintNewLine() {
        Print("");
    }

    private static Stack<ConsoleColor> foregroundTextColorStack = new Stack<ConsoleColor>();
}
