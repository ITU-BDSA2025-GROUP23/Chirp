using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using Chirp.Core.DTOs;
using Chirp.Infrastructure.Repositories;
using Chirp.Infrastructure.DataModel;

namespace Chirp.Web.Pages;

public class PublicModel : PaginationModel
{
    private readonly ICheepRepository _service;
    public List<CheepDTO> Cheeps { get; set; } = new();
    public Author? CurrentUser { get; set; }
    public int TotalCheeps { get; private set; }
    public int NumberOfCheeps => Cheeps.Count;
    public int TotalPages => (int)System.Math.Ceiling((double)TotalCheeps / PageSize);
    
    public PublicModel(ICheepRepository service)
    {
        _service = service;
    }

    public IActionResult OnGet([FromQuery(Name = "page")] int page = 1)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            CurrentUser = _service.GetAuthorByName(User.Identity.Name);
        }
        
        if (int.TryParse(Request.Query["p"], out var p) && p > 0) CurrentPage = p;
        else
            CurrentPage = 1;
            
        TotalCheeps = _service.GetCheepCount();

        var lastPage = System.Math.Max(1, (int)System.Math.Ceiling((double)TotalCheeps / PageSize));
        if (CurrentPage > lastPage) CurrentPage = lastPage;

        Cheeps = _service.GetPaginatedCheepsDTO(CurrentPage, PageSize);
        return Page();
        

    }
    
    public IActionResult OnPostFollow(string authorName)
    {
        CurrentUser = _service.GetAuthorByName(User.Identity.Name);
        
        var authorToFollow = _service.GetAuthorByName(authorName);
        
        if (CurrentUser != null && authorToFollow != null && CurrentUser != authorToFollow)
        {
            CurrentUser.Follow(authorToFollow);
            _service.SaveChanges();
        }

        return RedirectToPage("/Public", new { p = CurrentPage });
    }

    public IActionResult OnPostUnfollow(string authorName)
    {
        CurrentUser = _service.GetAuthorByName(User.Identity.Name);
        
        var authorToUnfollow = _service.GetAuthorByName(authorName);
        

        if (CurrentUser != null && authorToUnfollow != null && CurrentUser != authorToUnfollow)
        {
            if (CurrentUser.Following.Any(a => a.AuthorId == authorToUnfollow.AuthorId))
            {
                CurrentUser.Following.Remove(authorToUnfollow);
                _service.SaveChanges();
            }
        }

        return RedirectToPage("/Public", new { p = CurrentPage });
    }
}