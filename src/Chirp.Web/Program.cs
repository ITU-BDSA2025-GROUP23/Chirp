using Chirp.Infrastructure.DataModel;
using Chirp.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Chirp.Web;
using Chirp.Web.db;
using Chirp.Core;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var githubClientId = builder.Configuration["Authentication:GitHub:ClientId"];
        var githubClientSecret = builder.Configuration["Authentication:GitHub:ClientSecret"];
        
		if (githubClientId is null || githubClientSecret is null)
		{
    		throw new InvalidOperationException(
        	"GitHub authentication is not configured. " +
        	"Set Authentication:GitHub:ClientId and ClientSecret in appsettings or user secrets.");
		}

        builder.Services.AddRazorPages();

        string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        builder.Services.AddDbContext<ChatDBContext>(options => options.UseSqlite(connectionString));

        builder.Services.AddDefaultIdentity<IdentityUser>(options =>
        {
            options.SignIn.RequireConfirmedAccount = false;
        })
        .AddEntityFrameworkStores<ChatDBContext>();

        builder.Services.AddAuthentication()
        .AddGitHub(o =>
        {
            o.ClientId = githubClientId;
            o.ClientSecret = githubClientSecret;
            o.CallbackPath = "/signin-github";

            o.Scope.Add("user:email");
            o.SaveTokens = true;
        });
        builder.Services.AddScoped<ICheepRepository, CheepRepository>();

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ChatDBContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

            try
            {
                db.Database.Migrate();
                DbInitializer.SeedDatabase(db);
                logger.LogInformation("Database migrated and seeded.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Database initialization failed.");
            }
        }

        SeedUsersAsync(app).GetAwaiter().GetResult();

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapRazorPages();

        app.Run();
    }

static async Task SeedUsersAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    await EnsureUserAsync(userManager, "ropf@itu.dk", "LetM31n!");
    await EnsureUserAsync(userManager, "adho@itu.dk", "M32Want_Access");
}

static async Task EnsureUserAsync(
    UserManager<IdentityUser> userManager,
    string email,
    string password)
{
    var user = await userManager.FindByEmailAsync(email);
    if (user == null)
    {
        user = new IdentityUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }
}

}
