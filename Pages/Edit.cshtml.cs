using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MiddlewareEngine.Services;
using MiddlewareEngine.Models;
using System.Text.Json;

namespace MiddlewareEngine.Pages;

public class EditModel : PageModel
{
    private readonly IFunctionDefinitionService _functionService;

    public EditModel(IFunctionDefinitionService functionService)
    {
        _functionService = functionService;
    }

    [BindProperty]
    public FunctionDefinition Function { get; set; } = new();

    [BindProperty]
    public string ParametersJson { get; set; } = "[]";

    public async Task<IActionResult> OnGetAsync(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return RedirectToPage("Index");
        }

        var function = await _functionService.GetFunctionByIdAsync(id);
        if (function == null)
        {
            TempData["Error"] = "Function not found";
            return RedirectToPage("Index");
        }

        Function = function;
        ParametersJson = JsonSerializer.Serialize(Function.Parameters);
        
        return Page();
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

            var success = await _functionService.UpdateFunctionAsync(Function.Id!, Function);
            
            if (success)
            {
                TempData["Success"] = "Function updated successfully!";
                return RedirectToPage("Index");
            }
            else
            {
                TempData["Error"] = "Failed to update function";
                return Page();
            }
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return Page();
        }
    }
}
