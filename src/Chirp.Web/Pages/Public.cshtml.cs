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
    
    private readonly IAuthorRepository _authorRepository;
    public List<CheepDTO> Cheeps { get; set; } = new();
    public Author? CurrentUser { get; set; }
    public int TotalCheeps { get; private set; }
    public int NumberOfCheeps => Cheeps.Count;
    public int TotalPages => (int)System.Math.Ceiling((double)TotalCheeps / PageSize);
    
    public PublicModel(ICheepRepository service,IAuthorRepository authorRepository )
    {
        _service = service;
        _authorRepository = authorRepository;
    }

    public IActionResult OnGet([FromQuery(Name = "page")] int page = 1)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            CurrentUser = _service.GetAuthorByEmail(User.Identity!.Name!);
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
        /*
        if (CurrentUser == null)
        {
            return RedirectToPage("/postCheep");
        }
        */
        CurrentUser = _service.GetAuthorByEmail(User.Identity.Name);
        
        var authorToFollow = _service.GetAuthorByName(authorName);
        
        
        
        if (CurrentUser != null && authorToFollow != null && CurrentUser != authorToFollow)
        {
            _authorRepository.Follow(CurrentUser, authorToFollow);
            _authorRepository.SaveChanges();
        }

        return RedirectToPage("/Public", new { p = CurrentPage });
    }

    public IActionResult OnPostUnfollow(string authorName)
    {
        CurrentUser = _service.GetAuthorByEmail(User.Identity.Name);
        
        var authorToUnfollow = _service.GetAuthorByName(authorName);
        

        if (CurrentUser != null && authorToUnfollow != null && CurrentUser != authorToUnfollow)
        {
            if (CurrentUser.Following.Any(a => a.AuthorId == authorToUnfollow.AuthorId))
            {
                _authorRepository.UnFollow(CurrentUser, authorToUnfollow);
                _authorRepository.SaveChanges();
            }
        }

        return RedirectToPage("/Public", new { p = CurrentPage });
    }
}