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
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;

    public string? UserName { get; private set; }
    public string? Email { get; private set; }
    public IList<Cheep> MyCheeps { get; private set; } = new List<Cheep>();

    public AboutMeModel(
        ChatDBContext db,
        ICheepRepository repository,
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager)
    {
        _db = db;
        _repository = repository;
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

        UserName = user.UserName;
        Email = user.Email;

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
            author.Name = "Deleted user";
            author.Email = $"{Guid.NewGuid()}@deleted.local";
            _db.Authors.Update(author);
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