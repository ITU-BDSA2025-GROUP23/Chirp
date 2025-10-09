using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Chirp.Razor.Services;

namespace Chirp.Razor.Pages;

public class PublicModel : PageModel
{
    private readonly ICheepService _service;
    public IReadOnlyList<CheepViewModel> Cheeps { get; private set; } = Array.Empty<CheepViewModel>();
    public int Page { get; private set; } = 1;
    public bool HasNextPage { get; private set; }

    public PublicModel(ICheepService service) => _service = service;

    public void OnGet([FromQuery] int? page)
    {
        Page = page.HasValue && page.Value > 0 ? page.Value : 1;
        Cheeps = _service.GetCheeps(Page, 32);
        HasNextPage = Cheeps.Count == 32;
    }
}