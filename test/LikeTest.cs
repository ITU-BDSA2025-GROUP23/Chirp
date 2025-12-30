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

public class LikeTest :  IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task LikedCheepTest()
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

            context.Authors.AddRange(author1, author2);
            await context.SaveChangesAsync();
            
            var cheep1 = new Cheep { AuthorId  = author1.AuthorId, Text = "Test cheep", TimeStamp = DateTime.UtcNow };
            context.Cheeps.Add(cheep1);
            await context.SaveChangesAsync();
            
            cheepRepo.Like(cheep1, author2);
            cheepRepo.SaveChanges();

            var twoLiked = context.Authors.Include(a => a.Liked).Single(a => a.Email == "TestUser2@test.dk");
            var cheepOneLikes = context.Cheeps.Include(c => c.Likes).Single(c => c.CheepId == cheep1.CheepId);

            Assert.Contains(twoLiked.Liked, c => c.CheepId == cheep1.CheepId);
            Assert.Contains(cheepOneLikes.Likes, a => a.Email == "TestUser2@test.dk");
        }
    }

    [Fact]
    public async Task DislikedCheepTest()
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

            context.Authors.AddRange(author1, author2);
            await context.SaveChangesAsync();
            
            var cheep1 = new Cheep { AuthorId  = author1.AuthorId, Text = "Test cheep", TimeStamp = DateTime.UtcNow };
            context.Cheeps.Add(cheep1);
            await context.SaveChangesAsync();
            
            cheepRepo.Like(cheep1, author2);
            cheepRepo.Like(cheep1, author2);
            cheepRepo.SaveChanges();

            var twoLiked = context.Authors.Include(a => a.Liked).Single(a => a.Email == "TestUser2@test.dk");
            var cheepOneLikes = context.Cheeps.Include(c => c.Likes).Single(c => c.CheepId == cheep1.CheepId);

            Assert.DoesNotContain(twoLiked.Liked, c => c.CheepId == cheep1.CheepId);
            Assert.DoesNotContain(cheepOneLikes.Likes, a => a.Email == "TestUser2@test.dk");
        }
    }
    
    [Fact]
    public async Task CountLikesTest()
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

            var twoLiked = context.Authors.Include(a => a.Liked).Single(a => a.Email == "TestUser2@test.dk");
            var threeLiked = context.Authors.Include(a => a.Liked).Single(a => a.Email == "TestUser3@test.dk");
            var cheepOneLikesCount = context.Cheeps.Include(c => c.Likes).Single(c => c.CheepId == cheep1.CheepId).Likes.Count;

            Assert.Contains(twoLiked.Liked, c => c.CheepId == cheep1.CheepId);
            Assert.Contains(threeLiked.Liked, c => c.CheepId == cheep1.CheepId);
            Assert.Equal(2, cheepOneLikesCount);
        }
    }
}