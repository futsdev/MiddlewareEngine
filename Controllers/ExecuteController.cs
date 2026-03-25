using Microsoft.AspNetCore.Mvc;
using MiddlewareEngine.Models;
using MiddlewareEngine.Executors;

namespace MiddlewareEngine.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExecuteController : ControllerBase
{
    private readonly IExecutionEngine _executionEngine;
    private readonly ILogger<ExecuteController> _logger;

    public ExecuteController(
        IExecutionEngine executionEngine,
        ILogger<ExecuteController> logger)
    {
        _executionEngine = executionEngine;
        _logger = logger;
    }

    /// <summary>
    /// Execute a function by its function_id
    /// </summary>
    /// <param name="request">Function execution request containing function_id and parameters</param>
    /// <returns>Function execution response</returns>
    [HttpPost]
    public async Task<ActionResult<FunctionExecutionResponse>> Execute([FromBody] FunctionExecutionRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.FunctionId))
            {
                return BadRequest(new { error = "FunctionId is required" });
            }

            _logger.LogInformation("Executing function: {FunctionId}", request.FunctionId);

            var response = await _executionEngine.ExecuteFunctionAsync(
                request.FunctionId,
                request.Parameters);

            if (response.Success)
            {
                return Ok(response);
            }
            else
            {
                return BadRequest(response);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing function: {FunctionId}", request.FunctionId);
            return StatusCode(500, new FunctionExecutionResponse
            {
                Success = false,
                ErrorMessage = "Internal server error",
                FunctionId = request.FunctionId
            });
        }
    }

    /// <summary>
    /// Execute a function by its function_id (alternative endpoint with function_id in route)
    /// </summary>
    [HttpPost("{functionId}")]
    public async Task<ActionResult<FunctionExecutionResponse>> ExecuteById(
        string functionId,
        [FromBody] Dictionary<string, object>? parameters = null)
    {
        try
        {
            _logger.LogInformation("Executing function: {FunctionId}", functionId);

            var response = await _executionEngine.ExecuteFunctionAsync(functionId, parameters);

            if (response.Success)
            {
                return Ok(response);
            }
            else
            {
                return BadRequest(response);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing function: {FunctionId}", functionId);
            return StatusCode(500, new FunctionExecutionResponse
            {
                Success = false,
                ErrorMessage = "Internal server error",
                FunctionId = functionId
            });
        }
    }
}
