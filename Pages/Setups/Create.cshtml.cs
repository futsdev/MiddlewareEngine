using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MiddlewareEngine.Pages.Setups
{
    public class CreateModel : PageModel
    {
        public IActionResult OnGet()
        {
            return Page();
        }
    }
}
