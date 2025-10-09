using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Chirp.Razor.Services;

namespace Chirp.Razor.Pages;

public class UserTimelineModel : PageModel
{
    private readonly ICheepService _service;
    public string Username { get; private set; } = "";
    public IReadOnlyList<CheepViewModel> Cheeps { get; private set; } = Array.Empty<CheepViewModel>();
    public int Page { get; private set; } = 1;
    public bool HasNextPage { get; private set; }

    public UserTimelineModel(ICheepService service) => _service = service;

    public void OnGet(string username, [FromQuery] int? page)
    {
        Username = username;
        Page = page.HasValue && page.Value > 0 ? page.Value : 1;
        Cheeps = _service.GetCheepsFromAuthor(username, Page, 32);
        HasNextPage = Cheeps.Count == 32;
    }
}