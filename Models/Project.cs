using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MiddlewareEngine.Models;

public class Project
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("description")]
    public string? Description { get; set; }

    [BsonElement("status")]
    public string Status { get; set; } = "Active"; // Active, Inactive, Completed

    [BsonElement("priority")]
    public string Priority { get; set; } = "Medium"; // Critical, High, Medium, Low

    [BsonElement("customerName")]
    public string? CustomerName { get; set; }

    [BsonElement("customerEmail")]
    public string? CustomerEmail { get; set; }

    [BsonElement("customerPhone")]
    public string? CustomerPhone { get; set; }

    [BsonElement("customerCompany")]
    public string? CustomerCompany { get; set; }

    [BsonElement("startDate")]
    public DateTime? StartDate { get; set; }

    [BsonElement("endDate")]
    public DateTime? EndDate { get; set; }

    [BsonElement("setupIds")]
    public List<string> SetupIds { get; set; } = new();

    [BsonElement("instrumentIds")]
    public List<string> InstrumentIds { get; set; } = new();

    [BsonElement("testCaseIds")]
    public List<string> TestCaseIds { get; set; } = new();

    [BsonElement("tags")]
    public List<string> Tags { get; set; } = new();

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("createdBy")]
    public string? CreatedBy { get; set; }
}
