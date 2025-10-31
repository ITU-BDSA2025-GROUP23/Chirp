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
        var alice = new Author {  AuthorId = 13, Name = "alice", Email = "alice@example.com", Cheeps = new List<Cheep>() };
        var bob   = new Author {  AuthorId = 14, Name = "bob",   Email = "bob@example.com",   Cheeps = new List<Cheep>() };

        context.Authors.AddRange(alice, bob);
        context.SaveChanges();

        context.Cheeps.AddRange(
            new Cheep {CheepId = 700, AuthorId = alice.AuthorId, Author= alice,  Text = "Hello Chirp!",        TimeStamp = DateTime.UtcNow},
            new Cheep {CheepId = 701,  AuthorId = bob.AuthorId, Author= bob, Text = "Second cheep here!",  TimeStamp = DateTime.UtcNow}
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
