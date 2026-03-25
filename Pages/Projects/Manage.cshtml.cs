using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MiddlewareEngine.Pages.Projects;

public class ManageModel : PageModel
{
    public string Id { get; set; } = string.Empty;

    public void OnGet()
    {
    }
}
