namespace MiddlewareEngine.Models;

public class FunctionExecutionRequest
{
    public string FunctionId { get; set; } = string.Empty;
    public Dictionary<string, object>? Parameters { get; set; }
}

public class FunctionExecutionResponse
{
    public bool Success { get; set; }
    public object? Result { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
    public string FunctionId { get; set; } = string.Empty;
}
