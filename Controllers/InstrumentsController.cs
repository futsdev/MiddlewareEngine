using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MiddlewareEngine.Models;

namespace MiddlewareEngine.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InstrumentsController : ControllerBase
{
    private readonly IMongoCollection<Instrument> _instruments;

    public InstrumentsController(IMongoDatabase database)
    {
        _instruments = database.GetCollection<Instrument>("Instruments");
    }

    [HttpGet]
    public async Task<ActionResult<List<Instrument>>> GetAll()
    {
        var instruments = await _instruments.Find(_ => true).ToListAsync();
        return Ok(instruments);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Instrument>> GetById(string id)
    {
        var instrument = await _instruments.Find(i => i.Id == id).FirstOrDefaultAsync();
        if (instrument == null) return NotFound();
        return Ok(instrument);
    }

    [HttpPost]
    public async Task<ActionResult<Instrument>> Create([FromBody] Instrument instrument)
    {
        instrument.CreatedAt = DateTime.UtcNow;
        instrument.UpdatedAt = DateTime.UtcNow;
        await _instruments.InsertOneAsync(instrument);
        return CreatedAtAction(nameof(GetById), new { id = instrument.Id }, instrument);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(string id, [FromBody] Instrument instrument)
    {
        instrument.Id = id;
        instrument.UpdatedAt = DateTime.UtcNow;
        var result = await _instruments.ReplaceOneAsync(i => i.Id == id, instrument);
        if (result.MatchedCount == 0) return NotFound();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(string id)
    {
        var result = await _instruments.DeleteOneAsync(i => i.Id == id);
        if (result.DeletedCount == 0) return NotFound();
        return NoContent();
    }
}
