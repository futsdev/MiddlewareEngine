using MiddlewareEngine.Models;
using MiddlewareEngine.Repositories;

namespace MiddlewareEngine.Services;

public interface ICampaignService
{
    Task<List<Campaign>> GetAllCampaignsAsync();
    Task<Campaign?> GetCampaignByIdAsync(string id);
    Task<List<Campaign>> GetCampaignsByProjectIdAsync(string projectId);
    Task<Campaign> CreateCampaignAsync(Campaign campaign);
    Task<bool> UpdateCampaignAsync(Campaign campaign);
    Task<bool> DeleteCampaignAsync(string id);
}

public class CampaignService : ICampaignService
{
    private readonly ICampaignRepository _repository;
    private readonly ILogger<CampaignService> _logger;

    public CampaignService(ICampaignRepository repository, ILogger<CampaignService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<List<Campaign>> GetAllCampaignsAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<Campaign?> GetCampaignByIdAsync(string id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<List<Campaign>> GetCampaignsByProjectIdAsync(string projectId)
    {
        return await _repository.GetByProjectIdAsync(projectId);
    }

    public async Task<Campaign> CreateCampaignAsync(Campaign campaign)
    {
        _logger.LogInformation("Creating campaign: {Name}", campaign.Name);
        return await _repository.CreateAsync(campaign);
    }

    public async Task<bool> UpdateCampaignAsync(Campaign campaign)
    {
        _logger.LogInformation("Updating campaign: {Id}", campaign.Id);
        return await _repository.UpdateAsync(campaign);
    }

    public async Task<bool> DeleteCampaignAsync(string id)
    {
        _logger.LogInformation("Deleting campaign: {Id}", id);
        return await _repository.DeleteAsync(id);
    }
}
