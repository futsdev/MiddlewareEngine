using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MiddlewareEngine.Models;

public class Setup
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("description")]
    public string? Description { get; set; }

    [BsonElement("type")]
    public string Type { get; set; } = string.Empty; // RF Testing, 5G NR, WiFi, LTE, etc.

    [BsonElement("status")]
    public string Status { get; set; } = "Active"; // Active, Inactive, Maintenance

    [BsonElement("instrumentIds")]
    public List<string> InstrumentIds { get; set; } = new();

    [BsonElement("projectIds")]
    public List<string> ProjectIds { get; set; } = new();

    [BsonElement("configuration")]
    public Dictionary<string, object>? Configuration { get; set; }

    [BsonElement("location")]
    public string? Location { get; set; }

    [BsonElement("tags")]
    public List<string> Tags { get; set; } = new();

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("createdBy")]
    public string? CreatedBy { get; set; }
}
