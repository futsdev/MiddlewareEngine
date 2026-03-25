using Microsoft.AspNetCore.Mvc;
using MiddlewareEngine.Models;
using MiddlewareEngine.Services;
using System.Text.Json;

namespace MiddlewareEngine.Controllers;

[ApiController]
[Route("api/campaigns")]
public class CampaignsController : ControllerBase
{
    private readonly ICampaignService _campaignService;
    private readonly ILogger<CampaignsController> _logger;

    public CampaignsController(ICampaignService campaignService, ILogger<CampaignsController> logger)
    {
        _campaignService = campaignService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<Campaign>>> GetAll([FromQuery] string? projectId = null)
    {
        try
        {
            var campaigns = string.IsNullOrEmpty(projectId)
                ? await _campaignService.GetAllCampaignsAsync()
                : await _campaignService.GetCampaignsByProjectIdAsync(projectId);
            return Ok(campaigns);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting campaigns");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Campaign>> GetById(string id)
    {
        try
        {
            var campaign = await _campaignService.GetCampaignByIdAsync(id);
            if (campaign == null)
                return NotFound();
            return Ok(campaign);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting campaign {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost]
    public async Task<ActionResult<Campaign>> Create([FromBody] Campaign campaign)
    {
        try
        {
            var created = await _campaignService.CreateCampaignAsync(campaign);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating campaign");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(string id, [FromBody] Campaign campaign)
    {
        try
        {
            if (id != campaign.Id)
                return BadRequest("ID mismatch");

            // Convert JsonElement to proper objects for MongoDB serialization
            ConvertJsonElementsInCampaign(campaign);

            var updated = await _campaignService.UpdateCampaignAsync(campaign);
            if (!updated)
                return NotFound();
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating campaign {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    private void ConvertJsonElementsInCampaign(Campaign campaign)
    {
        if (campaign.TestGroups == null) return;

        foreach (var testGroup in campaign.TestGroups)
        {
            if (testGroup.TestCaseExecutions == null) continue;

            foreach (var testCase in testGroup.TestCaseExecutions)
            {
                if (testCase.ParameterOverrides != null)
                    testCase.ParameterOverrides = ConvertJsonElementDictionary(testCase.ParameterOverrides);

                if (testCase.TestCaseDefinition == null) continue;

                ConvertOperationsInSection(testCase.TestCaseDefinition.Setup);
                ConvertOperationsInSection(testCase.TestCaseDefinition.Teardown);
                ConvertOperationsInSection(testCase.TestCaseDefinition.Cleanup);

                if (testCase.TestCaseDefinition.Steps != null)
                {
                    foreach (var step in testCase.TestCaseDefinition.Steps)
                    {
                        if (step.Actions == null) continue;
                        foreach (var action in step.Actions)
                        {
                            ConvertOperationsList(action.PreConditions);
                            ConvertOperationsList(action.Operations);
                            ConvertOperationsList(action.PostConditions);
                        }
                    }
                }
            }
        }
    }

    private void ConvertOperationsInSection(TestCaseSection? section)
    {
        if (section?.Operations != null)
            ConvertOperationsList(section.Operations);
    }

    private void ConvertOperationsList(List<OperationWithFunction>? operations)
    {
        if (operations == null) return;

        foreach (var op in operations)
        {
            if (op.ParameterOverrides != null)
                op.ParameterOverrides = ConvertJsonElementDictionary(op.ParameterOverrides);

            if (op.FunctionDefinition != null)
                op.FunctionDefinition = ConvertJsonElementDictionary(op.FunctionDefinition);
        }
    }

    private Dictionary<string, object> ConvertJsonElementDictionary(Dictionary<string, object> dict)
    {
        var result = new Dictionary<string, object>();
        foreach (var kvp in dict)
        {
            result[kvp.Key] = ConvertJsonElement(kvp.Value);
        }
        return result;
    }

    private object ConvertJsonElement(object value)
    {
        if (value is JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.String => element.GetString() ?? "",
                JsonValueKind.Number => element.TryGetInt32(out int i) ? i : element.GetDouble(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Null => null!,
                JsonValueKind.Object => element.EnumerateObject().ToDictionary(
                    p => p.Name,
                    p => ConvertJsonElement((object)p.Value)
                ),
                JsonValueKind.Array => element.EnumerateArray().Select(e => ConvertJsonElement((object)e)).ToList(),
                _ => value
            };
        }
        return value;
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(string id)
    {
        try
        {
            var deleted = await _campaignService.DeleteCampaignAsync(id);
            if (!deleted)
                return NotFound();
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting campaign {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }
}
