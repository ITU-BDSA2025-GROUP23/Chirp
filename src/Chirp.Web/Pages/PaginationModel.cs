using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
namespace Chirp.Web.Pages;

public class PaginationModel : PageModel
{
    [BindProperty(SupportsGet = true, Name = "p")]

    public int CurrentPage { get; set; } = 1;

    public int PageSize { get; set; } = 32;
}