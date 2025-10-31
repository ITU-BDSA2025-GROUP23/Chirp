using Chirp.Razor.DataModel;
using Database;
using Microsoft.EntityFrameworkCore;
using Chirp.Razor;

//Region build app and register DbContext
var builder = WebApplication.CreateBuilder(args);


builder.Services.AddRazorPages();

string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ChatDBContext>(options => options.UseSqlite(connectionString));
builder.Services.AddScoped<ICheepRepository, CheepRepository>();

var app = builder.Build();



//if (app.Environment.IsDevelopment())
//{
    using (var scope = app.Services.CreateScope()){
    var context = scope.ServiceProvider.GetRequiredService<ChatDBContext>();
    context.Database.Migrate();
	
 if (!context.Authors.Any() && !context.Cheeps.Any())
    {
        var alice = new Author { Username = "alice", Email = "alice@example.com", Cheeps = new List<Cheep>() };
        var bob   = new Author { Username = "bob",   Email = "bob@example.com",   Cheeps = new List<Cheep>() };

        context.Authors.AddRange(alice, bob);
        context.SaveChanges();

        context.Cheeps.AddRange(
            new Cheep { Text = "Hello Chirp!",        Timestamp = DateTime.UtcNow, AuthorId = alice.id },
            new Cheep { Text = "Second cheep here!",  Timestamp = DateTime.UtcNow, AuthorId = bob.id }
        );
        context.SaveChanges();
    }

}
//}


//Region MRGA
app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapRazorPages();

app.Run();
