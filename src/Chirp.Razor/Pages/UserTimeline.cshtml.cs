using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Chirp.Razor.DataModel;
using System.Collections.Generic;

namespace Chirp.Razor.Pages;

public class UserTimelineModel : PageModel
{
    private readonly ICheepRepository _service;
    public IEnumerable<Cheep>? Cheeps { get; set; } 

    public UserTimelineModel(ICheepRepository service)
    {
        _service = service;
    }

    public ActionResult OnGet()
    {
        Cheeps = _service.GetAllCheeps();
        return Page();
    }
}