using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Chirp.Core.DTOs;
using Chirp.Infrastructure.Repositories;
namespace Chirp.Web.Pages;


public class PostCheepModel : PageModel
{
    [BindProperty] public string Message { get; set; }
    
    private readonly ICheepRepository _repository;
    
    public List<CheepDTO> Cheeps { get; set; } = new();

    public PostCheepModel(ICheepRepository repository)
    {
        _repository = repository;
    }

    public IActionResult OnPost()
    {

        if (!ModelState.IsValid)
        {
            return Page();
        }

        _repository.CreateCheep("LeBron", "LeRonTeRon@gmail.com", Message);
        
        
        return RedirectToPage("/Public");
    }
}