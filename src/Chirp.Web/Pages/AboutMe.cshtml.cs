using Chirp.Infrastructure.DataModel;
using Chirp.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Chirp.Web.Pages;

[Authorize]
public class AboutMeModel : PageModel
{
    private readonly ChatDBContext _db;
    private readonly ICheepRepository _repository;
    private readonly IAuthorRepository _authorRepo;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;

    [BindProperty(SupportsGet = true, Name = "author")]
    public string? AuthorName { get; set; }
    public string? Email { get; private set; }
    public int FollowingCount { get; set; }
    public int FollowersCount { get; set; }
    public IList<Cheep> MyCheeps { get; private set; } = new List<Cheep>();
    public IList<Author> Following { get; private set; } = new List<Author>();
    public IList<Author> Followers { get; private set; } = new List<Author>();

    public AboutMeModel(
        ChatDBContext db,
        ICheepRepository repository,
        IAuthorRepository authorRepo,
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager)
    {
        _db = db;
        _repository = repository;
        _authorRepo = authorRepo;
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToPage("/Public");
        }

        if (string.IsNullOrWhiteSpace(AuthorName))
            AuthorName = RouteData.Values["author"]?.ToString();
        
        if (string.IsNullOrWhiteSpace(AuthorName))
        {
            AuthorName = user.UserName;
        }
        
        var auth = _repository.GetAuthorByName(AuthorName)
                   ?? _repository.GetAuthorByEmail(AuthorName);
        
        Email = auth.Email;
        AuthorName = auth.Name;
        
        FollowingCount = _repository.GetFollowing(AuthorName!).Count;
        FollowersCount = _repository.GetFollowers(AuthorName!).Count;

        if (!string.IsNullOrEmpty(Email))
        {
            var author = _repository.GetAuthorByEmail(Email);
            if (author != null)
            {
                MyCheeps = await _db.Cheeps
                    .Include(c => c.Author)
                    .Where(c => c.Author != null && c.Author.AuthorId == author.AuthorId)
                    .OrderByDescending(c => c.TimeStamp)
                    .ToListAsync();

                Following = _repository.GetFollowing(AuthorName!)
                    .OrderBy(a => a.Name)
                    .ToList();
                
                Followers = _repository.GetFollowers(AuthorName!)
                    .OrderBy(a => a.Name)
                    .ToList();
                
            }
        }

        return Page();
    }

    public async Task<IActionResult> OnPostForgetMeAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToPage("/Public");
        }

        var author = _repository.GetAuthorByEmail(user.Email!);
        if (author != null)
        {
			_authorRepo.DeleteAuthor(author);
		}

        var externalLogins = await _userManager.GetLoginsAsync(user);
        foreach (var login in externalLogins)
        {
            await _userManager.RemoveLoginAsync(user, login.LoginProvider, login.ProviderKey);
        }

        await _userManager.DeleteAsync(user);
        await _db.SaveChangesAsync();

        await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
        await HttpContext.SignOutAsync();

        return RedirectToPage("/Public");
    }
}