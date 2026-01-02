using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Chirp.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using Chirp.Web.Pages;
using Microsoft.AspNetCore.Http;          
using Microsoft.AspNetCore.Mvc;          
using Microsoft.AspNetCore.Mvc.RazorPages; 
using System.Security.Claims;

namespace Chirp.test;

public class ChirpUITest : IClassFixture<TestingWebApplicationFactory>
{
    private readonly TestingWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly HttpClient _loggedInClient;

    public ChirpUITest(TestingWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();

        _loggedInClient = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            HandleCookies = true
        });
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
        Assert.Contains("Post a Take!", html);
        Assert.Contains("What's your HOT TAKE?", html);
        Assert.Contains("<form method=\"post\"", html);
        Assert.Contains("name=\"Message\"", html);
        Assert.Contains("type=\"submit\"", html);
        Assert.Contains(">POST</b></button>", html);
    }
    
    [Fact]
    public async Task PostEndsUpInCheeps()
    {
        //Arrange
        var getResponse = await _loggedInClient.GetAsync("/postCheep");
        var getHtml = await getResponse.Content.ReadAsStringAsync();

        var match = Regex.Match(
            getHtml,
            "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"",
            RegexOptions.IgnoreCase);
        
        var antiForgeryToken = match.Groups[1].Value;
        
        var form = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("Message", "Test PostEndsUpInCheeps"),
            new KeyValuePair<string, string>("__RequestVerificationToken", antiForgeryToken)
        });

        var postResponse = await _loggedInClient.PostAsync("/postCheep", form);
        
        using var scope = _factory.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<ICheepRepository>();
        
        //act
        var cheeps = repo.GetCheepsByAuthor("test@example.com").ToList();
        
        //Assert
        Assert.Contains(cheeps, c => c.Text == "Test PostEndsUpInCheeps");
    }
    [Fact]
    public void PostButtonRedirects()
    {
        //arrange
        using var scope = _factory.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<ICheepRepository>();

        var model = new PostCheepModel(repo);
        
        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(
            new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.Name, "test@example.com") },
                "Test"));

        model.PageContext = new PageContext
        {
            HttpContext = httpContext
        };
        
        model.Message = "Send me to my page";

        // Act
        var result = model.OnPost();

        // Assert
        var redirect = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/MyPage", redirect.PageName);
    }

    [Fact]
    public async Task Follow_UnfollowButtonTest()
    {
        
        using var scope = _factory.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<ICheepRepository>();
        
        repo.CreateAuthor("other", "other@example.dk");
        repo.CreateCheep("other", "other@example.dk", "This is a Test");

        var resp = await _client.GetAsync("/");
        resp.EnsureSuccessStatusCode();
        var html = await resp.Content.ReadAsStringAsync();

        Assert.DoesNotContain("data-test=\"follow-button\"", html);
        Assert.DoesNotContain("data-test=\"unfollow-button\"", html);
        
    }

    [Fact]
    public async Task Logged_InFollowButtonTest()
    {
        using var scope = _factory.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<ICheepRepository>();

        repo.CreateAuthor("test", "test@example.com");
        repo.CreateCheep("other", "other@example.dk", "This is a Test");

        var resp = await _loggedInClient.GetAsync("/");
        resp.EnsureSuccessStatusCode();
        var html = await resp.Content.ReadAsStringAsync();
        
        Assert.Contains("data-test=\"follow-button\"", html);
        Assert.DoesNotContain("data-test=\"unfollow-button\"", html);
    }

    [Fact]
    public async Task UnfollowOnFollowedUserTest()
    {
        using var scope = _factory.Services.CreateScope();
        var cheepRepo = scope.ServiceProvider.GetRequiredService<ICheepRepository>();
        var authorRepo = scope.ServiceProvider.GetRequiredService<IAuthorRepository>();

        var test = cheepRepo.GetAuthorByEmail("test@example.com");
        Assert.NotNull(test);
        var other = cheepRepo.CreateAuthor("other", "other@example.dk");
        cheepRepo.CreateCheep("other", "other@example.dk", "This is a Test");
        cheepRepo.SaveChanges();
        Assert.NotNull(other);
        
        authorRepo.Follow(test, other);
        authorRepo.SaveChanges();
        
        var resp = await _loggedInClient.GetAsync("/");
        resp.EnsureSuccessStatusCode();
        var html = await resp.Content.ReadAsStringAsync();
        
        Assert.Contains("data-test=\"unfollow-button\"", html);
    }
    
    
    [Fact]
    public async Task LikeUITest()
    {
        using var scope = _factory.Services.CreateScope();
        var cheepRepo = scope.ServiceProvider.GetRequiredService<ICheepRepository>();
        var authorRepo = scope.ServiceProvider.GetRequiredService<IAuthorRepository>();
        
        var test = cheepRepo.GetAuthorByEmail("test@example.com");
        Assert.NotNull(test);
        
        var other = cheepRepo.CreateAuthor("other", "other@example.dk");
        cheepRepo.CreateCheep("other", "other@example.dk", "This is a Test (liked)");
        cheepRepo.CreateCheep("other", "other@example.dk", "This is a Test (not liked)");
        cheepRepo.SaveChanges();
        Assert.NotNull(other);
        
        Assert.NotNull(other.Cheeps);
        Assert.NotEmpty(other.Cheeps);
        
        var likedCheep = other.Cheeps!
            .OrderBy(c => c.CheepId)
            .First();
        
        var notLikedCheep = other.Cheeps!
            .OrderBy(c => c.CheepId)
            .Last();
        
        cheepRepo.Like(likedCheep, test);
        cheepRepo.SaveChanges();
        
        
        
        var resp = await _loggedInClient.GetAsync("/other");
        resp.EnsureSuccessStatusCode();
        var html = await resp.Content.ReadAsStringAsync();
        
        Assert.Contains("bball-liked", html);
        Assert.Contains("Unlike", html);
        
        Assert.Contains("class=\"bball\"", html);
        Assert.Contains("Like", html);
    }
}