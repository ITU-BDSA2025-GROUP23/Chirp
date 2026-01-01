using Microsoft.AspNetCore.Mvc;
using Chirp.Core.DTOs;
using Chirp.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Chirp.Web.Pages;

public class  PostCheepModel : PageModel //PaginationModel?
{
    [BindProperty] 
    public string Message { get; set; }= string.Empty;
    
    public bool CanPost => User?.Identity?.IsAuthenticated ?? false;
    private readonly ICheepRepository _repository;
    
    public List<CheepDTO> Cheeps { get; set; } = new();
    public PostCheepModel(ICheepRepository repository) => _repository = repository;
    
    public IActionResult OnPost()
    {
        if (!(User.Identity?.IsAuthenticated ?? false))
            return RedirectToPage("/Public");

        var email = User.Identity?.Name;
        if (string.IsNullOrWhiteSpace(email))
            return RedirectToPage("/Public");

        var userName = email;
        _repository.CreateCheep(userName, email, Message);

        return RedirectToPage("/MyPage");
    }
}