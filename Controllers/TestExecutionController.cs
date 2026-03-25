using Microsoft.AspNetCore.Mvc;
using MiddlewareEngine.Executors;
using MiddlewareEngine.Models;
using MiddlewareEngine.Services;

namespace MiddlewareEngine.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestExecutionController : ControllerBase
{
    private readonly TestCaseExecutor _testCaseExecutor;
    private readonly TestCaseService _testCaseService;
    private readonly ILogger<TestExecutionController> _logger;

    public TestExecutionController(
        TestCaseExecutor testCaseExecutor,
        TestCaseService testCaseService,
        ILogger<TestExecutionController> logger)
    {
        _testCaseExecutor = testCaseExecutor;
        _testCaseService = testCaseService;
        _logger = logger;
    }

    /// <summary>
    /// Execute a test case by ID
    /// </summary>
    [HttpPost("{testCaseId}")]
    public async Task<ActionResult<TestCaseExecution>> ExecuteTestCase(string testCaseId)
    {
        try
        {
            _logger.LogInformation("Executing test case: {TestCaseId}", testCaseId);
            var execution = await _testCaseExecutor.ExecuteTestCaseAsync(testCaseId);
            return Ok(execution);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing test case: {TestCaseId}", testCaseId);
            return StatusCode(500, new { error = "Failed to execute test case", message = ex.Message });
        }
    }

    /// <summary>
    /// Execute a test case with inline definition (without saving)
    /// </summary>
    [HttpPost("execute")]
    public async Task<ActionResult<TestCaseExecution>> ExecuteInlineTestCase([FromBody] TestCase testCase)
    {
        try
        {
            _logger.LogInformation("Executing inline test case: {TestCaseName}", testCase.Name);
            var execution = await _testCaseExecutor.ExecuteTestCaseAsync(testCase);
            return Ok(execution);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing inline test case: {TestCaseName}", testCase.Name);
            return StatusCode(500, new { error = "Failed to execute test case", message = ex.Message });
        }
    }

    /// <summary>
    /// Get execution details by ID
    /// </summary>
    [HttpGet("execution/{executionId}")]
    public async Task<ActionResult<TestCaseExecution>> GetExecution(string executionId)
    {
        try
        {
            var execution = await _testCaseService.GetRecentExecutionsAsync(100);
            var result = execution.FirstOrDefault(e => e.Id == executionId);
            
            if (result == null)
            {
                return NotFound(new { error = "Execution not found", executionId });
            }
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving execution: {ExecutionId}", executionId);
            return StatusCode(500, new { error = "Failed to retrieve execution", message = ex.Message });
        }
    }

    /// <summary>
    /// Get execution history for a specific test case
    /// </summary>
    [HttpGet("history/{testCaseId}")]
    public async Task<ActionResult<List<TestCaseExecution>>> GetExecutionHistory(string testCaseId)
    {
        try
        {
            _logger.LogInformation("Retrieving execution history for test case: {TestCaseId}", testCaseId);
            var executions = await _testCaseService.GetExecutionsByTestCaseIdAsync(testCaseId);
            return Ok(executions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving execution history: {TestCaseId}", testCaseId);
            return StatusCode(500, new { error = "Failed to retrieve execution history", message = ex.Message });
        }
    }

    /// <summary>
    /// Get all recent executions
    /// </summary>
    [HttpGet("recent")]
    public async Task<ActionResult<List<TestCaseExecution>>> GetRecentExecutions([FromQuery] int limit = 20)
    {
        try
        {
            var executions = await _testCaseService.GetRecentExecutionsAsync(limit);
            return Ok(executions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving recent executions");
            return StatusCode(500, new { error = "Failed to retrieve recent executions", message = ex.Message });
        }
    }
}
