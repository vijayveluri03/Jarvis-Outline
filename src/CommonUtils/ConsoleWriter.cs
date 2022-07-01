using System;
using System.Collections.Generic;
using System.Diagnostics;

public static class ConsoleWriter
{
    private static ConsoleColor previousColor = ConsoleColor.Black;
    // Foreground Colors for the text

    public static void Initialize()
    {
        previousColor = Console.ForegroundColor;
    }
    public static void DestroyAndCleanUp()
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
        if( foregroundTextColorStack.Count > 0 )
            Console.ForegroundColor = foregroundTextColorStack.Peek();
    }

    // Print message in console

    public static void PrintURL(string message)
    {
        Console.ForegroundColor = foregroundTextColorStack.Peek();

        Console.Write(message + "\n");
    }
    public static void Print(string message, params object[] parms)
    {
        ConsoleColor foregroundColor;
        if (foregroundTextColorStack.TryPeek(out foregroundColor))
            Console.ForegroundColor = foregroundColor;

        Console.Write(message + "\n", parms);
    }
    public static void PrintText(string message)
    {
        ConsoleColor foregroundColor;
        if (foregroundTextColorStack.TryPeek(out foregroundColor))
            Console.ForegroundColor = foregroundColor;

        Console.Write(message + "\n");
    }
    public static void PrintInColor(string message, ConsoleColor foregroundColor, params object[] parms)
    {
        PushColor(foregroundColor);
        Print(message, parms);
        PopColor();
    }
    public static void PrintWithOutLineBreak(string message, params object[] parms)
    {
        Console.ForegroundColor = foregroundTextColorStack.Peek();

        Console.Write(message, parms);
    }
    public static void PrintWithColorWithOutLineBreak(string message, ConsoleColor color, params object[] parms)
    {
        PushColor(color);
        Console.Write(message, parms);
        PopColor();
    }

    public static void Print()
    {
        EmptyLine();
    }
    public static void EmptyLine()
    {
        Print("");
    }
    public static void Clear()
    {
        Console.Clear();
    }

    private static Stack<ConsoleColor> foregroundTextColorStack = new Stack<ConsoleColor>();
}
