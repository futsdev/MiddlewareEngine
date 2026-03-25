using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MiddlewareEngine.Models;

public class Instrument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("description")]
    public string? Description { get; set; }

    [BsonElement("type")]
    public string Type { get; set; } = string.Empty; // Spectrum Analyzer, Signal Generator, Network Analyzer, etc.

    [BsonElement("manufacturer")]
    public string? Manufacturer { get; set; }

    [BsonElement("model")]
    public string? Model { get; set; }

    [BsonElement("serialNumber")]
    public string? SerialNumber { get; set; }

    [BsonElement("ipAddress")]
    public string? IpAddress { get; set; }

    [BsonElement("connectionType")]
    public string ConnectionType { get; set; } = "SCPI"; // SCPI, REST, SDK (kept for backward compatibility)

    [BsonElement("connections")]
    public List<InstrumentConnection> Connections { get; set; } = new();

    [BsonElement("status")]
    public string Status { get; set; } = "Available"; // Available, In Use, Maintenance, Offline

    [BsonElement("location")]
    public string? Location { get; set; }

    [BsonElement("calibrationDate")]
    public DateTime? CalibrationDate { get; set; }

    [BsonElement("nextCalibrationDate")]
    public DateTime? NextCalibrationDate { get; set; }

    [BsonElement("projectIds")]
    public List<string> ProjectIds { get; set; } = new();

    [BsonElement("setupIds")]
    public List<string> SetupIds { get; set; } = new();

    [BsonElement("tags")]
    public List<string> Tags { get; set; } = new();

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("createdBy")]
    public string? CreatedBy { get; set; }
}
