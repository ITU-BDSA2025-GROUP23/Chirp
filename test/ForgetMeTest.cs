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
            
            Assert.NotNull(author1);
            
            var authorFromDb = await context.Authors
                .Include(a => a.Cheeps)
                .FirstAsync(a => a.AuthorId == author1.AuthorId);

            Assert.NotNull(authorFromDb);


            var cheeps = authorFromDb.Cheeps!;
            Assert.Single(cheeps);

            var firstCheep = cheeps.First(); 
            Assert.Equal("Test!", firstCheep.Text);

            repository.DeleteAuthor(authorFromDb);
            repository.SaveChanges();

            var updatedAuthor1 = await context.Authors
                .Include(a => a.Cheeps)
                .FirstAsync(a => a.AuthorId == author1.AuthorId);


            var updatedCheeps = updatedAuthor1.Cheeps!;
            Assert.NotEmpty(updatedCheeps);

            Assert.All(updatedCheeps, c =>
                Assert.Equal("*This take has been deleted*", c.Text));

        }
    }

    [Fact]
    public async Task LikeDeletedTest()
    {
        using var connection = new SqliteConnection("Filename=:memory:");
        await connection.OpenAsync();
        var builder = new DbContextOptionsBuilder<ChatDBContext>().UseSqlite(connection);

        using var context = new ChatDBContext(builder.Options);
        await context.Database.EnsureCreatedAsync(); // Applies the schema to the database

        await using (context)
        await using (connection)
        {
            IAuthorRepository authorRepo = new AuthorRepository(context);
            ICheepRepository cheepRepo = new CheepRepository(context);

            // Act
            var author1 = new Author { Name = "TestUser1", Email = "TestUser1@test.dk", Cheeps = new List<Cheep>() };
            var author2 = new Author { Name = "TestUser2", Email = "TestUser2@test.dk", Cheeps = new List<Cheep>() };
            var author3 = new Author { Name = "TestUser3", Email = "TestUser3@test.dk", Cheeps = new List<Cheep>() };

            context.Authors.AddRange(author1, author2,  author3);
            await context.SaveChangesAsync();
            
            var cheep1 = new Cheep { AuthorId  = author1.AuthorId, Text = "Test cheep", TimeStamp = DateTime.UtcNow };
            context.Cheeps.Add(cheep1);
            await context.SaveChangesAsync();
            
            cheepRepo.Like(cheep1, author2);
            cheepRepo.Like(cheep1, author3);
            cheepRepo.SaveChanges();

            authorRepo.DeleteAuthor(author1);
            authorRepo.SaveChanges();

            var twoLiked = context.Authors.Include(a => a.Liked).Single(a => a.Email == "TestUser2@test.dk");
            var threeLiked = context.Authors.Include(a => a.Liked).Single(a => a.Email == "TestUser3@test.dk");
            var cheepOneLikesCount = context.Cheeps.Include(c => c.Likes).Single(c => c.CheepId == cheep1.CheepId).Likes.Count;

            Assert.DoesNotContain(twoLiked.Liked, c => c.CheepId == cheep1.CheepId);
            Assert.DoesNotContain(threeLiked.Liked, c => c.CheepId == cheep1.CheepId);
            Assert.Equal(0, cheepOneLikesCount);
        }
    }
}