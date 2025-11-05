using Chirp.Infrastructure.DataModel;
using Chirp.Infrastructure.Repositories;
using Database;
using Microsoft.EntityFrameworkCore;
using Chirp.Web;
using Chirp.Web.db; 
using Chirp.Core;

public class Program
{
    public static void Main(string[] args)
    {
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

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();
        app.MapRazorPages();

        app.Run();
    }
}