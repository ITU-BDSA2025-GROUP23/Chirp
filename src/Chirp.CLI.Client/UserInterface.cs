using System;
using System.Collections.Generic;

namespace Chirp.CLI.Client;

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
}