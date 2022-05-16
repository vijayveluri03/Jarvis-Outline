using System;
using System.Collections.Generic;
using System.Diagnostics;

public static class ConsoleWriter
{
    public const int SLEEP_TIME_IN_MS = 30;
    private static ConsoleColor previousColor = ConsoleColor.Black;
    // Foreground Colors for the text

    public static void OnAppLaunched()
    {
        previousColor = Console.ForegroundColor;
    }
    public static void OnAppKilled()
    {
        Console.ForegroundColor = previousColor;
    }

    public static void PushColor(ConsoleColor color)
    {
        foregroundTextColorStack.Push(color);
        Console.ForegroundColor = color;
    }
    public static void PopColor()
    {
        Utils.Assert(foregroundTextColorStack.Count > 0);
        foregroundTextColorStack.Pop();
    }

    // Print message in console

    public static void PrintURL(string message)
    {
        Console.ForegroundColor = foregroundTextColorStack.Peek();

        Console.Write(message + "\n");
        System.Threading.Thread.Sleep(SLEEP_TIME_IN_MS);
    }
    public static void Print(string message, params object[] parms)
    {
        ConsoleColor foregroundColor;
        if (foregroundTextColorStack.TryPeek(out foregroundColor))
            Console.ForegroundColor = foregroundColor;

        Console.Write(message + "\n", parms);
        System.Threading.Thread.Sleep(SLEEP_TIME_IN_MS);
    }
    public static void PrintInColor(string message, ConsoleColor foregroundColor, params object[] parms)
    {
        PushColor(foregroundColor);
        Print(message, parms);
        PopColor();
    }
    public static void PrintWithOutLineBreak(string message, bool enableDelay = true, params object[] parms)
    {
        Console.ForegroundColor = foregroundTextColorStack.Peek();

        Console.Write(message, parms);
        if (enableDelay)
            System.Threading.Thread.Sleep(SLEEP_TIME_IN_MS);
    }
    public static void Print()
    {
        PrintNewLine();
    }
    public static void PrintNewLine()
    {
        Print("");
    }

    private static Stack<ConsoleColor> foregroundTextColorStack = new Stack<ConsoleColor>();
}
