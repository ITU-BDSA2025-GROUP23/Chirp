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

public class FollowTest :  IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task Follow_FollowingTest()
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

            // Act
            var author1 = new Author { Name = "TestUser1", Email = "TestUser1@test.dk", Cheeps = new List<Cheep>() };
            var author2 = new Author { Name = "TestUser2", Email = "TestUser2@test.dk", Cheeps = new List<Cheep>() };
            
            context.Authors.AddRange(author1, author2);
            await context.SaveChangesAsync();
            
            repository.Follow(author1, author2);
            repository.SaveChanges();
            
            var oneReload   = context.Authors.Include(a => a.Followers).Single(a => a.Email == "TestUser1@test.dk");
            var twoReload = context.Authors.Include(a => a.Following).Single(a => a.Email == "TestUser2@test.dk");
            
            Assert.Contains(twoReload.Followers, a => a.Email == "TestUser1@test.dk");
            Assert.Contains(oneReload.Following, a => a.Email == "TestUser2@test.dk");
            
        }
        
    }
    
    [Fact]
    public async Task UnFollowTest()
    {
        using var connection = new SqliteConnection("Filename=:memory:");
        await connection.OpenAsync();
        var builder = new DbContextOptionsBuilder<ChatDBContext>().UseSqlite(connection);

        using var context = new ChatDBContext(builder.Options);
        await context.Database.EnsureCreatedAsync(); // Applies the schema to the database
        
        await using (context)
        await using (connection)
        {
            IAuthorRepository repository = new AuthorRepository(context);

            // Act
            var author1 = new Author { Name = "TestUser1", Email = "TestUser1@test.dk", Cheeps = new List<Cheep>() };
            var author2 = new Author { Name = "TestUser2", Email = "TestUser2@test.dk", Cheeps = new List<Cheep>() };
            
            context.Authors.AddRange(author1, author2);
            await context.SaveChangesAsync();
            
            repository.Follow(author1, author2);
            repository.SaveChanges();
            
            repository.UnFollow(author1, author2);
            repository.SaveChanges();
            
            var oneReload   = context.Authors.Include(a => a.Followers).Single(a => a.Email == "TestUser1@test.dk");
            var twoReload = context.Authors.Include(a => a.Following).Single(a => a.Email == "TestUser2@test.dk");
            
            Assert.DoesNotContain(twoReload.Followers, a => a.Email == "TestUser1@test.dk");
            Assert.DoesNotContain(oneReload.Following, a => a.Email == "TestUser2@test.dk");
            
        }
    }

    [Fact]
    public async Task FollowerCountTest()
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

            // Act
            var author1 = new Author { Name = "TestUser1", Email = "TestUser1@test.dk", Cheeps = new List<Cheep>() };
            var author2 = new Author { Name = "TestUser2", Email = "TestUser2@test.dk", Cheeps = new List<Cheep>() };
            var author3 = new Author { Name = "TestUser3", Email = "TestUser3@test.dk", Cheeps = new List<Cheep>() };

            context.Authors.AddRange(author1, author2);
            await context.SaveChangesAsync();

            repository.Follow(author1, author2);
            repository.Follow(author3, author2);
            repository.SaveChanges();

            var oneFollowingCount = context.Authors.Include(a => a.Followers)
                .Single(a => a.Email == "TestUser1@test.dk").Following.Count;
            var twoFollowersCount = context.Authors.Include(a => a.Following)
                .Single(a => a.Email == "TestUser2@test.dk").Followers.Count;

            Assert.Equal(2, twoFollowersCount);
            Assert.Equal(1, oneFollowingCount);
        }
    }
}