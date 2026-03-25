using Microsoft.AspNetCore.Mvc;
using MiddlewareEngine.Models;
using MiddlewareEngine.Services;

namespace MiddlewareEngine.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestCasesController : ControllerBase
{
    private readonly TestCaseService _testCaseService;
    private readonly ILogger<TestCasesController> _logger;

    public TestCasesController(TestCaseService testCaseService, ILogger<TestCasesController> logger)
    {
        _testCaseService = testCaseService;
        _logger = logger;
    }

    /// <summary>
    /// Get all test cases
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<TestCase>>> GetAll([FromQuery] string? tag = null, [FromQuery] string? status = null)
    {
        try
        {
            if (!string.IsNullOrEmpty(tag))
            {
                var testCases = await _testCaseService.GetTestCasesByTagAsync(tag);
                return Ok(testCases);
            }

            if (!string.IsNullOrEmpty(status))
            {
                var testCases = await _testCaseService.GetTestCasesByStatusAsync(status);
                return Ok(testCases);
            }

            var allTestCases = await _testCaseService.GetAllTestCasesAsync();
            return Ok(allTestCases);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving test cases");
            return StatusCode(500, new { error = "Failed to retrieve test cases", message = ex.Message });
        }
    }

    /// <summary>
    /// Get test case by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<TestCase>> GetById(string id)
    {
        try
        {
            var testCase = await _testCaseService.GetTestCaseByIdAsync(id);
            if (testCase == null)
            {
                return NotFound(new { error = "Test case not found", id });
            }
            return Ok(testCase);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving test case {Id}", id);
            return StatusCode(500, new { error = "Failed to retrieve test case", message = ex.Message });
        }
    }

    /// <summary>
    /// Create a new test case
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<TestCase>> Create([FromBody] TestCase testCase)
    {
        try
        {
            var created = await _testCaseService.CreateTestCaseAsync(testCase);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating test case");
            return StatusCode(500, new { error = "Failed to create test case", message = ex.Message });
        }
    }

    /// <summary>
    /// Update an existing test case
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<TestCase>> Update(string id, [FromBody] TestCase testCase)
    {
        try
        {
            var updated = await _testCaseService.UpdateTestCaseAsync(id, testCase);
            if (!updated)
            {
                return NotFound(new { error = "Test case not found", id });
            }
            
            // Return the updated test case
            var updatedTestCase = await _testCaseService.GetTestCaseByIdAsync(id);
            return Ok(updatedTestCase);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating test case {Id}", id);
            return StatusCode(500, new { error = "Failed to update test case", message = ex.Message });
        }
    }

    /// <summary>
    /// Delete a test case
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(string id)
    {
        try
        {
            var deleted = await _testCaseService.DeleteTestCaseAsync(id);
            if (!deleted)
            {
                return NotFound(new { error = "Test case not found", id });
            }
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting test case {Id}", id);
            return StatusCode(500, new { error = "Failed to delete test case", message = ex.Message });
        }
    }

    /// <summary>
    /// Duplicate a test case
    /// </summary>
    [HttpPost("{id}/duplicate")]
    public async Task<ActionResult<TestCase>> Duplicate(string id)
    {
        try
        {
            var duplicated = await _testCaseService.DuplicateTestCaseAsync(id);
            if (duplicated == null)
            {
                return NotFound(new { error = "Test case not found", id });
            }
            return CreatedAtAction(nameof(GetById), new { id = duplicated.Id }, duplicated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error duplicating test case {Id}", id);
            return StatusCode(500, new { error = "Failed to duplicate test case", message = ex.Message });
        }
    }

    /// <summary>
    /// Get execution history for a test case
    /// </summary>
    [HttpGet("{id}/executions")]
    public async Task<ActionResult<List<TestCaseExecution>>> GetExecutionHistory(string id)
    {
        try
        {
            var executions = await _testCaseService.GetExecutionHistoryAsync(id);
            return Ok(executions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving execution history for test case {Id}", id);
            return StatusCode(500, new { error = "Failed to retrieve execution history", message = ex.Message });
        }
    }

    /// <summary>
    /// Get recent executions across all test cases
    /// </summary>
    [HttpGet("executions/recent")]
    public async Task<ActionResult<List<TestCaseExecution>>> GetRecentExecutions([FromQuery] int limit = 10)
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
