using Chirp.Shared;

namespace Chirp.CLI.Client;

internal static class UserInterface
{
    public static void PrintCheeps(IEnumerable<Cheep> cheeps)
    {
        foreach (var c in cheeps)
        {
            var localTime = DateTimeOffset.FromUnixTimeSeconds(c.Timestamp).ToLocalTime();
            var timeStr = localTime.ToString("dd.MM.yy HH.mm.ss"); // matches your earlier output style
            Console.WriteLine($"{c.Author} @ {timeStr}: {c.Message}");
        }
    }
}
