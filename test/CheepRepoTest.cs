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

public class CheepRepoTest :  IClassFixture<WebApplicationFactory<Program>>
{
    private readonly ChatDBContext _context;
    
    [Fact]
    public async Task CreateAuthorTest()
    {
        //arrange 
        using var connection = new SqliteConnection("Filename=:memory:");
        await connection.OpenAsync();
        var builder = new DbContextOptionsBuilder<ChatDBContext>().UseSqlite(connection);

        using var context = new ChatDBContext(builder.Options);
        await context.Database.EnsureCreatedAsync(); // Applies the schema to the database

        ICheepRepository repository = new CheepRepository(context);

        // Act
        var author = repository.CreateAuthor("TestUser","TestUser@itu.dk");
        
        var result = repository.GetAuthorByName("TestUser");
        
        // Assert
        Assert.Equal(result, author);
    }
    
    
}