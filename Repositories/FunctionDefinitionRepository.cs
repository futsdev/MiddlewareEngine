using MongoDB.Driver;
using MongoDB.Bson;
using MiddlewareEngine.Models;
using MiddlewareEngine.Configuration;
using Microsoft.Extensions.Options;

namespace MiddlewareEngine.Repositories;

public interface IFunctionDefinitionRepository
{
    Task<List<FunctionDefinition>> GetAllAsync();
    Task<FunctionDefinition?> GetByIdAsync(string id);
    Task<FunctionDefinition?> GetByFunctionIdAsync(string functionId);
    Task<FunctionDefinition> CreateAsync(FunctionDefinition functionDefinition);
    Task<bool> UpdateAsync(string id, FunctionDefinition functionDefinition);
    Task<bool> DeleteAsync(string id);
    Task<List<FunctionDefinition>> GetActiveAsync();
}

public class FunctionDefinitionRepository : IFunctionDefinitionRepository
{
    private readonly IMongoCollection<FunctionDefinition> _functionDefinitions;

    public FunctionDefinitionRepository(IOptions<MongoDbSettings> settings)
    {
        var mongoClient = new MongoClient(settings.Value.ConnectionString);
        var mongoDatabase = mongoClient.GetDatabase(settings.Value.DatabaseName);
        _functionDefinitions = mongoDatabase.GetCollection<FunctionDefinition>(
            settings.Value.FunctionDefinitionsCollectionName);
    }

    public async Task<List<FunctionDefinition>> GetAllAsync()
    {
        return await _functionDefinitions.Find(_ => true).ToListAsync();
    }

    public async Task<FunctionDefinition?> GetByIdAsync(string id)
    {
        try
        {
            // Try direct string comparison first
            var result = await _functionDefinitions.Find(x => x.Id == id).FirstOrDefaultAsync();
            
            if (result == null)
            {
                // If not found, try converting to ObjectId
                if (ObjectId.TryParse(id, out var objectId))
                {
                    var filter = Builders<FunctionDefinition>.Filter.Eq("_id", objectId);
                    result = await _functionDefinitions.Find(filter).FirstOrDefaultAsync();
                }
            }
            
            if (result == null)
            {
                // Log all available IDs for debugging
                var allFunctions = await _functionDefinitions.Find(_ => true).Project(f => new { f.Id, f.FunctionId, f.Name }).ToListAsync();
                Console.WriteLine($"[FunctionRepository] Function with Id '{id}' not found. Available functions:");
                foreach (var func in allFunctions)
                {
                    Console.WriteLine($"  - Id: {func.Id}, FunctionId: {func.FunctionId}, Name: {func.Name}");
                }
            }
            else
            {
                Console.WriteLine($"[FunctionRepository] Successfully found function: {result.Name} (Id: {result.Id})");
            }
            
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[FunctionRepository] Error in GetByIdAsync: {ex.Message}");
            throw;
        }
    }

    public async Task<FunctionDefinition?> GetByFunctionIdAsync(string functionId)
    {
        return await _functionDefinitions
            .Find(x => x.FunctionId == functionId && x.IsActive)
            .FirstOrDefaultAsync();
    }

    public async Task<FunctionDefinition> CreateAsync(FunctionDefinition functionDefinition)
    {
        functionDefinition.CreatedAt = DateTime.UtcNow;
        functionDefinition.UpdatedAt = DateTime.UtcNow;
        await _functionDefinitions.InsertOneAsync(functionDefinition);
        return functionDefinition;
    }

    public async Task<bool> UpdateAsync(string id, FunctionDefinition functionDefinition)
    {
        functionDefinition.UpdatedAt = DateTime.UtcNow;
        var result = await _functionDefinitions.ReplaceOneAsync(x => x.Id == id, functionDefinition);
        return result.IsAcknowledged && result.ModifiedCount > 0;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var result = await _functionDefinitions.DeleteOneAsync(x => x.Id == id);
        return result.IsAcknowledged && result.DeletedCount > 0;
    }

    public async Task<List<FunctionDefinition>> GetActiveAsync()
    {
        return await _functionDefinitions.Find(x => x.IsActive).ToListAsync();
    }
}
