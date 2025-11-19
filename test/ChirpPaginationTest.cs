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
using Chirp.Web.Pages;

namespace Chirp.test;

public class ChirpPaginationTest: IClassFixture<TestingWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ChirpPaginationTest(TestingWebApplicationFactory factory)
    {
        
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = true
        });
        
    }

    [Fact]
    public async Task PageEqualsOne()
    {
        //Arrange 
        using var connection = new SqliteConnection("Filename=:memory:");
        await connection.OpenAsync();
        var builder = new DbContextOptionsBuilder<ChatDBContext>().UseSqlite(connection);

        using var context = new ChatDBContext(builder.Options);
        await context.Database.EnsureCreatedAsync(); // Applies the schema to the database

        ICheepRepository repository = new CheepRepository(context);
        
        //act
        var resultA = repository.GetPaginatedCheepsDTO(0,10);
        var resultB = repository.GetPaginatedCheepsDTO(1,10);
        
        //Assert
        Assert.Equal(resultA, resultB);
    }
    
    [Fact]
    public async Task PageSizeEquals32()
    {
        //Arrange 
        using var connection = new SqliteConnection("Filename=:memory:");
        await connection.OpenAsync();
        var builder = new DbContextOptionsBuilder<ChatDBContext>().UseSqlite(connection);

        using var context = new ChatDBContext(builder.Options);
        await context.Database.EnsureCreatedAsync(); // Applies the schema to the database

        ICheepRepository repository = new CheepRepository(context);
        
        //act
        var resultA = repository.GetPaginatedCheepsDTO(1,0);
        var resultB = repository.GetPaginatedCheepsDTO(1,32);
        
        //Assert
        Assert.Equal(resultA, resultB);
    }
}