using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using Chirp.Core.DTOs;
using Chirp.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;


namespace Chirp.Web.Pages;
public class MyPageModel : PaginationModel
{

    private readonly ICheepRepository _repository;

    public MyPageModel(ICheepRepository repository) => _repository = repository;
    public IActionResult OnGet()
    {
            if (!(User.Identity?.IsAuthenticated ?? false))
                return RedirectToPage("/Public");   
            
            var email = User.Identity!.Name!;
            
            var author = _repository.GetAuthorByEmail(email);
            
            var authorName = author?.Name ?? email;
            
            return RedirectToPage("/UserTimeline", new { author = authorName });
    }
}

