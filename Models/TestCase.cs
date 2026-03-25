using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace MiddlewareEngine.Models;

/// <summary>
/// Represents an operation that can be executed (REST, SCPI, SDK, SSH)
/// </summary>
public class Operation
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    // Type: RestApi, Scpi, SdkMethod, Ssh
    public string OperationType { get; set; } = string.Empty;
    
    // Reference to FunctionDefinition ID or inline definition
    public string? FunctionDefinitionId { get; set; }
    
    // Inline operation details (alternative to FunctionDefinitionId)
    public Dictionary<string, object>? OperationDetails { get; set; }
    
    // Parameters for this specific execution
    public Dictionary<string, object>? Parameters { get; set; }
    
    // Expected result for validation
    public string? ExpectedResult { get; set; }
    
    // Timeout in seconds
    public int TimeoutSeconds { get; set; } = 30;
    
    // Continue on failure
    public bool ContinueOnFailure { get; set; } = false;
    
    // Order for execution
    public int Order { get; set; } = 0;
}

/// <summary>
/// Represents an action with pre and post conditions
/// </summary>
public class TestAction
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    // Pre-condition operations
    public List<Operation> PreConditions { get; set; } = new();
    
    // Main operations
    public List<Operation> Operations { get; set; } = new();
    
    // Post-condition operations
    public List<Operation> PostConditions { get; set; } = new();
    
    // Order for execution
    public int Order { get; set; } = 0;
    
    // Continue to next action on failure
    public bool ContinueOnFailure { get; set; } = false;
    
    // Delay before execution (in milliseconds)
    public int DelayBeforeMs { get; set; } = 0;
    
    // Delay after execution (in milliseconds)
    public int DelayAfterMs { get; set; } = 0;
}

/// <summary>
/// Represents a step in a test case
/// </summary>
public class TestStep
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    // Multiple actions per step
    public List<TestAction> Actions { get; set; } = new();
    
    // Order for execution
    public int Order { get; set; } = 0;
    
    // Continue to next step on failure
    public bool ContinueOnFailure { get; set; } = false;
}

/// <summary>
/// Represents a complete test case
/// </summary>
public class TestCase
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    // Test case category/tags
    public List<string> Tags { get; set; } = new();
    
    // Test case priority (High, Medium, Low)
    public string Priority { get; set; } = "Medium";
    
    // Test steps
    public List<TestStep> Steps { get; set; } = new();
    
    // Global setup operations (run before all steps)
    public List<Operation> SetupOperations { get; set; } = new();
    
    // Global teardown operations (run after all steps)
    public List<Operation> TeardownOperations { get; set; } = new();
    
    // Post-test cleanup operations (run after teardown)
    public List<Operation> PostTestCleanup { get; set; } = new();
    
    // Test case status
    public string Status { get; set; } = "Draft"; // Draft, Active, Deprecated
    
    // Created/Updated metadata
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;
    
    // Execution history reference
    public List<string> ExecutionHistoryIds { get; set; } = new();
}

/// <summary>
/// Result of an operation execution
/// </summary>
public class OperationResult
{
    public string OperationId { get; set; } = string.Empty;
    public string OperationName { get; set; } = string.Empty;
    public bool Success { get; set; }
    public object? Result { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
    public double ExecutionTimeMs { get; set; }
}

/// <summary>
/// Result of an action execution
/// </summary>
public class TestActionResult
{
    public string ActionId { get; set; } = string.Empty;
    public string ActionName { get; set; } = string.Empty;
    public bool Success { get; set; }
    public List<OperationResult> PreConditionResults { get; set; } = new();
    public List<OperationResult> OperationResults { get; set; } = new();
    public List<OperationResult> PostConditionResults { get; set; } = new();
    public string? ErrorMessage { get; set; }
    public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
    public double ExecutionTimeMs { get; set; }
}

/// <summary>
/// Result of a step execution
/// </summary>
public class StepResult
{
    public string StepId { get; set; } = string.Empty;
    public string StepName { get; set; } = string.Empty;
    public bool Success { get; set; }
    public List<TestActionResult> ActionResults { get; set; } = new();
    public string? ErrorMessage { get; set; }
    public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
    public double ExecutionTimeMs { get; set; }
}

/// <summary>
/// Complete test case execution result
/// </summary>
public class TestCaseExecution
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string TestCaseId { get; set; } = string.Empty;
    public string TestCaseName { get; set; } = string.Empty;
    
    public bool Success { get; set; }
    public string Status { get; set; } = "Running"; // Running, Completed, Failed, Aborted
    
    public List<OperationResult> SetupResults { get; set; } = new();
    public List<StepResult> StepResults { get; set; } = new();
    public List<OperationResult> TeardownResults { get; set; } = new();
    
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public double TotalExecutionTimeMs { get; set; }
    
    public string? ErrorMessage { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}
