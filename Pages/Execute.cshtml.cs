using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MiddlewareEngine.Services;
using MiddlewareEngine.Models;
using MiddlewareEngine.Executors;
using System.Text.Json;

namespace MiddlewareEngine.Pages;

public class ExecuteModel : PageModel
{
    private readonly IFunctionDefinitionService _functionService;
    private readonly IExecutionEngine _executionEngine;

    public ExecuteModel(IFunctionDefinitionService functionService, IExecutionEngine executionEngine)
    {
        _functionService = functionService;
        _executionEngine = executionEngine;
    }

    public List<FunctionDefinition> Functions { get; set; } = new();
    public FunctionDefinition? SelectedFunction { get; set; }
    
    [BindProperty]
    public string FunctionId { get; set; } = string.Empty;
    
    [BindProperty]
    public string ParametersJson { get; set; } = "{}";
    
    public FunctionExecutionResponse? ExecutionResult { get; set; }

    public async Task OnGetAsync(string? functionId = null)
    {
        Functions = await _functionService.GetActiveFunctionsAsync();
        
        if (!string.IsNullOrEmpty(functionId))
        {
            SelectedFunction = await _functionService.GetFunctionByFunctionIdAsync(functionId);
            FunctionId = functionId;
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        Functions = await _functionService.GetActiveFunctionsAsync();
        SelectedFunction = await _functionService.GetFunctionByFunctionIdAsync(FunctionId);
        
        try
        {
            Dictionary<string, object>? parameters = null;
            
            if (!string.IsNullOrEmpty(ParametersJson) && ParametersJson != "{}")
            {
                parameters = JsonSerializer.Deserialize<Dictionary<string, object>>(ParametersJson);
            }
            
            ExecutionResult = await _executionEngine.ExecuteFunctionAsync(FunctionId, parameters);
            
            if (ExecutionResult.Success)
            {
                TempData["Success"] = "Function executed successfully!";
            }
            else
            {
                TempData["Error"] = ExecutionResult.ErrorMessage;
            }
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }
        
        return Page();
    }
}
