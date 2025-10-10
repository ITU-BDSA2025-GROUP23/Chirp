using Database;

var builder = WebApplication.CreateBuilder(args);

// Resolve DB path (env > appsettings > connstring > fallback)
string? dbPath = Environment.GetEnvironmentVariable("CHIRPDBPATH")
                 ?? builder.Configuration["Database:Path"]
                 ?? builder.Configuration.GetConnectionString("Chirp")
                 ?? "db/chirp.db"; // keep fallback inside db/ to match your repo

// Make it absolute so the host (incl. test host) always finds the same file
if (!Path.IsPathRooted(dbPath))
{
    dbPath = Path.Combine(builder.Environment.ContentRootPath, dbPath);
}

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddSingleton<ICheepService, CheepService>();
builder.Services.AddSingleton<DBFacade>(_ => new DBFacade(dbPath));


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.MapRazorPages();
app.Run();
public partial class Program { }