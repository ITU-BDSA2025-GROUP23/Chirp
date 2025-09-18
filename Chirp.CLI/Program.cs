using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using SimpleDB;


namespace Chirp.CLI{
    public record Cheep(string Author, string Message, long Timestamp);

    internal static class Program
    {
   
        private static string DbFile = Path.Combine(AppContext.BaseDirectory, "Data", "chirp_cli_db.csv");
		
		private static CSVDatabase<Cheep> db = new CSVDatabase<Cheep>(DbFile);
        
        static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                UserInterface.PrintUsage();
                return 1;
            }

            var cmd = args[0].ToLowerInvariant();
            try
            {
                switch (cmd)
                {
                    case "read":
                        ReadCheeps();
                        return 0;

                    case "cheep":
                        if (args.Length < 2)
                        {
                            Console.Error.WriteLine("Missing message. Usage: Chirp.CLI cheep \"Hello, world!\"");
                            return 2;
                        }
                        var message = string.Join(" ", args, 1, args.Length - 1);
                        WriteCheep(message);
                        return 0;

                    default:
                        Console.Error.WriteLine($"Unknown command: {cmd}");
                        UserInterface.PrintUsage();
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

        public static void ReadCheeps()
        {
            foreach (var cheep in db.Read())
            {
                UserInterface.ReadCheep(cheep);
            }
        }
        
        
        public static void WriteCheep(string message)
        {
            var cheep = new Cheep(Environment.UserName, message, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        
            db.Store(cheep);

            UserInterface.Saved(message);
        }
	}
}