using System.Linq;
using System.Security.Claims;
using Chirp.Infrastructure.DataModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Chirp.Web.db; 
using Microsoft.AspNetCore.Mvc; 

namespace Chirp.test; 

public class TestingWebApplicationFactory : WebApplicationFactory<Program>
{
    private SqliteConnection? _connection;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
           
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ChatDBContext>));

            if (descriptor is not null)
                services.Remove(descriptor);

         
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            services.AddDbContext<ChatDBContext>(options =>
            {
                options.UseSqlite(_connection);
            });

        
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ChatDBContext>();

            db.Database.EnsureDeleted();
            db.Database.Migrate();
            DbInitializer.SeedDatabase(db);
            
            services.PostConfigure<MvcOptions>(options =>
            {
                for (int i = options.Filters.Count - 1; i >= 0; i--)
                {
                    if (options.Filters[i] is AutoValidateAntiforgeryTokenAttribute)
                    {
                        options.Filters.RemoveAt(i);
                    }
                }
                
                options.Filters.Add(new IgnoreAntiforgeryTokenAttribute());
            });
        });
        builder.ConfigureTestServices(services =>
        {
            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                    options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                    TestAuthHandler.SchemeName, options => { });
            
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ChatDBContext>();

            const string testEmail = "test@example.com";

            if (!db.Authors.Any(a => a.Email == testEmail))
            {
                db.Authors.Add(new Author
                {
                    Email = testEmail,
                    Name = testEmail,
                    Cheeps = new List<Cheep>()
                });

                db.SaveChanges();
            }
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            _connection?.Dispose();
        }
    }
}