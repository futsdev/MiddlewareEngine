using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace MiddlewareEngine.Models;

public class FunctionDefinition
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("function_id")]
    [JsonPropertyName("function_id")]
    public string FunctionId { get; set; } = string.Empty;

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("description")]
    public string? Description { get; set; }

    [BsonElement("execution_type")]
    [JsonPropertyName("execution_type")]
    public string ExecutionType { get; set; } = "RestApi";

    [BsonElement("operation_type")]
    [JsonPropertyName("operation_type")]
    public string OperationType { get; set; } = "READ";

    [BsonElement("execution_config")]
    [JsonPropertyName("execution_config")]
    public ExecutionConfig ExecutionConfig { get; set; } = new();

    [BsonElement("parameters")]
    public List<FunctionParameter> Parameters { get; set; } = new();

    [BsonElement("created_at")]
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updated_at")]
    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("is_active")]
    [JsonPropertyName("is_active")]
    public bool IsActive { get; set; } = true;
}

public enum ExecutionType
{
    RestApi,
    ScpiCommand,
    SdkMethod,
    Ssh
}

public enum OperationType
{
    READ,
    WRITE,
    FILE_UPLOAD,
    FILE_DOWNLOAD
}

public class ExecutionConfig
{
    // REST API Configuration
    [BsonElement("url")]
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [BsonElement("http_method")]
    [JsonPropertyName("http_method")]
    public string? HttpMethod { get; set; } // GET, POST, PUT, DELETE

    [BsonElement("headers")]
    [JsonPropertyName("headers")]
    public Dictionary<string, string>? Headers { get; set; }

    [BsonElement("timeout")]
    [JsonPropertyName("timeout")]
    public int? Timeout { get; set; }

    // SCPI Configuration
    [BsonElement("scpi_command")]
    [JsonPropertyName("scpi_command")]
    public string? ScpiCommand { get; set; }

    [BsonElement("connection_string")]
    [JsonPropertyName("connection_string")]
    public string? ConnectionString { get; set; }

    // SDK Method Configuration
    [BsonElement("assembly_name")]
    [JsonPropertyName("assembly_name")]
    public string? AssemblyName { get; set; }

    [BsonElement("class_name")]
    [JsonPropertyName("class_name")]
    public string? ClassName { get; set; }

    [BsonElement("method_name")]
    [JsonPropertyName("method_name")]
    public string? MethodName { get; set; }

    // SSH Configuration
    [BsonElement("ssh_host")]
    [JsonPropertyName("ssh_host")]
    public string? SshHost { get; set; }

    [BsonElement("ssh_port")]
    [JsonPropertyName("ssh_port")]
    public int? SshPort { get; set; } = 22;

    [BsonElement("ssh_username")]
    [JsonPropertyName("ssh_username")]
    public string? SshUsername { get; set; }

    [BsonElement("ssh_password")]
    [JsonPropertyName("ssh_password")]
    public string? SshPassword { get; set; }

    [BsonElement("ssh_key_path")]
    [JsonPropertyName("ssh_key_path")]
    public string? SshKeyPath { get; set; }

    // File Operation Configuration
    [BsonElement("remote_path")]
    [JsonPropertyName("remote_path")]
    public string? RemotePath { get; set; }

    [BsonElement("local_path")]
    [JsonPropertyName("local_path")]
    public string? LocalPath { get; set; }

    [BsonElement("file_name")]
    [JsonPropertyName("file_name")]
    public string? FileName { get; set; }

    [BsonElement("max_file_size_mb")]
    [JsonPropertyName("max_file_size_mb")]
    public int? MaxFileSizeMb { get; set; } = 100;

    // Custom fields for any execution type
    [BsonElement("custom_fields")]
    [JsonPropertyName("custom_fields")]
    public Dictionary<string, object>? CustomFields { get; set; }
}

public class FunctionParameter
{
    [BsonElement("name")]
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("type")]
    [JsonPropertyName("type")]
    public string Type { get; set; } = "string";

    [BsonElement("required")]
    [JsonPropertyName("required")]
    public bool Required { get; set; } = true;

    [BsonElement("default_value")]
    [JsonPropertyName("default_value")]
    public object? DefaultValue { get; set; }

    [BsonElement("description")]
    [JsonPropertyName("description")]
    public string? Description { get; set; }
}
