using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace MiddlewareEngine.Models;

public class Campaign
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("project_id")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string ProjectId { get; set; } = string.Empty;

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("description")]
    public string? Description { get; set; }

    [BsonElement("setup_id")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? SetupId { get; set; }

    [BsonElement("setup_ids")]
    public List<string> SetupIds { get; set; } = new();

    [BsonElement("status")]
    public string Status { get; set; } = "Draft"; // Draft, Ready, Running, Completed, Failed

    [BsonElement("test_groups")]
    public List<TestGroup> TestGroups { get; set; } = new();

    [BsonElement("created_by")]
    public string? CreatedBy { get; set; }

    [BsonElement("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("tags")]
    public List<string> Tags { get; set; } = new();
}

public class TestGroup
{
    [BsonElement("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("description")]
    public string? Description { get; set; }

    [BsonElement("order")]
    public int Order { get; set; }

    [BsonElement("retry_count")]
    public int RetryCount { get; set; } = 0;

    [BsonElement("delay_seconds")]
    public int DelaySeconds { get; set; } = 0;

    [BsonElement("abort_on_failure")]
    public bool AbortOnFailure { get; set; } = true;

    [BsonElement("enabled")]
    public bool Enabled { get; set; } = true;

    [BsonElement("setup_ids")]
    public List<string> SetupIds { get; set; } = new();

    [BsonElement("test_case_executions")]
    public List<CampaignTestCase> TestCaseExecutions { get; set; } = new();
}

public class CampaignTestCase
{
    [BsonElement("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [BsonElement("test_case_id")]
    public string TestCaseId { get; set; } = string.Empty;

    [BsonElement("test_case_name")]
    public string TestCaseName { get; set; } = string.Empty;

    [BsonElement("order")]
    public int Order { get; set; }

    [BsonElement("retry_count")]
    public int RetryCount { get; set; } = 0;

    [BsonElement("delay_seconds")]
    public int DelaySeconds { get; set; } = 0;

    [BsonElement("abort_on_failure")]
    public bool AbortOnFailure { get; set; } = true;

    [BsonElement("enabled")]
    public bool Enabled { get; set; } = true;

    [BsonElement("parameter_overrides")]
    public Dictionary<string, object> ParameterOverrides { get; set; } = new();

    [BsonElement("expected_result")]
    public string? ExpectedResult { get; set; }
    
    [BsonElement("test_case_definition")]
    public CompleteTestCaseDefinition? TestCaseDefinition { get; set; }
}

public class CompleteTestCaseDefinition
{
    [BsonElement("id")]
    public string? Id { get; set; }
    
    [BsonElement("name")]
    public string? Name { get; set; }
    
    [BsonElement("description")]
    public string? Description { get; set; }
    
    [BsonElement("tags")]
    public List<string> Tags { get; set; } = new();
    
    [BsonElement("setup")]
    public TestCaseSection Setup { get; set; } = new();
    
    [BsonElement("steps")]
    public List<CompleteStep> Steps { get; set; } = new();
    
    [BsonElement("teardown")]
    public TestCaseSection Teardown { get; set; } = new();
    
    [BsonElement("cleanup")]
    public TestCaseSection Cleanup { get; set; } = new();
}

public class TestCaseSection
{
    [BsonElement("section_name")]
    public string? SectionName { get; set; }
    
    [BsonElement("assigned_instrument")]
    public SectionInstrument? AssignedInstrument { get; set; }
    
    [BsonElement("operations")]
    public List<OperationWithFunction> Operations { get; set; } = new();
}

public class OperationWithFunction
{
    [BsonElement("id")]
    public string? Id { get; set; }
    
    [BsonElement("function_id")]
    public string? FunctionId { get; set; }
    
    [BsonElement("function_name")]
    public string? FunctionName { get; set; }
    
    [BsonElement("order")]
    public int Order { get; set; }
    
    [BsonElement("enabled")]
    public bool Enabled { get; set; } = true;
    
    [BsonElement("parameter_overrides")]
    public Dictionary<string, object> ParameterOverrides { get; set; } = new();
    
    [BsonElement("function_definition")]
    [BsonIgnoreIfNull]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object>? FunctionDefinition { get; set; }
}

public class CompleteStep
{
    [BsonElement("id")]
    public string? Id { get; set; }
    
    [BsonElement("name")]
    public string? Name { get; set; }
    
    [BsonElement("description")]
    public string? Description { get; set; }
    
    [BsonElement("order")]
    public int Order { get; set; }
    
    [BsonElement("assigned_instrument")]
    public SectionInstrument? AssignedInstrument { get; set; }
    
    [BsonElement("actions")]
    public List<CompleteAction> Actions { get; set; } = new();
}

public class CompleteAction
{
    [BsonElement("id")]
    public string? Id { get; set; }
    
    [BsonElement("name")]
    public string? Name { get; set; }
    
    [BsonElement("description")]
    public string? Description { get; set; }
    
    [BsonElement("order")]
    public int Order { get; set; }
    
    [BsonElement("pre_conditions")]
    public List<OperationWithFunction> PreConditions { get; set; } = new();
    
    [BsonElement("operations")]
    public List<OperationWithFunction> Operations { get; set; } = new();
    
    [BsonElement("post_conditions")]
    public List<OperationWithFunction> PostConditions { get; set; } = new();
}

public class SectionInstrument
{
    [BsonElement("setup_id")]
    public string SetupId { get; set; } = string.Empty;
    
    [BsonElement("instrument_id")]
    public string InstrumentId { get; set; } = string.Empty;
}

public class CampaignFunction
{
    [BsonElement("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [BsonElement("function_id")]
    public string FunctionId { get; set; } = string.Empty;
    
    [BsonElement("function_name")]
    public string FunctionName { get; set; } = string.Empty;
    
    [BsonElement("order")]
    public int Order { get; set; }
    
    [BsonElement("parameter_overrides")]
    public Dictionary<string, object> ParameterOverrides { get; set; } = new();
    
    [BsonElement("enabled")]
    public bool Enabled { get; set; } = true;
}
