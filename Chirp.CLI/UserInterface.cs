using System;
using System.Collections.Generic;

namespace Chirp.CLI;

public static class UserInterface
{
    private static string DisplayFormat = "MM/dd/yy HH:mm:ss";
    public static void ReadCheep(Cheep cheep)
    {
            DateTimeOffset dto = DateTimeOffset.FromUnixTimeSeconds(cheep.Timestamp);
            string realDate = dto.LocalDateTime.ToString(DisplayFormat);
            Console.Write(cheep.Author + " @ " + realDate + ": " + cheep.Message + "\n");
    }

    public static void Saved(String message)
    {
        Console.WriteLine(" the chirp: " + message + " was saved");
    }
    
    public static void PrintUsage()
    {
        Console.WriteLine("Usage:");
        Console.WriteLine("  Chirp.CLI read");
        Console.WriteLine("  Chirp.CLI cheep \"Hello, world!\"");
        Console.WriteLine();
        Console.WriteLine("Or via dotnet run --:");
        Console.WriteLine("  dotnet run -- read");
        Console.WriteLine("  dotnet run -- cheep \"Hello, world!\"");
    }
}