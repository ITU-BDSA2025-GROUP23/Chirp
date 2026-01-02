using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using Chirp.Core.DTOs;
using Chirp.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Chirp.Infrastructure.DataModel;



namespace Chirp.Web.Pages;
public class FollowingTimelineModel : PaginationModel
{
    [BindProperty(SupportsGet = true, Name = "author")]
    public string? AuthorName { get; set; }
    private readonly ICheepRepository _service;

    private readonly IAuthorRepository _authorRepository;
    public List<CheepDTO> Cheeps { get; set; } = new();
    public int TotalCheeps { get; private set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCheeps / PageSize);
    public List<CheepDTO>  AllCheeps = new List<CheepDTO>();
    
    public Author? CurrentUser { get; set; }
    public FollowingTimelineModel(ICheepRepository service,IAuthorRepository authorRepository )
    {
        _service = service;
        _authorRepository = authorRepository;
    }
    
    public IActionResult OnGet()
{
    if (!(User.Identity?.IsAuthenticated ?? false))
        return RedirectToPage("/Public");

    var email = User.Identity!.Name;
    if (string.IsNullOrWhiteSpace(email))
        return RedirectToPage("/Public");

    if (string.IsNullOrWhiteSpace(AuthorName))
        AuthorName = RouteData.Values["author"]?.ToString();

    if (CurrentPage < 1) CurrentPage = 1;

    var author = _service.GetAuthorByName(email) ?? _service.GetAuthorByEmail(email);
    author ??= _service.CreateAuthor(email, email);

    if (author == null)
        return RedirectToPage("/Public");

    var followingList = author.Following ?? new List<Author>();

    foreach (var following in followingList)
    {
        AllCheeps.AddRange(_service.GetPaginatedCheepsDTO(CurrentPage, PageSize, following.Name));
    }

    TotalCheeps = AllCheeps.Count;

    var lastPage = Math.Max(1, (int)Math.Ceiling((double)TotalCheeps / PageSize));
    if (CurrentPage > lastPage) CurrentPage = lastPage;

    Cheeps = AllCheeps
        .OrderByDescending(c => c.TimeIso)
        .Skip((CurrentPage - 1) * PageSize)
        .Take(PageSize)
        .ToList();

    CurrentUser = _service.GetAuthorByEmail(email);
    return Page();
}

public IActionResult OnPostUnfollow(string authorName)
{
    if (!(User.Identity?.IsAuthenticated ?? false))
        return RedirectToPage("/Public");

    var email = User.Identity!.Name;
    if (string.IsNullOrWhiteSpace(email))
        return RedirectToPage("/Public");

    CurrentUser = _service.GetAuthorByName(email) ?? _service.GetAuthorByEmail(email);
    if (CurrentUser == null)
        return RedirectToPage("/Public");

    var authorToUnfollow = _service.GetAuthorByName(authorName);
    if (authorToUnfollow == null || CurrentUser.AuthorId == authorToUnfollow.AuthorId)
        return RedirectToPage("/Public", new { p = CurrentPage });

    var following = CurrentUser.Following ?? new List<Author>();
    if (following.Any(a => a.AuthorId == authorToUnfollow.AuthorId))
    {
        _authorRepository.UnFollow(CurrentUser, authorToUnfollow);
        _authorRepository.SaveChanges();
    }

    return RedirectToPage("/Public", new { p = CurrentPage });
}
    
    public IActionResult OnPostLiked(int id)
    {
        var cheep = _service.GetCheepById(id);
        if (cheep == null) return RedirectToPage("/FollowingTimeline", new { p = CurrentPage });

        var email = User.Identity?.Name;
        if (string.IsNullOrWhiteSpace(email)) return RedirectToPage("/Public");

        var currentUser = _service.GetAuthorByEmail(email);
        if (currentUser == null) return RedirectToPage("/Public");

        _service.Like(cheep, currentUser);
        _service.SaveChanges();

        return RedirectToPage("/FollowingTimeline", null, new { p = CurrentPage }, $"cheep-{id}");
    }
}

