
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FoundryBlazor.Extensions;
public static class ConsoleHelpers
{
    public static void WriteTrace(this String str)
    {
        $"... {str}".WriteLine(ConsoleColor.DarkMagenta);
    }
    public static void WriteLine<T>(this T entity, ConsoleColor? color = null)
    {
        if (color.HasValue)
            Console.ForegroundColor = color.Value;

        var options = new JsonSerializerOptions()
        {
            IncludeFields = true,
            IgnoreReadOnlyFields = true,
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        Console.WriteLine(entity != null ? JsonSerializer.Serialize(entity, typeof(T), options) : "null");
        Console.ResetColor();
    }



    public static string WriteSuccess(this string message)
    {
        return message.WriteLine(ConsoleColor.Green);
    }
    public static string WriteError(this string message)
    {
        return message.WriteLine(ConsoleColor.Red);
    }
	public static string WriteInfo(this string message)
    {
        return message.WriteLine(ConsoleColor.Cyan);
    }

    public static string WriteLine(this string message, ConsoleColor? color = null)
    {
        if (color.HasValue)
            Console.ForegroundColor = color.Value;
        Console.WriteLine(message);
        Console.ResetColor();
        return message;
    }

    public static string Write(this string message, ConsoleColor? color = null)
    {
        if (color.HasValue)
            Console.ForegroundColor = color.Value;
        Console.Write(message);
        Console.ResetColor();
        return message;
    }

    public static string WriteInLine(this string message, ConsoleColor? color = null)
    {
        if (color.HasValue)
            Console.ForegroundColor = color.Value;
        Console.Write(message.Trim() + " "); //make sure there's a space at the end of the message since it's inline.
        Console.ResetColor();
        return message;
    }

}


