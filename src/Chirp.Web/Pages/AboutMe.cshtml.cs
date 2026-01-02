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

    public string? AuthorName { get; private set; }
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
    var identityUser = await _userManager.GetUserAsync(User);
    if (identityUser == null)
        return RedirectToPage("/Public");

    var email = await _userManager.GetEmailAsync(identityUser);
    var userName = await _userManager.GetUserNameAsync(identityUser);

    if (string.IsNullOrWhiteSpace(email))
        return RedirectToPage("/Public");

    if (string.IsNullOrWhiteSpace(userName))
        userName = email;

    var author = _repository.GetAuthorByEmail(email)
                 ?? _repository.GetAuthorByName(userName);

    author ??= _repository.CreateAuthor(userName, email);

    if (author == null)
        return RedirectToPage("/Public");

    var changed = false;

    if (!string.Equals(author.Email, email, StringComparison.OrdinalIgnoreCase))
    {
        author.Email = email;
        changed = true;
    }

    if (!string.IsNullOrWhiteSpace(userName) &&
        !string.Equals(author.Name, userName, StringComparison.Ordinal))
    {
        author.Name = userName;
        changed = true;
    }

    if (changed)
    {
        _authorRepo.SaveChanges();
    }

    Email = author.Email;
    AuthorName = author.Name;

    Following = _repository.GetFollowing(AuthorName!)
        .OrderBy(a => a.Name)
        .ToList();

    Followers = _repository.GetFollowers(AuthorName!)
        .OrderBy(a => a.Name)
        .ToList();

    FollowingCount = Following.Count;
    FollowersCount = Followers.Count;

    MyCheeps = await _db.Cheeps
        .Include(c => c.Author)
        .Where(c => c.Author != null && c.Author.AuthorId == author.AuthorId)
        .OrderByDescending(c => c.TimeStamp)
        .ToListAsync();

    return Page();
}

    public async Task<IActionResult> OnPostForgetMeAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToPage("/Public");
        }

        var email = await _userManager.GetEmailAsync(user);
        if (string.IsNullOrWhiteSpace(email))
            return RedirectToPage("/Public");

        var author = _repository.GetAuthorByEmail(email);
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