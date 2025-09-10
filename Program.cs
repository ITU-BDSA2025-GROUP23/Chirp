using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace Chirp.cli {
    public record Cheep(string Author, string Message, long Timestamp);
    internal static class Program
    {
        // CSV database file
        private static string DbFile = Path.Combine(AppContext.BaseDirectory, "Data", "chirp_cli_db.csv");

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

            foreach (var line in File.ReadLines(DbFile, Encoding.UTF8))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                var fields = ParseCsvLine(line);
                if (fields.Count < 3) continue;

                var author = fields[0];
                if (!long.TryParse(fields[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var unix))
                    continue;

                var message = fields[2];

                var dtLocal = DateTimeOffset.FromUnixTimeSeconds(unix).ToLocalTime().DateTime;
                Console.WriteLine($"{author} @ {dtLocal.ToString(DisplayFormat, CultureInfo.InvariantCulture)}: {message}");
            }
        }

        private static void WriteCheep(string message, string DbFile)
        {
            // Get OS username for author
            var author = Environment.UserName ?? "unknown";

            // Current Unix timestamp (UTC seconds since epoch)
            var unixNow = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            // Escape fields for CSV (RFC 4180-ish minimal: double quotes inside and wrap if needed)
            var authorEsc = CsvEscape(author);
            var messageEsc = CsvEscape(message);

            var line = $"{authorEsc},{unixNow.ToString(CultureInfo.InvariantCulture)},{messageEsc}";

            // Ensure file exists and append
            using var sw = new StreamWriter(DbFile, append: true, Encoding.UTF8);
            sw.WriteLine(line);
        }

        /// <summary>
        /// Escapes a CSV field by doubling quotes and wrapping in quotes if it contains comma, quote, or newline.
        /// </summary>
        private static string CsvEscape(string value)
        {
            if (value == null) return "\"\"";
            var needsQuotes = value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r');
            var escaped = value.Replace("\"", "\"\"");
            return needsQuotes ? $"\"{escaped}\"" : escaped;
        }

        /// <summary>
        /// Parses a single CSV line into fields, handling quotes and commas.
        /// Minimal compliant parser for this assignment.
        /// </summary>
        private static List<string> ParseCsvLine(string line)
        {
            var result = new List<string>();
            if (line == null) return result;

            var sb = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (inQuotes)
                {
                    if (c == '"')
                    {
                        // Double-quote inside quoted field -> literal quote
                        if (i + 1 < line.Length && line[i + 1] == '"')
                        {
                            sb.Append('"');
                            i++; // skip second quote
                        }
                        else
                        {
                            // end of quoted field
                            inQuotes = false;
                        }
                    }
                    else
                    {
                        sb.Append(c);
                    }
                }
                else
                {
                    if (c == ',')
                    {
                        result.Add(sb.ToString());
                        sb.Clear();
                    }
                    else if (c == '"')
                    {
                        inQuotes = true;
                    }
                    else
                    {
                        sb.Append(c);
                    }
                }
            }

            result.Add(sb.ToString());
            return result;
        }
    }
}
