using Microsoft.AspNetCore.Mvc;
using MiddlewareEngine.Services;

namespace MiddlewareEngine.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AssembliesController : ControllerBase
{
    private readonly IAssemblyManager _assemblyManager;
    private readonly ILogger<AssembliesController> _logger;

    public AssembliesController(IAssemblyManager assemblyManager, ILogger<AssembliesController> logger)
    {
        _assemblyManager = assemblyManager;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<string>>> GetAssemblies()
    {
        try
        {
            var assemblies = await _assemblyManager.GetAvailableAssembliesAsync();
            return Ok(assemblies);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting assemblies");
            return StatusCode(500, new { error = "Failed to retrieve assemblies" });
        }
    }

    [HttpGet("{assemblyName}/classes")]
    public async Task<ActionResult<List<string>>> GetClasses(string assemblyName)
    {
        try
        {
            var classes = await _assemblyManager.GetClassesInAssemblyAsync(assemblyName);
            return Ok(classes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting classes from assembly: {AssemblyName}", assemblyName);
            return StatusCode(500, new { error = "Failed to retrieve classes" });
        }
    }

    [HttpGet("{assemblyName}/classes/{className}/methods")]
    public async Task<ActionResult> GetMethods(string assemblyName, string className)
    {
        try
        {
            var methods = await _assemblyManager.GetMethodsInClassAsync(assemblyName, className);
            var methodInfos = methods.Select(m => new
            {
                name = m.Name,
                isStatic = m.IsStatic,
                returnType = m.ReturnType.Name,
                parameters = m.GetParameters().Select(p => new
                {
                    name = p.Name,
                    type = p.ParameterType.Name,
                    hasDefaultValue = p.HasDefaultValue,
                    defaultValue = p.DefaultValue
                }).ToList()
            }).ToList();

            return Ok(methodInfos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting methods from class: {ClassName}", className);
            return StatusCode(500, new { error = "Failed to retrieve methods" });
        }
    }

    [HttpPost("upload")]
    public async Task<ActionResult> UploadAssembly([FromForm] IFormFile file)
    {
        try
        {
            var assemblyName = await _assemblyManager.UploadAssemblyAsync(file);
            return Ok(new { 
                message = "Assembly uploaded successfully", 
                assemblyName 
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading assembly");
            return StatusCode(500, new { error = "Failed to upload assembly" });
        }
    }
}
