using Microsoft.AspNetCore.Mvc;
using Chirp.Core.DTOs;
using Chirp.Infrastructure.Repositories;
using Chirp.Infrastructure.DataModel;

namespace Chirp.Web.Pages;

public class UserTimelineModel : PaginationModel
{
    [BindProperty(SupportsGet = true, Name = "author")]
    public string? AuthorName { get; set; }

    private readonly ICheepRepository _service;

    public List<CheepDTO> Cheeps { get; set; } = new();
    public int TotalCheeps { get; private set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCheeps / PageSize);
    
    public int FollowingCount { get; set; }
    
    public int FollowersCount { get; set; }
    public Author? CurrentUser { get; set; }
    public string? CurrentUserName { get; set; }
    public UserTimelineModel(ICheepRepository service) => _service = service;

    public IActionResult OnGet()
    {
        if (string.IsNullOrWhiteSpace(AuthorName))
            AuthorName = RouteData.Values["author"]?.ToString();

        if (CurrentPage < 1) CurrentPage = 1;

        TotalCheeps = _service.GetCheepCount(AuthorName);
        
        FollowingCount = _service.GetFollowing(AuthorName!).Count;
        
        FollowersCount = _service.GetFollowers(AuthorName!).Count;

        var lastPage = Math.Max(1, (int)Math.Ceiling((double)TotalCheeps / PageSize));
        if (CurrentPage > lastPage) CurrentPage = lastPage;
        
        CurrentUser = _service.GetAuthorByEmail(User.Identity?.Name ?? "");
        CurrentUserName = CurrentUser?.Name ?? "";
        
        Cheeps = _service.GetPaginatedCheepsDTO(CurrentPage, PageSize, AuthorName);
        return Page();
    }
    
    public IActionResult OnPostLiked(int id)
    {
        var cheep = _service.GetCheepById(id);
        if (cheep == null) return RedirectToPage("/UserTimeline", new { author = AuthorName, p = CurrentPage });

        var email = User.Identity?.Name;
        if (string.IsNullOrWhiteSpace(email)) return RedirectToPage("/Public");

        var currentUser = _service.GetAuthorByEmail(email);
        if (currentUser == null) return RedirectToPage("/Public");

        _service.Like(cheep, currentUser);
        _service.SaveChanges();

        return RedirectToPage(
            "/UserTimeline",
            pageHandler: null,
            routeValues: new { author = AuthorName, p = CurrentPage },
            fragment: $"cheep-{id}"
        );
    }
}