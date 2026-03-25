using Microsoft.AspNetCore.Mvc;
using MiddlewareEngine.Models;
using MiddlewareEngine.Services;
using MongoDB.Driver;

namespace MiddlewareEngine.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ValidationController : ControllerBase
{
    private readonly TestCaseService _testCaseService;
    private readonly IFunctionDefinitionService _functionService;
    private readonly ILogger<ValidationController> _logger;

    public ValidationController(
        TestCaseService testCaseService,
        IFunctionDefinitionService functionService,
        ILogger<ValidationController> logger)
    {
        _testCaseService = testCaseService;
        _functionService = functionService;
        _logger = logger;
    }

    /// <summary>
    /// Validate all test cases and check for invalid function IDs
    /// </summary>
    [HttpGet("testcases")]
    public async Task<ActionResult> ValidateTestCases()
    {
        try
        {
            var testCases = await _testCaseService.GetAllTestCasesAsync();
            var functions = await _functionService.GetAllFunctionsAsync();
            var validFunctionIds = functions.Select(f => f.Id).ToHashSet();

            var invalidTestCases = new List<object>();

            foreach (var testCase in testCases)
            {
                var invalidOperations = new List<string>();

                // Check setup operations
                if (testCase.SetupOperations != null)
                {
                    foreach (var op in testCase.SetupOperations)
                    {
                        if (!string.IsNullOrEmpty(op.FunctionDefinitionId) && 
                            !validFunctionIds.Contains(op.FunctionDefinitionId))
                        {
                            invalidOperations.Add($"Setup: {op.Name} (ID: {op.FunctionDefinitionId})");
                        }
                    }
                }

                // Check step operations
                if (testCase.Steps != null)
                {
                    foreach (var step in testCase.Steps)
                    {
                        if (step.Actions != null)
                        {
                            foreach (var action in step.Actions)
                            {
                                if (action.Operations != null)
                                {
                                    foreach (var op in action.Operations)
                                    {
                                        if (!string.IsNullOrEmpty(op.FunctionDefinitionId) && 
                                            !validFunctionIds.Contains(op.FunctionDefinitionId))
                                        {
                                            invalidOperations.Add($"Step '{step.Name}' > Action '{action.Name}' > Operation: {op.Name} (ID: {op.FunctionDefinitionId})");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                // Check teardown operations
                if (testCase.TeardownOperations != null)
                {
                    foreach (var op in testCase.TeardownOperations)
                    {
                        if (!string.IsNullOrEmpty(op.FunctionDefinitionId) && 
                            !validFunctionIds.Contains(op.FunctionDefinitionId))
                        {
                            invalidOperations.Add($"Teardown: {op.Name} (ID: {op.FunctionDefinitionId})");
                        }
                    }
                }

                if (invalidOperations.Any())
                {
                    invalidTestCases.Add(new
                    {
                        TestCaseId = testCase.Id,
                        TestCaseName = testCase.Name,
                        InvalidOperations = invalidOperations
                    });
                }
            }

            return Ok(new
            {
                TotalTestCases = testCases.Count,
                InvalidTestCasesCount = invalidTestCases.Count,
                ValidFunctionIds = validFunctionIds.OrderBy(id => id).ToList(),
                InvalidTestCases = invalidTestCases
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating test cases");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get function IDs mapping
    /// </summary>
    [HttpGet("functions")]
    public async Task<ActionResult> GetFunctionIds()
    {
        try
        {
            var functions = await _functionService.GetAllFunctionsAsync();
            var mapping = functions.Select(f => new
            {
                MongoId = f.Id,
                FunctionId = f.FunctionId,
                Name = f.Name,
                Type = f.ExecutionType
            }).ToList();

            return Ok(mapping);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting function IDs");
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
