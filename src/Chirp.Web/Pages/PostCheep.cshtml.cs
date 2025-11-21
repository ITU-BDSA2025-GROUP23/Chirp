using Microsoft.AspNetCore.Mvc;
using Chirp.Core.DTOs;
using Chirp.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Chirp.Web.Pages;

public class  PostCheepModel : PageModel //PaginationModel?
{
    [BindProperty] public string Message { get; set; }
    
    private readonly ICheepRepository _repository;
    
    public List<CheepDTO> Cheeps { get; set; } = new();
    public PostCheepModel(ICheepRepository repository) => _repository = repository;
    
   /* public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }
        return RedirectToPage("/Public");
    }*/

    public IActionResult OnPost()
    {
        var email = User.Identity.Name;
        var userName = email;
        var text = Message;

        /*if (userName.Name == null)
        {
            return RedirectToPage("/");
        }*/
        
        _repository.CreateCheep(userName,email, text);
        return RedirectToPage("/MyPage");
    }
}