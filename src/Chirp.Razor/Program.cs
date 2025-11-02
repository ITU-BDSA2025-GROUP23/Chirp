using Chirp.Razor.DataModel;
using Database;
using Microsoft.EntityFrameworkCore;
using Chirp.Razor;
using Chirp.Razor.db;

//Region build app and register DbContext
var builder = WebApplication.CreateBuilder(args);


builder.Services.AddRazorPages();

string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ChatDBContext>(options => options.UseSqlite(connectionString));
builder.Services.AddScoped<ICheepRepository, CheepRepository>();

var app = builder.Build();



using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ChatDBContext>();
    db.Database.Migrate();                  
    DbInitializer.SeedDatabase(db);        
}


//Region MRGA
app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapRazorPages();

app.Run();
