using System.IO;             
using System.Linq;            
using Microsoft.AspNetCore.Http;
using SimpleDB;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var dbPath = Path.Combine(AppContext.BaseDirectory, "Data", "chirp_cli_db.csv");
var db = new CSVDatabase<Cheep>(dbPath);

app.MapGet("/cheeps", () =>
{
    var cheeps = db.Read().ToList();
    Console.WriteLine($"Read {cheeps.Count} cheeps from CSV");
    foreach (var c in cheeps)
    {
        Console.WriteLine(c);
    }
    return Results.Json(cheeps);
});

app.MapPost("/cheep", (Cheep cheep) =>
{
    db.Store(cheep);
    return Results.Created($"/cheeps/{cheep.Timestamp}", cheep);
});

app.Run();

public record Cheep (string Author, string Message, long Timestamp);
