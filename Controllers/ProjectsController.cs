using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MiddlewareEngine.Models;

namespace MiddlewareEngine.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly IMongoCollection<Project> _projects;

    public ProjectsController(IMongoDatabase database)
    {
        _projects = database.GetCollection<Project>("Projects");
    }

    [HttpGet]
    public async Task<ActionResult<List<Project>>> GetAll()
    {
        var projects = await _projects.Find(_ => true).ToListAsync();
        return Ok(projects);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Project>> GetById(string id)
    {
        var project = await _projects.Find(p => p.Id == id).FirstOrDefaultAsync();
        if (project == null) return NotFound();
        return Ok(project);
    }

    [HttpPost]
    public async Task<ActionResult<Project>> Create([FromBody] Project project)
    {
        project.CreatedAt = DateTime.UtcNow;
        project.UpdatedAt = DateTime.UtcNow;
        await _projects.InsertOneAsync(project);
        return CreatedAtAction(nameof(GetById), new { id = project.Id }, project);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(string id, [FromBody] Project project)
    {
        project.Id = id;
        project.UpdatedAt = DateTime.UtcNow;
        var result = await _projects.ReplaceOneAsync(p => p.Id == id, project);
        if (result.MatchedCount == 0) return NotFound();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(string id)
    {
        var result = await _projects.DeleteOneAsync(p => p.Id == id);
        if (result.DeletedCount == 0) return NotFound();
        return NoContent();
    }
}
