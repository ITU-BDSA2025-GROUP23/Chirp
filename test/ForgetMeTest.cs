using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using System;
using System.IO;
using Chirp.Web;
using Chirp.Infrastructure.Repositories;
using Chirp.Infrastructure.DataModel;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Chirp.Web.test;

public class ForgetMeTest :  IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task UserToDeletedTest()
    {
        //arrange 
        using var connection = new SqliteConnection("Filename=:memory:");
        await connection.OpenAsync();
        var builder = new DbContextOptionsBuilder<ChatDBContext>().UseSqlite(connection);

        using var context = new ChatDBContext(builder.Options);
        await context.Database.EnsureCreatedAsync(); // Applies the schema to the database
        
        await using (context)
        await using (connection)
        {
            IAuthorRepository repository = new AuthorRepository(context);
            ICheepRepository cheepRepo = new CheepRepository(context);

            // Act
            var author1 = new Author { Name = "TestUser1", Email = "TestUser1@test.dk", Cheeps = new List<Cheep>() };
            var author2 = new Author { Name = "TestUser2", Email = "TestUser2@test.dk", Cheeps = new List<Cheep>() };
            
            context.Authors.AddRange(author1, author2);
            await context.SaveChangesAsync();
            
            repository.DeleteAuthor(author1);
            repository.SaveChanges();

            
            var updatedAuthor1 = await context.Authors
                .Include(a => a.Cheeps)
                .FirstAsync(a => a.AuthorId == author1.AuthorId);

            Assert.Equal("Deleted user", updatedAuthor1.Name);
            Assert.NotEqual("TestUser1@test.dk", updatedAuthor1.Email);
            Assert.EndsWith("@deleted.local", updatedAuthor1.Email, StringComparison.OrdinalIgnoreCase);
        }
        
    }
    
   [Fact]
    public async Task UnFollowTest()
    {
        //arrange 
        using var connection = new SqliteConnection("Filename=:memory:");
        await connection.OpenAsync();
        var builder = new DbContextOptionsBuilder<ChatDBContext>().UseSqlite(connection);

        using var context = new ChatDBContext(builder.Options);
        await context.Database.EnsureCreatedAsync(); // Applies the schema to the database
        
        await using (context)
        await using (connection)
        {
            IAuthorRepository repository = new AuthorRepository(context);
            ICheepRepository cheepRepo = new CheepRepository(context);


            var author1 = cheepRepo.CreateAuthor("TestUser1", "TestUser1@test.dk");
            cheepRepo.CreateCheep("TestUser1", "TestUser1@test.dk", "Test!");

            var authorFromDb = await context.Authors
                .Include(a => a.Cheeps)
                .FirstAsync(a => a.AuthorId == author1.AuthorId);

            Assert.Single(authorFromDb.Cheeps);
            Assert.Equal("Test!", authorFromDb.Cheeps.First().Text);

            repository.DeleteAuthor(authorFromDb);
            repository.SaveChanges();

            var updatedAuthor1 = await context.Authors
                .Include(a => a.Cheeps)
                .FirstAsync(a => a.AuthorId == author1.AuthorId);

            Assert.NotEmpty(updatedAuthor1.Cheeps);
            Assert.All(updatedAuthor1.Cheeps, c =>
                Assert.Equal("*This take has been deleted*", c.Text));

        }
    }
}