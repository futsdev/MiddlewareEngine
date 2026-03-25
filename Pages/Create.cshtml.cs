using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MiddlewareEngine.Services;
using MiddlewareEngine.Models;
using System.Text.Json;

namespace MiddlewareEngine.Pages;

public class CreateModel : PageModel
{
    private readonly IFunctionDefinitionService _functionService;

    public CreateModel(IFunctionDefinitionService functionService)
    {
        _functionService = functionService;
    }

    [BindProperty]
    public FunctionDefinition Function { get; set; } = new();

    [BindProperty]
    public string ParametersJson { get; set; } = "[]";

    public void OnGet()
    {
        Function.ExecutionConfig = new ExecutionConfig();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            // Parse parameters from JSON
            if (!string.IsNullOrEmpty(ParametersJson))
            {
                Function.Parameters = JsonSerializer.Deserialize<List<FunctionParameter>>(ParametersJson) ?? new();
            }

            await _functionService.CreateFunctionAsync(Function);
            TempData["Success"] = "Function created successfully!";
            return RedirectToPage("Index");
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return Page();
        }
    }
}
