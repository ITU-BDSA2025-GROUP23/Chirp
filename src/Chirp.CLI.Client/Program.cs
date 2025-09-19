using CommandLine;
using SimpleDB;

namespace Chirp.CLI.client;

public record Cheep(string Author, string Message, long Timestamp);
internal static class Program
{
    private static readonly string DbFile = Path.Combine(AppContext.BaseDirectory, "Data", "chirp_cli_db.csv");
    private static readonly CSVDatabase<Cheep> db = new CSVDatabase<Cheep>(DbFile);

    static int Main(string[] args)
    {
        return Parser.Default
            .ParseArguments<Read, Cheepd>(args)
            .MapResult(
                (Read opt) =>
                {
                    var cheeps = db.Read();
                    foreach (var c in cheeps) UserInterface.ReadCheep(c);
                    return 0;
                },
                (Cheepd opt) =>
                {
                    var item = new Cheep(Environment.UserName, opt.Message, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                    db.Store(item);
                    UserInterface.Saved(opt.Message);
                    return 0;
                },
                errs => 1
            );
    }
    [Verb("read", HelpText = "Read all cheeps.")]
    public class Read
    {
    
    }

    [Verb("cheep", HelpText = "make a new cheep.")]
    public class Cheepd
    {
        [Value(0, Required = true, HelpText = "The cheep message text.")]
        public string Message { get; set; } = "";
    }
}