using Chirp.Razor.DataModel;
using Database;
using Microsoft.EntityFrameworkCore;
using Chirp.Razor;

//Region build app and register DbContext
var builder = WebApplication.CreateBuilder(args);


builder.Services.AddRazorPages();

string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ChatDBContext>(options => options.UseSqlite(connectionString));
builder.Services.AddScoped<ICheepService, CheepService>();

var app = builder.Build();


//Region MRGA
app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapRazorPages();

app.Run();
