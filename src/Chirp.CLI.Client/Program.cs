using System.Net.Http.Json;
using Chirp.Shared;
using CommandLine;

namespace Chirp.CLI.Client;

internal static class Program
{
    public static async Task<int> Main(string[] args)
    {
        return await Parser.Default
            .ParseArguments<ReadOptions, CheepOptions>(args)
            .MapResult(
                (ReadOptions o) => RunRead(o),
                (CheepOptions o) => RunCheep(o),
                _ => Task.FromResult(1)
            );
    }

    private static HttpClient CreateHttpClient()
    {
        var baseUrl = Environment.GetEnvironmentVariable("CHIRP_DB_URL") ?? "http://localhost:5263";

        if (!baseUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !baseUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            baseUrl = "http://" + baseUrl;
        }

        baseUrl = baseUrl.TrimEnd('/');

        return new HttpClient
        {
            BaseAddress = new Uri(baseUrl, UriKind.Absolute),
            Timeout = TimeSpan.FromSeconds(10)
        };
    }

    private static async Task<int> RunRead(ReadOptions options)
    {
        try
        {
            using var http = CreateHttpClient();

            var cheeps = await http.GetFromJsonAsync<List<Cheep>>("/cheeps");
            if (cheeps is null)
            {
                Console.Error.WriteLine("Error: Received no data from /cheeps.");
                return 1;
            }

            var ordered = cheeps.OrderBy(c => c.Timestamp).ToList();

            if (options.Count is > 0)
            {
                ordered = ordered.TakeLast(options.Count.Value).ToList();
            }

            UserInterface.PrintCheeps(ordered);
            return 0;
        }
        catch (HttpRequestException ex)
        {
            Console.Error.WriteLine($"HTTP error while reading cheeps: {ex.Message}");
            return 1;
        }
        catch (TaskCanceledException)
        {
            Console.Error.WriteLine("HTTP timeout while reading cheeps.");
            return 1;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Unexpected error while reading cheeps: {ex.Message}");
            return 1;
        }
    }

    private static async Task<int> RunCheep(CheepOptions options)
    {
        try
        {
            using var http = CreateHttpClient();

            var author = Environment.UserName;
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            var cheep = new Cheep(author, options.Message, timestamp);

            var response = await http.PostAsJsonAsync("/cheep", cheep);
            if (!response.IsSuccessStatusCode)
            {
                Console.Error.WriteLine($"Error: POST /cheep failed ({(int)response.StatusCode} {response.ReasonPhrase}).");
                return 1;
            }

            Console.WriteLine($"the chirp: {options.Message} was saved");
            return 0;
        }
        catch (HttpRequestException ex)
        {
            Console.Error.WriteLine($"HTTP error while storing cheep: {ex.Message}");
            return 1;
        }
        catch (TaskCanceledException)
        {
            Console.Error.WriteLine("HTTP timeout while storing cheep.");
            return 1;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Unexpected error while storing cheep: {ex.Message}");
            return 1;
        }
    }
}

[Verb("read", HelpText = "Read cheeps from the remote database.")]
internal sealed class ReadOptions
{
    // This makes "read 10" work (positional argument)
    [Value(0, Required = false, HelpText = "Optional: show only the last N cheeps.")]
    public int? Count { get; set; }
}

[Verb("cheep", HelpText = "Store a cheep in the remote database.")]
internal sealed class CheepOptions
{
    [Value(0, Required = true, HelpText = "The cheep message.")]
    public required string Message { get; set; }
}
