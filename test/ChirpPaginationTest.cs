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
	[Fact]
    public async Task TestEntityMapping()
    {
        //Arrange 
        using var connection = new SqliteConnection("Filename=:memory:");
        await connection.OpenAsync();
        var builder = new DbContextOptionsBuilder<ChatDBContext>().UseSqlite(connection);

        using var context = new ChatDBContext(builder.Options);
        await context.Database.EnsureCreatedAsync(); 

        ICheepRepository repository = new CheepRepository(context);
	
		string username = "username";
        string email = "email@itu.dk";
        string text = "Testing if DTO gets mapped correctly";

        //act
		repository.CreateCheep(username,  email, text);
		var result = repository.GetPaginatedCheepsDTO(1,10,"username");
		var cheep = context.Cheeps.First();
		var timestamp = cheep.TimeStamp.ToString("yyyy-MM-dd HH:mm");

        //Assert
		Assert.Equal(1, result[0].Id);
		Assert.True(result[0].Id != 0);
		Assert.Equal(text, result[0].Text);
		Assert.Equal(timestamp, result[0].TimeIso);
		Assert.Equal(username, result[0].AuthorName);	
    }
}