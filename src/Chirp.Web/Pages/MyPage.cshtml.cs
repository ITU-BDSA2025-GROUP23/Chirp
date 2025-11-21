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
    public IActionResult OnGet()
    {
            if (!(User.Identity?.IsAuthenticated ?? false))
                return RedirectToPage("/Public");   

            var name = User.Identity!.Name!;
            return RedirectToPage("/UserTimeline", new { author = name });
    }
}

