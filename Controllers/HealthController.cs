using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MiddlewareEngine.Configuration;
using Microsoft.Extensions.Options;

namespace MiddlewareEngine.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly IOptions<MongoDbSettings> _settings;
    private readonly ILogger<HealthController> _logger;

    public HealthController(IOptions<MongoDbSettings> settings, ILogger<HealthController> logger)
    {
        _settings = settings;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult> CheckHealth()
    {
        var result = new
        {
            api = "OK",
            timestamp = DateTime.UtcNow,
            mongodb = await CheckMongoDbConnection()
        };

        return Ok(result);
    }

    [HttpGet("mongodb")]
    public async Task<ActionResult> CheckMongoDbConnection()
    {
        try
        {
            var client = new MongoClient(_settings.Value.ConnectionString);
            var database = client.GetDatabase(_settings.Value.DatabaseName);
            
            // Try to ping the database
            await database.RunCommandAsync((Command<MongoDB.Bson.BsonDocument>)"{ping:1}");
            
            var collections = await database.ListCollectionNamesAsync();
            var collectionList = await collections.ToListAsync();

            return Ok(new
            {
                status = "Connected",
                connectionString = MaskConnectionString(_settings.Value.ConnectionString),
                database = _settings.Value.DatabaseName,
                collections = collectionList,
                message = "MongoDB connection successful"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MongoDB connection failed");
            return StatusCode(503, new
            {
                status = "Disconnected",
                connectionString = MaskConnectionString(_settings.Value.ConnectionString),
                database = _settings.Value.DatabaseName,
                error = ex.Message,
                message = "MongoDB connection failed. Please check if MongoDB is running and connection string is correct."
            });
        }
    }

    private string MaskConnectionString(string connectionString)
    {
        // Mask password in connection string for security
        if (string.IsNullOrEmpty(connectionString)) return "Not configured";
        
        if (connectionString.Contains("@"))
        {
            var parts = connectionString.Split('@');
            if (parts.Length > 1)
            {
                return $"mongodb://***:***@{parts[1]}";
            }
        }
        
        return connectionString.Contains("localhost") ? connectionString : "mongodb://***:***@***";
    }
}
