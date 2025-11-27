using System.Linq;
using System.Security.Claims;
using Chirp.Infrastructure.DataModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Chirp.test;

public class ChirpUITest : IClassFixture<TestingWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly HttpClient _loggedInClient;

    public ChirpUITest(TestingWebApplicationFactory factory)
    {
        _client = factory.CreateClient();

        _loggedInClient = factory.CreateClient();
        _loggedInClient.DefaultRequestHeaders.Add("X-Test-User", "test@example.com");
    }

    [Fact]
    public async Task LogInBeforePost()
    {
        // Arrange + Act
        var response = await _client.GetAsync("/postCheep");
        var html = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Contains("YOU HAVE TO LOG IN TO USE THIS FUNCTION", html);
    }

    [Fact]
    public async Task CanAccessPost()
    {
        // Arrange + Act
        var response = await _loggedInClient.GetAsync("/postCheep");
        var html = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Contains("Post a Cheep!", html);
        Assert.Contains("What's on your mind?", html);
        Assert.Contains("<form method=\"post\"", html);
        Assert.Contains("name=\"Message\"", html);
        Assert.Contains("type=\"submit\"", html);
        Assert.Contains(">POST</button>", html);
    }
}