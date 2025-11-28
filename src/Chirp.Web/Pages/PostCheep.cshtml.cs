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
        var email = User?.Identity?.Name;
        var userName = email!;
        var text = Message;
        if (string.IsNullOrWhiteSpace(email))
        {
            return Challenge(); 
        }

        if (string.IsNullOrWhiteSpace(Message))
        {
            ModelState.AddModelError(nameof(Message), "Message is required.");
            return Page();
        }
        
        _repository.CreateCheep(userName,email, text);
        return RedirectToPage("/MyPage");
    }
}