using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MiddlewareEngine.Pages.Campaigns;

public class BuilderModel : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string? Id { get; set; }

    public void OnGet()
    {
    }
}
