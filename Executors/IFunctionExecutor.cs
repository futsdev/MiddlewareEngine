using MiddlewareEngine.Models;

namespace MiddlewareEngine.Executors;

public interface IFunctionExecutor
{
    Task<FunctionExecutionResponse> ExecuteAsync(FunctionDefinition definition, Dictionary<string, object>? parameters);
}
