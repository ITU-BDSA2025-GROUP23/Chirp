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
    
    public string? CurrentUserName { get; set; }
    
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

            if (CurrentUser == null)
            {
                var email = User?.Identity?.Name;
                if (string.IsNullOrWhiteSpace(email))
                    return RedirectToPage("/Public");
                CurrentUser = _service.CreateAuthor(email, email);   
            }
            CurrentUserName = CurrentUser?.Name ?? "";
        }
        else
        {
            CurrentUserName = null;
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
        var email = User.Identity?.Name;
        if (string.IsNullOrWhiteSpace(email))
            return RedirectToPage("/Public");
        
        CurrentUser = _service.GetAuthorByEmail(email);
        if (CurrentUser == null)
            return RedirectToPage("/Public");
        
        var authorToFollow = _service.GetAuthorByName(authorName);
        
        
        
        if (CurrentUser != null && authorToFollow != null 
                                && CurrentUser != authorToFollow && CurrentUser.Name != authorToFollow.Name)
        {
            _authorRepository.Follow(CurrentUser, authorToFollow);
            _authorRepository.SaveChanges();
        }

        return RedirectToPage("/Public", new { p = CurrentPage });
    }

    public IActionResult OnPostUnfollow(string authorName)
    {
        var email = User.Identity?.Name;
        if (string.IsNullOrWhiteSpace(email))
            return RedirectToPage("/Public");
        
        CurrentUser = _service.GetAuthorByEmail(email);
        if (CurrentUser == null)
            return RedirectToPage("/Public");
        
        var authorToUnFollow = _service.GetAuthorByName(authorName);
        
        if (authorToUnFollow != null &&
            CurrentUser.AuthorId != authorToUnFollow.AuthorId &&
            CurrentUser.Name != authorToUnFollow.Name)
        {
            if ((CurrentUser.Following ?? new List<Author>())
                .Any(a => a.AuthorId == authorToUnFollow.AuthorId))
            {
                _authorRepository.UnFollow(CurrentUser, authorToUnFollow);
                _authorRepository.SaveChanges();
            }
        }

        return RedirectToPage("/Public", new { p = CurrentPage });
    }
    public IActionResult OnPostLiked(int id)
    {
        var cheep = _service.GetCheepById(id);
        if (cheep == null) return RedirectToPage("/Public", new { p = CurrentPage });

        var email = User.Identity?.Name;
        if (string.IsNullOrWhiteSpace(email)) return RedirectToPage("/Public");

        var currentUser = _service.GetAuthorByEmail(email);
        if (currentUser == null) return RedirectToPage("/Public");

        _service.Like(cheep, currentUser);
        _service.SaveChanges();

        return RedirectToPage("/Public", null, new { p = CurrentPage }, $"cheep-{id}");
    }
}