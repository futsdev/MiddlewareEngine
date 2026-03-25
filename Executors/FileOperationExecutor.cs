using MiddlewareEngine.Models;
using System.Net.Http.Headers;

namespace MiddlewareEngine.Executors;

public class FileOperationExecutor : IFunctionExecutor
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<FileOperationExecutor> _logger;

    public FileOperationExecutor(IHttpClientFactory httpClientFactory, ILogger<FileOperationExecutor> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<FunctionExecutionResponse> ExecuteAsync(
        FunctionDefinition definition,
        Dictionary<string, object>? parameters)
    {
        try
        {
            var config = definition.ExecutionConfig;
            var operationType = definition.OperationType?.ToUpper();

            return operationType switch
            {
                "FILE_UPLOAD" => await ExecuteFileUploadAsync(config, parameters),
                "FILE_DOWNLOAD" => await ExecuteFileDownloadAsync(config, parameters),
                _ => throw new NotSupportedException($"Operation type {operationType} is not supported for file operations")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "File operation execution failed");
            return new FunctionExecutionResponse
            {
                Success = false,
                ErrorMessage = ex.Message,
                ExecutedAt = DateTime.UtcNow
            };
        }
    }

    private async Task<FunctionExecutionResponse> ExecuteFileUploadAsync(
        ExecutionConfig config,
        Dictionary<string, object>? parameters)
    {
        var url = config.Url ?? throw new ArgumentException("URL is required for file upload");
        
        var localPath = parameters?.ContainsKey("localPath") == true
            ? parameters["localPath"]?.ToString()
            : config.LocalPath;

        if (string.IsNullOrEmpty(localPath) || !File.Exists(localPath))
            throw new ArgumentException("Valid local file path is required");

        var fileInfo = new FileInfo(localPath);
        var maxSizeMb = config.MaxFileSizeMb ?? 100;
        if (fileInfo.Length > maxSizeMb * 1024 * 1024)
            throw new InvalidOperationException($"File size exceeds maximum allowed size of {maxSizeMb}MB");

        var client = _httpClientFactory.CreateClient();
        
        // Set timeout if specified
        if (config.Timeout.HasValue)
            client.Timeout = TimeSpan.FromSeconds(config.Timeout.Value);

        using var content = new MultipartFormDataContent();
        using var fileStream = File.OpenRead(localPath);
        using var streamContent = new StreamContent(fileStream);
        
        streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        
        var fileName = parameters?.ContainsKey("fileName") == true
            ? parameters["fileName"]?.ToString()
            : config.FileName ?? fileInfo.Name;

        content.Add(streamContent, "file", fileName ?? fileInfo.Name);

        // Add additional form fields if provided
        if (parameters != null)
        {
            foreach (var param in parameters.Where(p => p.Key != "localPath" && p.Key != "fileName"))
            {
                content.Add(new StringContent(param.Value?.ToString() ?? ""), param.Key);
            }
        }

        // Add headers
        if (config.Headers != null)
        {
            foreach (var header in config.Headers)
            {
                client.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        var startTime = DateTime.UtcNow;
        var response = await client.PostAsync(url, content);
        var duration = DateTime.UtcNow - startTime;

        var responseBody = await response.Content.ReadAsStringAsync();

        return new FunctionExecutionResponse
        {
            Success = response.IsSuccessStatusCode,
            Result = new
            {
                statusCode = (int)response.StatusCode,
                fileName,
                fileSizeBytes = fileInfo.Length,
                uploadedTo = url,
                durationMs = duration.TotalMilliseconds,
                responseBody
            },
            ErrorMessage = response.IsSuccessStatusCode ? null : $"HTTP {response.StatusCode}: {responseBody}",
            ExecutedAt = DateTime.UtcNow
        };
    }

    private async Task<FunctionExecutionResponse> ExecuteFileDownloadAsync(
        ExecutionConfig config,
        Dictionary<string, object>? parameters)
    {
        var url = config.Url ?? throw new ArgumentException("URL is required for file download");
        
        var localPath = parameters?.ContainsKey("localPath") == true
            ? parameters["localPath"]?.ToString()
            : config.LocalPath;

        if (string.IsNullOrEmpty(localPath))
            throw new ArgumentException("Local path is required");

        var client = _httpClientFactory.CreateClient();
        
        // Set timeout if specified
        if (config.Timeout.HasValue)
            client.Timeout = TimeSpan.FromSeconds(config.Timeout.Value);

        // Add headers
        if (config.Headers != null)
        {
            foreach (var header in config.Headers)
            {
                client.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        // Build query string from parameters
        if (parameters != null && parameters.Any())
        {
            var queryString = string.Join("&", parameters
                .Where(p => p.Key != "localPath")
                .Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value?.ToString() ?? "")}"));
            
            if (!string.IsNullOrEmpty(queryString))
                url = $"{url}?{queryString}";
        }

        var startTime = DateTime.UtcNow;
        var response = await client.GetAsync(url);
        var duration = DateTime.UtcNow - startTime;

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            return new FunctionExecutionResponse
            {
                Success = false,
                ErrorMessage = $"HTTP {response.StatusCode}: {errorBody}",
                ExecutedAt = DateTime.UtcNow
            };
        }

        // Ensure local directory exists
        var directory = Path.GetDirectoryName(localPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        // Download file
        using var fileStream = File.Create(localPath);
        await response.Content.CopyToAsync(fileStream);
        var fileSizeBytes = fileStream.Length;

        var fileName = response.Content.Headers.ContentDisposition?.FileName?.Trim('"')
            ?? Path.GetFileName(localPath);

        return new FunctionExecutionResponse
        {
            Success = true,
            Result = new
            {
                statusCode = (int)response.StatusCode,
                fileName,
                fileSizeBytes,
                localPath,
                downloadedFrom = url,
                durationMs = duration.TotalMilliseconds
            },
            ExecutedAt = DateTime.UtcNow
        };
    }
}
