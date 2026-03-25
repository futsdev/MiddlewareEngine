using MiddlewareEngine.Models;
using MongoDB.Driver;

namespace MiddlewareEngine.Repositories;

public interface ICampaignRepository
{
    Task<List<Campaign>> GetAllAsync();
    Task<Campaign?> GetByIdAsync(string id);
    Task<List<Campaign>> GetByProjectIdAsync(string projectId);
    Task<Campaign> CreateAsync(Campaign campaign);
    Task<bool> UpdateAsync(Campaign campaign);
    Task<bool> DeleteAsync(string id);
}

public class CampaignRepository : ICampaignRepository
{
    private readonly IMongoCollection<Campaign> _campaigns;

    public CampaignRepository(IMongoDatabase database)
    {
        _campaigns = database.GetCollection<Campaign>("campaigns");
    }

    public async Task<List<Campaign>> GetAllAsync()
    {
        return await _campaigns.Find(_ => true).ToListAsync();
    }

    public async Task<Campaign?> GetByIdAsync(string id)
    {
        return await _campaigns.Find(c => c.Id == id).FirstOrDefaultAsync();
    }

    public async Task<List<Campaign>> GetByProjectIdAsync(string projectId)
    {
        return await _campaigns.Find(c => c.ProjectId == projectId).ToListAsync();
    }

    public async Task<Campaign> CreateAsync(Campaign campaign)
    {
        campaign.CreatedAt = DateTime.UtcNow;
        campaign.UpdatedAt = DateTime.UtcNow;
        await _campaigns.InsertOneAsync(campaign);
        return campaign;
    }

    public async Task<bool> UpdateAsync(Campaign campaign)
    {
        campaign.UpdatedAt = DateTime.UtcNow;
        var result = await _campaigns.ReplaceOneAsync(c => c.Id == campaign.Id, campaign);
        return result.ModifiedCount > 0;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var result = await _campaigns.DeleteOneAsync(c => c.Id == id);
        return result.DeletedCount > 0;
    }
}
