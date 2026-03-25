using MongoDB.Bson.Serialization.Attributes;

namespace MiddlewareEngine.Models;

public class InstrumentConnection
{
    [BsonElement("type")]
    public string Type { get; set; } = string.Empty; // SCPI, REST, SSH, SDK, VISA

    [BsonElement("ipAddress")]
    public string? IpAddress { get; set; }

    [BsonElement("port")]
    public int? Port { get; set; }

    [BsonElement("username")]
    public string? Username { get; set; }

    [BsonElement("password")]
    public string? Password { get; set; }

    [BsonElement("endpoint")]
    public string? Endpoint { get; set; } // For REST APIs

    [BsonElement("visaAddress")]
    public string? VisaAddress { get; set; } // For VISA/SCPI

    [BsonElement("sshKey")]
    public string? SshKey { get; set; } // For SSH connections

    [BsonElement("notes")]
    public string? Notes { get; set; }
}
