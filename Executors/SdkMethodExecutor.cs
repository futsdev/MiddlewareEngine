using MiddlewareEngine.Models;
using MiddlewareEngine.Services;
using System.Reflection;

namespace MiddlewareEngine.Executors;

public class SdkMethodExecutor : IFunctionExecutor
{
    private readonly ILogger<SdkMethodExecutor> _logger;
    private readonly IAssemblyManager _assemblyManager;

    public SdkMethodExecutor(ILogger<SdkMethodExecutor> logger, IAssemblyManager assemblyManager)
    {
        _logger = logger;
        _assemblyManager = assemblyManager;
    }

    public async Task<FunctionExecutionResponse> ExecuteAsync(
        FunctionDefinition definition,
        Dictionary<string, object>? parameters)
    {
        var response = new FunctionExecutionResponse
        {
            FunctionId = definition.FunctionId
        };

        try
        {
            var config = definition.ExecutionConfig;
            if (string.IsNullOrEmpty(config.AssemblyName) || 
                string.IsNullOrEmpty(config.ClassName) || 
                string.IsNullOrEmpty(config.MethodName))
            {
                throw new InvalidOperationException("SDK method configuration is incomplete");
            }

            // Load the assembly using AssemblyManager (supports custom uploaded DLLs)
            var assembly = _assemblyManager.LoadAssembly(config.AssemblyName);
            if (assembly == null)
            {
                throw new InvalidOperationException($"Assembly '{config.AssemblyName}' not found. Make sure it's a system assembly or uploaded via /api/assemblies/upload");
            }

            // Get the type
            var type = assembly.GetType(config.ClassName);
            if (type == null)
            {
                throw new InvalidOperationException($"Type '{config.ClassName}' not found in assembly '{config.AssemblyName}'");
            }

            // Get the method
            var method = type.GetMethod(config.MethodName, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
            if (method == null)
            {
                throw new InvalidOperationException($"Method '{config.MethodName}' not found in type '{config.ClassName}'");
            }

            // Prepare parameters
            var methodParams = method.GetParameters();
            var args = new object?[methodParams.Length];

            for (int i = 0; i < methodParams.Length; i++)
            {
                var param = methodParams[i];
                if (parameters != null && parameters.ContainsKey(param.Name!))
                {
                    args[i] = ConvertParameter(parameters[param.Name!], param.ParameterType);
                }
                else if (param.HasDefaultValue)
                {
                    args[i] = param.DefaultValue;
                }
                else
                {
                    throw new ArgumentException($"Required parameter '{param.Name}' not provided");
                }
            }

            // Invoke the method
            object? instance = null;
            if (!method.IsStatic)
            {
                // Create instance if method is not static
                instance = Activator.CreateInstance(type);
            }

            var result = method.Invoke(instance, args);

            // Handle async methods
            if (result is Task task)
            {
                await task;
                var resultProperty = task.GetType().GetProperty("Result");
                result = resultProperty?.GetValue(task);
            }

            response.Success = true;
            response.Result = result;

            _logger.LogInformation("SDK method executed successfully: {ClassName}.{MethodName}", 
                config.ClassName, config.MethodName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing SDK method function: {FunctionId}", definition.FunctionId);
            response.Success = false;
            response.ErrorMessage = ex.Message;
        }

        return response;
    }

    private object? ConvertParameter(object value, Type targetType)
    {
        try
        {
            if (value == null)
                return null;

            if (targetType.IsAssignableFrom(value.GetType()))
                return value;

            if (targetType == typeof(string))
                return value.ToString();

            if (targetType.IsEnum)
                return Enum.Parse(targetType, value.ToString()!);

            return Convert.ChangeType(value, targetType);
        }
        catch (Exception ex)
        {
            throw new ArgumentException($"Cannot convert parameter value '{value}' to type '{targetType.Name}'", ex);
        }
    }
}
