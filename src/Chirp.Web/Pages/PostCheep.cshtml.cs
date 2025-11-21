using Microsoft.AspNetCore.Mvc;
using Chirp.Core.DTOs;
using Chirp.Infrastructure.Repositories;
namespace Chirp.Web.Pages;
using Microsoft.AspNetCore.Mvc.RazorPages;


public class  PostCheepModel : PageModel //PaginationModel?
{
    [BindProperty] public string Message { get; set; }
    
    private readonly ICheepRepository _repository;
    
    public List<CheepDTO> Cheeps { get; set; } = new();
    public PostCheepModel(ICheepRepository repository) => _repository = repository;
    
    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }
        return RedirectToPage("/Public");
    }
}