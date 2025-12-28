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

    public Cheep chp;
    
    
    
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
                CurrentUser = _service.GetAuthorByEmail(User.Identity!.Name!);
            }

            if (CurrentUser == null)
            {
                var email = User?.Identity?.Name;
                var userName = email!;
                CurrentUser = _service.CreateAuthor(userName, email);   
            }
            CurrentUserName = CurrentUser.Name;
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
        CurrentUser = _service.GetAuthorByEmail(User.Identity.Name);
        
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
        CurrentUser = _service.GetAuthorByEmail(User.Identity.Name);
        
        var authorToUnFollow = _service.GetAuthorByName(authorName);
        

        if (CurrentUser != null && authorToUnFollow != null 
                                && CurrentUser != authorToUnFollow && CurrentUser.Name != authorToUnFollow.Name)
        {
            if (CurrentUser.Following.Any(a => a.AuthorId == authorToUnFollow.AuthorId))
            {
                _authorRepository.UnFollow(CurrentUser, authorToUnFollow);
                _authorRepository.SaveChanges();
            }
        }

        return RedirectToPage("/Public", new { p = CurrentPage });
    }
	public IActionResult OnPostLiked(int id)
    {
        var Cheep = _service.GetCheepById(id);
        
        CurrentUser = _service.GetAuthorByEmail(User.Identity!.Name!);
        _service.Like(Cheep, CurrentUser);
        _service.SaveChanges();
        
    return RedirectToPage("/Public", null, new { p = CurrentPage }, $"cheep-{id}");
	}
}