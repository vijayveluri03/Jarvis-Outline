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

    public static void PushIndent()
    {
        indentTabCount++;
        GenerateIndentationPrefix();
    }

    public static void PopIndent()
    {
        indentTabCount--;
        if ( indentTabCount < 0 )
            indentTabCount = 0;
        GenerateIndentationPrefix();
    }

    public static void ClearIndent()
    {
        indentTabCount = 0;
        GenerateIndentationPrefix();
    }

    // Print message in console

    public static void PrintURL(string message)
    {
        Console.ForegroundColor = foregroundTextColorStack.Peek();

        message = GetIndentationPrefix() + message;
        Console.Write(message + "\n");
        OnNewLine();
    }
    public static void Print(string message, params object[] parms)
    {
        if (foregroundTextColorStack.Count > 0)
            Console.ForegroundColor = foregroundTextColorStack.Peek();

        message = GetIndentationPrefix() + message;
        Console.Write(message + "\n", parms);
        OnNewLine();
    }
    public static void PrintText(string message)
    {
        if (foregroundTextColorStack.Count > 0)
            Console.ForegroundColor = foregroundTextColorStack.Peek();

        message = GetIndentationPrefix() + message;
        Console.Write(message + "\n");
        OnNewLine();
    }
    public static void PrintInColor(string message, ConsoleColor foregroundColor, params object[] parms)
    {
        PushColor(foregroundColor);
        Print(message, parms);
        PopColor();
    }

    public static void IndentWithOutLineBreak()
    {
        Console.Write(GetIndentationPrefix());
        OnAvoidingNewLine();
    }
    public static void PrintWithOutLineBreak(string message, params object[] parms)
    {
        Console.ForegroundColor = foregroundTextColorStack.Peek();
        Console.Write(message, parms);
        OnAvoidingNewLine();
    }
    public static void PrintWithColorWithOutLineBreak(string message, ConsoleColor color, params object[] parms)
    {
        PushColor(color);
        Console.Write(message, parms);
        PopColor();
        OnAvoidingNewLine();
    }

    public static void InsertNewLineIfAvoidingLineBreaks()
    {
        if (AreLineBreaksAvoided())
            EmptyLine();
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


    private static string GetIndentationPrefix()
    {
        return indentationPrefix;
    }
    private static void GenerateIndentationPrefix()
    {
        indentationPrefix = "";
        for (int i = 0; i < indentTabCount; i++)
        {
            //indentationPrefix += "\t";
            indentationPrefix += "  ";
        }
    }

    private static void OnNewLine()
    {
        isAvoidingLineBreaks = false;
    }
    private static void OnAvoidingNewLine()
    {
        isAvoidingLineBreaks = true;
    }
    private static bool AreLineBreaksAvoided()
    {
        return isAvoidingLineBreaks;
    }

    private static Stack<ConsoleColor> foregroundTextColorStack = new Stack<ConsoleColor>();
    private static int indentTabCount = 0;
    private static string indentationPrefix = "";
    private static bool isAvoidingLineBreaks = false;
}
