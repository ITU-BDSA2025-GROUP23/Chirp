using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Chirp.Razor.DataModel;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Database;

namespace Chirp.Razor.Pages;

public class PaginationModel : PageModel
{
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 32;
}
public class PublicModel : PaginationModel
{
        private readonly ICheepService _service;
        public List<Cheep> Cheeps => _service.GetCheeps();
        public List<Cheep> CurrentPageCheeps { get; set; }
        public int NumberOfCheeps => Cheeps.Count;
        public int TotalPages => (int)Math.Ceiling((double)NumberOfCheeps / PageSize);

        public PublicModel(ICheepService service)
        {
            _service = service;
        }
        public ActionResult OnGet(int? id = 1)
        {
            CurrentPage = id ?? 1;
            CurrentPageCheeps = _service.GetPaginatedCheeps(CurrentPage, PageSize);
            return Page();
        }
}