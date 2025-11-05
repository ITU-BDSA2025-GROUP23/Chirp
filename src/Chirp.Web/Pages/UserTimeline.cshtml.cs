using Microsoft.AspNetCore.Mvc;
using Chirp.Core.DTOs;

namespace Chirp.Web.Pages;

public class UserTimelineModel : PaginationModel
{
    [BindProperty(SupportsGet = true, Name = "author")]
    public string? AuthorName { get; set; }

    private readonly ICheepRepository _service;

    public List<CheepDTO> Cheeps { get; set; } = new();
    public int TotalCheeps { get; private set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCheeps / PageSize);

    public UserTimelineModel(ICheepRepository service) => _service = service;

    public IActionResult OnGet()
    {
        if (string.IsNullOrWhiteSpace(AuthorName))
            AuthorName = RouteData.Values["author"]?.ToString();

        if (CurrentPage < 1) CurrentPage = 1;

        TotalCheeps = _service.GetCheepCount(AuthorName);

        var lastPage = Math.Max(1, (int)Math.Ceiling((double)TotalCheeps / PageSize));
        if (CurrentPage > lastPage) CurrentPage = lastPage;

        Cheeps = _service.GetPaginatedCheepsDTO(CurrentPage, PageSize, AuthorName);
        return Page();
    }
}