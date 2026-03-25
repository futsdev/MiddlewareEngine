using Microsoft.AspNetCore.Mvc;
using MiddlewareEngine.Models;
using MiddlewareEngine.Services;

namespace MiddlewareEngine.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FunctionsController : ControllerBase
{
    private readonly IFunctionDefinitionService _functionService;
    private readonly ILogger<FunctionsController> _logger;

    public FunctionsController(
        IFunctionDefinitionService functionService,
        ILogger<FunctionsController> logger)
    {
        _functionService = functionService;
        _logger = logger;
    }

    /// <summary>
    /// Get all function definitions
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<FunctionDefinition>>> GetAll()
    {
        try
        {
            var functions = await _functionService.GetAllFunctionsAsync();
            return Ok(functions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving functions");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get all function IDs for debugging
    /// </summary>
    [HttpGet("ids")]
    public async Task<ActionResult> GetIds()
    {
        try
        {
            var functions = await _functionService.GetAllFunctionsAsync();
            var ids = functions.Select(f => new { f.Id, f.FunctionId, f.Name, f.ExecutionType }).ToList();
            return Ok(ids);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving function IDs");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get active function definitions
    /// </summary>
    [HttpGet("active")]
    public async Task<ActionResult<List<FunctionDefinition>>> GetActive()
    {
        try
        {
            var functions = await _functionService.GetActiveFunctionsAsync();
            return Ok(functions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active functions");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get function definition by MongoDB ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<FunctionDefinition>> GetById(string id)
    {
        try
        {
            var function = await _functionService.GetFunctionByIdAsync(id);
            if (function == null)
            {
                return NotFound(new { error = $"Function with ID '{id}' not found" });
            }
            return Ok(function);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving function by ID: {Id}", id);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get function definition by function_id
    /// </summary>
    [HttpGet("by-function-id/{functionId}")]
    public async Task<ActionResult<FunctionDefinition>> GetByFunctionId(string functionId)
    {
        try
        {
            var function = await _functionService.GetFunctionByFunctionIdAsync(functionId);
            if (function == null)
            {
                return NotFound(new { error = $"Function with FunctionId '{functionId}' not found" });
            }
            return Ok(function);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving function by FunctionId: {FunctionId}", functionId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Create a new function definition
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<FunctionDefinition>> Create([FromBody] FunctionDefinition functionDefinition)
    {
        try
        {
            var created = await _functionService.CreateFunctionAsync(functionDefinition);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating function");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Update an existing function definition
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult> Update(string id, [FromBody] FunctionDefinition functionDefinition)
    {
        try
        {
            var existing = await _functionService.GetFunctionByIdAsync(id);
            if (existing == null)
            {
                return NotFound(new { error = $"Function with ID '{id}' not found" });
            }

            functionDefinition.Id = id;
            var success = await _functionService.UpdateFunctionAsync(id, functionDefinition);
            
            if (success)
            {
                return NoContent();
            }
            
            return StatusCode(500, new { error = "Update failed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating function: {Id}", id);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Delete a function definition
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(string id)
    {
        try
        {
            var existing = await _functionService.GetFunctionByIdAsync(id);
            if (existing == null)
            {
                return NotFound(new { error = $"Function with ID '{id}' not found" });
            }

            var success = await _functionService.DeleteFunctionAsync(id);
            
            if (success)
            {
                return NoContent();
            }
            
            return StatusCode(500, new { error = "Delete failed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting function: {Id}", id);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }
}
