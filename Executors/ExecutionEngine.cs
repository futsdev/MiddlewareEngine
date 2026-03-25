using MiddlewareEngine.Models;
using MiddlewareEngine.Services;

namespace MiddlewareEngine.Executors;

public interface IExecutionEngine
{
    Task<FunctionExecutionResponse> ExecuteFunctionAsync(string functionId, Dictionary<string, object>? parameters);
    Task<FunctionExecutionResponse> ExecuteFunctionAsync(FunctionDefinition definition, Dictionary<string, object>? parameters);
}

public class ExecutionEngine : IExecutionEngine
{
    private readonly IFunctionDefinitionService _functionService;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ExecutionEngine> _logger;

    public ExecutionEngine(
        IFunctionDefinitionService functionService,
        IServiceProvider serviceProvider,
        ILogger<ExecutionEngine> logger)
    {
        _functionService = functionService;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task<FunctionExecutionResponse> ExecuteFunctionAsync(
        string functionId,
        Dictionary<string, object>? parameters)
    {
        try
        {
            // Get function definition
            var definition = await _functionService.GetFunctionByFunctionIdAsync(functionId);
            if (definition == null)
            {
                return new FunctionExecutionResponse
                {
                    Success = false,
                    ErrorMessage = $"Function with ID '{functionId}' not found",
                    FunctionId = functionId
                };
            }

            return await ExecuteFunctionAsync(definition, parameters);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing function: {FunctionId}", functionId);
            return new FunctionExecutionResponse
            {
                Success = false,
                ErrorMessage = ex.Message,
                FunctionId = functionId
            };
        }
    }

    public async Task<FunctionExecutionResponse> ExecuteFunctionAsync(
        FunctionDefinition definition,
        Dictionary<string, object>? parameters)
    {
        try
        {
            if (!definition.IsActive)
            {
                return new FunctionExecutionResponse
                {
                    Success = false,
                    ErrorMessage = $"Function '{definition.FunctionId}' is not active",
                    FunctionId = definition.FunctionId
                };
            }

            // Validate parameters
            ValidateParameters(definition, parameters);

            // Get appropriate executor based on execution type and operation type
            IFunctionExecutor executor;

            var operationType = definition.OperationType?.ToUpper();
            
            // Check if it's a file operation
            if (operationType == "FILE_UPLOAD" || operationType == "FILE_DOWNLOAD")
            {
                // For file operations, route based on execution type
                executor = definition.ExecutionType switch
                {
                    "Ssh" => _serviceProvider.GetRequiredService<SshExecutor>(),
                    "RestApi" or _ => _serviceProvider.GetRequiredService<FileOperationExecutor>()
                };
            }
            else
            {
                // Standard operations
                executor = definition.ExecutionType switch
                {
                    "RestApi" => _serviceProvider.GetRequiredService<RestApiExecutor>(),
                    "ScpiCommand" => _serviceProvider.GetRequiredService<ScpiExecutor>(),
                    "SdkMethod" => _serviceProvider.GetRequiredService<SdkMethodExecutor>(),
                    "Ssh" => _serviceProvider.GetRequiredService<SshExecutor>(),
                    _ => throw new NotSupportedException($"Execution type '{definition.ExecutionType}' is not supported")
                };
            }

            _logger.LogInformation("Executing function: {FunctionId} ({Name}) with type: {ExecutionType}, operation: {OperationType}", 
                definition.FunctionId, definition.Name, definition.ExecutionType, definition.OperationType);

            // Execute the function
            return await executor.ExecuteAsync(definition, parameters);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing function: {FunctionId}", definition.FunctionId);
            return new FunctionExecutionResponse
            {
                Success = false,
                ErrorMessage = ex.Message,
                FunctionId = definition.FunctionId
            };
        }
    }

    private void ValidateParameters(FunctionDefinition definition, Dictionary<string, object>? parameters)
    {
        foreach (var param in definition.Parameters.Where(p => p.Required))
        {
            if (parameters == null || !parameters.ContainsKey(param.Name))
            {
                throw new ArgumentException($"Required parameter '{param.Name}' is missing");
            }
        }
    }
}
