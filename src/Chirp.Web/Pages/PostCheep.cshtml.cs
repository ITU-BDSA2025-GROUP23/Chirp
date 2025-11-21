using Microsoft.AspNetCore.Mvc;
using Chirp.Core.DTOs;
using Chirp.Infrastructure.Repositories;
namespace Chirp.Web.Pages;


public class  PostCheepModel
{
    [BindProperty] public string Message { get; set; }
    
    private readonly ICheepRepository _repository;
    
    public List<CheepDTO> Cheeps { get; set; } = new();
    public PostCheepModel(ICheepRepository repository) => _repository = repository;
    
    /*public IActionResult OnPost()
    {
        
    }
    */
}