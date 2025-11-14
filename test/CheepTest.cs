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
using System.ComponentModel.DataAnnotations;

namespace Chirp.Web.test;

public class CheepTest :  IClassFixture<WebApplicationFactory<Program>>
{
    private readonly ChatDBContext _context;
    
	[Fact]
	public async Task getAuthorsCheep()
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
        string text = "This cheep is by username123";
        

        // Act
        repository.CreateCheep(username,  email, text);
        
        
        // Assert
        var cheeps = repository.GetCheepsByAuthor(username).ToList();

    	Assert.Single(cheeps);
    	var cheep = cheeps[0];
    	Assert.Equal(text, cheep.Text);  
    }
	
	[Fact]
	public async Task getAllCheeps()
    {
        //arrange 
        using var connection = new SqliteConnection("Filename=:memory:");
        await connection.OpenAsync();
        var builder = new DbContextOptionsBuilder<ChatDBContext>().UseSqlite(connection);

        using var context = new ChatDBContext(builder.Options);
        await context.Database.EnsureCreatedAsync(); // Applies the schema to the database

        ICheepRepository repository = new CheepRepository(context);

        string username1 = "username1";
        string email1 = "email1@itu.dk";
        string text1 = "This cheep is by username1";
        
		string username2 = "username2";
        string email2 = "email2@itu.dk";
        string text2 = "This cheep is by username2";
        // Act
        repository.CreateCheep(username1,  email1, text1);
        repository.CreateCheep(username2,  email2, text2);
        
        // Assert
        var cheeps = repository.GetAllCheeps().ToList();

    	var cheep1 = cheeps[0];
		var cheep2 = cheeps[1];
    	Assert.Equal(text1, cheep2.Text);
		Assert.Equal(text2, cheep1.Text); 
    }

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
	[Fact]
	public async Task LenghtExceptionThrown()
   {
    // arrange
    string tooLongText = "This message not within range :-( flan ingridientlist: 1 cup white sugar 3 large eggs 1 (14 ounce) can sweetened condensed milk 1 (12 fluid ounce) can evaporated milk 1 tablespoon vanilla extract";
    var cheep = new Cheep
    {
        Text = tooLongText,
        TimeStamp = DateTime.UtcNow,
        AuthorId = 1
    };

    var context = new ValidationContext(cheep);

    // act + assert
    Assert.Throws<ValidationException>(() =>
        Validator.ValidateObject(cheep, context, validateAllProperties: true));
	}
}
