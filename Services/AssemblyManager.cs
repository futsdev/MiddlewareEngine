using System.Reflection;

namespace MiddlewareEngine.Services;

public interface IAssemblyManager
{
    Task<List<string>> GetAvailableAssembliesAsync();
    Task<List<string>> GetClassesInAssemblyAsync(string assemblyName);
    Task<List<MethodInfo>> GetMethodsInClassAsync(string assemblyName, string className);
    Task<string> UploadAssemblyAsync(IFormFile file);
    Assembly? LoadAssembly(string assemblyName);
}

public class AssemblyManager : IAssemblyManager
{
    private readonly ILogger<AssemblyManager> _logger;
    private readonly string _assembliesPath;

    public AssemblyManager(ILogger<AssemblyManager> logger)
    {
        _logger = logger;
        _assembliesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CustomAssemblies");
        
        if (!Directory.Exists(_assembliesPath))
        {
            Directory.CreateDirectory(_assembliesPath);
        }
    }

    public async Task<List<string>> GetAvailableAssembliesAsync()
    {
        var assemblies = new List<string>
        {
            // Common system assemblies
            "System.Private.CoreLib",
            "System.Console",
            "System.Runtime",
            "System.Linq",
            "System.Collections"
        };

        // Add custom uploaded assemblies
        await Task.Run(() =>
        {
            if (Directory.Exists(_assembliesPath))
            {
                var dllFiles = Directory.GetFiles(_assembliesPath, "*.dll");
                assemblies.AddRange(dllFiles.Select(f => Path.GetFileNameWithoutExtension(f)));
            }
        });

        return assemblies.Distinct().OrderBy(a => a).ToList();
    }

    public async Task<List<string>> GetClassesInAssemblyAsync(string assemblyName)
    {
        return await Task.Run(() =>
        {
            try
            {
                var assembly = LoadAssembly(assemblyName);
                if (assembly == null) return new List<string>();

                return assembly.GetExportedTypes()
                    .Where(t => t.IsClass && t.IsPublic)
                    .Select(t => t.FullName ?? t.Name)
                    .OrderBy(n => n)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting classes from assembly: {AssemblyName}", assemblyName);
                return new List<string>();
            }
        });
    }

    public async Task<List<MethodInfo>> GetMethodsInClassAsync(string assemblyName, string className)
    {
        return await Task.Run(() =>
        {
            try
            {
                var assembly = LoadAssembly(assemblyName);
                if (assembly == null) return new List<MethodInfo>();

                var type = assembly.GetType(className);
                if (type == null) return new List<MethodInfo>();

                return type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance)
                    .Where(m => !m.IsSpecialName) // Exclude property getters/setters
                    .OrderBy(m => m.Name)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting methods from class: {ClassName}", className);
                return new List<MethodInfo>();
            }
        });
    }

    public async Task<string> UploadAssemblyAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("Invalid file");
        }

        if (!file.FileName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Only DLL files are supported");
        }

        var fileName = Path.GetFileName(file.FileName);
        var filePath = Path.Combine(_assembliesPath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        _logger.LogInformation("Assembly uploaded: {FileName}", fileName);
        return Path.GetFileNameWithoutExtension(fileName);
    }

    public Assembly? LoadAssembly(string assemblyName)
    {
        try
        {
            // Try loading from GAC first
            return Assembly.Load(assemblyName);
        }
        catch
        {
            try
            {
                // Try loading from custom assemblies folder
                var assemblyPath = Path.Combine(_assembliesPath, $"{assemblyName}.dll");
                if (File.Exists(assemblyPath))
                {
                    return Assembly.LoadFrom(assemblyPath);
                }

                // Try loading from base directory
                var baseAssemblyPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{assemblyName}.dll");
                if (File.Exists(baseAssemblyPath))
                {
                    return Assembly.LoadFrom(baseAssemblyPath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load assembly: {AssemblyName}", assemblyName);
            }

            return null;
        }
    }
}
