using Microsoft.AspNetCore.Mvc.RazorPages;
using MiddlewareEngine.Services;
using MiddlewareEngine.Models;

namespace MiddlewareEngine.Pages;

public class IndexModel : PageModel
{
    private readonly IFunctionDefinitionService _functionService;

    public IndexModel(IFunctionDefinitionService functionService)
    {
        _functionService = functionService;
    }

    public List<FunctionDefinition> Functions { get; set; } = new();

    public async Task OnGetAsync()
    {
        Functions = await _functionService.GetAllFunctionsAsync();
    }
}
