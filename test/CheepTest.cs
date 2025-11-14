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

public class CheepTest :  IClassFixture<WebApplicationFactory<Program>>
{
    private readonly ChatDBContext _context;
    
    [Fact]
    public async Task LenghtApproved()
    {
        //arrange 
        using var connection = new SqliteConnection("Filename=:memory:");
        await connection.OpenAsync();
        var builder = new DbContextOptionsBuilder<ChatDBContext>().UseSqlite(connection);

        using var context = new ChatDBContext(builder.Options);
        await context.Database.EnsureCreatedAsync(); // Applies the schema to the database

        ICheepRepository repository = new CheepRepository(context);

        string username = "username123";
        string email = "email123@itu.dk";
        string text = "This message within range :-)";
        

        // Act
        repository.CreateCheep(username,  email, text);
        
        
        // Assert
        var cheeps = repository.GetCheepsByAuthor(username).ToList();

    	Assert.Single(cheeps);
    	var cheep = cheeps[0];
    	Assert.Equal(text, cheep.Text);                
    	Assert.True(cheep.Text.Length <= 160);  
    }
	[Fact]
	public async Task LenghtDenied()
    {
        //arrange 
        using var connection = new SqliteConnection("Filename=:memory:");
        await connection.OpenAsync();
        var builder = new DbContextOptionsBuilder<ChatDBContext>().UseSqlite(connection);

        using var context = new ChatDBContext(builder.Options);
        await context.Database.EnsureCreatedAsync(); // Applies the schema to the database

        ICheepRepository repository = new CheepRepository(context);

        string username = "username123";
        string email = "email123@itu.dk";
        string text = "This message not within range :-( flan ingridientlist: 1 cup white sugar 3 large eggs 1 (14 ounce) can sweetened condensed milk 1 (12 fluid ounce) can evaporated milk 1 tablespoon vanilla extract";
        // for some reason I can only do it on one line !? 
        

        // Act
        repository.CreateCheep(username,  email, text);
        
        
        // Assert
        var cheeps = repository.GetCheepsByAuthor(username).ToList();

    	Assert.Single(cheeps);
    	var cheep = cheeps[0];
    	Assert.Equal(text, cheep.Text);                
    	Assert.False(cheep.Text.Length <= 160);  
    }
}
