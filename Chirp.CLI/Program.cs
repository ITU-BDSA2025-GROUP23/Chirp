using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration; 
using SimpleDB;

namespace Chirp.cli {
    public record Cheep(string Author, string Message, long Timestamp);

    internal static class Program
    {
        // CSV database file
        private static string DbFile = Path.Combine(AppContext.BaseDirectory, "Data", "chirp_cli_db.csv");
		
		private static CSVDatabase<Cheep> db = new CSVDatabase<Cheep>(DbFile);
			
        // Output time format to match example: "08/01/23 14:09:20"
        private static string DisplayFormat = "MM/dd/yy HH:mm:ss";

        static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                PrintUsage();
                return 1;
            }

            var cmd = args[0].ToLowerInvariant();
            try
            {
                switch (cmd)
                {
                    case "read":
                        ReadCheeps(DbFile, DisplayFormat);
                        return 0;

                    case "cheep":
                        if (args.Length < 2)
                        {
                            Console.Error.WriteLine("Missing message. Usage: Chirp.CLI cheep \"Hello, world!\"");
                            return 2;
                        }
                        var message = string.Join(" ", args, 1, args.Length - 1);
                        WriteCheep(message, DbFile);
                        return 0;

                    default:
                        Console.Error.WriteLine($"Unknown command: {cmd}");
                        PrintUsage();
                        return 3;
                }
            }
            catch (Exception ex)
            {
                // Keep it simple: predictable error reporting for CLI use
                Console.Error.WriteLine($"Error: {ex.Message}");
                return 10;
            }
        }

        private static void PrintUsage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("  Chirp.CLI read");
            Console.WriteLine("  Chirp.CLI cheep \"Hello, world!\"");
            Console.WriteLine();
            Console.WriteLine("Or via dotnet run --:");
            Console.WriteLine("  dotnet run -- read");
            Console.WriteLine("  dotnet run -- cheep \"Hello, world!\"");
        }

        private static void ReadCheeps(string DbFile, string DisplayFormat)
        {
            if (!File.Exists(DbFile))
            {
                // Empty DB is not an error; just no output.
                return;
            }

            
                foreach (var cheep in db.Read())
                {
                    DateTimeOffset dto = DateTimeOffset.FromUnixTimeSeconds(cheep.Timestamp);
                    string realDate = dto.LocalDateTime.ToString(DisplayFormat);
                    Console.Write(cheep.Author + " @ " + realDate + ": " + cheep.Message + "\n");
                    
                }
           
        }

        private static void WriteCheep(string message, string DbFile)
        {
          
        	var cheep = new Cheep(Environment.UserName, message, DateTimeOffset.UtcNow.ToUnixTimeSeconds());

        	db.Store(cheep);
      
                    
            Console.WriteLine(" the chirp: " + message + " was saved");
        }
	}
}