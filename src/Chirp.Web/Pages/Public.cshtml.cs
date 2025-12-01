using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using Chirp.Core.DTOs;
using Chirp.Infrastructure.Repositories;

namespace Chirp.Web.Pages;

public class PublicModel : PaginationModel
{

    private readonly ICheepRepository _service;

    public List<CheepDTO> Cheeps { get; set; } = new();
    public int TotalCheeps { get; private set; }
    public int NumberOfCheeps => Cheeps.Count;
    public int TotalPages => (int)System.Math.Ceiling((double)TotalCheeps / PageSize);

    public PublicModel(ICheepRepository service)
    {
        _service = service;
    }

    public IActionResult OnGet([FromQuery(Name = "page")] int page = 1)
    {
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
        var currentUser = _service.GetAuthorByName(User.Identity.Name);
        var authorToFollow = _service.GetAuthorByName(authorName);

        if (currentUser != null && authorToFollow != null && currentUser.AuthorId != authorToFollow.AuthorId)
        {
            currentUser.Follow(authorToFollow);
            _service.SaveChanges();
        }
        
        Console.WriteLine($"{currentUser.Name} now follows: {string.Join(", ", currentUser.Following.Select(a => a.Name))}");

        return RedirectToPage("/Public", new { p = CurrentPage });
    }
}