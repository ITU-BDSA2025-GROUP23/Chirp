using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using Chirp.Core.DTOs;
using Chirp.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;



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
    public FollowingTimelineModel(ICheepRepository service,IAuthorRepository authorRepository )
    {
        _service = service;
        _authorRepository = authorRepository;
    }
    
    public IActionResult OnGet()
    {
            if (!(User.Identity?.IsAuthenticated ?? false))
                return RedirectToPage("/Public");   
            
            if (string.IsNullOrWhiteSpace(AuthorName))
                AuthorName = RouteData.Values["author"]?.ToString();

            if (CurrentPage < 1) CurrentPage = 1;
        
            var author = _service.GetAuthorByName(User.Identity!.Name!);
            
            foreach(var following in author.Following)
            {
                Cheeps = _service.GetPaginatedCheepsDTO(CurrentPage, PageSize, following.Name);

                AllCheeps.AddRange(Cheeps);
            }

            TotalCheeps = AllCheeps.Count;
            
            var lastPage = Math.Max(1, (int)Math.Ceiling((double)TotalCheeps / PageSize));
            if (CurrentPage > lastPage) CurrentPage = lastPage;
            
            Cheeps = AllCheeps
                .OrderByDescending(c => c.TimeIso)
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();
            
            return Page(); 
    }
    
    public IActionResult OnPostUnfollow(string authorName)
    {
        var CurrentUser = _service.GetAuthorByEmail(User.Identity.Name);
        
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

