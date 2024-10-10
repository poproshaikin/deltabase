using System.Diagnostics;

namespace Utils;

public static class Logger
{
    public static void Log(string s)
    {
        Console.WriteLine( $"{DateTime.Now} : {s}");
    }
    
    public static void Log(params string[] lines)
    {
        foreach (var s in lines)
        {
            Log(s);
        }
    }
    
    public static void LogHeader(string s)
    {
        Console.WriteLine($"{DateTime.Now} --- {s} ---");
    }

    public static void Debug(string s)
    {
        StackFrame penultimate = GetStackMethods()[1];
        
        Console.WriteLine();
        Console.WriteLine($" {penultimate.ToString()}");
        Console.WriteLine($" ^^^ ^^^ ^^^ ^^^");
        Log(s);
        Console.WriteLine();
    }
    
    private static StackFrame[] GetStackMethods()
    {
        return new StackTrace(true).GetFrames();
    }
}