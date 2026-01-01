using Chirp.Shared;
using Microsoft.AspNetCore.Http.Json;
using SimpleDB;

var builder = WebApplication.CreateBuilder(args);

// Force JSON to use PascalCase property names (Author/Message/Timestamp)
builder.Services.Configure<JsonOptions>(o =>
{
    o.SerializerOptions.PropertyNamingPolicy = null;
    o.SerializerOptions.DictionaryKeyPolicy = null;
});

var app = builder.Build();

// Use env var on Azure; fallback to local CSV file during dev
var dbPath =
    Environment.GetEnvironmentVariable("CHIRP_DB_PATH")
    ?? Path.Combine(app.Environment.ContentRootPath, "chirp_db.csv");

var dir = Path.GetDirectoryName(dbPath);
if (!string.IsNullOrWhiteSpace(dir))
{
    Directory.CreateDirectory(dir);
}

IDatabaseRepository<Cheep> db = new CSVDatabase<Cheep>(dbPath);

// Optional: nicer root route so browser isn't 404
app.MapGet("/", () => Results.Ok("Chirp.CSVDB.Service is running. Try GET /cheeps"));

app.MapGet("/cheeps", () => Results.Ok(db.Read()));

app.MapPost("/cheep", (Cheep cheep) =>
{
    db.Store(cheep);
    return Results.Ok();
});

app.Run();

// for integration tests (WebApplicationFactory)
public partial class Program { }
