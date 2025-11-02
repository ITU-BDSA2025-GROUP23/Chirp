using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;                 
using Chirp.Razor.DataModel;
using Database;

namespace Chirp.Razor.Pages;

public class PaginationModel : PageModel
{
    [BindProperty(Name = "p", SupportsGet = true)]
    public int CurrentPage { get; set; } = 1;

    public int PageSize { get; set; } = 32;
}

public class PublicModel : PaginationModel
{
    private readonly ICheepRepository _service;

    public List<Cheep> Cheeps { get; set; } = new();
    public int TotalCheeps { get; private set; }
    public int NumberOfCheeps => Cheeps.Count;
    public int TotalPages => (int)System.Math.Ceiling((double)TotalCheeps / PageSize);

    public PublicModel(ICheepRepository service)
    {
        _service = service;
    }

    public IActionResult OnGet([FromQuery(Name = "page")] int page = 1)
    {
        if (int.TryParse(Request.Query["p"], out var p) && p > 0) CurrentPage = p;
        else
            CurrentPage = 1;
            
        TotalCheeps = _service.GetAllCheeps().Count();

        var lastPage = System.Math.Max(1, (int)System.Math.Ceiling((double)TotalCheeps / PageSize));
        if (CurrentPage > lastPage) CurrentPage = lastPage;

        Cheeps = _service.GetPaginatedCheeps(CurrentPage, PageSize);
        return Page();
    }
}