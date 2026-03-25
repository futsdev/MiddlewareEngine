using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MiddlewareEngine.Models;

namespace MiddlewareEngine.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SetupsController : ControllerBase
{
    private readonly IMongoCollection<Setup> _setups;

    public SetupsController(IMongoDatabase database)
    {
        _setups = database.GetCollection<Setup>("Setups");
    }

    [HttpGet]
    public async Task<ActionResult<List<Setup>>> GetAll()
    {
        var setups = await _setups.Find(_ => true).ToListAsync();
        return Ok(setups);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Setup>> GetById(string id)
    {
        var setup = await _setups.Find(s => s.Id == id).FirstOrDefaultAsync();
        if (setup == null) return NotFound();
        return Ok(setup);
    }

    [HttpPost]
    public async Task<ActionResult<Setup>> Create([FromBody] Setup setup)
    {
        setup.CreatedAt = DateTime.UtcNow;
        setup.UpdatedAt = DateTime.UtcNow;
        await _setups.InsertOneAsync(setup);
        return CreatedAtAction(nameof(GetById), new { id = setup.Id }, setup);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(string id, [FromBody] Setup setup)
    {
        setup.Id = id;
        setup.UpdatedAt = DateTime.UtcNow;
        var result = await _setups.ReplaceOneAsync(s => s.Id == id, setup);
        if (result.MatchedCount == 0) return NotFound();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(string id)
    {
        var result = await _setups.DeleteOneAsync(s => s.Id == id);
        if (result.DeletedCount == 0) return NotFound();
        return NoContent();
    }
}
