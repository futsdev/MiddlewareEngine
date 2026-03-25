using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MiddlewareEngine.Models;

namespace MiddlewareEngine.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RolesController : ControllerBase
{
    private readonly IMongoCollection<Role> _roles;

    public RolesController(IMongoDatabase database)
    {
        _roles = database.GetCollection<Role>("Roles");
    }

    [HttpGet]
    public async Task<ActionResult<List<Role>>> GetAll()
    {
        var roles = await _roles.Find(_ => true).ToListAsync();
        return Ok(roles);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Role>> GetById(string id)
    {
        var role = await _roles.Find(r => r.Id == id).FirstOrDefaultAsync();
        if (role == null)
            return NotFound();
        return Ok(role);
    }

    [HttpPost]
    public async Task<ActionResult<Role>> Create(Role role)
    {
        role.CreatedAt = DateTime.UtcNow;
        await _roles.InsertOneAsync(role);
        return CreatedAtAction(nameof(GetById), new { id = role.Id }, role);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(string id, Role role)
    {
        role.Id = id;
        var result = await _roles.ReplaceOneAsync(r => r.Id == id, role);
        if (result.MatchedCount == 0)
            return NotFound();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(string id)
    {
        var result = await _roles.DeleteOneAsync(r => r.Id == id);
        if (result.DeletedCount == 0)
            return NotFound();
        return NoContent();
    }
}
