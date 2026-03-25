using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MiddlewareEngine.Pages.Projects
{
    public class EditModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string? Id { get; set; }

        public IActionResult OnGet()
        {
            if (string.IsNullOrEmpty(Id))
            {
                return RedirectToPage("/Projects/Index");
            }
            return Page();
        }
    }
}
