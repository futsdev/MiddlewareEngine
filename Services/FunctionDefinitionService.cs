using MiddlewareEngine.Models;
using MiddlewareEngine.Repositories;

namespace MiddlewareEngine.Services;

public interface IFunctionDefinitionService
{
    Task<List<FunctionDefinition>> GetAllFunctionsAsync();
    Task<FunctionDefinition?> GetFunctionByIdAsync(string id);
    Task<FunctionDefinition?> GetFunctionByFunctionIdAsync(string functionId);
    Task<FunctionDefinition> CreateFunctionAsync(FunctionDefinition functionDefinition);
    Task<bool> UpdateFunctionAsync(string id, FunctionDefinition functionDefinition);
    Task<bool> DeleteFunctionAsync(string id);
    Task<List<FunctionDefinition>> GetActiveFunctionsAsync();
}

public class FunctionDefinitionService : IFunctionDefinitionService
{
    private readonly IFunctionDefinitionRepository _repository;
    private readonly ILogger<FunctionDefinitionService> _logger;

    public FunctionDefinitionService(
        IFunctionDefinitionRepository repository,
        ILogger<FunctionDefinitionService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<List<FunctionDefinition>> GetAllFunctionsAsync()
    {
        try
        {
            return await _repository.GetAllAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all functions");
            throw;
        }
    }

    public async Task<FunctionDefinition?> GetFunctionByIdAsync(string id)
    {
        try
        {
            return await _repository.GetByIdAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving function by ID: {Id}", id);
            throw;
        }
    }

    public async Task<FunctionDefinition?> GetFunctionByFunctionIdAsync(string functionId)
    {
        try
        {
            return await _repository.GetByFunctionIdAsync(functionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving function by FunctionId: {FunctionId}", functionId);
            throw;
        }
    }

    public async Task<FunctionDefinition> CreateFunctionAsync(FunctionDefinition functionDefinition)
    {
        try
        {
            // Check if function_id already exists
            var existing = await _repository.GetByFunctionIdAsync(functionDefinition.FunctionId);
            if (existing != null)
            {
                throw new InvalidOperationException($"Function with FunctionId '{functionDefinition.FunctionId}' already exists");
            }

            return await _repository.CreateAsync(functionDefinition);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating function");
            throw;
        }
    }

    public async Task<bool> UpdateFunctionAsync(string id, FunctionDefinition functionDefinition)
    {
        try
        {
            return await _repository.UpdateAsync(id, functionDefinition);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating function: {Id}", id);
            throw;
        }
    }

    public async Task<bool> DeleteFunctionAsync(string id)
    {
        try
        {
            return await _repository.DeleteAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting function: {Id}", id);
            throw;
        }
    }

    public async Task<List<FunctionDefinition>> GetActiveFunctionsAsync()
    {
        try
        {
            return await _repository.GetActiveAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active functions");
            throw;
        }
    }
}
