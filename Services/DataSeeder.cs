using System.Text.Json;
using MiddlewareEngine.Models;
using MiddlewareEngine.Repositories;

namespace MiddlewareEngine.Services;

public interface IDataSeeder
{
    Task SeedAsync();
    Task<int> GetFunctionCountAsync();
}

public class DataSeeder : IDataSeeder
{
    private readonly IFunctionDefinitionRepository _repository;
    private readonly ILogger<DataSeeder> _logger;

    public DataSeeder(IFunctionDefinitionRepository repository, ILogger<DataSeeder> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<int> GetFunctionCountAsync()
    {
        var functions = await _repository.GetAllAsync();
        return functions.Count;
    }

    public async Task SeedAsync()
    {
        try
        {
            // Check if data already exists
            var existingFunctions = await _repository.GetAllAsync();
            if (existingFunctions.Any())
            {
                _logger.LogInformation("Database already contains {Count} function(s). Skipping seed.", existingFunctions.Count);
                return;
            }

            // Load seed data from JSON file
            var seedDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "SeedData.json");
            
            if (!File.Exists(seedDataPath))
            {
                _logger.LogWarning("Seed data file not found at {Path}", seedDataPath);
                return;
            }

            var jsonData = await File.ReadAllTextAsync(seedDataPath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            
            var seedFunctions = JsonSerializer.Deserialize<List<FunctionDefinition>>(jsonData, options);

            if (seedFunctions == null || !seedFunctions.Any())
            {
                _logger.LogWarning("No seed data found in file");
                return;
            }

            // Insert seed data
            _logger.LogInformation("Seeding database with {Count} functions...", seedFunctions.Count);
            
            int successCount = 0;
            foreach (var function in seedFunctions)
            {
                try
                {
                    await _repository.CreateAsync(function);
                    successCount++;
                    _logger.LogDebug("Seeded function: {FunctionId} - {Name}", function.FunctionId, function.Name);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to seed function: {FunctionId}", function.FunctionId);
                }
            }

            _logger.LogInformation("Successfully seeded {Count} of {Total} functions", successCount, seedFunctions.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during database seeding");
            throw;
        }
    }
}
